using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class BedInteractable : MonoBehaviour, IInteractable
{
    [Header("Prompt Text")]
    [SerializeField] private string promptBase = "E to Sleep";

    #region Interaction
    public string GetPromptText()
    {
        //Show current hoard amount in the interaction prompt
        return promptBase;
    }

    public void Interact()
    {
        TaskReporter taskReporter = GetComponent<TaskReporter>();
        if (taskReporter != null)
        {
            taskReporter.ReportTask();
        }
    }
    #endregion
}