/*

Interactable door that rotates around a hinge point when opened or closed.
Handles smooth animation, door audio, and noise meter impact.

*/
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DoorInteractable : MonoBehaviour, IInteractable
{
    [Header("Hinge Settings")]
    [SerializeField] private Transform hingePoint;
    [SerializeField] private Vector3 hingeAxis = Vector3.up;

    [Header("Door Settings")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openDuration = 0.8f;

    [Header("Audio")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    //Door state
    private bool isOpen = false;
    private bool isMoving = false;

    //Current rotation tracking
    private float currentAngle = 0f;

    //Audio
    private AudioSource audioSource;

    #region Initialization
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        //Configure audio source for 3D door sounds
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;
    }
    #endregion

    #region Interaction
    public string GetPromptText()
    {
        return isOpen ? "E to Close" : "E to Open";
    }

    public void Interact()
    {
        //Prevent interaction while door animation is running
        if (isMoving) return;

        StopAllCoroutines();

        float targetAngle = isOpen ? 0f : openAngle;

        PlayDoorSound();

        StartCoroutine(AnimateDoor(currentAngle, targetAngle));

        isOpen = !isOpen;
    }
    #endregion

    #region Door Animation
    private IEnumerator AnimateDoor(float startAngle, float endAngle)
    {
        isMoving = true;

        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / openDuration);

            //Apply easing so motion starts and stops smoothly
            float easedT = EaseInOutCubic(t);

            float newAngle = Mathf.Lerp(startAngle, endAngle, easedT);
            float delta = newAngle - currentAngle;

            //Rotate door around hinge point
            transform.RotateAround(hingePoint.position, hingeAxis, delta);

            currentAngle = newAngle;

            yield return null;
        }

        currentAngle = endAngle;
        isMoving = false;
    }

    private float EaseInOutCubic(float t)
    {
        return t < 0.5f
            ? 4f * t * t * t
            : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }
    #endregion

    #region Audio
    private void PlayDoorSound()
    {
        if (audioSource == null) return;

        //Opening sound
        if (!isOpen && openSound != null)
        {
            audioSource.PlayOneShot(openSound);

            if (NoiseMeter.Instance != null)
                NoiseMeter.Instance.AddNoise(30f);
        }
        //Closing sound
        else if (isOpen && closeSound != null)
        {
            audioSource.PlayOneShot(closeSound);

            if (NoiseMeter.Instance != null)
                NoiseMeter.Instance.AddNoise(30f);
        }
    }
    #endregion
}