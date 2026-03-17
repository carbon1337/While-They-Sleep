using UnityEngine;
using UnityEngine.InputSystem;

public class GameOverController : MonoBehaviour
{
    public static GameOverController Instance;

    [Header("Player")]
    [SerializeField] private FPSController fpsController;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private CharacterController characterController;

    [Header("Audio to stop (assign both ambients here)")]
    [SerializeField] private AudioSource[] audioToStop;

    private bool gameOver;

    void Awake()
    {
        //Ensure only one GameOverController exists
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }

        Instance = this;

        // Auto-find if you didn’t assign in Inspector
        if (playerInput == null) playerInput = FindFirstObjectByType<PlayerInput>();
        if (fpsController == null) fpsController = FindFirstObjectByType<FPSController>();
        if (characterController == null) characterController = FindFirstObjectByType<CharacterController>();
    }

    public void TriggerLose()
    {
        //player movement freezes
        if (fpsController != null)
        {
            fpsController.SetFrozen(true);
            fpsController.SetLookState(false);
        }

        if (playerInput != null) playerInput.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        JumpscareManager.Instance.TriggerJumpscare();
    }
}