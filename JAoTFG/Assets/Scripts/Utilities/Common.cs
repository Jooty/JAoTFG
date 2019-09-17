using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Common
{

    /// <summary>
    /// Checks if an object exists.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool Exists(object obj)
    {
        return obj != null;
    }

    /// <summary>
    /// Changes a float value over time
    /// </summary>
    /// <param name="value">The variable to change, by reference.</param>
    /// <param name="target"></param>
    /// <param name="dur">The time it takes to reach the target.</param>
    public static void ChangeFloatOverTime(ref float value, float target, float dur)
    {
        value = Mathf.Lerp(value, target, Time.deltaTime / dur);
    }

    /// <summary>
    /// Returns the difference between firstMin and firstMax 
    /// relative to the difference between secMin and secMax, based of the dependant.
    /// </summary>
    /// <param name="firstMin">The minimum number for the first set of numbers.</param>
    /// <param name="firstMax">The maxmimum number for the first set of numbers.</param>
    /// <param name="secMin">The minimum number for the second set of numbers.</param>
    /// <param name="secMax">The maximum number for the second set of numbers.</param>
    /// <param name="dependant">The variable between secMin and secMax.</param>
    /// <returns></returns>
    public static float GetFloatByRelativePercent(float firstMin, float firstMax, float secMin, float secMax, float dependant)
    {
        var firstPercent = dependant / (secMax - secMin);
        var calc = (firstPercent * (firstMax - firstMin)) + firstMin;

        return calc;
    }

}
