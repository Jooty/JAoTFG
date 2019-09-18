using System;
using UnityEngine;

public static class ExtensionMethods
{

    /// <summary>
    /// Climbs the transform hierachy upwards,
    /// and returns the first parent transform according to tag.
    /// </summary>
    /// <param name="childObject"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static Transform FindParentWithTag(this Transform childObject, string tag)
    {
        Transform t = childObject.transform;
        while (t.parent != null)
        {
            if (t.parent.tag == tag)
            {
                return t.parent;
            }
            t = t.parent.transform;
        }
        return null; 
    }

    /// <summary>
    /// Empties a string.
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    public static string Empty(this string val)
    {
        val = "";
        return val;
    }

    /// <summary>
    /// Toggles a boolean.
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    public static bool Toggle(this bool val)
    {
        return !val;
    }

    public static bool IsEmpty(this Array array)
    {
        return array.Length == 0;
    }

}
