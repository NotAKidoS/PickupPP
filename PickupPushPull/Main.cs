using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using MelonLoader;
using UnityEngine;
using UnityEngine.Events;

namespace PickupPushPull;

public class PickupPushPull : MelonMod
{
    private static MelonPreferences_Category melonCategoryPickupPushPull;
    private static MelonPreferences_Entry<float> melonEntryPushPullSpeed;

    private static PickupPP m_pickupPP = null;

    public override void OnApplicationStart()
    {
        melonCategoryPickupPushPull = MelonPreferences.CreateCategory(nameof(PickupPushPull));
        melonEntryPushPullSpeed = melonCategoryPickupPushPull.CreateEntry("PushPullSpeed", 2f, description: "Up/down on VR joystick or left bumper on Gamepad.");

        melonCategoryPickupPushPull.SaveToFile(false);
        melonEntryPushPullSpeed.OnValueChangedUntyped += UpdateSettings;
        MelonLoader.MelonCoroutines.Start(WaitForLocalPlayer());
    }

    System.Collections.IEnumerator WaitForLocalPlayer()
    {
        while (PlayerSetup.Instance == null)
            yield return null;
        m_pickupPP = PlayerSetup.Instance.gameObject.AddComponent<PickupPP>();
    }

    private static void UpdateSettings()
    {
        m_pickupPP.ppSpeed = melonEntryPushPullSpeed.Value;
    }
}

class PickupPP : MonoBehaviour
{

    public float ppSpeed = 2f;

    private bool gamepadInput = false;
    private CursorLockMode savedCursorLockState;
    private bool buttonPressed = false;

    void Start()
    {
        this.gamepadInput = MetaPort.Instance.settings.GetSettingsBool("ControlEnableGamepad");
        MetaPort.Instance.settings.settingBoolChanged.AddListener(new UnityAction<string, bool>(this.SettingsBoolChanged));
    }

    void Update()
    {
        //VR input doesnt really need locking :shrug:
        if (!gamepadInput)
        {
            CVRInputManager.Instance.objectPushPull += CVRInputManager.Instance.scrollValue * ppSpeed;
        }
        else
        {
            bool button = Input.GetButton("Controller Left Button");
            if (button)
            {
                if (!buttonPressed)
                {
                    buttonPressed = true;
                    savedCursorLockState = Cursor.lockState;
                    Cursor.lockState = CursorLockMode.None;
                    PlayerSetup.Instance._movementSystem.disableCameraControl = true;
                }
                CVRInputManager.Instance.objectPushPull += CVRInputManager.Instance.rawLookVector.y * ppSpeed;
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
    }

    private void SettingsBoolChanged(string name, bool value)
    {
        if (name == "ControlEnableGamepad")
        {
            this.gamepadInput = value;
        }
    }
}