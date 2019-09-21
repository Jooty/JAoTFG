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
            $"gear: {player.usingManGear}" +
            $"gas: {player.gas / player.totalMaxGas}\n" +
            $"thrust-power: {player.thrustPower}\n" +
            $"thrust: {player.isThrusting}\n" +
            $"iswaitingtoland: {player.isWaitingToLand}\n" +
            $"canjump: {player.canJump}\n" +
            $"wantstojump: {player.wantsToJump}\n" +
            $"issliding: {player.isSliding}\n";
    }

}
