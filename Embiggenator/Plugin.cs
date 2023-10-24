using Channel3.ModKit;
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
            VERSION = "0.2.0";

        public static LogSource log;
        private Config _config;

        public static int inventorySlots = 0;

        public Plugin()
        {
            log = new LogSource(MODNAME);
            _config = new Config(GUID);
            _config
                .Group("Inventory")
                    .Entry(out var inventorySlotsEntry, "Slot Count", 64, "Total number of inventory slots players should have.")
                .EndGroup()
                .Load()
                .Save();

            inventorySlots = inventorySlotsEntry.Get();
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
        }
    }
}


