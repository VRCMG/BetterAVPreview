using System.Linq;
using System.Reflection;
using Harmony;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Avatars.Components;
using VRC.UI;


namespace AVPreview
{
    public class Controller
    {
        private static ActionMenuDriver actionMenu;
        private static AvatarPlayableController CurrentController;
        private static bool Patch;

        public static void VRChat_OnUiManagerInit()
        {
            actionMenu = ActionMenuDriver.field_Public_Static_ActionMenuDriver_0;
            Main.listener.OnDisabled += delegate { if (Patch) ResetState(); };
        }
        
        public static void HarmonyPatches()
        {
            Main.HarmonyInstance.Patch(typeof(ActionMenuDriver).GetMethod(nameof(ActionMenuDriver.Method_Public_AvatarPlayableController_0)), new HarmonyMethod(typeof(Controller).GetMethod(nameof(GetPlayableController))));
            foreach (MethodInfo method in typeof(PageAvatar).GetMethods().Where(m => m.Name.Contains("Method_Private_Void_String_GameObject_AvatarPerformanceStats"))) Main.HarmonyInstance.Patch(method, null, new HarmonyMethod(typeof(Controller).GetMethod(nameof(SetState))));
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

        public static void SetState()
        {
            VRCAvatarDescriptor CurrentDescriptor = GameObject.Find("UserInterface/MenuContent/Screens/Avatar/AvatarPreviewBase/MainRoot/MainModel/").GetComponentInChildren<VRCAvatarDescriptor>();
            if (CurrentDescriptor != null && Main.toggleClone.GetComponent<Toggle>().isOn)
            {
                SetPlayableController(CurrentDescriptor);
                actionMenu.field_Public_VRC_AvatarDescriptor_0 = CurrentDescriptor;
                // reposition ActionMenu
                Patch = true;
            }
        }

        public static void OnButtonToggle(bool isOn)
        {
            if (isOn)
            {
                if (CurrentController != null) CurrentController.enabled = true;
                SetState();
                actionMenu.gameObject.SetActive(true);
            }
            else
            {
                if (CurrentController != null) CurrentController.enabled = false;
                if (Patch) ResetState();
                actionMenu.gameObject.SetActive(false);
            }
        }
        
        private static void ResetState()
        {
            VRCAvatarDescriptor CurrentDescriptor = VRCPlayer.field_Internal_Static_VRCPlayer_0.GetComponentInChildren<VRCAvatarDescriptor>();
            SetPlayableController(CurrentDescriptor);
            actionMenu.field_Public_VRC_AvatarDescriptor_0 = CurrentDescriptor;
            // ResetAVState(); Todo
            // reposition ActionMenu
            Patch = false;
        }

        // private static void ResetAVState() { } ToDo

        private static void SetPlayableController(VRCAvatarDescriptor avatarDescriptor)
        {
            CurrentController = VRCPlayer.field_Internal_Static_VRCPlayer_0.GetComponentInChildren<AvatarPlayableController>();
            // CurrentController.field_Private_Animator_0 = null;
            CurrentController.Method_Public_Void_VRCAvatarDescriptor_String_Boolean_PDM_0(avatarDescriptor, null, true);
        }
    }
}