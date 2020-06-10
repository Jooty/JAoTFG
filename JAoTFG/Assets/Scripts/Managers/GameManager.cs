using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    // Singleton
    [HideInInspector] public static GameManager instance;

    public Gamemode gamemode;

    // Locals
    private SceneController sceneController;
    private AudioSource musicPlayer;
    private Animator anim;

    private void Awake()
    {
        if (GameManager.instance)
        {
            Destroy(gameObject);
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        sceneController = GetComponent<SceneController>();
        musicPlayer = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        LoadAudio();
    }

    private void LoadAudio()
    {
        if (!Application.isEditor)
        {
            AudioSettings.Load();
            AudioListener.volume = AudioSettings.Master;
            musicPlayer.volume = AudioSettings.Music;
        }
        else
        {
            AudioSettings.SetMasterVolume(1);
            AudioSettings.SetMusicVolume(.15f);
            AudioSettings.SetSFXVolume(.15f);
        }
    }

    public void ChangeGameMode(Gamemode mode)
    {
        switch (mode)
        {
            case Gamemode.arena:
                sceneController.ChangeScene("UpdatedArena");
                break;
        }
    }

}
