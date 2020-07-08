using UnityEngine;

public class GConsoleSimpleToggle : MonoBehaviour
{
    public string toggleCMD = string.Empty;
    public GameObject consoleObject;

    void Update()
    {
        if (toggleCMD == string.Empty)
        {
            return;
        }
        if (Input.GetButtonDown(toggleCMD))
        {
            consoleObject.SetActive(!consoleObject.activeSelf);
        }
    }
}
