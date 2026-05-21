using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioLowPassFilter))]
public class SimpleAudioOcclusion : MonoBehaviour
{
    #region References

    [Header("References")]
    public Transform listener;

    #endregion

    #region Occlusion Settings

    [Header("Occlusion Settings")]
    public LayerMask occlusionLayers;

    public float normalVolume = 0.35f;
    public float occludedVolume = 0.08f;

    public float normalCutoff = 22000f;
    public float occludedCutoff = 900f;

    public float transitionSpeed = 8f;

    #endregion

    #region Private Variables

    private AudioSource audioSource;
    private AudioLowPassFilter lowPassFilter;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        lowPassFilter = GetComponent<AudioLowPassFilter>();

        if (listener == null && Camera.main != null)
        {
            listener = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (listener == null)
            return;

        UpdateOcclusion();
    }

    #endregion

    #region Occlusion Logic

    private void UpdateOcclusion()
    {
        Vector3 directionToListener = listener.position - transform.position;
        float distanceToListener = directionToListener.magnitude;

        bool isOccluded = Physics.Raycast(
            transform.position,
            directionToListener.normalized,
            distanceToListener,
            occlusionLayers
        );

        float targetVolume = isOccluded ? occludedVolume : normalVolume;
        float targetCutoff = isOccluded ? occludedCutoff : normalCutoff;

        audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, Time.deltaTime * transitionSpeed);
        lowPassFilter.cutoffFrequency = Mathf.Lerp(lowPassFilter.cutoffFrequency, targetCutoff, Time.deltaTime * transitionSpeed);
    }

    #endregion
}