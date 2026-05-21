using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class LightController : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource lightAudioSource;

    public AudioClip hummingAudioClip;
    public AudioClip lightsOnAudioClip;
    public AudioClip lightsOffAudioClip;

    [Header("Lights")]
    public GameObject pointLight;
    public GameObject spotLight;

    [Header("Settings")]
    public bool lightOnByDefault = true;

    private bool isLightOn = false;
    private Coroutine lightCoroutine;

    private void Awake()
    {
        lightAudioSource = GetComponent<AudioSource>();

        isLightOn = lightOnByDefault;

        if (isLightOn)
        {
            SetLightObjects(true);

            lightAudioSource.clip = hummingAudioClip;
            lightAudioSource.loop = true;
            lightAudioSource.Play();
        }
        else
        {
            SetLightObjects(false);
        }
    }

    public void ToggleLight()
    {
        if (lightCoroutine != null)
        {
            StopCoroutine(lightCoroutine);
        }

        if (isLightOn)
        {
            lightCoroutine = StartCoroutine(TurnLightOff());
        }
        else
        {
            lightCoroutine = StartCoroutine(TurnLightOn());
        }
    }

    private IEnumerator TurnLightOn()
    {
        isLightOn = true;

        lightAudioSource.Stop();
        lightAudioSource.loop = false;

        lightAudioSource.PlayOneShot(lightsOnAudioClip);

        SetLightObjects(true);

        yield return new WaitForSeconds(lightsOnAudioClip.length);

        lightAudioSource.clip = hummingAudioClip;
        lightAudioSource.loop = true;
        lightAudioSource.Play();
    }

    private IEnumerator TurnLightOff()
    {
        isLightOn = false;

        lightAudioSource.Stop();
        lightAudioSource.loop = false;

        lightAudioSource.PlayOneShot(lightsOffAudioClip);

        SetLightObjects(false);

        yield return new WaitForSeconds(lightsOffAudioClip.length);
    }

    private void SetLightObjects(bool active)
    {
        pointLight.SetActive(active);
        spotLight.SetActive(active);
    }
}