using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class NightUI : MonoBehaviour
{
    public TMP_Text nightText;
    public CanvasGroup textCanvasGroup;
    public CanvasGroup panelCanvasGroup;

    public bool isTextFaded = false;

    [Header("Typing Effects")]
    public float typeSpeed = 0.03f;
    public float textDuration = 2f;
    public float fadeDuration = 0.5f;

    [Header("Typing Audio")]
    public AudioSource typingAudioSource;
    public AudioClip typeSound;
    public int soundInterval = 2; //Play sound every X characters
    private int charCounter = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        panelCanvasGroup.alpha = 1;
        
        string displayedText = "Night " + SceneManager.GetActiveScene().buildIndex.ToString();
        StartCoroutine(TypeMonologue(displayedText));
    }

    IEnumerator TypeMonologue(string message)
    {
        isTextFaded = false;

        //Initialize text
        nightText.text = "";
        charCounter = 0;

        yield return new WaitForSeconds(1.25f);

        //Set visible
        if (textCanvasGroup != null)
        {
            textCanvasGroup.alpha = 1f;
        }

        //Typing effect
        for (int i = 0; i < message.Length; i++)
        {
            char currentChar = message[i];

            nightText.text += currentChar;

            //Only play sound on non-space characters
            if (!char.IsWhiteSpace(currentChar))
            {
                charCounter++;

                if (charCounter >= soundInterval)
                {
                    PlayTypeSound();
                    charCounter = 0;
                }
            }

            yield return new WaitForSeconds(typeSpeed);
        }

        //Wait after typing
        yield return new WaitForSeconds(textDuration);

        //Fade out over time
        if (panelCanvasGroup != null)
        {
            float startAlpha = panelCanvasGroup.alpha;
            float time = 0f;

            while (time < fadeDuration)
            {
                time += Time.deltaTime;

                float t = time / fadeDuration;
                panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);

                yield return null;
            }

            //Ensure it dissapears if fading fails
            panelCanvasGroup.alpha = 0f;

            isTextFaded = true;
        }
    }

    void PlayTypeSound()
    {
        if (typingAudioSource != null && typeSound != null)
        {
            //Slight pitch variation so it doesn't sound repetitive
            typingAudioSource.pitch = Random.Range(0.9f, 1.1f);
            typingAudioSource.PlayOneShot(typeSound);
        }
    }
}
