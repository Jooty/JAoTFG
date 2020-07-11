using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class MainMenuController : MonoBehaviour
{

    // disclaimers panel
    [Header("Disclaimers")]
    public float DisclaimerReadTime = 5;
    [SerializeField] private GameObject disclaimerPanel;
    [SerializeField] private Button disclaimerButton;

    // verify panel
    [Header("Verify")]
    [SerializeField] private TextMeshProUGUI verifyText;
    [SerializeField] private TextMeshProUGUI verifyBody;
    [SerializeField] private Button verifyButton;

    // profiles panel
    [Header("Profiles")]
    [SerializeField] private TextMeshProUGUI profileButtonText;
    [SerializeField] private GameObject profilesListGrid;
    [SerializeField] private GameObject profileButtonPrefab;
    [SerializeField] private GameObject profileNewProfilePanel;
    [SerializeField] private TMP_InputField profileNewInput;
    [SerializeField] private GameObject profileDisplay;
    [SerializeField] private TextMeshProUGUI profileDisplayName;
    [SerializeField] private TextMeshProUGUI profileDisplayLevel;
    [SerializeField] private TextMeshProUGUI profileDisplayKills;
    [SerializeField] private TextMeshProUGUI profileDisplayHighestScore;
    [SerializeField] private TextMeshProUGUI profileDisplayTotalScore;
    [SerializeField] private TextMeshProUGUI profileNameError;
    [SerializeField] private TextMeshProUGUI profileRenameError;
    [SerializeField] private TMP_InputField profileRenameInput;
    [SerializeField] private GameObject profileBuggedPanel;
    private RuntimeAnimatorController menuAnimController;
    private List<GameObject> profileListings;
    private PlayerProfile selectedProfile;
    private GameObject spawnedCharacterBody;

    // buttons
    [Header("Buttons")]
    [SerializeField] private Button singleplayerButton;
    [SerializeField] private Button multiplayerButton;

    // options panel
    [Header("Options")]
    [SerializeField] private Slider master;
    [SerializeField] private Slider sfx;
    [SerializeField] private Slider music;

    // misc
    [Header("Misc")]
    [SerializeField] private TextMeshProUGUI versionText;
    private GameObject[] characterBodies;

    private void Start()
    {
        master.value = AudioSettings.Master;
        sfx.value = AudioSettings.SFX;
        music.value = AudioSettings.Music;

        disclaimerPanel.SetActive(true);
        if (!Application.isEditor)
        {
            StartCoroutine(disclaimerButtonTimer());
        }
        else
        {
            disclaimerButton.interactable = true;
        }

        SetVersionText();
        SetDefaultProfile();

        menuAnimController = Resources.Load<RuntimeAnimatorController>("Animation/menuCharacterAnimController");
        characterBodies = Resources.LoadAll<GameObject>("CharacterBodies/Humans");
    }

    private void Update()
    {
        if (selectedProfile != null)
        {
            singleplayerButton.interactable = true;
            multiplayerButton.interactable = true;
        }
        else
        {
            singleplayerButton.interactable = false;
            multiplayerButton.interactable = false;
        }
    }

    #region Buttons

    public void BTN_Profiles()
    {
        listProfiles();
    }

    public void BTN_CreateProfile()
    {
        createProfile();
    }

    public void BTN_ProfileRenameSave()
    {
        if (doesProfileAlreadyExist(name))
        {
            profileNameError.text = "A profile already exists with that name!";
            return;
        }
        else if (name.Length < 3)
        {
            profileNameError.text = "Name must be at least 3 characters long!";
            return;
        }
        else
        {
            profileDisplayName.text = profileRenameInput.text;
            selectedProfile.name = profileRenameInput.text;
            ProfileManager.SaveExistingProfile(selectedProfile);
        }
    }

    public void BTN_ProfileDeleteConfirm()
    {
        ProfileManager.DeleteProfile(selectedProfile);

        Destroy(profileListings.First(x => x.name == selectedProfile.name));
        profileListings.Remove(profileListings.First(x => x.name == selectedProfile.name));
        profileButtonText.text = "Create Profile";
    }

    public void BTN_SetProfile()
    {
        profileButtonText.text = selectedProfile.name;
        ProfileManager.SetActiveProfile(selectedProfile);
    }

    public void BTN_Start()
    {
        GameManager.instance.ChangeGameMode(Gamemode.singleplayer);
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

    public void BTN_DisclaimerAccept()
    {
        VerifyDiscordAccount();
    }

    #endregion

    #region Verify

    private void VerifyDiscordAccount()
    {
        if (Application.isEditor)
        {
            SetVerifyMessage("SUCCESSFULLY VERIFIED", "You are using Unity editor.");

            return;
        }

        var user = DiscordManager.current.CurrentUser;
        if (user == null)
        {
            SetVerifyMessage("VERIFICATION FAILED", "Could not connect to Discord.");

            return;
        }

        var username = $"{user.username}#{user.discriminator}";

        var storeManager = DiscordManager.current.rawDiscordClient.GetStoreManager();
        storeManager.FetchEntitlements((result) =>
        {
            if (storeManager.HasSkuEntitlement(593719279161966592))
            {
                SetVerifyMessage("SUCCESSFULLY VERIFIED", "You have been verified through Discord.");
            }
            else
            {
                SetVerifyMessage("VERIFICATION FAILED", "You are not verified to run this application.");
            }
        });
    }

    private void SetVerifyMessage(string header, string body)
    {
        verifyText.text = header;
        verifyBody.text = body;

        if (header == "SUCCESSFULLY VERIFIED")
        {
            verifyText.text = header;
            verifyBody.text = body;

            verifyButton.interactable = true;
        }
    }

    private void SetVersionText()
    {
        if (!Application.isEditor)
        {
            var appManager = DiscordManager.current.rawDiscordClient.GetApplicationManager();
            versionText.text = $"BRANCH: <size=20>{appManager.GetCurrentBranch()}</size>";
        }
        else
        {
            versionText.text = $"BRANCH: <size=20>editor</size>";
        }
    }

    #endregion

    #region Profiles

    private void listProfiles()
    {
        profileListings = new List<GameObject>();
        if (!ProfileManager.DoesAnyProfileExist()) return;

        var profiles = ProfileManager.loadAllProfiles();
        foreach (var profile in profiles)
        {
            GameObject listing = Instantiate(profileButtonPrefab, profilesListGrid.transform);
            listing.name = profile.name;
            listing.GetComponent<Button>().onClick.AddListener(() => showProfile(profile.name));
            listing.GetComponentInChildren<TextMeshProUGUI>().text = profile.name;

            profileListings.Add(listing);
        }
    }

    private void createProfile()
    {
        string name = profileNewInput.text;
        if (doesProfileAlreadyExist(name))
        {
            profileNameError.text = "A profile already exists with that name!";
            return;
        }
        else if (name.Length < 3)
        {
            profileNameError.text = "Name must be at least 3 characters long!";
            return;
        }
        else
        {
            PlayerProfile newProfile = new PlayerProfile(name);
            ProfileManager.AddProfile(newProfile);
            selectedProfile = newProfile;

            GameObject listing = Instantiate(profileButtonPrefab, profilesListGrid.transform);
            listing.name = newProfile.name;
            listing.GetComponent<Button>().onClick.AddListener(() => showProfile(newProfile.name));
            listing.GetComponentInChildren<TextMeshProUGUI>().text = newProfile.name;

            profileListings.Add(listing);

            profileButtonText.text = newProfile.name;
            profileNewProfilePanel.SetActive(false);

            showProfile(newProfile.name);
        }
    }

    private void showProfile(string _profile)
    {
        if (ProfileManager.TryLoadProfile(_profile, out PlayerProfile profile))
        {
            selectedProfile = profile;

            // set text
            profileDisplayName.text = profile.name;
            profileDisplayLevel.text = $"Level {profile.level.ToString()}";
            profileDisplayKills.text = $"Kills: {profile.kills.ToString()}";
            profileDisplayHighestScore.text = $"Highest Score: {profile.highestScore.ToString()}";
            profileDisplayTotalScore.text = $"Total Score: {profile.totalScore.ToString()}";

            // set character display
            if (spawnedCharacterBody) Destroy(spawnedCharacterBody); // remove old one if exists
            GameObject cBody = getCharacterBody(profile.preferredHumanCharacterBody);
            spawnedCharacterBody = Instantiate(cBody);
            spawnedCharacterBody.transform.position = Vector3.zero;
            spawnedCharacterBody.GetComponent<Animator>().runtimeAnimatorController = menuAnimController;
            spawnedCharacterBody.AddComponent<SimpleRotate>().speed = 20;

            profileDisplay.SetActive(true);
        }
        else
        {
            profileBuggedPanel.SetActive(true);
            Destroy(profileListings.First(x => x.name == selectedProfile.name));
            profileListings.Remove(profileListings.First(x => x.name == selectedProfile.name));
        }
    }

    private void SetDefaultProfile()
    {
        string lastProfileName = PlayerPrefs.GetString("LastUsedProfile");
        if (lastProfileName != "")
        {
            if (ProfileManager.TryLoadProfile(lastProfileName, out PlayerProfile loaded))
            {
                selectedProfile = loaded;
                profileButtonText.text = loaded.name;
                singleplayerButton.interactable = true;
                multiplayerButton.interactable = true;
            }
        }
        else
        {
            Debug.Log("No last profile found!");
        }
    }

    private GameObject getCharacterBody(string name)
    {
        return characterBodies.FirstOrDefault(x => x.name == name);
    }

    private bool doesProfileAlreadyExist(string name)
    {
        if (ProfileManager.TryLoadProfile(name, out var profile))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion

    private IEnumerator disclaimerButtonTimer()
    {
        disclaimerButton.interactable = false;

        yield return new WaitForSeconds(DisclaimerReadTime);

        disclaimerButton.interactable = true;
    }
}