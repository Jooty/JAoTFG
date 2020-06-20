using TMPro;
using UnityEngine;

public class PlayerDebugUIManager : MonoBehaviour
{
    public TextMeshProUGUI debugtext;

    private PlayerController localPlayer;

    private void Start()
    {
        localPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    private void Update()
    {
        debugtext.text =
            $"max speed: {GameVariables.HERO_MAX_SPEED}\n" +
            $"gear: {localPlayer.usingManGear}\n" +
            $"left-distance: {localPlayer.GetLeftHook()?.tetherDistance}\n" +
            $"right-distance: {localPlayer.GetRightHook()?.tetherDistance}\n" +
            $"left-status: {localPlayer.GetLeftHook()?.status}\n" +
            $"right-status: {localPlayer.GetRightHook()?.status}\n" +
            $"grounded: {localPlayer.IsGrounded()}\n" +
            $"gas: {localPlayer.gas / localPlayer.totalMaxGas}\n" +
            $"thrust-power: {localPlayer.thrustPower}\n" +
            $"thrust: {localPlayer.isThrusting}\n" +
            $"iswaitingtoland: {localPlayer.isWaitingToLand}\n" +
            $"canjump: {localPlayer.canJump}\n" +
            $"issliding: {localPlayer.isSliding}\n" +
            $"velocity: {localPlayer.currentSpeed}\n" +
            $"using gear: {localPlayer.usingManGear}";
    }
}