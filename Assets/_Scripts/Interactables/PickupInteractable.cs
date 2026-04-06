/*

Pickup interactable that lets the player pick up, hold, rotate,
drop, and throw physics objects. Also handles collision audio,
noise meter impact, and optional goblin voice lines on pickup.

*/
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
public class PickupInteractable : MonoBehaviour, IInteractable
{
    //Core references
    private Rigidbody rb;
    private PlayerInput playerInput;
    private FPSController fpsController;

    //Input actions
    private InputAction rotateAction;
    private InputAction lookAction;
    private InputAction throwAction;

    //Hold state
    private bool isHeld = false;
    private Transform holdPoint;
    private Quaternion heldRotation;

    [Header("Hold Settings")]
    [SerializeField] private float holdDistance = 2f;
    [SerializeField] private float followSpeed = 15f;

    [Header("Rotate Settings")]
    [SerializeField] private float rotateSensitivity = 0.2f;

    [Header("Throw Settings")]
    [SerializeField] private float throwForce = 12f;
    [SerializeField] private float throwUpwardForce = 1.5f;
    [SerializeField] private float throwSpinTorque = 6f;

    [Header("Item Sounds")]
    [SerializeField] private AudioSource itemAudioSource;
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private AudioClip[] collisionClips;

    [SerializeField] private float collisionMinImpact = 1.5f;
    [SerializeField] private float collisionCooldown = 0.2f;
    [SerializeField] private float collisionPitchJitter = 0.05f;

    private float nextCollisionSoundTime = 0f;

    [Header("Noise Meter Settings")]
    [Tooltip("Heavier or denser items generate more noise from the same impact. Example: pillow 0.6, book 1.0, statue 1.6")]
    [SerializeField] private float itemDensity = 1.0f;

    [SerializeField] private float minNoiseOnHit = 2f;
    [SerializeField] private float maxNoiseOnHit = 12f;

    [Tooltip("Impact magnitude that maps to max noise before the curve.")]
    [SerializeField] private float impactForMaxNoise = 8f;

    [Tooltip("Extra multiplier when the item is being held.")]
    [SerializeField] private float heldImpactNoiseMultiplier = 1.25f;

    [Tooltip("Shapes how noise ramps with impact. X = normalized impact, Y = noise strength.")]
    [SerializeField] private AnimationCurve impactNoiseCurve =
        new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.4f, 0.15f),
            new Keyframe(0.75f, 0.65f),
            new Keyframe(1f, 1f)
        );

    #region Initialization
    private void Awake()
    {
        //Get required physics component
        rb = GetComponent<Rigidbody>();

        //Find player systems used for input and look control
        playerInput = FindFirstObjectByType<PlayerInput>();
        fpsController = FindFirstObjectByType<FPSController>();

        //Cache input actions used while holding an object
        if (playerInput != null)
        {
            rotateAction = playerInput.actions["RotateHeld"];
            lookAction = playerInput.actions["Look"];
            throwAction = playerInput.actions["Throw"];
        }

        //Use assigned item audio source, otherwise fall back to local AudioSource
        if (itemAudioSource == null)
        {
            itemAudioSource = GetComponent<AudioSource>();

            if (itemAudioSource == null)
            {
                itemAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        //Configure item audio for 3D playback
        itemAudioSource.playOnAwake = false;
        itemAudioSource.spatialBlend = 1f;
        itemAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
    }
    #endregion

    #region Enable Disable
    private void OnEnable()
    {
        if (throwAction != null)
        {
            throwAction.performed += OnThrowPerformed;
        }
    }

    private void OnDisable()
    {
        if (throwAction != null)
        {
            throwAction.performed -= OnThrowPerformed;
        }
    }
    #endregion

    #region Update Loop
    private void Update()
    {
        //Only run hold behavior while the object is actively being held
        if (!isHeld || holdPoint == null)
            return;

        HandleHeldMovement();
        HandleHeldRotation();
    }
    #endregion

    #region Interaction
    public void Interact()
    {
        if (!isHeld)
            PickUp();
        else
            Drop();
    }

    public string GetPromptText()
    {
        return isHeld ? "E to Drop" : "E to Pick Up";
    }
    #endregion

    #region Held Object Logic
    private void HandleHeldMovement()
    {
        //Move the rigidbody toward the target hold position in front of the camera
        Vector3 targetPosition = holdPoint.position + holdPoint.forward * holdDistance;
        Vector3 moveDirection = targetPosition - transform.position;

        rb.linearVelocity = moveDirection * followSpeed;
    }

    private void HandleHeldRotation()
    {
        if (rotateAction == null || lookAction == null)
            return;

        bool isRotating = rotateAction.IsPressed();

        //Only freeze camera look while the rotate input is actively being held
        if (fpsController != null)
        {
            fpsController.SetLookState(isRotating);
        }

        if (!isRotating)
        {
            rb.MoveRotation(heldRotation);
            return;
        }

        Vector2 mouseDelta = lookAction.ReadValue<Vector2>();
        float mouseX = mouseDelta.x * rotateSensitivity;
        float mouseY = mouseDelta.y * rotateSensitivity;

        Quaternion yawRotation = Quaternion.AngleAxis(mouseX, holdPoint.up);
        Quaternion pitchRotation = Quaternion.AngleAxis(-mouseY, holdPoint.right);

        heldRotation = yawRotation * pitchRotation * heldRotation;
        rb.MoveRotation(heldRotation);
    }

    private void PickUp()
    {
        isHeld = true;
        holdPoint = Camera.main != null ? Camera.main.transform : null;

        //Disable gravity and heavily damp movement while held
        rb.useGravity = false;
        rb.linearDamping = 10f;
        rb.angularDamping = 10f;

        //Clear any current motion before holding
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        //Store current object rotation as the held base rotation
        heldRotation = rb.rotation;

        //Reports that the task was completed if possible
        TaskReporter taskReporter = GetComponent<TaskReporter>();
        if (taskReporter != null)
        {
            taskReporter.ReportTask();
        }

        PlayPickupSound();

        if(this.gameObject.tag == "Food")
        {
            Destroy(this.gameObject, 0.2f);
        }
    }

    private void Drop()
    {
        isHeld = false;
        holdPoint = null;

        //Restore regular physics behavior
        rb.useGravity = true;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;

        //Re-enable camera look in case object rotation was active
        fpsController?.SetLookState(false);
    }

    private void Throw()
    {
        if (holdPoint == null)
            return;

        Vector3 forward = holdPoint.forward;
        Vector3 up = holdPoint.up;

        isHeld = false;
        holdPoint = null;

        //Restore regular physics before applying throw force
        rb.useGravity = true;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;

        fpsController?.SetLookState(false);

        //Reset movement so the throw starts cleanly
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        //Apply forward and upward impulse
        Vector3 throwDirection = (forward * throwForce) + (up * throwUpwardForce);
        rb.AddForce(throwDirection, ForceMode.Impulse);

        //Apply optional spin for extra motion
        if (throwSpinTorque > 0f)
        {
            Vector3 torqueAxis = (up + (Vector3.right * 0.25f)).normalized;
            rb.AddTorque(torqueAxis * throwSpinTorque, ForceMode.Impulse);
        }

        heldRotation = rb.rotation;
    }

    private void OnThrowPerformed(InputAction.CallbackContext ctx)
    {
        if (!isHeld) return;

        Throw();
    }
    #endregion

    #region Audio
    private void PlayPickupSound()
    {
        if (pickupClip == null || itemAudioSource == null)
            return;

        itemAudioSource.pitch = 1f;
        itemAudioSource.PlayOneShot(pickupClip, 1f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collisionClips == null || collisionClips.Length == 0) return;
        if (itemAudioSource == null) return;
        if (Time.time < nextCollisionSoundTime) return;

        float impact = collision.relativeVelocity.magnitude;

        //Ignore very small impacts
        if (impact < collisionMinImpact) return;

        //Play random collision sound with slight pitch variation
        AudioClip clip = collisionClips[Random.Range(0, collisionClips.Length)];
        if (clip != null)
        {
            float pitch = 1f;

            if (collisionPitchJitter > 0f)
            {
                pitch += Random.Range(-collisionPitchJitter, collisionPitchJitter);
            }

            itemAudioSource.pitch = pitch;

            float volume01 = Mathf.Clamp01((impact - collisionMinImpact) / 6f);
            float finalVolume = Mathf.Lerp(0.25f, 1f, volume01);

            itemAudioSource.PlayOneShot(clip, finalVolume);
        }

        AddNoiseFromImpact(impact);

        nextCollisionSoundTime = Time.time + collisionCooldown;
    }
    #endregion

    #region Noise
    private void AddNoiseFromImpact(float impact)
    {
        if (NoiseMeter.Instance == null)
            return;

        //Normalize impact so minimum impact begins near 0 and max configured impact reaches 1
        float impact01 = Mathf.InverseLerp(collisionMinImpact, impactForMaxNoise, impact);

        //Use the curve to make small bumps quiet and larger hits spike harder
        float shapedNoise = Mathf.Clamp01(impactNoiseCurve.Evaluate(impact01));

        float finalNoise = Mathf.Lerp(minNoiseOnHit, maxNoiseOnHit, shapedNoise);

        //Heavier objects generate more noise from the same collision
        finalNoise *= Mathf.Max(0f, itemDensity);

        //Held impacts can optionally be a bit riskier
        if (isHeld)
        {
            finalNoise *= heldImpactNoiseMultiplier;
        }

        NoiseMeter.Instance.AddNoise(finalNoise);
    }
    #endregion
}