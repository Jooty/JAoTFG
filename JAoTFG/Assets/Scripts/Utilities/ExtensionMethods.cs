using System;
using System.Collections.Generic;
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
    /// Returns all child gameobjects with tag.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static List<GameObject> FindObjectsWithTag(this Transform parent, string tag)
    {
        List<GameObject> taggedGameObjects = new List<GameObject>();

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.tag == tag)
            {
                taggedGameObjects.Add(child.gameObject);
            }
            if (child.childCount > 0)
            {
                taggedGameObjects.AddRange(FindObjectsWithTag(child, tag));
            }
        }
        return taggedGameObjects;
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

    public static Vector3 ChangeY(this Vector3 vec, float newY)
    {
        return new Vector3(vec.x, newY, vec.z);
    }
}