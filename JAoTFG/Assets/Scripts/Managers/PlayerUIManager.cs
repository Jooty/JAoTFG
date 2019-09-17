using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUIManager : MonoBehaviour
{

    private GameObject localPlayerGO;
    private Player localPlayer;
    private PlayerController localPlayerController;

    private GameObject characterBar;
    private TextMeshProUGUI nameText;
    private Image portrait;
    private Slider healthBar, staminaBar;
    private TextMeshProUGUI healthBarText, staminaBarText;

    private GameObject maneuverGearPanel;
    private Slider totalGasBar, burstGasBar;
    private TextMeshProUGUI totalGasBarText, burstGasBarText;

    private void Start()
    {
        localPlayerGO = GameObject.FindGameObjectWithTag("Player");

        if (localPlayerGO)
        {
            localPlayer = localPlayerGO.GetComponent<Player>();
            localPlayerController = localPlayerGO.GetComponent<PlayerController>();

            characterBar = GameObject.Find("HUD/CharacterBar");
            nameText = characterBar.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            portrait = characterBar.transform.GetChild(4).GetChild(0).GetComponent<Image>();
            healthBar = characterBar.transform.GetChild(1).GetComponent<Slider>();
            healthBarText = healthBar.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            staminaBar = characterBar.transform.GetChild(2).GetComponent<Slider>();
            staminaBarText = staminaBar.transform.GetChild(2).GetComponent<TextMeshProUGUI>();

            if (localPlayer.hasManeuverGear)
            {
                maneuverGearPanel = GameObject.Find("HUD/ManeuverGearUI");
                totalGasBar = maneuverGearPanel.transform.GetChild(1).GetChild(0).GetComponent<Slider>();
                burstGasBar = maneuverGearPanel.transform.GetChild(1).GetChild(1).GetComponent<Slider>();
                totalGasBarText = totalGasBar.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                burstGasBarText = burstGasBar.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            }

            nameText.text = localPlayer.Name;
            portrait.sprite = localPlayer.portrait;
        }
    }

    private void Update()
    {
        healthBar.maxValue = localPlayer.maxHealth;
        healthBar.value = localPlayer.health;
        staminaBar.maxValue = localPlayer.maxStamina;
        staminaBar.value = localPlayer.stamina;

        healthBarText.text = $"Health: {localPlayer.health} / {localPlayer.maxHealth}";
        staminaBarText.text = $"Stamina: {localPlayer.stamina} / {localPlayer.maxStamina}";

        if (localPlayer.hasManeuverGear)
        {
            //maneuverGearPanel.SetActive(true);

            totalGasBar.maxValue = localPlayer.maneuverGear.totalMaxGas;
            totalGasBar.value = localPlayer.maneuverGear.gas;
            burstGasBar.maxValue = localPlayer.maneuverGear.maxBurstGas;
            burstGasBar.value = localPlayer.maneuverGear.burstGas;

            totalGasBarText.text = $"Gas: {(int)localPlayer.maneuverGear.gas} / {(int)localPlayer.maneuverGear.totalMaxGas}";
            burstGasBarText.text = $"Burst: {(int)localPlayer.maneuverGear.burstGas} / {(int)localPlayer.maneuverGear.maxBurstGas}";
        }
        else
        {
            //maneuverGearPanel.SetActive(false);
        }
    }

}
