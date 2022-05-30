using System;
using UnhollowerBaseLib;
using HarmonyLib;
using UnityEngine;

namespace Embiggenator
{
    public class PluginComponent : MonoBehaviour
    {
        public static int additionalInventorySlots = 0;

        public PluginComponent (IntPtr ptr) : base(ptr)
        {
        }

        [HarmonyPostfix]
        public static void Init(CubeSavegame savegame, bool isServerJoin)
        {
            if (additionalInventorySlots > 0)
            {
                BepInExLoader.log.LogMessage(string.Format("Embiggenating inventory by {0} slots", additionalInventorySlots));
                CharacterManager.increasePlayerInventorySizeByResearch(additionalInventorySlots);
            }
        }
    }
}