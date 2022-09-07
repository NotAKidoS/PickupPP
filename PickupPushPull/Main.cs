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

    public override void OnApplicationStart()
    {
        melonCategoryPickupPushPull = MelonPreferences.CreateCategory(nameof(PickupPushPull));
        melonEntryPushPullSpeed = melonCategoryPickupPushPull.CreateEntry("PushPullSpeed", 1f, description: "Up/down on right joystick for VR. Left bumper + Up/down on right joystick for Gamepad.");

        melonCategoryPickupPushPull.SaveToFile(false);
        melonEntryPushPullSpeed.OnValueChangedUntyped += UpdateSettings;

        UpdateSettings();
    }

    private static void UpdateSettings()
    {
        HarmonyPatches.ppSpeed = melonEntryPushPullSpeed.Value;
    }

    [HarmonyPatch]
    private class HarmonyPatches
    {

        public static float ppSpeed = melonEntryPushPullSpeed.Value;

        private static bool lockedGamepadInput = false;
        private static CursorLockMode savedCursorLockState;

        //Gamepad Input Patch
        [HarmonyPostfix]
        [HarmonyPatch(typeof(InputModuleGamepad), "UpdateInput")]
        private static void AfterUpdateInput(ref InputModuleGamepad __instance)
        {
            bool button1 = Input.GetButton("Controller Left Button");
            if (button1)
            {
                if (!lockedGamepadInput)
                {
                    lockedGamepadInput = true;
                    savedCursorLockState = Cursor.lockState;
                    Cursor.lockState = CursorLockMode.None;
                    PlayerSetup.Instance._movementSystem.disableCameraControl = true;
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
            CVRInputManager.Instance.objectPushPull += CVRInputManager.Instance.scrollValue * ppSpeed;
        }
    }
}