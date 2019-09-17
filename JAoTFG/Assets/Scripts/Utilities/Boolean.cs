using UnityEngine;

public class Boolean
{
    
    public static void SetBooleanRandom(ref bool value)
    {
        var r = Random.Range(1, 2);
        if (r == 1)
        {
            value = true;
        }
        else
        {
            value = false;
        }
    }

    public static bool GetRandomBool()
    {
        var r = Random.Range(1, 2);
        if (r == 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}