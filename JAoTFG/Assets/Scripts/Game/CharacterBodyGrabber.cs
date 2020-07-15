using UnityEngine;

public static class CharacterBodyGrabber
{

    public static GameObject GetCharacterBody(string bodyName)
    {
        var go = Resources.Load<GameObject>($"CharacterBodies/Humans/{bodyName}");
        if (go)
        {
            return go;
        }
        else
        {
            Debug.LogError($"Could not find characterbody with name \"{bodyName}\"!");

            return null;
        }
    }

    public static GameObject GetCurrentCharacterBody()
    {
        var currentProfile = ProfileManager.currentProfile;
        if (currentProfile == null)
        {
            Debug.LogError("Tried to get characterbody while current profile was null!");

            return null;
        }

        var go = Resources.Load<GameObject>($"CharacterBodies/Humans/{currentProfile.preferredHumanCharacterBody}");
        if (!go)
        {
            return go;
        }
        else
        {
            Debug.LogError($"Could not find characterbody with name \"{currentProfile.preferredHumanCharacterBody}\"!");

            return null;
        }
    }

    public static GameObject[] GetAllCharacterBodies()
    {
        return Resources.LoadAll<GameObject>("CharacterBodies/Humans");
    }

}