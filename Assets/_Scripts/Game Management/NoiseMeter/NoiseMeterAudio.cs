using UnityEngine;
using UnityEngine.UI;

public class NoiseMeterAmbientAudio : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider noiseSlider;
    [SerializeField] private AudioSource ambientSource;

    [Header("Volume")]
    [SerializeField] private float minVolume = 0.0f;
    [SerializeField] private float maxVolume = 0.85f;

    [Tooltip("Controls how volume ramps as noise increases (x=noise 0-1, y=volume 0-1).")]
    [SerializeField] private AnimationCurve volumeCurve =
        new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.7f, 0.15f),
            new Keyframe(0.9f, 0.55f),
            new Keyframe(1f, 1f)
        );

    [Header("Optional: Pitch Rise")]
    [SerializeField] private bool scalePitch = true;
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.10f;

    private bool isGameOver = false;

    void Awake()
    {
        if (ambientSource != null)
        {
            ambientSource.loop = true;

            // Only auto-play if we are not in game over state
            if (!isGameOver && !ambientSource.isPlaying)
                ambientSource.Play();
        }
    }

    void Update()
    {
        if (isGameOver) return;
        if (noiseSlider == null || ambientSource == null) return;
        if (!ambientSource.enabled) return;

        // Slider.normalizedValue is always 0..1 even if min/max aren't 0/1
        float t = Mathf.Clamp01(noiseSlider.normalizedValue);

        float shaped = Mathf.Clamp01(volumeCurve.Evaluate(t));
        ambientSource.volume = Mathf.Lerp(minVolume, maxVolume, shaped);

        if (scalePitch)
            ambientSource.pitch = Mathf.Lerp(minPitch, maxPitch, shaped);
    }

    // Call this when the player loses
    public void SetGameOver(bool gameOver)
    {
        isGameOver = gameOver;

        if (!isGameOver) return;

        if (ambientSource != null)
        {
            ambientSource.Stop();
            ambientSource.volume = 0f;

            // Disable so nothing else can restart it
            ambientSource.enabled = false;
        }
    }
}