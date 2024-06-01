using C3.ModKit;
using HarmonyLib;
using Unfoundry;

namespace Embiggenator
{
    [UnfoundryMod(Plugin.GUID)]
    public class Plugin : UnfoundryPlugin
    {
        public const string
            MODNAME = "Embiggenator",
            AUTHOR = "erkle64",
            GUID = AUTHOR + "." + MODNAME,
            VERSION = "1.3.0";

        public static LogSource log;

        public static TypedConfigEntry<int> inventorySlots;
        public static TypedConfigEntry<bool> disableInventoryResearch;

        public Plugin()
        {
            log = new LogSource(MODNAME);
            new Config(GUID)
                .Group("Inventory")
                    .Entry(out inventorySlots, "Slot Count", 70, "Total number of inventory slots players should have.")
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
                if (inventorySlots.Get() > 0)
                {
                    var currentInventorySlots = InventoryManager.inventoryManager_getInventorySlotCountByPtr(character.inventoryPtr);
                    var additionalInventorySlots = inventorySlots.Get() - currentInventorySlots;
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

            [HarmonyPatch(typeof(CharacterManager), nameof(CharacterManager.increasePlayerInventorySizeByResearch))]
            [HarmonyPrefix]
            public static bool CharacterManager_increasePlayerInventorySizeByResearch()
            {
                return !disableInventoryResearch.Get();
            }
        }
    }
}


