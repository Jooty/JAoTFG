using UnityEngine;
using UnityEngine.UI;

public class ManeuverGearUIManager : MonoBehaviour
{

    public Slider gasTankSlider;
    public Image crosshair;
    public Image crosshair_hookLeft;
    public Image crosshair_hookRight;

    public Color defaultCrosshairColor;
    public Color canHookColor;
    public Color hookedColor;

    private PlayerController playerController;

    private void Start()
    {
        this.playerController = FindObjectOfType<PlayerController>();

        gasTankSlider.maxValue = playerController.maxGas;
    }

    private void Update()
    {
        gasTankSlider.value = playerController.currentGas;

        SetCrosshairColor();
        SetHooksColor();
    }

    private void SetCrosshairColor()
    {
        if (playerController.IsPlayerInHookRange())
        {
            crosshair.color = canHookColor;
        }
        else
        {
            crosshair.color = defaultCrosshairColor;
        }
    }

    private void SetHooksColor()
    {
        if (playerController.GetLeftHook()?.status == HookStatus.attached)
        {
            crosshair_hookLeft.color = hookedColor;
        }
        else
        {
            crosshair_hookLeft.color = defaultCrosshairColor;
        }

        if (playerController.GetRightHook()?.status == HookStatus.attached)
        {
            crosshair_hookRight.color = hookedColor;
        }
        else
        {
            crosshair_hookRight.color = defaultCrosshairColor;
        }
    }

}
