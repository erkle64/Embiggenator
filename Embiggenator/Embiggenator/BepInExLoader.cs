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

            var config_inventorySlots = Config.Bind("Inventory", "inventorySlots", 72, "Number of slots for character inventory space.");
            PluginComponent.inventorySlots = config_inventorySlots.Value;
            if (PluginComponent.inventorySlots < 0) PluginComponent.inventorySlots = 0;

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

                var original = AccessTools.Method(typeof(CharacterManager), "joinWorld");
                var post = AccessTools.Method(typeof(PluginComponent), "joinWorld");
                harmony.Patch(original, postfix: new HarmonyMethod(post));
            }
            catch
            {
                log.LogError("Harmony - FAILED to Apply Patch's!");
            }
        }
    }
}