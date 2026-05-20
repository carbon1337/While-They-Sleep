/*

Handles the intro text shown at the start of each night.

Example:
Night 1
The First Raid

*/

using System.Collections;
using TMPro;
using UnityEngine;

public class NightIntroDisplay : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private CanvasGroup introCanvasGroup;
    [SerializeField] private TextMeshProUGUI nightTitleText;
    [SerializeField] private TextMeshProUGUI nightSubtitleText;

    [Header("Night Info")]
    [SerializeField] private string nightTitle = "Night 1";
    [SerializeField] private string nightSubtitle = "";

    [Header("Timing")]
    [SerializeField] private float startDelay = 0.5f;
    [SerializeField] private float fadeInTime = 1f;
    [SerializeField] private float holdTime = 2f;
    [SerializeField] private float fadeOutTime = 1f;

    [Header("Player Control")]
    [SerializeField] private bool freezePlayerDuringIntro = false;
    [SerializeField] private MonoBehaviour playerController;

    #region Initialization
    private void Start()
    {
        SetIntroText();
        StartCoroutine(PlayIntroSequence());
    }
    #endregion

    #region Intro Sequence
    private void SetIntroText()
    {
        if (nightTitleText != null)
        {
            nightTitleText.text = nightTitle;
        }

        if (nightSubtitleText != null)
        {
            nightSubtitleText.text = nightSubtitle;
            nightSubtitleText.gameObject.SetActive(!string.IsNullOrWhiteSpace(nightSubtitle));
        }

        if (introCanvasGroup != null)
        {
            introCanvasGroup.alpha = 0f;
            introCanvasGroup.blocksRaycasts = false;
            introCanvasGroup.interactable = false;
        }
    }

    private IEnumerator PlayIntroSequence()
    {
        SetPlayerControl(false);

        yield return new WaitForSeconds(startDelay);

        yield return FadeCanvasGroup(0f, 1f, fadeInTime);

        yield return new WaitForSeconds(holdTime);

        yield return FadeCanvasGroup(1f, 0f, fadeOutTime);

        SetPlayerControl(true);
    }

    private IEnumerator FadeCanvasGroup(float startAlpha, float targetAlpha, float duration)
    {
        if (introCanvasGroup == null)
        {
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float percentComplete = elapsedTime / duration;
            introCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, percentComplete);

            yield return null;
        }

        introCanvasGroup.alpha = targetAlpha;
    }
    #endregion

    #region Player Control
    private void SetPlayerControl(bool enabled)
    {
        if (!freezePlayerDuringIntro || playerController == null)
        {
            return;
        }

        playerController.enabled = enabled;
    }
    #endregion
}