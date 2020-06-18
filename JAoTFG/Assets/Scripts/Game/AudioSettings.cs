using UnityEngine;

public static class AudioSettings
{
    public static float Master { get; private set; }
    public static float SFX { get; private set; }
    public static float Music { get; private set; }

    public static void Load()
    {
        Master = PlayerPrefs.GetFloat("AUDIO-Master");
        SFX = PlayerPrefs.GetFloat("AUDIO-SFX");
        Music = PlayerPrefs.GetFloat("AUDIO-Music");
    }

    public static void SetMasterVolume(float newV)
    {
        Master = newV;
        PlayerPrefs.SetFloat("AUDIO-Master", newV);
    }

    public static void SetSFXVolume(float newV)
    {
        SFX = newV;
        PlayerPrefs.SetFloat("AUDIO-SFX", newV);
    }

    public static void SetMusicVolume(float newV)
    {
        Music = newV;
        PlayerPrefs.SetFloat("AUDIO-Music", newV);
    }
}