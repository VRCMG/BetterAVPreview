using UnhollowerRuntimeLib;
using Harmony;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using AVPreview.Utils;
using UnhollowerRuntimeLib.XrefScans;

namespace AVPreview
{
    public static class BuildInfo
    {
        public const string Name = "BetterAVPreview";
        public const string Author = "Davi";
        public const string Version = "1.0.3";
    }

    public class Main : MelonMod
    {
        private static MelonMod Instance;
        public static HarmonyInstance HarmonyInstance => Instance.Harmony;
        public static EnableDisableListener listener;
        public static GameObject toggleClone;

        // To use: XRef(nameof(<insert class>.<insert method>));
        // public static void XRef(string Name) { foreach (var instance in XrefScanner.UsedBy(typeof(AvatarPlayableController).GetMethod(Name))) if (instance.TryResolve() != null) MelonLogger.Msg(instance.TryResolve().FullDescription()); }
        // To use: XRefStr(<string to find on methods>);
        // public static void XRefStr(string Str) { foreach (var method in typeof(<insert class>).GetMethods()) { try { foreach (UnhollowerRuntimeLib.XrefScans.XrefInstance instance in UnhollowerRuntimeLib.XrefScans.XrefScanner.XrefScan(method)) { try { if (instance.Type == UnhollowerRuntimeLib.XrefScans.XrefType.Global) { MelonLogger.Msg($"{method.Name}, {instance.ReadAsObject().ToString()}"); } } catch { } } } catch { } } }

        public override void OnApplicationStart()
        {
            // XRef(nameof(ActionMenu.Method_Private_Void_PDM_1));
            Instance = this;
            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();
            HarmonyPatches();
            MelonLogger.Msg("Successfully loaded!");
        }

        private static void HarmonyPatches() => Controller.HarmonyPatches();

        public override void VRChat_OnUiManagerInit()
        {
            listener = GameObject.Find("UserInterface/MenuContent/Screens/Avatar").AddComponent<EnableDisableListener>();

            GameObject Menu = GameObject.Find("UserInterface/MenuContent/Screens");

            toggleClone = Object.Instantiate(Menu.transform.Find("Settings/MousePanel/").Find("InvertedMouse").gameObject, Menu.transform.Find("Avatar").transform);

            toggleClone.GetComponent<Toggle>().onValueChanged = new Toggle.ToggleEvent();
            toggleClone.GetComponent<Toggle>().onValueChanged.AddListener((UnityEngine.Events.UnityAction<bool>)((isOn) => { OnButtonToggle(isOn); }));

            toggleClone.transform.Find("Label").GetComponent<Text>().text = "BetterAVPreview";
            toggleClone.name = "ToggleRot";

            toggleClone.GetComponent<Toggle>().isOn = false;

            Controller.VRChat_OnUiManagerInit();
            Rotator.VRChat_OnUiManagerInit();
        }

        private static void OnButtonToggle(bool isOn)
        {
            Controller.OnButtonToggle(isOn);
            Rotator.OnButtonToggle(isOn);
        }
    }
}