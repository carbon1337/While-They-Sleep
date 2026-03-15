/*

Handles player interaction by raycasting forward from the camera,
detecting interactable objects, showing a UI prompt, and calling
their Interact() function when the interact input is pressed.

*/
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

interface IInteractable
{
    public void Interact();
    string GetPromptText();
}

[RequireComponent(typeof(PlayerInput))]
public class Interactor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public Transform interactionSource;
    public float interactRange = 3f;

    [Header("UI")]
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private TMP_Text promptText;

    //Input system references
    private PlayerInput playerInput;
    private InputAction interactAction;

    //Currently detected interactable object
    private IInteractable currentInteractable;

    #region Initialization
    private void Awake()
    {
        //Get required input component
        playerInput = GetComponent<PlayerInput>();

        //Use main camera as the interaction source
        if (Camera.main != null)
        {
            interactionSource = Camera.main.transform;
        }

        //Get interact input action
        interactAction = playerInput.actions["Interact"];
    }
    #endregion

    #region Update Loop
    private void Update()
    {
        HandleRaycast();
        HandleInteractionInput();
    }
    #endregion

    #region Interaction
    private void HandleInteractionInput()
    {
        //Interact with the currently targeted object when the button is pressed
        if (interactAction.WasPressedThisFrame() && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    private void HandleRaycast()
    {
        if (interactionSource == null)
            return;

        //Raycast forward from the interaction source
        Ray interactionRay = new Ray(interactionSource.position, interactionSource.forward);

        if (Physics.Raycast(interactionRay, out RaycastHit hitInfo, interactRange))
        {
            //Check if the hit object has an interactable component
            if (hitInfo.collider.TryGetComponent(out IInteractable interactObject))
            {
                currentInteractable = interactObject;
                ShowPrompt(interactObject.GetPromptText());
                return;
            }
        }

        ClearCurrentInteractable();
    }
    #endregion

    #region UI
    private void ShowPrompt(string text)
    {
        //Enable the prompt if needed
        if (interactPrompt != null && !interactPrompt.activeSelf)
        {
            interactPrompt.SetActive(true);
        }

        //Update prompt text
        if (promptText != null)
        {
            promptText.text = text;
        }
    }

    private void ClearCurrentInteractable()
    {
        currentInteractable = null;

        //Hide prompt when no interactable is detected
        if (interactPrompt != null && interactPrompt.activeSelf)
        {
            interactPrompt.SetActive(false);
        }
    }
    #endregion
}