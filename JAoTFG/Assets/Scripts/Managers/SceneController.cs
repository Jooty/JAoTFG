using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using TMPro;

[RequireComponent(typeof(SceneController))]
public class SceneController : MonoBehaviour
{

    /// <summary>
    /// This is only implemented for prototyping purposes. 
    /// Scenes load too quickly to effecively test this script.
    /// </summary>
    [Range(1, 10)] [SerializeField] private float loadingWaitTime = 3f;
    [Range(1, 3)] [SerializeField] private float fadeWaitTime = 1f;

    public Image fadeImage;
    public string FirstSceneToLoad = "MainMenu";
    private Image background;
    private TextMeshProUGUI quoteText;

    private Sprite[] backgrounds;

    private string sceneToLoad;
    [SerializeField] private string currentlyLoading;
    private AsyncOperation currentOperation;

    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;

        background = GameObject.Find("Canvas/Background").GetComponent<Image>();
        quoteText = GameObject.Find("Canvas/BottomPanel/QuoteText").GetComponent<TextMeshProUGUI>();

        SetBackground();
        SetQuote();

        ChangeScene(FirstSceneToLoad);
    }

    private void SceneManager_activeSceneChanged(Scene lastScene, Scene newScene)
    {
        if (newScene.name == "LoadingScreen")
        {
            background = GameObject.Find("Canvas/Background").GetComponent<Image>();
            quoteText = GameObject.Find("Canvas/BottomPanel/QuoteText").GetComponent<TextMeshProUGUI>();

            SetBackground();
            SetQuote();
        }
    }

    public void ChangeScene(string sceneName)
    {
        currentlyLoading = sceneName;
        StartCoroutine(changeScene());
    }

    private IEnumerator changeScene()
    {
        yield return StartCoroutine(changeToLoadingSceneIfNeeded());
        yield return StartCoroutine(Fade(false));

        if (currentlyLoading != "MainMenu")
            GetComponent<AudioSource>().Stop();

        StartCoroutine(beginLoading());
    }

    IEnumerator beginLoading()
    {
        currentOperation = SceneManager.LoadSceneAsync(currentlyLoading);
        currentOperation.allowSceneActivation = false;

        while (currentOperation.progress < 0.9f) yield return null;

        StartCoroutine(LoadingWaitTimer());

        StartCoroutine(goToNextScene()); 
    }

    IEnumerator goToNextScene()
    {
        yield return StartCoroutine(Fade());
        currentOperation.allowSceneActivation = true;

        currentlyLoading.Empty();
        currentOperation = null;

        StartCoroutine(Fade(false));
    }

    IEnumerator changeToLoadingSceneIfNeeded()
    {
        if (SceneManager.GetActiveScene().name == "LoadingScreen") yield break;

        yield return StartCoroutine(Fade());

        SceneManager.LoadScene("LoadingScreen");
    }

    IEnumerator Fade(bool _out = true)
    {
        if (_out)
            anim.SetTrigger("fadeOut");
        else
            anim.SetTrigger("fadeIn");

        yield return new WaitForSeconds(fadeWaitTime);
    }

    private void SetBackground()
    {
        backgrounds = Resources.LoadAll<Sprite>("LoadingScreen/Backgrounds");
        background.sprite = backgrounds[UnityEngine.Random.Range(0, backgrounds.Length)];
    }

    private void SetQuote()
    {
        var quotes = GetQuotes();
        quoteText.text = quotes[UnityEngine.Random.Range(0, quotes.Length)];
    }

    private string[] GetQuotes()
    {
        var data = Resources.Load<TextAsset>("LoadingScreen/Quotes").text;

        var newData = data;
        bool doQuoteData = false;
        string quoteData = "";
        var quotes = new List<string>();
        foreach (char c in newData)
        {
            if (c == '[')
            {
                doQuoteData = true;
            }
            else if (doQuoteData)
            {
                if (c == ']')
                {
                    doQuoteData = false;

                    quotes.Add(quoteData);
                    newData.Replace(quoteData, "");
                    quoteData = "";

                    continue;
                }

                quoteData += c;
            }
            else
                continue;
        }

        return quotes.ToArray();
    }

    private IEnumerator LoadingWaitTimer()
    {
        yield return new WaitForSeconds(loadingWaitTime);
    }

}
