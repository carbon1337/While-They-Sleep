using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    private FPSController fPSController;
    private PlayerInput playerInput;
    private InputAction escapeAction;

    private bool isPaused;
    private bool isOnMenu;
    public GameObject PlayerUI;
    public GameObject pauseUI;

    #region Initialization
    void Start()
    {
        //Get controller and input for pause menu reference
        fPSController = Object.FindFirstObjectByType<FPSController>();
        playerInput = Object.FindFirstObjectByType<PlayerInput>();

        if(playerInput != null)
        {
            escapeAction = playerInput.actions["Escape"];
        }

        pauseUI?.SetActive(false);
    }
    #endregion

    #region Update Loop
    void Update()
    {
        //Track if the player is on the main menu scene
        if(SceneManager.GetActiveScene().name == "MainMenu")
        {
            isOnMenu = true;
        }
        else
        {
            isOnMenu = false;
        }

        HandlePauseMenu();
    }
    #endregion

    #region Pause Menu
    private void HandlePauseMenu()
    {
        //Only allow the player to pause if they aren't on the main menu
        if(playerInput!= null && !isOnMenu)
        {
            //Flip between pause/unpause on button press
            if(escapeAction.WasPressedThisFrame())
            {
                isPaused = !isPaused;
            }

            //Disables/enables the UI and player movement
            pauseUI?.SetActive(isPaused);
            fPSController.SetFrozen(isPaused);
            PlayerUI?.SetActive(!isPaused);
        }

        //Actually "Pauses" the game if not on menu
        if(!isOnMenu)
        {
            if(isPaused)
            {
                //Time stops and cursor appears visible
                Time.timeScale = 0f;


                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                //Time resumes and cursor disappears
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        else //Always show cursor on main menu
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    #endregion

    //functions that switch scenes reference screenfader for game feel
    #region Button Functions
    public void ResumeGame()
    {
        isPaused = !isPaused;
        Debug.Log("resuming" + isPaused);

    }

    public void PlayGame()
    {
        ScreenFader.Instance.FadeToScene("Level");
    }

    public void ReturnToMenu()
    {
        ScreenFader.Instance.FadeToScene("MainMenu");
    }

    public void QuitGame()
    {
        StartCoroutine(QuitGameRoutine());
    }

    private IEnumerator QuitGameRoutine()
    {
        yield return StartCoroutine(ScreenFader.Instance.FadeOut());
        Application.Quit();
    }
    #endregion
}