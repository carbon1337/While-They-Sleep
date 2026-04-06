/*

Handles flashlight pickup and toggling.

Player can pick up the flashlight once. After that,
it stays attached to the player and can be toggled on/off.

*/

using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightInteractable : MonoBehaviour, IInteractable
{
    [Header("References")]
    public GameObject flashlight;
    public Transform holdPoint;
    public MeshRenderer renderer;

    [Header("Runtime")]
    private bool isFlashlightOn = false;
    private bool isHeld = false;

    private PlayerInput playerInput;
    private InputAction toggleFlashlightAction;

    #region Initialization
    void Awake()
    {
        playerInput = FindFirstObjectByType<PlayerInput>();

        if (playerInput != null)
        {
            toggleFlashlightAction = playerInput.actions["ToggleFlashlight"];
        }
    }
    #endregion

    #region Update Loop
    void Update()
    {
        //Only allow toggling after flashlight has been picked up
        if (isHeld && toggleFlashlightAction != null && toggleFlashlightAction.WasPressedThisFrame())
        {
            isFlashlightOn = !isFlashlightOn;
            flashlight.SetActive(isFlashlightOn);
        }
    }
    #endregion

    #region Interaction
    public void Interact()
    {
        //Prevent re-picking up
        if (isHeld)
        {
            return;
        }

        //Report for flashlight task
        TaskReporter taskReporter = GetComponent<TaskReporter>();
        if (taskReporter != null)
        {
            taskReporter.ReportTask();
        }

        PickUpFlashlight();
    }

    public string GetPromptText()
    {
        return "E to Pick Up";
    }

    void PickUpFlashlight()
    {
        isHeld = true;

        //Disable viewmodel of flashlight
        renderer.enabled = false;

        //Attach to player hold point
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        //Optional: disable collider so it doesn't interfere
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        //Optional: disable rigidbody physics
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
    #endregion
}