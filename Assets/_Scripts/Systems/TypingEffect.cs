using System.Collections;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;

public class TypingEffect : MonoBehaviour
{
    public TMP_Text tmpText;
    private AudioSource typeAudioSource;


    [Header("Monologue UI")]
    public string textMessage;
    public CanvasGroup textCanvasGroup;
    public float typeSpeed = 0.03f;
    public float displayDuration = 2f;
    public float fadeDuration = 0.5f;

    [Header("Monologue Audio")]
    public AudioClip typeSound;
    public int soundInterval = 2; //Play sound every X characters
    private int charCounter = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tmpText = GetComponent<TMP_Text>();
        typeAudioSource = GetComponent<AudioSource>();
        textCanvasGroup = GetComponent<CanvasGroup>();

    }

    void Awake()
    {
        tmpText.text = "";
    }

    public void StartTyping()
    {
        StartCoroutine(TypeText(textMessage));
    }

    private IEnumerator TypeText(string message)
    {
        //Initialize text
        tmpText.text = "";
        charCounter = 0;

        //Set visible
        if (textCanvasGroup != null)
        {
            textCanvasGroup.alpha = 1f;
        }

        //Typing effect
        for (int i = 0; i < message.Length; i++)
        {
            char currentChar = message[i];

            tmpText.text += currentChar;

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
        yield return new WaitForSeconds(displayDuration);

        //Fade out over time
        if (textCanvasGroup != null)
        {
            float startAlpha = textCanvasGroup.alpha;
            float time = 0f;

            while (time < fadeDuration)
            {
                time += Time.deltaTime;

                float t = time / fadeDuration;
                textCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);

                yield return null;
            }

            //Ensure it dissapears if fading fails
            textCanvasGroup.alpha = 0f;
        }
    }

    private void PlayTypeSound()
    {
        if (typeAudioSource != null && typeSound != null)
        {
            //Slight pitch variation so it doesn't sound repetitive
            typeAudioSource.pitch = Random.Range(0.9f, 1.1f);
            typeAudioSource.PlayOneShot(typeSound);
        }
    }
}
