using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerDebugUIManager : MonoBehaviour
{

    public TextMeshProUGUI debugtext;

    private PlayerController player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    private void Update()
    {
        debugtext.text = $"speed: {player.rigid.velocity.magnitude}\n" +
            $"max speed: {GameVariables.HERO_MAX_SPEED}\n" +
            $"gear: {player.usingManGear}\n" +
            $"left-distance: {player.GetLeftHook()?.tetherDistance}\n" +
            $"right-distance: {player.GetRightHook()?.tetherDistance}\n" +
            $"left-status: {player.GetLeftHook()?.status}\n" +
            $"right-status: {player.GetRightHook()?.status}\n" +
            $"grounded: {player.IsGrounded()}\n" +
            $"gas: {player.gas / player.totalMaxGas}\n" +
            $"thrust-power: {player.thrustPower}\n" +
            $"thrust: {player.isThrusting}\n" +
            $"iswaitingtoland: {player.isWaitingToLand}\n" +
            $"canjump: {player.canJump}\n" +
            $"issliding: {player.isSliding}\n" +
            $"velocity: {player.rigid.velocity.magnitude}\n" +
            $"using gear: {player.usingManGear}";
    }

}
