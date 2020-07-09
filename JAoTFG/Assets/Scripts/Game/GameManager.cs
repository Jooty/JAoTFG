using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton
    [HideInInspector] public static GameManager instance;

    public Gamemode gamemode;

    // all player related controls
    private ThirdPersonCamera tpc;
    private PlayerController playerController;
    private PlayerAnimator playerAnimator;
    private Rigidbody playerRigidbody;

    private Vector3 playerVelOnPause;

    // Locals
    private SceneController sceneController;
    private AudioSource musicPlayer;
    private Animator anim;

    private void Awake()
    {
        CheckForDuplicateGameManagers();
        DontDestroyOnLoad(gameObject);

        this.sceneController = GetComponent<SceneController>();
        this.musicPlayer = GetComponent<AudioSource>();
        this.anim = GetComponent<Animator>();
    }

    private void Start()
    {
        // TODO: unlock
        // LoadAudio();

        GetAndSetAllPlayerControls();

        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void CheckForDuplicateGameManagers()
    {
        if (instance)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    private void GetAndSetAllPlayerControls()
    {
        tpc = FindObjectOfType<ThirdPersonCamera>();
        playerController = FindObjectOfType<PlayerController>();
        playerAnimator = FindObjectOfType<PlayerAnimator>();
        playerRigidbody = playerController.gameObject.GetComponent<Rigidbody>();
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
            case Gamemode.mainMenu:
                sceneController.ChangeScene("MainMenu");
                break;
        }
    }

    public void ReloadLevel()
    {
        sceneController.ReloadCurrentScene();
    }

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
    {
        GetAndSetAllPlayerControls();
    }

    public void PauseGame()
    {
        // cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // disable all player related objects
        tpc.enabled = false;
        playerController.enabled = false;
        playerAnimator.enabled = false;
        playerVelOnPause = playerRigidbody.velocity;
        playerRigidbody.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void ResumeGame()
    {
        // cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // enable all player related objects
        tpc.enabled = true;
        playerController.enabled = true;
        playerAnimator.enabled = true;
        playerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;

        playerRigidbody.velocity = playerVelOnPause;
    }
}