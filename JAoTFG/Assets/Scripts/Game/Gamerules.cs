using System;
using System.Collections.Generic;
using System.Linq;

public static class Gamerules
{

    public static float HERO_MAX_SPEED = 65f;
    public static float HERO_AIR_ROTATE_SPEED = 3f;

    public static bool MG_RETRACT_ON_GAS = true;
    public static float MG_HOOK_RANGE = 95f;
    public static float MG_GAS_REEL_SPEED_MULTIPLIER = 2.8f;

    public static float TITAN_DISSIPATE_TIMER = 30;

    public static float FIELD_OF_VIEW = 90;

    public static bool SetVariable<T>(string varName, T newVal) where T
        : struct, IComparable, IConvertible, IComparable<T>, IEquatable<T>
    {
        var result = typeof(Gamerules).GetField(varName);
        if (result != null)
        {
            result.SetValue(null, newVal);

            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool TryGetVariableValue(string varName, out object value)
    {
        var result = typeof(Gamerules).GetField(varName);
        if (result != null)
        {
            value = result.GetValue(null);

            return true;
        }
        else
        {
            value = "";
            return false;
        }
    }

    public static Dictionary<string, string> GetAllRules()
    {
        return typeof(Gamerules)
            .GetFields()
            .ToDictionary(x => x.Name, x => x.GetValue(null).ToString());
    }

}
