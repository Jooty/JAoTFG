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
    [SerializeField] public float DisclaimerReadTime = 5;
    [SerializeField] private Button disclaimerButton;

    // verify panel
    [SerializeField] private TextMeshProUGUI verifyText, verifyBody;
    [SerializeField] private Button verifyButton;

    // options panel
    [SerializeField] private Slider master, sfx, music;

    // misc
    [SerializeField] private TextMeshProUGUI versionText;

    private void Start()
    {
        master.value = AudioSettings.Master;
        sfx.value = AudioSettings.SFX;
        music.value = AudioSettings.Music;

        if (!Application.isEditor)
        {
            StartCoroutine(disclaimerButtonTimer());
        }
        else
        {
            disclaimerButton.interactable = true;
        }

        SetVersionText();
    }

    #region Buttons

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

    private void VerifyDiscordAccount()
    {
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

    private IEnumerator disclaimerButtonTimer()
    {
        disclaimerButton.interactable = false;

        yield return new WaitForSeconds(DisclaimerReadTime);

        disclaimerButton.interactable = true;
    }
}