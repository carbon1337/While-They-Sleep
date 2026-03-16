/*

Handles global screen fading for scene transitions.

Creates a fullscreen overlay at runtime and fades it
using a CanvasGroup. This object persists between scenes
so it can safely run transitions without being destroyed.

Typical usage:

ScreenFader.Instance.FadeToScene("Level");

*/

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    //Singleton reference so other scripts can access the fader
    public static ScreenFader Instance { get; private set; }

    [Header("Fade Settings")]
    public float fadeDuration = 0.6f;

    private CanvasGroup canvasGroup;

    #region Initialization

    void Awake()
    {
        //Ensure only one ScreenFader exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        //Persist across scene loads
        DontDestroyOnLoad(gameObject);

        //Create the fade overlay
        CreateOverlay();
    }

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    //Creates fullscreen canvas and fade panel
    void CreateOverlay()
    {
        GameObject canvasGO = new GameObject("FadeCanvas");
        canvasGO.transform.SetParent(transform);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject panelGO = new GameObject("FadePanel");
        panelGO.transform.SetParent(canvasGO.transform);

        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = Color.black;

        canvasGroup = panelGO.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        //Stretch panel across the entire screen
        RectTransform rect = panelGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    #endregion


    #region Fade

    //Fade screen from transparent → black
    public IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;

        canvasGroup.blocksRaycasts = true;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    //Fade screen from black → transparent
    public IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(timer / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    #endregion


    #region Scene Transition

    //Public function used to start a fade transition to a new scene
    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeToSceneRoutine(sceneName));
    }

    //Handles fade → scene load → fade in
    IEnumerator FadeToSceneRoutine(string sceneName)
    {
        //Fade screen to black
        yield return StartCoroutine(FadeOut());

        //Begin loading scene
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName);

        //Wait for load to finish
        while (!loadOp.isDone)
        {
            yield return null;
        }

        //Fade back into gameplay
        yield return StartCoroutine(FadeIn());
    }

    #endregion
}