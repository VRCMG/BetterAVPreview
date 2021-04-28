using UnhollowerRuntimeLib;
using Harmony;
using MelonLoader;
using UnityEngine;
using AVPreview.Utils;

namespace AVPreview
{
    public static class BuildInfo
    {
        public const string Name = "AVPreview3.0";
        public const string Author = "Davi";
        public const string Version = "1.0.0";
    }

    public class Main : MelonMod
    {
        private static MelonMod Instance;
        public static HarmonyInstance HarmonyInstance => Instance.Harmony;
        public static EnableDisableListener listener;

        public override void OnApplicationStart()
        {
            Instance = this;
            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();
            HarmonyPatches();
            MelonLogger.Msg("Successfully loaded!");
        }

        public override void VRChat_OnUiManagerInit()
        {
            listener = GameObject.Find("UserInterface/MenuContent/Screens/Avatar").AddComponent<EnableDisableListener>();
            Controller.VRChat_OnUiManagerInit();
            Rotator.VRChat_OnUiManagerInit();
        }

        private static void HarmonyPatches()
        {
            Controller.HarmonyPatches();
        }
    }
}