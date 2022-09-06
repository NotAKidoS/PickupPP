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
        melonEntryPushPullSpeed = melonCategoryPickupPushPull.CreateEntry("PushPullSpeed", 2f, description: "Up/down on VR joystick or left bumper on Gamepad.");

        melonCategoryPickupPushPull.SaveToFile(false);
        melonEntryPushPullSpeed.OnValueChangedUntyped += UpdateSettings;
    }

    private static void UpdateSettings()
    {
        HarmonyPatches.ppSpeed = melonEntryPushPullSpeed.Value;
    }

    [HarmonyPatch]
    private class HarmonyPatches
    {

        public static float ppSpeed = 2f;

        //Gamepad Input Patch
        private static CursorLockMode savedCursorLockState;
        private static bool gamepadButton1 = false;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(InputModuleGamepad), nameof(InputModuleGamepad.UpdateInput))]
        private static void AfterUpdateInput(ref InputModuleGamepad __instance)
        {
            bool button1 = Input.GetButton("Controller Left Button");
            if (button1)
            {
                if (!gamepadButton1)
                {
                    gamepadButton1 = true;
                    savedCursorLockState = Cursor.lockState;
                    Cursor.lockState = CursorLockMode.None;
                    PlayerSetup.Instance._movementSystem.disableCameraControl = true;
                }
                CVRInputManager.Instance.objectPushPull += CVRInputManager.Instance.rawLookVector.y * ppSpeed;
            }
            else
            {
                if (gamepadButton1)
                {
                    gamepadButton1 = false;
                    Cursor.lockState = savedCursorLockState;
                    PlayerSetup.Instance._movementSystem.disableCameraControl = false;
                }
            }
        }

        //VR Input Patch
        [HarmonyPostfix]
        [HarmonyPatch(typeof(InputModuleSteamVR), nameof(InputModuleSteamVR.UpdateInput))]
        private static void AfterUpdateInput(ref InputModuleSteamVR __instance)
        {
            CVRInputManager.Instance.objectPushPull += CVRInputManager.Instance.scrollValue * ppSpeed;
        }
    }
}