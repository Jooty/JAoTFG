using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;

public static class ProfileManager
{

    private const string DEFAULT_PROFILE_NAME = "DEFAULT_PLACEHOLDER";

    public static PlayerProfile currentProfile;

    public static void SaveExistingProfile(PlayerProfile newProfile)
    {
        var profiles = loadAllProfiles();
        if (loadAllProfiles() == null)
        {
            AddProfile(newProfile);

            return;
        }

        // Since LINQ returns a new instance, we get the index of it rather than the object itself.
        var localProfileIndex = Array.IndexOf(profiles, profiles.First(x => x.name == newProfile.name));
        if (profiles[localProfileIndex].name != DEFAULT_PROFILE_NAME)
        {
            profiles[localProfileIndex] = newProfile;

            var path = getPathOrCreate();
            string json = JsonHelper.ToJson(profiles, true);
            File.WriteAllText(path, json);
        }
        else
        {
            Debug.LogError($"Profile \"{newProfile.name}\" could not be found!");

            return;
        }
    }

    public static void AddProfile(PlayerProfile newProfile)
    {
        var profiles = loadAllProfiles()?.ToList() ?? new List<PlayerProfile>();

        profiles.Add(newProfile);

        var path = getPathOrCreate();
        string json = JsonHelper.ToJson(profiles.ToArray(), true);
        File.WriteAllText(path, json);
    }

    public static bool TryLoadProfile(string profileName, out PlayerProfile loadedProfile)
    {
        PlayerProfile[] profiles = loadAllProfiles();

        if (profiles != null)
        {
            if (profiles.Any(x => x.name == profileName))
            {
                loadedProfile = profiles.First(x => x.name == profileName);
                return true;
            }
            else
            {
                loadedProfile = null;
                return false;
            }
        }
        else
        {
            loadedProfile = null;
            return false;
        }
    }

    public static void DeleteProfile(PlayerProfile profile)
    {
        var profiles = loadAllProfiles();

        if (profiles.Any(x => x.name == profile.name))
        {
            var newProfiles = profiles.ToList();
            // I don't know why, but we have to remove by the name
            newProfiles.Remove(newProfiles.First(x => x.name == profile.name));

            // serialize it back into json format
            var path = getPathOrCreate();
            string json = JsonHelper.ToJson(newProfiles.ToArray(), true);
            File.WriteAllText(path, json);
        }
        else
        {
            Debug.LogError($"Profile \"{profile.name}\" could not be found!");

            return;
        }
    }

    public static void SetActiveProfile(PlayerProfile profile)
    {
        currentProfile = profile;
        PlayerPrefs.SetString("LastUsedProfile", currentProfile.name);
    }

    public static bool DoesAnyProfileExist()
    {
        var profiles = loadAllProfiles();
        if (profiles != null)
        {
            if (profiles.Length == 0 || profiles[0]?.name == DEFAULT_PROFILE_NAME)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    public static PlayerProfile[] loadAllProfiles()
    {
        var path = getPathOrCreate();

        string content = File.ReadAllText(path);
        if (content != "")
        {
            return JsonHelper.FromJson<PlayerProfile>(content);
        }
        else
        {
            return null;
        }
    }

    public static string getPathOrCreate()
    {
        var path = $"{Application.persistentDataPath}\\PlayerProfiles.json";

        if (File.Exists(path))
        {
            return path;
        }
        else
        {
            File.Create(path);

            return path;
        }
    }

}