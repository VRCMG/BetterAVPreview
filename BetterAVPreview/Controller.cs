using System.Linq;
using System.Reflection;
using Harmony;
using UnityEngine;
using UnityEngine.UI;
using VRC;
using VRC.UI;
using VRCSDK2;
using VRC.SDK3.Avatars.Components;

namespace AVPreview
{
    public class Controller
    {
        private static ActionMenuDriver actionMenuDriver;
        private static AvatarPlayableController CurrentController;
        private static VRCPlayer Player() => VRCPlayer.field_Internal_Static_VRCPlayer_0;
        private static GameObject MainModel() => GameObject.Find("UserInterface/MenuContent/Screens/Avatar/AvatarPreviewBase/MainRoot/MainModel/");
        private static bool PreviewIs3() => MainModel().GetComponentInChildren<VRCAvatarDescriptor>() != null;
        private static bool Patch;

        public static void VRChat_OnUiManagerInit()
        {
            actionMenuDriver = ActionMenuDriver.field_Public_Static_ActionMenuDriver_0;
            Main.listener.OnDisabled += delegate { if (Patch) ResetState(); };
        }
        
        public static void HarmonyPatches()
        {
            Main.HarmonyInstance.Patch(typeof(ActionMenuDriver).GetMethod(nameof(ActionMenuDriver.Method_Public_AvatarPlayableController_0)), new HarmonyMethod(typeof(Controller).GetMethod(nameof(GetPlayableController))));
            // To patch method that gets av type Main.HarmonyInstance.Patch(typeof(<Class>).GetMethod(nameof(<Class>.<Method>)), new HarmonyMethod(typeof(Controller).GetMethod(nameof(GetIs3))));
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

        public static bool GetIs3(ref bool __result)
        {
            if (Patch)
            {
                __result = PreviewIs3();
                return false;
            }
            else return true;
        }

        public static void OnButtonToggle(bool isOn)
        {
            if (isOn)
            {
                SetState();
                actionMenuDriver.gameObject.SetActive(true);
            }
            else
            {
                if (MainModel().GetComponentInChildren<DynamicBoneController>() != null) MainModel().GetComponentInChildren<DynamicBoneController>().enabled = false;
                if (CurrentController != null) CurrentController.enabled = false;
                if (Patch) ResetState();
                actionMenuDriver.gameObject.SetActive(false);
            }
        }

        public static void SetState()
        {
            if (PreviewIs3())
            {
                VRCAvatarDescriptor CurrentDescriptor = MainModel().GetComponentInChildren<VRCAvatarDescriptor>();
                if (CurrentController == null)
                {
                    CurrentController = CurrentDescriptor.gameObject.AddComponent<AvatarPlayableController>();
                    CurrentController.field_Private_Boolean_0 = true;
                    CurrentController.field_Private_Boolean_1 = true;
                    CurrentController.field_Private_Boolean_2 = true;
                    CurrentController.field_Private_Boolean_3 = false;
                    CurrentController.field_Private_VRCPlayer_0 = Player();
                }
                if (CurrentController != null && Main.toggleClone.GetComponent<Toggle>().isOn)
                {
                    CurrentController.enabled = true;
                    SetPlayableController(CurrentDescriptor, CurrentController);
                    actionMenuDriver.field_Public_VRC_AvatarDescriptor_0 = CurrentDescriptor;
                    actionMenuDriver.field_Public_ActionMenuOpener_0.field_Public_ActionMenu_0.Method_Public_Void_2();
                    RepositionActionMenu(false);
                    Patch = true;
                }
            }
            else
            {
                VRC_AvatarDescriptor CurrentDescriptor = MainModel().GetComponentInChildren<VRC_AvatarDescriptor>();
                if (Main.toggleClone.GetComponent<Toggle>().isOn)
                {
                    SetAVManager(CurrentDescriptor, null, null);
                    actionMenuDriver.field_Public_VRC_AvatarDescriptor_0 = CurrentDescriptor;
                    actionMenuDriver.field_Public_ActionMenuOpener_0.field_Public_ActionMenu_0.Method_Public_Void_2();
                    RepositionActionMenu(false);
                    Patch = true;
                }
            }
            if (MainModel().GetComponentInChildren<DynamicBoneController>() == null) CurrentController.gameObject.AddComponent<DynamicBoneController>().Method_Public_Void_IEnumerable_1_DynamicBone_EnumNPublicSealedva3vUnique_PDM_0(MainModel().GetComponentsInChildren<DynamicBone>().Cast<Il2CppSystem.Collections.Generic.IEnumerable<DynamicBone>>(), DynamicBoneController.EnumNPublicSealedva3vUnique.EnumValue0);
            else MainModel().GetComponentInChildren<DynamicBoneController>().enabled = true;
        }
        
        private static void ResetState()
        {
            if (Player().GetComponentInChildren<VRCAvatarDescriptor>() != null)
            {
                VRCAvatarDescriptor avatarDescriptor = Player().GetComponentInChildren<VRCAvatarDescriptor>();
                SetAVManager(null, avatarDescriptor, Player().GetComponentInChildren<AvatarPlayableController>());
                actionMenuDriver.field_Public_VRC_AvatarDescriptor_0 = avatarDescriptor;
            }
            else
            {
                VRC_AvatarDescriptor avatarDescriptor = Player().GetComponentInChildren<VRC_AvatarDescriptor>();
                SetAVManager(avatarDescriptor, null, null);
                actionMenuDriver.field_Public_VRC_AvatarDescriptor_0 = avatarDescriptor;
            }
            actionMenuDriver.field_Public_ActionMenuOpener_0.field_Public_ActionMenu_0.Method_Public_Void_2();
            RepositionActionMenu(true);
            Patch = false;
        }

        private static void SetPlayableController(VRCAvatarDescriptor avatarDescriptor, AvatarPlayableController controller)
        {
            SetAVManager(null, avatarDescriptor, controller);
            controller.Method_Public_Void_VRCAvatarDescriptor_String_Boolean_PDM_0(avatarDescriptor, null, true);
        }

        private static void SetAVManager(VRC_AvatarDescriptor avatarDescriptor0, VRCAvatarDescriptor avatarDescriptor, AvatarPlayableController controller)
        {
            VRCAvatarManager AVManager = Player().prop_VRCAvatarManager_0;
            AVManager.prop_VRC_AvatarDescriptor_0 = avatarDescriptor;
            if (avatarDescriptor != null)
            {
                AVManager.prop_VRCAvatarDescriptor_0 = avatarDescriptor;
                AVManager.prop_VRC_AvatarDescriptor_1 = null;
                AVManager.field_Private_AvatarPlayableController_0 = controller;
            }
            else
            {
                AVManager.prop_VRCAvatarDescriptor_0 = null;
                AVManager.prop_VRC_AvatarDescriptor_1 = avatarDescriptor0;
            }
            AVManager.field_Private_Animator_0 = avatarDescriptor.gameObject.GetComponent<Animator>();
        }

        private static void RepositionActionMenu(bool IsReset) // To do
        {
            if (IsReset)
            {}
            else
            {}
        }
    }
}