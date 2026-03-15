/* 

Basic FPS player controller handling movement, camera look,
crouching, footsteps, and head bob.

*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(AudioSource))]
public class FPSController : MonoBehaviour
{
    //Input system control scheme and individual actions
    private PlayerInput playerInput;
    private InputAction lookAction;
    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction crouchAction;

    private CharacterController characterController;

    [Header("Camera Settings")]
    private Camera playerCam;
    public float cameraYOffset = 1.5f;
    public float cameraZOffset = 0.8f;

    [Range(0f, 0.5f)] public float lookSens = 1f;
    public float xLookClamp = 75f;

    private float rotationX = 0f;
    public bool camIsFrozen = false;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    private float currentSpeed;
    public float gravity = -9.81f;

    private Vector3 velocity;

    [Header("Crouch Settings")]
    public bool crouchToggle = false;
    public float standHeight = 1.8f;
    public float crouchHeight = 1.1f;
    public float crouchSpeed = 3.5f;
    public float crouchCamDrop = 0.35f;
    public float crouchLerpSpeed = 12f;

    private bool isCrouched = false;

    [Header("Head Bob Settings")]
    public bool enableHeadBob = true;
    public float bobFrequency = 1.2f;
    public float bobAmplitude = 0.05f;
    public float bobSmoothing = 10f;

    private float bobTimer = 0f;
    private Vector3 camStandLocalPos;
    private float currentCamCrouchOffset;

    [Header("Footsteps Audio")]
    private List<AudioClip> footstepSounds = new List<AudioClip>();
    private AudioSource actionAudioSource;
    public float timeBetweenFootsteps = 0.6f;
    public float timeBetweenFootstepsRunning = 0.35f;
    public float timeBetweenFootstepsCrouch = 0.8f;

    [Header("Footstep Pitch Variation")]
    public float footstepPitchMin = 0.95f;
    public float footstepPitchMax = 1.05f;

    [Header("Footstep Volume")]
    [Range(0f, 1f)] public float runVolume = 1f;
    [Range(0f, 1f)] public float walkVolume = 0.6f;
    [Range(0f, 1f)] public float crouchVolume = 0.2f;

    private float timeSinceLastFootstep = 0f;
    private FootstepSwapper swapper;

    [Header("Noise Settings")]
    public float walkNoiseAmount = 1f;
    public float runNoiseAmount = 6f;

    private bool isFrozen = false;

    #region Initialization
    private void Awake()
    {
        playerCam = Camera.main;
        if (playerCam != null)
        {
            //Attach camera to player and position it using the configured offsets
            playerCam.transform.SetParent(transform);
            playerCam.transform.localPosition = new Vector3(0f, cameraYOffset, cameraZOffset);

            //Store camera position after setup (used for crouch offset)
            camStandLocalPos = playerCam.transform.localPosition;
            currentCamCrouchOffset = 0f;
        }

        //Lock and hide cursor (standard for FPS controls)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //Initialize ground layer
        swapper?.CheckLayers();

        swapper = GetComponent<FootstepSwapper>();
        
        //Initialize input, audio, and visual components
        playerInput = GetComponent<PlayerInput>();
        characterController = GetComponent<CharacterController>();
        actionAudioSource = GetComponent<AudioSource>();

        lookAction = playerInput.actions["Look"];
        moveAction = playerInput.actions["Move"];
        sprintAction = playerInput.actions["Sprint"];
        crouchAction = playerInput.actions["Crouch"];

        characterController.height = standHeight;
        characterController.center = new Vector3(0f, standHeight / 2f, 0f);

    }
    #endregion

    #region Update Loop
    private void Update()
    {
        if (isFrozen) return;

        //Get movement input and sprint state for movement-related functions
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        bool sprintHeld = sprintAction != null && sprintAction.IsPressed();

        HandleLook();
        HandleCrouch();
        HandleMovement(moveInput, sprintHeld);
        HandleFootsteps(moveInput, sprintHeld);
        HandleHeadBob(moveInput, sprintHeld);
    }
    #endregion

    #region Camera
    //Used for freezing cam externally
    public void SetLookState(bool state)
    {
        camIsFrozen = state;
    }

    void HandleLook()
    {
        if (playerCam == null || camIsFrozen) return;

        //Get mouse/gamepad look input
        Vector2 look = lookAction.ReadValue<Vector2>();

        //Apply sensitivity scaling
        float mouseX = look.x * lookSens;
        float mouseY = look.y * lookSens;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -xLookClamp, xLookClamp);

        playerCam.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleHeadBob(Vector2 moveInput, bool sprintHeld)
    {
        if (!enableHeadBob || playerCam == null) return;

        //Determine how strong the movement input is (0–1 range)
        float inputMag = Mathf.Clamp01(moveInput.magnitude);

        //Head bob only occurs while moving on the ground
        bool isMoving = inputMag > 0.1f && characterController.isGrounded;

        float bobOffsetY = 0f;

        if (isMoving)
        {
            //Freq controls bob speed, amp controls bob intensity
            float freq = bobFrequency;
            float amp = bobAmplitude;

            //Adjust bob based on current movement state
            if (isCrouched) { freq *= 0.9f; amp *= 0.6f; }
            else if (sprintHeld) { freq *= 2f; amp *= 1.2f; }

            //Advance the sine wave timer
            bobTimer += Time.deltaTime * freq * (0.5f + inputMag);

            //Generate vertical camera motion using a sine wave
            bobOffsetY = Mathf.Sin(bobTimer * Mathf.PI * 2f) * amp;
        }
        else
        {
            //Reset timer when player stops moving
            bobTimer = 0f;
        }

        //Calculate desired camera position
        Vector3 targetLocalPos = camStandLocalPos + new Vector3(0f, currentCamCrouchOffset + bobOffsetY, 0f);

        //Smoothly interpolate camera toward the target position
        playerCam.transform.localPosition = Vector3.Lerp(
            playerCam.transform.localPosition,
            targetLocalPos,
            bobSmoothing * Time.deltaTime
        );
    }
    #endregion

    #region Movement
    void HandleMovement(Vector2 input, bool sprintHeld)
    {
        //Convert input into world-space movement
        Vector3 move = transform.right * input.x + transform.forward * input.y;

        //Only update movement speed while grounded (prevents adding momentum mid-air)
        if (characterController.isGrounded)
        {
            if (isCrouched)
                currentSpeed = crouchSpeed;
            else if (sprintHeld)
                currentSpeed = sprintSpeed;
            else
                currentSpeed = moveSpeed;
        }

        //Move player using input and previously assigned speed
        characterController.Move(move * currentSpeed * Time.deltaTime);

        //Apply slight downward force to keep the player grounded
        if (characterController.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    void HandleCrouch()
    {
        if (crouchAction == null) return;

        //Determine whether the player intends to crouch
        bool desiredCrouch;

        if (!crouchToggle)
        {
            //Hold-to-crouch mode
            desiredCrouch = crouchAction.IsPressed();
        }
        else
        {
            //Toggle crouch on button press
            if (crouchAction.WasPressedThisFrame())
            {
                isCrouched = !isCrouched;
            }

            desiredCrouch = isCrouched;
        }

        isCrouched = desiredCrouch;

        //Determine target collider height
        float targetHeight = isCrouched ? crouchHeight : standHeight;

        //Current height used as the starting point for the transition
        float prevHeight = characterController.height;

        //Smoothly transition between crouch and stand
        float newHeight = Mathf.Lerp(prevHeight, targetHeight, crouchLerpSpeed * Time.deltaTime);

        //Apply new height and reposition controller center so collider remains grounded
        characterController.height = newHeight;
        characterController.center = new Vector3(0f, newHeight / 2f, 0f);

        //Offset player to keep feet planted when collider height changes
        float heightDelta = newHeight - prevHeight;
        if (Mathf.Abs(heightDelta) > 0.0001f)
            transform.position += Vector3.up * (heightDelta / 2f);

        //Smoothly move the camera up or down based on crouch state
        float targetOffset = isCrouched ? -crouchCamDrop : 0f;
        currentCamCrouchOffset = Mathf.Lerp(currentCamCrouchOffset, targetOffset, crouchLerpSpeed * Time.deltaTime);
    }

    #endregion

    #region Audio
    void HandleFootsteps(Vector2 moveInput, bool sprintHeld)
    {
        //Check terrain layer under the player to swap correct footstep sounds
        swapper?.CheckLayers();

        //Prevent footsteps from playing while in the air
        if (!characterController.isGrounded) return;

        float inputMag = moveInput.magnitude;

        //Ignore very small movement input
        if (inputMag < 0.1f) return;

        //Ensure there are valid footstep sounds available
        if (footstepSounds == null || footstepSounds.Count == 0) return;

        //Determine if the player is running (cannot run while crouched)
        bool isRunning = sprintHeld && !isCrouched;

        //Select time interval between footsteps depending on movement state
        float interval =
            isCrouched ? timeBetweenFootstepsCrouch :
            isRunning ? timeBetweenFootstepsRunning :
            timeBetweenFootsteps;

        //Only play a footstep if enough time has passed since the last one
        if (Time.time - timeSinceLastFootstep > interval)
        {
            //Randomly select footstep clip
            int n = Random.Range(0, footstepSounds.Count);

            //Determine volume based on movement state
            float volume =
                isCrouched ? crouchVolume :
                isRunning ? runVolume :
                walkVolume;

            //Temporarily apply pitch variation to avoid repetitive sounds
            float oldPitch = actionAudioSource.pitch;
            actionAudioSource.pitch = Random.Range(footstepPitchMin, footstepPitchMax);

            //Play the selected footstep sound
            actionAudioSource.PlayOneShot(footstepSounds[n], volume);

            //Only walking and running should raise the noise meter.
            //Crouch footsteps stay quiet and add no gameplay noise.
            if (!isCrouched && NoiseMeter.Instance != null)
            {
                NoiseMeter.Instance.AddNoise(isRunning ? runNoiseAmount : walkNoiseAmount);
            }

            //Restore original pitch and record the footstep time
            actionAudioSource.pitch = oldPitch;
            timeSinceLastFootstep = Time.time;
        }
    }

    public void SwapFootsteps(FootstepCollection collection)
    {
        if (collection == null) return;

        //Clear the current list of footstep sounds
        footstepSounds.Clear();

        //Ensure the collection contains valid sounds
        if (collection.footstepSounds == null) return;

        //Copy sounds from the terrain collection into the active list
        for (int i = 0; i < collection.footstepSounds.Count; i++)
        {
            footstepSounds.Add(collection.footstepSounds[i]);
        }
    }

    #endregion

    #region Utility
    public void SetFrozen(bool frozen)
    {
        //Used to externally freeze movement
        isFrozen = frozen;
    }
    #endregion
}