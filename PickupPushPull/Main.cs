using ABI.CCK.Components;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.Events;

[assembly: MelonInfo(typeof(PickupPushPull.PickupPushPull), nameof(PickupPushPull), "1.1.0", "NotAKidoS", "https://github.com/NotAKidOnSteam/PickupPP")]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]

namespace PickupPushPull;

internal class PickupPushPull : MelonMod
{
    private static MelonPreferences_Category melonCategoryPickupPushPull;
    private static MelonPreferences_Entry<float> melonEntryPushPullSpeed;
    private static MelonPreferences_Entry<float> melonEntryRotateSpeed;
    private static MelonPreferences_Entry<bool> melonEntryEnableRotation;
<<<<<<< Updated upstream

    private static PickupPP m_pickupPP = null;
=======
>>>>>>> Stashed changes

    public override void OnApplicationStart()
    {
        melonCategoryPickupPushPull = MelonPreferences.CreateCategory(nameof(PickupPushPull));
<<<<<<< Updated upstream
        melonEntryPushPullSpeed = melonCategoryPickupPushPull.CreateEntry("PushPullSpeed", 2f, description: "Up/down on VR joystick or left bumper on Gamepad.");
        melonEntryRotateSpeed = melonCategoryPickupPushPull.CreateEntry("RotateSpeed", 15f);
=======
        melonEntryPushPullSpeed = melonCategoryPickupPushPull.CreateEntry("PushPullSpeed", 1f, description: "Up/down on VR joystick or left bumper on Gamepad.");
        melonEntryRotateSpeed = melonCategoryPickupPushPull.CreateEntry("RotateSpeed", 1f);
>>>>>>> Stashed changes
        melonEntryEnableRotation = melonCategoryPickupPushPull.CreateEntry("EnableRotation", false, description: "Hold left trigger in VR or right bumper on Gamepad.");

        melonCategoryPickupPushPull.SaveToFile(false);
        melonEntryPushPullSpeed.OnValueChangedUntyped += UpdateSettings;
        melonEntryRotateSpeed.OnValueChangedUntyped += UpdateSettings;
        melonEntryEnableRotation.OnValueChangedUntyped += UpdateSettings;
<<<<<<< Updated upstream
        MelonLoader.MelonCoroutines.Start(WaitForLocalPlayer());
    }

    System.Collections.IEnumerator WaitForLocalPlayer()
    {
        while (PlayerSetup.Instance == null)
            yield return null;
        m_pickupPP = PlayerSetup.Instance.gameObject.AddComponent<PickupPP>();
=======

        UpdateSettings();
>>>>>>> Stashed changes
    }

    private static void UpdateSettings()
    {
<<<<<<< Updated upstream
        m_pickupPP.ppSpeed = melonEntryPushPullSpeed.Value;
        m_pickupPP.rotSpeed = melonEntryRotateSpeed.Value;
        m_pickupPP.enableRotation = melonEntryEnableRotation.Value;
=======
        HarmonyPatches.ppSpeed = melonEntryPushPullSpeed.Value;
        HarmonyPatches.rotSpeed = melonEntryRotateSpeed.Value;
        HarmonyPatches.enableRot = melonEntryEnableRotation.Value;
>>>>>>> Stashed changes
    }

    [HarmonyPatch]
    private class HarmonyPatches
    {
<<<<<<< Updated upstream
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CVRPickupObject), nameof(CVRPickupObject.Grab))]
        private static void AfterGrab(ref CVRPickupObject __instance, ref Transform handTransform, ref ControllerRay controllerRay, ref Vector3 hitPoint)
        {
            if (__instance.IsGrabbedByMe())
            {
                m_pickupPP.grabbedObjects.Add(__instance);
            }
        }
    }
}

class PickupPP : MonoBehaviour
{

    public float ppSpeed = 2f;
    public float rotSpeed = 15f;
    public List<CVRPickupObject> grabbedObjects = new List<CVRPickupObject>();
    public bool enableRotation = false;

    private bool telepathicGrab = false;
    private bool gamepadInput = false;
    private CursorLockMode savedCursorLockState;
    private bool buttonPressed = false;

    void Start()
    {
        this.telepathicGrab = MetaPort.Instance.settings.GetSettingsBool("GeneralTelepathicGrip");
        MetaPort.Instance.settings.settingBoolChanged.AddListener(new UnityAction<string, bool>(this.SettingsBoolChanged));

        this.gamepadInput = MetaPort.Instance.settings.GetSettingsBool("ControlEnableGamepad");
        MetaPort.Instance.settings.settingBoolChanged.AddListener(new UnityAction<string, bool>(this.SettingsBoolChanged));
    }

    void Update()
    {
        if (grabbedObjects.Count() > 0)
        {
            foreach (CVRPickupObject pickup in grabbedObjects)
            {
                if (!pickup.IsGrabbedByMe())
                {
                    grabbedObjects.Remove(pickup);
                    return;
                }
            }

        }
        else
        {
            return;
        }

        if (telepathicGrab)
        {
            //Use gamepad input if enabled, otherwise default to VR
            if (gamepadInput)
            {
                HandleGamepad();
            }
            else
            {
                HandleVR();
            }
        }
    }

    private void HandleGamepad()
    {
        bool button = Input.GetButton("Controller Left Button");
        bool button2 = Input.GetButton("Controller Right Button");
        if (button)
        {
            if (!buttonPressed)
            {
                buttonPressed = true;
                savedCursorLockState = Cursor.lockState;
                Cursor.lockState = CursorLockMode.None;
                PlayerSetup.Instance._movementSystem.disableCameraControl = true;
            }

            if (button2 && enableRotation)
            {
                foreach (CVRPickupObject pickup in grabbedObjects)
                {
                    pickup.initialRotationalOffset *= Quaternion.AngleAxis(rotSpeed * CVRInputManager.Instance.rawLookVector.y, Vector3.right) * Quaternion.AngleAxis(rotSpeed * CVRInputManager.Instance.rawLookVector.x, Vector3.up);
=======
        public static float ppSpeed = melonEntryPushPullSpeed.Value;
        public static float rotSpeed = melonEntryRotateSpeed.Value;
        public static bool enableRot = melonEntryEnableRotation.Value;

        //need to keep in mind we can grab multiple objects, lazy way of implementing ik
        public static List<CVRPickupObject> grabbedObjects = new();

        //Grab Patch
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CVRPickupObject), nameof(CVRPickupObject.Grab))]
        private static void AfterGrab(ref CVRPickupObject __instance, ref Transform handTransform, ref ControllerRay controllerRay, ref Vector3 hitPoint)
        {
            if (__instance.IsGrabbedByMe())
            {
                grabbedObjects.Add(__instance);
            }
        }

        //Gamepad Input Patch
        private static bool lockedGamepadInput = false;

        private static CursorLockMode savedCursorLockState;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(InputModuleGamepad), nameof(InputModuleGamepad.UpdateInput))]
        private static void AfterUpdateInput(ref InputModuleGamepad __instance)
        {
            bool button1 = Input.GetButton("Controller Left Button");
            bool button2 = Input.GetButton("Controller Right Button");

            if (button1)
            {
                if (!lockedGamepadInput)
                {
                    lockedGamepadInput = true;
                    savedCursorLockState = Cursor.lockState;
                    Cursor.lockState = CursorLockMode.None;
                    PlayerSetup.Instance._movementSystem.disableCameraControl = true;
                }

                if (button2 && enableRot)
                {
                    foreach (CVRPickupObject pickup in grabbedObjects)
                    {
                        if (pickup.IsGrabbedByMe())
                        {
                            pickup.initialRotationalOffset *= Quaternion.AngleAxis(rotSpeed * CVRInputManager.Instance.rawLookVector.y, Vector3.right) * Quaternion.AngleAxis(rotSpeed * CVRInputManager.Instance.rawLookVector.x, Vector3.up);
                        }
                        else
                        {
                            grabbedObjects.Remove(pickup);
                            return;
                        }
                    }
                }
                else
                {
                    CVRInputManager.Instance.objectPushPull += CVRInputManager.Instance.rawLookVector.y * ppSpeed;
                }
            }
            else
            {
                if (lockedGamepadInput)
                {
                    lockedGamepadInput = false;
                    Cursor.lockState = savedCursorLockState;
                    PlayerSetup.Instance._movementSystem.disableCameraControl = false;
>>>>>>> Stashed changes
                }
            }
            else
            {
                CVRInputManager.Instance.objectPushPull += CVRInputManager.Instance.rawLookVector.y * ppSpeed;
            }
        }
        else
        {
            if (buttonPressed)
            {
                buttonPressed = false;
                Cursor.lockState = savedCursorLockState;
                PlayerSetup.Instance._movementSystem.disableCameraControl = false;
            }
        }
    }
    private void HandleVR()
    {
        bool button = (CVRInputManager.Instance.interactLeftValue > 0.9);
        if (button && enableRotation)
        {
            if (!buttonPressed)
            {
                buttonPressed = true;
                PlayerSetup.Instance._movementSystem.canRot = false;
                PlayerSetup.Instance._movementSystem.disableCameraControl = true;
            }

<<<<<<< Updated upstream
            foreach (CVRPickupObject pickup in grabbedObjects)
            {
                pickup.initialRotationalOffset *= Quaternion.AngleAxis(rotSpeed * (CVRInputManager.Instance.floatDirection / 2), Vector3.right) * Quaternion.AngleAxis(rotSpeed * CVRInputManager.Instance.rawLookVector.x, Vector3.up);
            }
        }
        else
        {
            if (buttonPressed)
            {
                buttonPressed = false;
                PlayerSetup.Instance._movementSystem.canRot = true;
                PlayerSetup.Instance._movementSystem.disableCameraControl = false;
            }
            CVRInputManager.Instance.objectPushPull += CVRInputManager.Instance.scrollValue * ppSpeed;
=======
        //VR Input Patch
        private static bool lockedVRInput = false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(InputModuleSteamVR), nameof(InputModuleSteamVR.UpdateInput))]
        private static void AfterUpdateInput(ref InputModuleSteamVR __instance)
        {
            //might need to figure out how to read SteamVR input for A button...
            bool button = CVRInputManager.Instance.interactLeftValue > 0.9;

            if (button && enableRot)
            {
                if (!lockedVRInput)
                {
                    lockedVRInput = true;
                    PlayerSetup.Instance._movementSystem.canRot = false;
                    PlayerSetup.Instance._movementSystem.disableCameraControl = true;
                }
                foreach (CVRPickupObject pickup in grabbedObjects)
                {
                    if (pickup.IsGrabbedByMe())
                    {
                        pickup.initialRotationalOffset *= Quaternion.AngleAxis(rotSpeed * (CVRInputManager.Instance.floatDirection / 2), Vector3.right) * Quaternion.AngleAxis(rotSpeed * CVRInputManager.Instance.rawLookVector.x, Vector3.up);
                    }
                    else
                    {
                        grabbedObjects.Remove(pickup);
                        return;
                    }
                }
            }
            else
            {
                if (lockedVRInput)
                {
                    lockedVRInput = false;
                    PlayerSetup.Instance._movementSystem.canRot = true;
                    PlayerSetup.Instance._movementSystem.disableCameraControl = false;
                }
                CVRInputManager.Instance.objectPushPull += CVRInputManager.Instance.scrollValue * ppSpeed;
            }
>>>>>>> Stashed changes
        }
    }

    private void SettingsBoolChanged(string name, bool value)
    {
        if (name == "GeneralTelepathicGrip")
        {
            this.telepathicGrab = value;
        }
        if (name == "ControlEnableGamepad")
        {
            this.gamepadInput = value;
        }
    }
}