using System.Linq;
using System.Reflection;
using UnhollowerRuntimeLib;
using Harmony;
using MelonLoader;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.UI;
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
        private static ActionMenuDriver actionMenu;
        private static AvatarPlayableController CurrentController;
        private static bool Patch;

        public override void OnApplicationStart()
        {
            Instance = this;
            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();
            HarmonyPatches();
        }

        public override void VRChat_OnUiManagerInit()
        {
            actionMenu = ActionMenuDriver.field_Public_Static_ActionMenuDriver_0;
            EnableDisableListener listener = GameObject.Find("UserInterface/MenuContent/Screens/Avatar").AddComponent<EnableDisableListener>();
            listener.OnEnabled += delegate { actionMenu.gameObject.SetActive(true); };
            listener.OnDisabled += delegate { if (Patch) OnMenuClose(); };
        }
        
        private static void HarmonyPatches()
        {
            Instance.Harmony.Patch(typeof(ActionMenuDriver).GetMethod(nameof(ActionMenuDriver.Method_Public_AvatarPlayableController_0)), new HarmonyMethod(typeof(Main).GetMethod(nameof(GetPlayableController))));
            foreach (MethodInfo method in typeof(PageAvatar).GetMethods().Where(m => m.Name.Contains("Method_Private_Void_String_GameObject_AvatarPerformanceStats"))) Instance.Harmony.Patch(method, null, new HarmonyMethod(typeof(Main).GetMethod(nameof(OnPedestalAvatarInstantiated))));
        }

        public static bool GetPlayableController(ref AvatarPlayableController __result)
        {
            if (Patch)
            {
                __result = CurrentController;
                return false;
            }
            else return true;
        }

        public static void OnPedestalAvatarInstantiated()
        {
            VRCAvatarDescriptor CurrentDescriptor = GameObject.Find("UserInterface/MenuContent/Screens/Avatar/AvatarPreviewBase/MainRoot/MainModel/").GetComponentInChildren<VRCAvatarDescriptor>();
            if (CurrentDescriptor != null)
            {
                SetPlayableController(CurrentDescriptor);
                actionMenu.field_Public_VRC_AvatarDescriptor_0 = CurrentDescriptor;
                // reposition ActionMenu
                Patch = true;
            }
        }

        private static void OnMenuClose()
        {
            VRCAvatarDescriptor CurrentDescriptor = VRCPlayer.field_Internal_Static_VRCPlayer_0.GetComponentInChildren<VRCAvatarDescriptor>();
            SetPlayableController(CurrentDescriptor);
            actionMenu.field_Public_VRC_AvatarDescriptor_0 = CurrentDescriptor;
            // reposition ActionMenu
            Patch = false;
        }

        private static void SetPlayableController(VRCAvatarDescriptor avatarDescriptor)
        {
            CurrentController = VRCPlayer.field_Internal_Static_VRCPlayer_0.GetComponentInChildren<AvatarPlayableController>();
            CurrentController.Method_Public_Void_VRCAvatarDescriptor_String_Boolean_PDM_0(avatarDescriptor, null, true);
        }
    }
}