using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevConsoleManager : MonoBehaviour
{

    public static DevConsoleManager instance;

    public GameObject consoleGO;

    // all player related controls
    private ThirdPersonCamera tpc;
    private PlayerController playerController;
    private PlayerAnimator playerAnimator;

    private void Awake()
    {
        instance = this;

        tpc = FindObjectOfType<ThirdPersonCamera>();
        playerController = FindObjectOfType<PlayerController>();
        playerAnimator = FindObjectOfType<PlayerAnimator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ToggleConsole();
        }
    }

    private void ToggleConsole()
    {
        // cursor
        Cursor.lockState = (!consoleGO.activeInHierarchy) ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = !Cursor.visible;

        consoleGO.SetActive(!consoleGO.activeInHierarchy);

        // disable all player related objects
        tpc.enabled = !tpc.enabled;
        playerController.enabled = !playerController.enabled;
        playerAnimator.enabled = !playerAnimator.enabled;
    }

}
