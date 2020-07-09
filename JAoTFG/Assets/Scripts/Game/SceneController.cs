using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(SceneController))]
public class SceneController : MonoBehaviour
{
    /// <summary>
    /// This is only implemented for prototyping purposes.
    /// Scenes load too quickly to effecively test this script.
    /// </summary>
    [Range(1, 10)] [SerializeField] private float loadingWaitTime = 10f;

    public Image fadeImage;
    public string FirstSceneToLoad = "MainMenu";
    private Image background;
    private TextMeshProUGUI quoteText;

    private Sprite[] backgrounds;

    [SerializeField] private string currentlyLoading;
    private bool needsToGoToLoadingScene;
    private AsyncOperation currentOperation;

    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;

        if (SceneManager.GetActiveScene().name == "LoadingScreen")
        {
            background = GameObject.Find("Canvas/Background").GetComponent<Image>();
            quoteText = GameObject.Find("Canvas/BottomPanel/QuoteText").GetComponent<TextMeshProUGUI>();

            SetBackground();
            SetQuote();

            ChangeScene(FirstSceneToLoad);
        }
    }

    private void SceneManager_activeSceneChanged(Scene lastScene, Scene newScene)
    {
        anim.SetTrigger("fadeIn");

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
        if (currentOperation != null) return;

        currentlyLoading = sceneName;

        if (SceneManager.GetActiveScene().name != "LoadingScreen")
        {
            anim.SetTrigger("fadeOut");
            needsToGoToLoadingScene = true;
        }
        else
        {
            anim.SetTrigger("fadeIn");
        }
    }

    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void FadeOutCompletedEvent()
    {
        if (needsToGoToLoadingScene)
        {
            SceneManager.LoadScene("LoadingScreen");
            needsToGoToLoadingScene = false;
        }

        if (currentlyLoading == "")
        {
            currentOperation.allowSceneActivation = true;
            currentOperation = null;
        }
        else
        {
            anim.SetTrigger("fadeIn");
        }
    }

    public void FadeInCompletedEvent()
    {
        if (currentlyLoading != "")
        {
            StartCoroutine(beginLoading());
        }
    }

    private IEnumerator beginLoading()
    {
        currentOperation = SceneManager.LoadSceneAsync(currentlyLoading);
        currentOperation.allowSceneActivation = false;

        while (currentOperation.progress < 0.9f) yield return null;

        yield return new WaitForSeconds(loadingWaitTime);

        goToLoadedScene();
    }

    private void goToLoadedScene()
    {
        if (currentlyLoading != "MainMenu")
            GetComponent<AudioSource>().Stop();

        currentlyLoading = "";
        anim.SetTrigger("fadeOut");
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
}