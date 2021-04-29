using Harmony;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using MelonLoader;
using VRC.UI;

namespace AVPreview
{
    public class Rotator
    {
        private static Transform Avatar;
        private static Transform EmmButton;
        private static Transform ChangeButton;
        private static Transform FavButton;
        private static Transform AvPreview;
        private static Transform AvRoot;
        private static Transform AvModel;
        private static Quaternion InitRot;
        private static GameObject sliderCloneX;
        private static GameObject sliderCloneY;
        private static GameObject sliderCloneTextX;
        private static GameObject sliderCloneTextY;
        private static Vector3 OriScale;
        private static Vector3 proportion;
        private static bool UsingEmm() => MelonHandler.Mods.Any(m => m.Info.Name.Contains("emmVRC"));
        private static bool runOnce = false;

        public static void HarmonyPatches()
        {
            foreach (MethodInfo method in typeof(PageAvatar).GetMethods().Where(m => m.Name.Contains("Method_Private_Void_String_GameObject_AvatarPerformanceStats"))) Main.HarmonyInstance.Patch(method, new HarmonyMethod(typeof(Controller).GetMethod(nameof(Reposition))));
        }

        public static void Reposition() {  }

        public static void VRChat_OnUiManagerInit()
        {
            GameObject Menu = GameObject.Find("UserInterface/MenuContent/Screens");
            Transform sliderObj = Menu.transform.Find("Settings/MousePanel/SensitivitySlider");
            Avatar = Menu.transform.Find("Avatar");

            FavButton = Avatar.Find("Favorite Button");
            ChangeButton = Avatar.Find("Change Button");
            AvPreview = Avatar.Find("AvatarPreviewBase");
            AvRoot = AvPreview.Find("MainRoot");
            AvModel = AvRoot.Find("MainModel");
            
            sliderCloneX = Object.Instantiate(sliderObj.gameObject, Avatar.transform);
            sliderCloneTextX = Object.Instantiate(sliderObj.parent.Find("MouseSensitivityText").gameObject, sliderCloneX.transform);
            sliderCloneTextX.transform.position = sliderCloneX.transform.position;
            sliderCloneY = Object.Instantiate(sliderObj.gameObject, Avatar.transform);
            sliderCloneTextY = Object.Instantiate(sliderObj.parent.Find("MouseSensitivityText").gameObject, sliderCloneY.transform);
            sliderCloneTextY.transform.position = sliderCloneY.transform.position;
            
            sliderCloneX.GetComponent<Slider>().onValueChanged = new Slider.SliderEvent();
            sliderCloneY.GetComponent<Slider>().onValueChanged = new Slider.SliderEvent();
            sliderCloneX.GetComponent<Slider>().onValueChanged.AddListener((UnityEngine.Events.UnityAction<float>)((_) => { OnSlide(); }));
            sliderCloneY.GetComponent<Slider>().onValueChanged.AddListener((UnityEngine.Events.UnityAction<float>)((_) => { OnSlide(); }));
            sliderCloneX.GetComponent<Slider>().value = 0.5f;
            sliderCloneY.GetComponent<Slider>().value = 0.5f;
            
            sliderCloneTextX.GetComponent<Text>().text = "X:";
            sliderCloneTextY.GetComponent<Text>().text = "Y:";
            sliderCloneX.name = "RotationX";
            sliderCloneY.name = "RotationY";
            
            sliderCloneX.SetActive(false);
            sliderCloneY.SetActive(false);

            sliderCloneX.transform.position = ChangeButton.position;
            sliderCloneY.transform.position = ChangeButton.position;
        }

        public static void OnButtonToggle(bool isOn)
        {
            if (isOn)
            {
                proportion = (FavButton.position - ChangeButton.position) * 4 / 13;
                if (!runOnce)
                {
                    if (UsingEmm()) EmmButton = (from Button in Object.FindObjectsOfType<Button>() where Button.name == "Favorite Button(Clone)" && Button.gameObject.active == true select Button).First().transform;
                    sliderCloneTextX.transform.position -= Quaternion.Euler(0, 0, -90) * proportion * 5 + proportion / 2;
                    sliderCloneTextY.transform.position -= Quaternion.Euler(0, 0, -90) * proportion * 5 + proportion / 2;
                    sliderCloneX.transform.position -= proportion * 4;
                    sliderCloneY.transform.position -= proportion * 5 / 2;
                    runOnce = true;
                }
                if (UsingEmm())
                {
                    EmmButton.position += proportion * 3;
                    ChangeButton.position += proportion * 2;
                    FavButton.position += proportion;
                }
                else
                {
                    OriScale = AvPreview.localScale;
                    AvPreview.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                    AvPreview.position -= proportion * 2;
                    AvPreview.Find("FallbackRoot").position -= proportion;
                    AvPreview.Find("EditPlayerDirLight").position -= proportion;
                    Avatar.Find("Select Button").position -= proportion * 5 / 2;
                }
                AvModel.GetComponents<MonoBehaviour>().First(c => c.GetIl2CppType().FullName == "UnityStandardAssets.Utility.AutoMoveAndRotate").enabled = false;
                AvRoot.position -= proportion * 10;
                AvModel.position += proportion * 10;
                InitRot = AvRoot.rotation;
                AvRoot.rotation = new Quaternion(0, 0, 0, 0);
                sliderCloneX.SetActive(true);
                sliderCloneY.SetActive(true);
            }
            else
            {
                if (UsingEmm())
                {
                    EmmButton.position -= proportion * 3;
                    ChangeButton.position -= proportion * 2;
                    FavButton.position -= proportion;
                }
                else
                {
                    Avatar.Find("Select Button").position += proportion * 5 / 2;
                    AvPreview.Find("EditPlayerDirLight").position += proportion;
                    AvPreview.Find("FallbackRoot").position += proportion;
                    AvPreview.position += proportion * 2;
                    AvPreview.localScale = OriScale;
                }
                sliderCloneX.GetComponent<Slider>().value = 0.5f;
                sliderCloneY.GetComponent<Slider>().value = 0.5f;
                AvRoot.rotation = InitRot;
                AvModel.position -= proportion * 10;
                AvRoot.position += proportion * 10;
                AvModel.GetComponents<MonoBehaviour>().First(c => c.GetIl2CppType().FullName == "UnityStandardAssets.Utility.AutoMoveAndRotate").enabled = true;
                sliderCloneX.SetActive(false);
                sliderCloneY.SetActive(false);
            }
        }

        private static void OnSlide()
        {
            AvRoot.rotation = new Quaternion(0, 0, 0, 0);
            AvModel.rotation = Quaternion.Euler(0, 360 * sliderCloneY.GetComponent<Slider>().value - 180, 0);
            AvRoot.RotateAround(AvRoot.position, Vector3.right, 360 * sliderCloneX.GetComponent<Slider>().value - 180);
        }
    }
}