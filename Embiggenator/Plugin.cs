using C3.ModKit;
using HarmonyLib;
using Unfoundry;
using UnityEngine;
using UnityEngine.UI;

namespace Embiggenator
{
    [UnfoundryMod(GUID)]
    public class Plugin : UnfoundryPlugin
    {
        public const string
            MODNAME = "Embiggenator",
            AUTHOR = "erkle64",
            GUID = AUTHOR + "." + MODNAME,
            VERSION = "1.4.0";

        public static LogSource log;

        public static TypedConfigEntry<int> inventorySlots;
        private static TypedConfigEntry<int> maxVerticalSize;
        private static TypedConfigEntry<int> scrollSpeed;
        public static TypedConfigEntry<bool> disableInventoryResearch;

        public Plugin()
        {
            log = new LogSource(MODNAME);
            new Config(GUID)
                .Group("Inventory")
                    .Entry(out inventorySlots, "Slot Count", 70, "Total number of inventory slots players should have.", "Maximum size: 200")
                    .Entry(out maxVerticalSize, "Max Vertical Size", 8, "Maximum number rows to display before adding a scrollbar.")
                    .Entry(out scrollSpeed, "Scroll Speed", 40, "Speed to scroll the inventory window when using the mouse wheel.")
                .EndGroup()
                .Group("Research")
                    .Entry(out disableInventoryResearch, "Disable Inventory Research", true, "Prevents research from increasing inventory size.")
                .EndGroup()
                .Load()
                .Save();
        }

        public override void Load(Mod mod)
        {
            log.Log($"Loading {MODNAME} version {VERSION}");
        }

        [HarmonyPatch]
        public class Patch
        {
            [HarmonyPatch(typeof(CharacterManager), nameof(CharacterManager.joinWorld))]
            [HarmonyPostfix]
            public static void joinWorld(Character character, uint clientId)
            {
                var inventorySlots = Mathf.Clamp(Plugin.inventorySlots.Get(), 10, 200);
                if (inventorySlots > 0)
                {
                    var currentInventorySlots = InventoryManager.inventoryManager_getInventorySlotCountByPtr(character.inventoryPtr);
                    var additionalInventorySlots = inventorySlots - currentInventorySlots;
                    if (additionalInventorySlots > 0)
                    {
                        log.Log($"Embiggenating inventory by {additionalInventorySlots} slots for {character.username}");
                        InventoryManager.inventoryManager_enlargeInventory(character.inventoryId, (uint)additionalInventorySlots);
                    }
                    else
                    {
                        log.Log($"Skipping {character.username}, they already have {currentInventorySlots} slots");
                    }
                }
            }

            [HarmonyPatch(typeof(CharacterInventoryFrame), nameof(CharacterInventoryFrame.Init))]
            [HarmonyPostfix]
            public static void CharacterInventoryFrame_Init(CharacterInventoryFrame __instance)
            {
                if (__instance.transform.Find("ScrollBox")) return;

                log.Log("Creating scrollbox for character inventory frame.");

                UIBuilder.BeginWith(__instance.itemSlotContainer.transform.parent.gameObject)
                    .Element_ScrollBox("ScrollBox", contentBuilder =>
                    {
                        __instance.itemSlotContainer.transform.SetParent(contentBuilder.GameObject.transform.parent, false);
                        Object.DestroyImmediate(contentBuilder.GameObject);
                    })
                        .WithComponent<ScrollRect>(scrollBox =>
                        {
                            scrollBox.horizontal = false;
                            scrollBox.content = __instance.itemSlotContainer.GetComponent<RectTransform>();
                            scrollBox.movementType = ScrollRect.MovementType.Clamped;
                            scrollBox.scrollSensitivity = scrollSpeed.Get();
                        })
                        .With(scrollBox =>
                        {
                            var grid = __instance.itemSlotContainer.GetComponent<GridLayoutGroup>();
                            var maxSize = grid.cellSize.y * maxVerticalSize.Get() + grid.spacing.y * (maxVerticalSize.Get() - 1);
                            scrollBox.AddComponent<AdaptablePreferred>()
                                .Setup(__instance.itemSlotContainer.GetComponent<RectTransform>(), true, maxSize);
                            scrollBox.transform.SetAsFirstSibling();
                        })
                    .Done
                    .End();

                UIBuilder.BeginWith(__instance.itemSlotContainer.gameObject)
                    .SetRectTransform(0, 0, 0, 0, 0, 1, 0, 1, 1, 1)
                    .AutoSize(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize)
                    .End();
            }

            [HarmonyPatch(typeof(CharacterManager), nameof(CharacterManager.increasePlayerInventorySizeByResearch))]
            [HarmonyPrefix]
            public static bool CharacterManager_increasePlayerInventorySizeByResearch()
            {
                return !disableInventoryResearch.Get();
            }
        }
    }
}


