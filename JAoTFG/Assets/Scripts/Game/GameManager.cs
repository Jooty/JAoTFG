using DiscordRPC;
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

    private DiscordManager discord;

    private ScoreShow scoreShow;

    // Locals
    private SceneController sceneController;
    private AudioSource musicPlayer;
    private Animator anim;

    private void Awake()
    {
        CheckForDuplicateGameManagers();
        DontDestroyOnLoad(gameObject);

        discord = FindObjectOfType<DiscordManager>();

        this.sceneController = GetComponent<SceneController>();
        this.musicPlayer = GetComponent<AudioSource>();
        this.anim = GetComponent<Animator>();
    }

    private void Start()
    {
        // TODO: unlock
        // LoadAudio();

        GetAndSetAllPlayerControls();

        if (SceneManager.GetActiveScene().name == "UpdatedArena")
        {
            scoreShow = FindObjectOfType<ScoreShow>();
        }

        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void Update()
    {
        if (discord?.rawDiscordClient != null)
        {
            discord.rawDiscordClient.RunCallbacks();
        }
    }

    public void ChangeGameMode(Gamemode mode)
    {
        switch (mode)
        {
            case Gamemode.singleplayer:
                sceneController.ChangeScene("UpdatedArena");
                DiscordManager.current.SetPresence(getDiscordRPPerMode(Gamemode.singleplayer));
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

    public void PauseGame()
    {
        // cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // disable all player related objects
        if (playerController)
        {
            tpc.enabled = false;
            playerController.enabled = false;
            playerAnimator.enabled = false;
            playerVelOnPause = playerRigidbody.velocity;
            playerRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    public void ResumeGame()
    {
        // cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // enable all player related objects
        if (playerController)
        {
            tpc.enabled = true;
            playerController.enabled = true;
            playerAnimator.enabled = true;
            playerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        playerRigidbody.velocity = playerVelOnPause;
    }

    public void TitanDeathEvent(DeathInfo info)
    {
        scoreShow.ShowScore(info);
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
        if (SceneManager.GetActiveScene().name != "UpdatedArena") return;

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

    private DiscordPresence getDiscordRPPerMode(Gamemode mode)
    {
        DiscordAsset bigAsset = new DiscordAsset()
        {
            image = "logo_discord",
            tooltip = "JAoTFG"
        };
        DiscordAsset smallAsset = new DiscordAsset()
        {
            image = "",
            tooltip = ""
        };

        switch (mode)
        {
            case Gamemode.mainMenu:
                return new DiscordPresence()
                {
                    details = "Main Menu",
                    state = "Idling",
                    largeAsset = bigAsset,
                    smallAsset = smallAsset
                };
            case Gamemode.singleplayer:
                return new DiscordPresence()
                {
                    details = "Singleplayer",
                    state = "As Human",
                    largeAsset = bigAsset,
                    smallAsset = smallAsset
                };
            case Gamemode.multiplayer:
                return new DiscordPresence()
                {
                    details = "Multiplayer",
                    state = "As Human",
                    largeAsset = bigAsset,
                    smallAsset = smallAsset
                };
            default:
                return null;
        }
    }

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
    {
        GetAndSetAllPlayerControls();

        if (SceneManager.GetActiveScene().name == "UpdatedArena")
        {
            scoreShow = FindObjectOfType<ScoreShow>();
        }
    }
}