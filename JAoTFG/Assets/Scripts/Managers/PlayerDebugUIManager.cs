using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDebugUIManager : MonoBehaviour
{
    public TextMeshProUGUI debugtext;

    private bool isOn;

    private PlayerController localPlayer;

    // locals
    private Image background;

    private void Awake()
    {
        this.background = GetComponent<Image>();

        isOn = true;
    }

    private void Start()
    {
        localPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O)) TogglePanel();

        debugtext.text =
            $"max speed: {GameVariables.HERO_MAX_SPEED}\n" +
            $"gear: {localPlayer.usingManGear}\n" +
            $"left-distance: {localPlayer.GetLeftHook()?.tetherDistance}\n" +
            $"right-distance: {localPlayer.GetRightHook()?.tetherDistance}\n" +
            $"left-status: {localPlayer.GetLeftHook()?.status}\n" +
            $"right-status: {localPlayer.GetRightHook()?.status}\n" +
            $"grounded: {localPlayer.IsGrounded()}\n" +
            $"thrust-power: {localPlayer.thrustPower}\n" +
            $"thrust: {localPlayer.isThrusting}\n" +
            $"iswaitingtoland: {localPlayer.isWaitingToLand}\n" +
            $"canjump: {localPlayer.canJump}\n" +
            $"issliding: {localPlayer.isSliding}\n" +
            $"velocity: {(int)localPlayer.currentSpeed} mps\n" +
            $"using gear: {localPlayer.usingManGear}";
    }

    private void TogglePanel()
    {
        isOn = !isOn;
        background.enabled = isOn;
        debugtext.enabled = isOn;
    }
}