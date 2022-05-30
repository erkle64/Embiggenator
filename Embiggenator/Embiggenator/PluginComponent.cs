using System;
using UnhollowerBaseLib;
using HarmonyLib;
using UnityEngine;

namespace Embiggenator
{
    public class PluginComponent : MonoBehaviour
    {
        public static int inventorySlots = 0;

        public PluginComponent (IntPtr ptr) : base(ptr)
        {
        }

        [HarmonyPostfix]
        public static void Init(CubeSavegame savegame, bool isServerJoin)
        {
            if (inventorySlots > 0)
            {
                BepInExLoader.log.LogMessage(string.Format("Embiggenating inventory by {0} slots", inventorySlots));
                CharacterManager.increasePlayerInventorySizeByResearch(inventorySlots);
            }
        }

        [HarmonyPostfix]
        public static void joinWorld(Character character, uint clientId)
        {
            if (inventorySlots > 0)
            {
                var currentInventorySlots = InventoryManager.inventoryManager_getInventorySlotCount(character.inventoryPtr);
                var additionalInventorySlots = inventorySlots - currentInventorySlots;
                BepInExLoader.log.LogMessage(string.Format("joinWorld {0} {1} {2}", currentInventorySlots, additionalInventorySlots, inventorySlots));
                if (additionalInventorySlots > 0)
                {
                    BepInExLoader.log.LogMessage(string.Format("Embiggenating inventory by {0} slots for {1}", additionalInventorySlots, character.username));
                    InventoryManager.inventoryManager_enlargeInventory(character.inventoryId, (uint)additionalInventorySlots);
                }
                else
                {
                    BepInExLoader.log.LogMessage(string.Format("Skipping {0}, they already have {1} slots", character.username, currentInventorySlots));
                }
            }
        }
    }
}