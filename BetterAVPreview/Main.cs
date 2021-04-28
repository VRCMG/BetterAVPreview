using UnhollowerRuntimeLib;
using Harmony;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using AVPreview.Utils;

namespace AVPreview
{
    public static class BuildInfo
    {
        public const string Name = "BetterAVPreview";
        public const string Author = "Davi";
        public const string Version = "1.0.0";
    }

    public class Main : MelonMod
    {
        private static MelonMod Instance;
        public static HarmonyInstance HarmonyInstance => Instance.Harmony;
        public static EnableDisableListener listener;
        public static GameObject toggleClone;

        public override void OnApplicationStart()
        {
            Instance = this;
            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();
            HarmonyPatches();
            MelonLogger.Msg("Successfully loaded!");
        }

        private static void HarmonyPatches() => Controller.HarmonyPatches();

        public override void VRChat_OnUiManagerInit()
        {
            listener = GameObject.Find("UserInterface/MenuContent/Screens/Avatar").AddComponent<EnableDisableListener>();
            Controller.VRChat_OnUiManagerInit();
            Rotator.VRChat_OnUiManagerInit();

            GameObject Menu = GameObject.Find("UserInterface/MenuContent/Screens");

            toggleClone = Object.Instantiate(Menu.transform.Find("Settings/MousePanel/").Find("InvertedMouse").gameObject, Menu.transform.Find("Avatar").transform);

            toggleClone.GetComponent<Toggle>().onValueChanged = new Toggle.ToggleEvent();
            toggleClone.GetComponent<Toggle>().onValueChanged.AddListener((UnityEngine.Events.UnityAction<bool>)((isOn) => { OnButtonToggle(isOn); }));

            toggleClone.transform.Find("Label").GetComponent<Text>().text = "BetterAVPreview";
            toggleClone.name = "ToggleRot";

            toggleClone.GetComponent<Toggle>().isOn = false;

            Rotator.VRChat_OnUiManagerInit();
        }

        private static void OnButtonToggle(bool isOn)
        {
            Controller.OnButtonToggle(isOn);
            Rotator.OnButtonToggle(isOn);
        }
    }
}