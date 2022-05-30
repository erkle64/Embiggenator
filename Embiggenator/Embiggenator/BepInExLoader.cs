using BepInEx;
using BepInEx.Configuration;
using UnhollowerRuntimeLib;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Embiggenator
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class BepInExLoader : BepInEx.IL2CPP.BasePlugin
    {
        public const string
            MODNAME = "Embiggenator",
            AUTHOR = "erkle64",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "1.0.0";

        public static BepInEx.Logging.ManualLogSource log;

        public BepInExLoader()
        {
            log = Log;
        }

        public override void Load()
        {
            log.LogMessage("Registering PluginComponent in Il2Cpp");

            var config_additionalInventorySlots = Config.Bind("Inventory", "additionalSlots", 32, "Number of slots to add to character inventory space.");
            PluginComponent.additionalInventorySlots = config_additionalInventorySlots.Value;
            if (PluginComponent.additionalInventorySlots < 0) PluginComponent.additionalInventorySlots = 0;

            try
            {
                ClassInjector.RegisterTypeInIl2Cpp<PluginComponent>();

                var go = new GameObject("Erkle64_Embiggenator_PluginObject");
                go.AddComponent<PluginComponent>();
                Object.DontDestroyOnLoad(go);
            }
            catch
            {
                log.LogError("FAILED to Register Il2Cpp Type: PluginComponent!");
            }

            try
            {
                var harmony = new Harmony(GUID);

                var original = AccessTools.Method(typeof(CharacterManager), "Init");
                var post = AccessTools.Method(typeof(PluginComponent), "Init");
                harmony.Patch(original, postfix: new HarmonyMethod(post));
            }
            catch
            {
                log.LogError("Harmony - FAILED to Apply Patch's!");
            }
        }
    }
}