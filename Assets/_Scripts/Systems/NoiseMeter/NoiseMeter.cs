/*

Tracks how much noise the player is making over time.
Noise can be added by gameplay events, slowly decays after a delay,
and triggers a loss if it stays near maximum for too long.

Also handles playing a scream sound and notifying other systems
when the lose condition is reached.

*/
using UnityEngine;

public class NoiseMeter : MonoBehaviour
{
    public static NoiseMeter Instance { get; private set; }

    [Header("Noise Settings")]
    [SerializeField] private float maxNoise = 100f;
    [SerializeField] private float riseSpeed = 6f;
    [SerializeField] private float decaySpeed = 25f;
    [SerializeField] private float decayDelay = 3f;

    [Header("Lose Condition")]
    [SerializeField] private float nearMaxThreshold = 0.98f;
    [SerializeField] private float timeAtMaxToLose = 2f;

    [Header("Lose Scream")]
    [SerializeField] private AudioClip loseScreamClip;
    [SerializeField] private float screamVolume = 1f;

    //Audio source used for the lose scream
    private AudioSource screamSource;

    //Target noise is the actual gameplay value being pushed up and down
    private float targetNoise = 0f;

    //Displayed noise is the smoothed value used by UI and lose checks
    private float displayedNoise = 0f;

    //Tracks time since the last noise event so decay can begin after a delay
    private float timeSinceLastNoise = 999f;

    //Tracks how long the displayed noise has stayed near maximum
    private float timeNearMax = 0f;

    //Prevents the lose condition from firing multiple times
    private bool hasLost = false;

    public float NormalizedNoise => maxNoise > 0f ? Mathf.Clamp01(displayedNoise / maxNoise) : 0f;
    public bool IsSilent => displayedNoise <= 0.5f;
    public bool HasLost => hasLost;

    public System.Action OnLose;

    #region Initialization
    private void Awake()
    {
        //Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SetupScreamAudioSource();
    }

    private void SetupScreamAudioSource()
    {
        if (loseScreamClip == null)
            return;

        screamSource = GetComponent<AudioSource>();

        if (screamSource == null)
        {
            screamSource = gameObject.AddComponent<AudioSource>();
        }

        screamSource.playOnAwake = false;
        screamSource.loop = false;
        screamSource.spatialBlend = 0f;
        screamSource.volume = screamVolume;
        screamSource.clip = loseScreamClip;
    }
    #endregion

    #region Update Loop
    private void Update()
    {
        if (hasLost)
            return;

        UpdateNoiseDecay();
        UpdateDisplayedNoise();
        CheckLoseCondition();
    }

    private void UpdateNoiseDecay()
    {
        timeSinceLastNoise += Time.deltaTime;

        //Only begin reducing the target noise after the configured delay
        if (timeSinceLastNoise >= decayDelay)
        {
            targetNoise = Mathf.Max(0f, targetNoise - decaySpeed * Time.deltaTime);
        }
    }

    private void UpdateDisplayedNoise()
    {
        if (displayedNoise < targetNoise)
        {
            //Fast, responsive increase
            displayedNoise = Mathf.Lerp(displayedNoise, targetNoise, riseSpeed * Time.deltaTime);
        }
        else
        {
            //More controlled drop
            displayedNoise = Mathf.MoveTowards(displayedNoise, targetNoise, decaySpeed * Time.deltaTime);
        }
    }

    private void CheckLoseCondition()
    {
        if (NormalizedNoise >= nearMaxThreshold)
        {
            timeNearMax += Time.deltaTime;

            if (timeNearMax >= timeAtMaxToLose)
            {
                TriggerLose();
            }
        }
        else
        {
            timeNearMax = 0f;
        }
    }
    #endregion

    #region Noise Control
    public void AddNoise(float amount)
    {
        if (hasLost) return;
        if (amount <= 0f) return;

        targetNoise = Mathf.Clamp(targetNoise + amount, 0f, maxNoise);
        timeSinceLastNoise = 0f;
    }

    public void SetNoise(float amount)
    {
        if (hasLost) return;

        targetNoise = Mathf.Clamp(amount, 0f, maxNoise);
        timeSinceLastNoise = 0f;
    }

    public void ResetNoiseMeter()
    {
        targetNoise = 0f;
        displayedNoise = 0f;
        timeSinceLastNoise = 999f;
        timeNearMax = 0f;
        hasLost = false;
    }
    #endregion

    #region Lose Condition
    private void TriggerLose()
    {
        if (hasLost)
            return;

        hasLost = true;
        timeNearMax = 0f;

        Debug.Log("LOSE: Too noisy for too long.");

        PlayLoseScream();

        //Stop ambient tension systems
        FindFirstObjectByType<NoiseMeterAmbientAudio>()?.SetGameOver(true);

        //Notify game over systems
        GameOverController.Instance?.TriggerLose();
        OnLose?.Invoke();
    }

    private void PlayLoseScream()
    {
        if (screamSource == null || loseScreamClip == null)
            return;

        screamSource.volume = screamVolume;
        screamSource.Play();
    }
    #endregion
}