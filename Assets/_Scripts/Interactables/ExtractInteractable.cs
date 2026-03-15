/*

Interactable mattress that lets the player end the run by sleeping.
Checks extraction rules, freezes the player, fades the screen out,
then loads the extract UI scene.

*/
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ExtractMattressInteractable : MonoBehaviour, IInteractable
{
    [Header("Scene")]
    [SerializeField] private string extractSceneName = "ExtractUI";

    [Header("Prompt Text")]
    [SerializeField] private string promptBase = "E to Sleep";

    [Header("Rules")]
    [SerializeField] private bool requireAtLeastOneLoot = false;

    //Prevents extraction from being triggered more than once
    private bool extracting = false;

    #region Interaction
    public string GetPromptText()
    {
        //Show current hoard amount in the interaction prompt
        int hoard = (ScoreManager.Instance != null) ? ScoreManager.Instance.Score : 0;
        return $"{promptBase} [{hoard}]";
    }

    public void Interact()
    {
        //Prevent duplicate extraction calls
        if (extracting) return;

        int hoard = (ScoreManager.Instance != null) ? ScoreManager.Instance.Score : 0;

        //Optional rule requiring the player to collect at least one item
        if (requireAtLeastOneLoot && hoard <= 0)
            return;

        extracting = true;
        StartCoroutine(ExtractRoutine());
    }
    #endregion

    #region Extraction
    private IEnumerator ExtractRoutine()
    {
        //Find and freeze the player controller so movement and camera stop
        FPSController fpsController = FindFirstObjectByType<FPSController>();
        if (fpsController != null)
        {
            fpsController.SetFrozen(true);
            fpsController.SetLookState(false);
        }

        //Disable player input entirely before switching scenes
        PlayerInput playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = false;
        }

        //Stop all audio immediately so nothing carries into the extract UI scene
        AudioListener.pause = true;
        AudioListener.volume = 0f;

        //Fade the screen out before loading the next scene
        if (ScreenFader.Instance != null)
        {
            yield return ScreenFader.Instance.FadeOut();
        }

        //Ensure time scale is normal before changing scenes
        Time.timeScale = 1f;
        SceneManager.LoadScene(extractSceneName);
    }
    #endregion
}