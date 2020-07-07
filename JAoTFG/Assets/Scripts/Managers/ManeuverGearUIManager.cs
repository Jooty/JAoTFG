using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManeuverGearUIManager : MonoBehaviour
{

    public Slider gasTankSlider;
    public Image hookLeftCondition;
    public Image hookRightCondition;

    private PlayerController playerController;

    private void Start()
    {
        this.playerController = FindObjectOfType<PlayerController>();

        gasTankSlider.maxValue = playerController.maxGas;
    }

    private void Update()
    {
        gasTankSlider.value = playerController.currentGas;
    }

}
