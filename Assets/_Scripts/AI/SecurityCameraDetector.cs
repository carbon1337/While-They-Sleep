/*

Security camera spotlight that patrols back and forth, detects the player
within its spotlight cone, builds detection over time, triggers an alarm,
adds noise to the noise meter, and displays a world-space detection meter.

*/
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class SecurityCameraSpotlight : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Light spotLight;
    [SerializeField] private LayerMask obstructionMask;

    [Header("Pan Settings")]
    [SerializeField] private float panAngle = 60f;
    [SerializeField] private float panSpeed = 45f;
    [SerializeField] private float endPause = 0.15f;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRisePerSec = 4f;
    [SerializeField] private float detectionFallPerSec = 1.5f;
    [SerializeField] private float alarmDuration = 4f;
    [SerializeField] private float lockOnTurnSpeed = 250f;

    [Header("Noise Settings")]
    [SerializeField] private float alarmNoisePerSecond = 35f;

    [Header("Light Visuals")]
    [SerializeField] private Color patrolColor = Color.red;
    [SerializeField] private Color detectingColor = Color.white;
    [SerializeField] private float patrolIntensity = 6f;
    [SerializeField] private float detectingIntensity = 10f;

    [Header("Alarm Flicker")]
    [SerializeField] private float flickerMinIntensity = 2f;
    [SerializeField] private float flickerMaxIntensity = 14f;
    [SerializeField] private float flickerSpeed = 22f;

    [Header("Audio")]
    [SerializeField] private AudioClip detectingClip;
    [SerializeField] private float detectingMaxVolume = 0.7f;
    [SerializeField] private AudioClip alarmClip;
    [SerializeField] private float alarmVolume = 1f;

    [Header("Detection Meter UI")]
    [SerializeField] private Vector3 meterOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private Vector2 meterSize = new Vector2(1.2f, 0.15f);

    //Audio sources
    private AudioSource alarmSource;
    private AudioSource detectingSource;

    //Detection state
    private float detection = 0f;
    private bool alarmOn = false;
    private float alarmTimer = 0f;

    //Patrol state
    private float startYaw;
    private int panDir = 1;
    private float pauseTimer = 0f;

    //World-space UI
    private Transform meterRoot;
    private Image meterFill;

    #region Initialization
    private void Awake()
    {
        SetupAlarmAudio();
        SetupDetectingAudio();
        FindPlayerReference();
        SetupSpotlightIfMissing();
        BuildDetectionMeter();

        startYaw = transform.localEulerAngles.y;
        ApplyPatrolLight();
    }

    private void SetupAlarmAudio()
    {
        alarmSource = GetComponent<AudioSource>();
        alarmSource.loop = true;
        alarmSource.playOnAwake = false;
        alarmSource.clip = alarmClip;
        alarmSource.volume = alarmVolume;
        alarmSource.spatialBlend = 1f;
    }

    private void SetupDetectingAudio()
    {
        if (detectingClip == null)
            return;

        detectingSource = gameObject.AddComponent<AudioSource>();
        detectingSource.clip = detectingClip;
        detectingSource.loop = true;
        detectingSource.playOnAwake = false;
        detectingSource.spatialBlend = 1f;
        detectingSource.volume = 0f;
    }

    private void FindPlayerReference()
    {
        if (player != null)
            return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void SetupSpotlightIfMissing()
    {
        if (spotLight == null)
        {
            SetupSpotlight();
        }
    }
    #endregion

    #region Update Loop
    private void Update()
    {
        if (alarmOn)
        {
            UpdateAlarmState();
            return;
        }

        UpdatePatrolState();
        UpdateDetectionState();
        UpdateDetectingAudio();
        ApplyDetectingLight();
        UpdateMeter(detection);

        if (detection >= 1f)
        {
            StartAlarm();
        }
    }

    private void UpdateAlarmState()
    {
        alarmTimer += Time.deltaTime;

        LockOnPlayer();
        AlarmFlicker();
        AddAlarmNoise();
        UpdateMeter(1f);

        if (alarmTimer >= alarmDuration)
        {
            ResetAlarm();
        }
    }

    private void UpdatePatrolState()
    {
        Pan();
    }

    private void UpdateDetectionState()
    {
        bool seesPlayer = CanSeePlayer();

        if (seesPlayer)
        {
            detection = Mathf.Clamp01(detection + detectionRisePerSec * Time.deltaTime);
        }
        else
        {
            detection = Mathf.Clamp01(detection - detectionFallPerSec * Time.deltaTime);
        }
    }
    #endregion

    #region Detection
    private bool CanSeePlayer()
    {
        if (player == null || spotLight == null)
            return false;

        Vector3 origin = spotLight.transform.position;
        Vector3 toPlayer = player.position - origin;

        float distanceToPlayer = toPlayer.magnitude;
        if (distanceToPlayer > spotLight.range)
            return false;

        Vector3 directionToPlayer = toPlayer.normalized;

        float halfAngle = spotLight.spotAngle * 0.5f;
        float angleToPlayer = Vector3.Angle(spotLight.transform.forward, directionToPlayer);
        if (angleToPlayer > halfAngle)
            return false;

        if (Physics.Raycast(origin, directionToPlayer, distanceToPlayer, obstructionMask, QueryTriggerInteraction.Ignore))
            return false;

        return true;
    }
    #endregion

    #region Patrol
    private void Pan()
    {
        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
            return;
        }

        float halfPanAngle = panAngle * 0.5f;
        float yawDelta = panSpeed * panDir * Time.deltaTime;

        transform.localRotation *= Quaternion.Euler(0f, yawDelta, 0f);

        float yawOffset = Mathf.DeltaAngle(startYaw, transform.localEulerAngles.y);

        if (yawOffset > halfPanAngle)
        {
            panDir = -1;
            pauseTimer = endPause;
        }
        else if (yawOffset < -halfPanAngle)
        {
            panDir = 1;
            pauseTimer = endPause;
        }
    }

    private void LockOnPlayer()
    {
        if (player == null)
            return;

        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0f;

        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, lockOnTurnSpeed * Time.deltaTime);
    }
    #endregion

    #region Alarm
    private void StartAlarm()
    {
        alarmOn = true;
        alarmTimer = 0f;

        if (alarmClip != null)
        {
            alarmSource.Play();
        }

        if (detectingSource != null && detectingSource.isPlaying)
        {
            detectingSource.Stop();
        }

        spotLight.color = detectingColor;
        spotLight.intensity = detectingIntensity;
    }

    private void ResetAlarm()
    {
        alarmOn = false;
        alarmTimer = 0f;
        detection = 0f;

        if (alarmSource.isPlaying)
        {
            alarmSource.Stop();
        }

        ApplyPatrolLight();
    }

    private void AddAlarmNoise()
    {
        if (NoiseMeter.Instance != null)
        {
            NoiseMeter.Instance.AddNoise(alarmNoisePerSecond * Time.deltaTime);
        }
    }

    private void AlarmFlicker()
    {
        float flickerNoise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0.123f);
        spotLight.intensity = Mathf.Lerp(flickerMinIntensity, flickerMaxIntensity, flickerNoise);
        spotLight.color = detectingColor;
    }
    #endregion

    #region Audio
    private void UpdateDetectingAudio()
    {
        if (detectingSource == null)
            return;

        if (detection > 0.01f)
        {
            if (!detectingSource.isPlaying)
            {
                detectingSource.Play();
            }

            detectingSource.volume = detection * detectingMaxVolume;
        }
        else
        {
            if (detectingSource.isPlaying)
            {
                detectingSource.Stop();
            }
        }
    }
    #endregion

    #region Light Visuals
    private void ApplyPatrolLight()
    {
        if (spotLight == null)
            return;

        spotLight.color = patrolColor;
        spotLight.intensity = patrolIntensity;
    }

    private void ApplyDetectingLight()
    {
        if (spotLight == null)
            return;

        float detectionAmount = detection;
        spotLight.color = Color.Lerp(patrolColor, detectingColor, detectionAmount);
        spotLight.intensity = Mathf.Lerp(patrolIntensity, detectingIntensity, detectionAmount);
    }

    private void SetupSpotlight()
    {
        GameObject lightObject = new GameObject("SecuritySpotlight");
        lightObject.transform.SetParent(transform);
        lightObject.transform.localPosition = Vector3.zero;

        spotLight = lightObject.AddComponent<Light>();
        spotLight.type = LightType.Spot;
        spotLight.range = 15f;
        spotLight.spotAngle = 60f;
        spotLight.color = patrolColor;
        spotLight.intensity = patrolIntensity;
        spotLight.shadows = LightShadows.Soft;
    }
    #endregion

    #region Detection Meter UI
    private void BuildDetectionMeter()
    {
        GameObject root = new GameObject("DetectionMeter");
        root.transform.SetParent(transform);
        root.transform.localPosition = meterOffset;

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        root.AddComponent<CanvasScaler>();
        root.AddComponent<GraphicRaycaster>();

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = meterSize;

        GameObject background = new GameObject("BG");
        background.transform.SetParent(root.transform, false);

        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.color = new Color(0f, 0f, 0f, 0.6f);

        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(background.transform, false);

        meterFill = fill.AddComponent<Image>();
        meterFill.color = Color.red;
        meterFill.type = Image.Type.Filled;
        meterFill.fillMethod = Image.FillMethod.Horizontal;

        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        meterRoot = root.transform;
    }

    private void UpdateMeter(float value)
    {
        if (meterFill != null)
        {
            meterFill.fillAmount = value;
        }

        if (meterRoot != null && Camera.main != null)
        {
            meterRoot.LookAt(Camera.main.transform);
            meterRoot.Rotate(0f, 180f, 0f);
        }
    }
    #endregion
}