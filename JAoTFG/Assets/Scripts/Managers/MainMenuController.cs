using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{

    [SerializeField] private Slider master, sfx, music;

    private void Start()
    {
        master.value = AudioSettings.Master;
        sfx.value = AudioSettings.SFX;
        music.value = AudioSettings.Music;
    }

    public void BTN_Start()
    {
        GameManager.instance.ChangeGameMode(Gamemode.arena);
    }

    public void BTN_Exit()
    {
        Application.Quit();
    }

    public void Slider_MasterVolume()
    {
        AudioSettings.SetMasterVolume(master.value);
    }

    public void Slider_SFXVolume()
    {
        AudioSettings.SetSFXVolume(sfx.value);
    }

    public void Slider_MusicVolume()
    {
        AudioSettings.SetMusicVolume(music.value);
    }

}
