using ABI.CCK.Components;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace PickupPushPull;

public class PickupPushPull : MelonMod
{
    private static MelonPreferences_Category melonCategoryPickupPushPull;
    private static MelonPreferences_Entry<float> melonEntryPushPullSpeed;
    private static MelonPreferences_Entry<float> melonEntryRotateSpeed;
    private static MelonPreferences_Entry<bool> melonEntryEnableRotation;

    public override void OnApplicationStart()
    {
        melonCategoryPickupPushPull = MelonPreferences.CreateCategory(nameof(PickupPushPull));
        melonEntryPushPullSpeed = melonCategoryPickupPushPull.CreateEntry("PushPullSpeed", 1f, description: "Up/down on right joystick for VR. Left bumper + Up/down on right joystick for Gamepad.");
        melonEntryRotateSpeed = melonCategoryPickupPushPull.CreateEntry<float>("RotateSpeed", 1f);
        melonEntryEnableRotation = melonCategoryPickupPushPull.CreateEntry<bool>("EnableRotation", false, description: "Hold left trigger in VR or right bumper on Gamepad.");

        melonCategoryPickupPushPull.SaveToFile(false);
        melonEntryPushPullSpeed.OnValueChangedUntyped += UpdateSettings;
        melonEntryRotateSpeed.OnValueChangedUntyped += UpdateSettings;
        melonEntryEnableRotation.OnValueChangedUntyped += UpdateSettings;

        UpdateSettings();
    }
    private static void UpdateSettings()
    {
        HarmonyPatches.ppSpeed = melonEntryPushPullSpeed.Value;
        HarmonyPatches.rotSpeed = melonEntryRotateSpeed.Value;
        HarmonyPatches.enableRot = melonEntryEnableRotation.Value;
    }

    [HarmonyPatch]
    private class HarmonyPatches
    {

        public static float ppSpeed = melonEntryPushPullSpeed.Value;
        public static float rotSpeed = melonEntryRotateSpeed.Value;
        public static bool enableRot = melonEntryEnableRotation.Value;

        public static List<CVRPickupObject> grabbedObjects = new List<CVRPickupObject>();

        private static bool lockedVRInput = false;
        private static bool lockedGamepadInput = false;
        private static CursorLockMode savedCursorLockState;

        //Grab Patches
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CVRPickupObject), "Grab")]
        private static void AfterGrab(ref CVRPickupObject __instance)
        {
            if (__instance.IsGrabbedByMe())
            {
                grabbedObjects.Add(__instance);
            }
        }

        //Gamepad Input Patch
        [HarmonyPostfix]
        [HarmonyPatch(typeof(InputModuleGamepad), "UpdateInput")]
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
                    foreach (CVRPickupObject cvrpickupObject in grabbedObjects)
                    {
                        if (!cvrpickupObject.IsGrabbedByMe())
                        {
                            grabbedObjects.Remove(cvrpickupObject);
                            break;
                        }
                        cvrpickupObject.initialRotationalOffset *= Quaternion.AngleAxis(rotSpeed * CVRInputManager.Instance.rawLookVector.y, Vector3.right) * Quaternion.AngleAxis(rotSpeed * CVRInputManager.Instance.rawLookVector.x, Vector3.up);
                    }
                    return;
                }

                CVRInputManager.Instance.objectPushPull += CVRInputManager.Instance.rawLookVector.y * ppSpeed;
            }
            else
            {
                if (lockedGamepadInput)
                {
                    lockedGamepadInput = false;
                    Cursor.lockState = savedCursorLockState;
                    PlayerSetup.Instance._movementSystem.disableCameraControl = false;
                }
            }
        }

        //VR Input Patch
        [HarmonyPostfix]
        [HarmonyPatch(typeof(InputModuleSteamVR), "UpdateInput")]
        private static void AfterUpdateInput(ref InputModuleSteamVR __instance)
        {
            bool button = CVRInputManager.Instance.interactLeftValue > 0.9;
            if (button && enableRot)
            {
                if (!lockedVRInput)
                {
                    lockedVRInput = true;
                    PlayerSetup.Instance._movementSystem.canRot = false;
                    PlayerSetup.Instance._movementSystem.disableCameraControl = true;
                }
                foreach (CVRPickupObject cvrpickupObject in grabbedObjects)
                {
                    if (!cvrpickupObject.IsGrabbedByMe())
                    {
                        grabbedObjects.Remove(cvrpickupObject);
                        break;
                    }
                    cvrpickupObject.initialRotationalOffset *= Quaternion.AngleAxis(rotSpeed * (CVRInputManager.Instance.floatDirection / 2f), Vector3.right) * Quaternion.AngleAxis(rotSpeed * CVRInputManager.Instance.rawLookVector.x, Vector3.up);
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
        }
    }
}