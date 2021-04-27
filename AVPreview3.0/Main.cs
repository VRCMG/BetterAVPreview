using Harmony;
using MelonLoader;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using System.Reflection;
using UnhollowerRuntimeLib.XrefScans;

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

        private static AvatarPlayableController CurrentController;
        private static bool Patch = true;

        public override void OnApplicationStart()
        {
            Instance = this;
            Instance.Harmony.Patch(typeof(ActionMenuDriver).GetMethod(nameof(ActionMenuDriver.Method_Public_AvatarPlayableController_0)), new HarmonyMethod(typeof(Main).GetMethod(nameof(Prefix))), new HarmonyMethod(typeof(Main).GetMethod(nameof(Postfix))));
        }

        // To use: AVPreview.Main.XRef(nameof(<insert class>.<insert method>));
        public static void XRef(string Name) { foreach (var instance in XrefScanner.UsedBy(typeof(AvatarPlayableController).GetMethod(Name))) if (instance.TryResolve() != null) MelonLogger.Msg(instance.TryResolve().FullDescription()); }

        public static void Prefix(MethodBase __originalMethod) { if (Patch) PatchMenu(); }

        public static void Postfix(MethodBase __originalMethod) { if (Patch) PatchMenu(); }

        private static void PatchMenu()
        {
            MelonLogger.Msg("Patching ActionMenu...");
            foreach (var i in ActionMenuDriver.field_Public_Static_ActionMenuDriver_0.field_Public_ActionMenuOpener_0.field_Public_ActionMenu_0.field_Public_List_1_ObjectNPublicPaSiAcObUnique_0) i.field_Public_AvatarPlayableController_0 = CurrentController;
            foreach (var i in ActionMenuDriver.field_Public_Static_ActionMenuDriver_0.field_Public_ActionMenuOpener_1.field_Public_ActionMenu_0.field_Public_List_1_ObjectNPublicPaSiAcObUnique_0) i.field_Public_AvatarPlayableController_0 = CurrentController;
        }

        private static void SetPlayableController()
        {
            VRCAvatarDescriptor avatarDescriptor = GameObject.Find("UserInterface/MenuContent/Screens/Avatar/AvatarPreviewBase/MainRoot/MainModel/").GetComponentInChildren<VRCAvatarDescriptor>();
            CurrentController = avatarDescriptor.gameObject.AddComponent(GameObject.Find($"{VRCPlayer.field_Internal_Static_VRCPlayer_0.gameObject.name}/AnimationController/PlayableController").GetComponent<AvatarPlayableController>());
            CurrentController.field_Private_VRCPlayer_0 = VRCPlayer.field_Internal_Static_VRCPlayer_0;
            CurrentController.Method_Public_Void_VRCAvatarDescriptor_String_Boolean_PDM_0(avatarDescriptor, null, true);

            ActionMenuDriver actionMenu = ActionMenuDriver.field_Public_Static_ActionMenuDriver_0;
            actionMenu.field_Public_VRC_AvatarDescriptor_0 = avatarDescriptor;
            actionMenu.gameObject.SetActive(true);
        }
    }

    // Got from https://answers.unity.com/questions/530178/how-to-get-a-component-from-an-object-and-add-it-t.html
    public static class Extention
    {
        public static T GetCopyOf<T>(this Component comp, T other) where T : Component
        {
            System.Type type = comp.GetType();
            if (type != other.GetType()) return null; // type mis-match
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }
            FieldInfo[] finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }
            return comp as T;
        }

        public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
        {
            return go.AddComponent<T>().GetCopyOf(toAdd) as T;
        }
    }
}