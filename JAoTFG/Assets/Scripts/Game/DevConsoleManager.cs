using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DevConsoleManager : MonoBehaviour
{

    public static DevConsoleManager instance;

    public bool isToggledOn;

    public GameObject consoleGO;

    private void Awake()
    {
        instance = this;
        isToggledOn = false;
    }

    private void Start()
    {
        InitiateCommands();
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
        isToggledOn = !isToggledOn;
        consoleGO.SetActive(isToggledOn);

        // pause/unpause
        if (isToggledOn) GameManager.instance.PauseGame();
        else GameManager.instance.ResumeGame();
    }

    private void InitiateCommands()
    {
        GConsole.AddCommand("gamerules", "Lists all gamerules.", ShowGamerules);
        GConsole.AddCommand("heroMaxSpeed", "Changes the hero's max speed.", ChangeHeroMaxSpeed);
        GConsole.AddCommand("heroRotateSpeed", "Changed the hero's rotation speed.", ChangeHeroRotateSpeed);
        GConsole.AddCommand("heroGasReel", "Toggles gas reeling on boost.", ToggleHeroGasReel);
        GConsole.AddCommand("heroHookRange", "Changed the hero's hook range.", ChangeHeroHookRange);
        GConsole.AddCommand("heroReelSpeed", "Changes the hero's reel speed.", ChangeHeroReelSpeed);
        GConsole.AddCommand("titanDissipateTime", "The amount of time that must pass before a titan body will dissipate.", ChangeTitanDissipateTimer);
        GConsole.AddCommand("fov", "Sets the FOV", ChangeFOV);
    }

    private string ShowGamerules(string param)
    {
        var rules = Gamerules.GetAllRules();

        var builder = new StringBuilder();

        foreach (KeyValuePair<string, string> rule in rules)
        {
            builder.Append($"\n{rule.Key} = {rule.Value}");
        }

        return builder.ToString();
    }

    private string ChangeHeroMaxSpeed(string param)
    {
        if (float.TryParse(param, out var val))
        {
            Gamerules.SetVariable("HERO_MAX_SPEED", val);

            return "Successfully set to " + param + ".";
        }
        else
        {
            return "That is not a valid number, or the value is too high/low.";
        }
    }

    private string ChangeHeroRotateSpeed(string param)
    {
        if (float.TryParse(param, out var val))
        {
            Gamerules.SetVariable("HERO_AIR_ROTATE_SPEED", val);

            return "Successfully set to " + param + ".";
        }
        else
        {
            return "That is not a valid number, or the value is too high/low.";
        }
    }

    private string ToggleHeroGasReel(string param)
    {
        if (bool.TryParse(param, out var val))
        {
            Gamerules.SetVariable("MG_RETRACT_ON_GAS", val);

            return "Successfully set to " + param + ".";
        }
        else
        {
            return "That is not a valid number, or the value is too high/low.";
        }
    }

    private string ChangeHeroHookRange(string param)
    {
        if (float.TryParse(param, out var val))
        {
            Gamerules.SetVariable("MG_HOOK_RANGE", val);

            return "Successfully set to " + param + ".";
        }
        else
        {
            return "That is not a valid number, or the value is too high/low.";
        }
    }

    private string ChangeHeroReelSpeed(string param)
    {
        if (float.TryParse(param, out var val))
        {
            Gamerules.SetVariable("MG_GAS_REEL_SPEED_MULTIPLIER", val);

            return "Successfully set to " + param + ".";
        }
        else
        {
            return "That is not a valid number, or the value is too high/low.";
        }
    }

    private string ChangeTitanDissipateTimer(string param)
    {
        if (float.TryParse(param, out var val))
        {
            Gamerules.SetVariable("TITAN_DISSIPATE_TIMER", val);

            return "Successfully set to " + param + ".";
        }
        else
        {
            return "That is not a valid number, or the value is too high/low.";
        }
    }

    private string ChangeFOV(string param)
    {
        if (float.TryParse(param, out var val))
        {
            Gamerules.SetVariable("FIELD_OF_VIEW", val);
            Camera.main.fieldOfView = val;

            return "Successfully set to " + param + ".";
        }
        else
        {
            return "That is not a valid number, or the value is too high/low.";
        }
    }

}
