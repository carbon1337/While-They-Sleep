using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TaskUIController : MonoBehaviour
{
    [Header("Task List UI")]
    public Transform taskListParent;
    public GameObject taskRowPrefab;

    [Header("Monologue UI")]
    public TMP_Text monologueText;
    public CanvasGroup monologueCanvasGroup;
    public float typeSpeed = 0.03f;
    public float monologueDuration = 2f;
    public float fadeDuration = 0.5f;

    [Header("Monologue Audio")]
    public AudioSource monologueAudioSource;
    public AudioClip typeSound;
    public int soundInterval = 2; //Play sound every X characters
    private int charCounter = 0;

    [Header("Runtime")]
    private List<GameObject> spawnedRows = new List<GameObject>();
    private Coroutine typingCoroutine;
    private TaskData lastCompletedTask;

    private NightUI nightUI;

    #region Initialization
    void Awake()
    {
        nightUI = FindFirstObjectByType<NightUI>();
    }
    #endregion

    #region Task List
    public void DisplayTasks(List<TaskData> tasks)
    {
        ClearRows();

        //Only show the most recently completed task
        if (lastCompletedTask != null)
        {
            CreateTaskRow(lastCompletedTask);
        }

        foreach (TaskData task in tasks)
        {
            //Show active tasks normally
            if (task.taskState == TaskState.Active)
            {
                CreateTaskRow(task);
            }
        }

    }

    public void SetLastCompletedTask(TaskData task)
    {
        lastCompletedTask = task;
    }

    void CreateTaskRow(TaskData task)
    {
        //Add new row at the location of taskList using the taskRow prefab
        GameObject newRow = Instantiate(taskRowPrefab, taskListParent);

        //Add new row to runtime list
        spawnedRows.Add(newRow);

        //Trigger the setup of the task
        TaskUIRow row = newRow.GetComponent<TaskUIRow>();
        if (row != null)
        {
            row.Setup(task);
        }
    }

    //Reset the rows
    void ClearRows()
    {
        foreach (GameObject row in spawnedRows)
        {
            Destroy(row);
        }

        spawnedRows.Clear();
    }
    #endregion

    #region Monologue UI
    public void DisplayMonologueUI(TaskData task)
    {
        if (task == null || string.IsNullOrWhiteSpace(task.description))
        {
            return;
        }


        //Stop any currently running typing effect
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeMonologue(task.description));
    }

    IEnumerator TypeMonologue(string message)
    {
        //Initialize text
        monologueText.text = "";
        charCounter = 0;

        //Wait for night text to be fully faded before displaying
        if(nightUI.isTextFaded == false)
        {
            yield return new WaitForSeconds(nightUI.textDuration + nightUI.fadeDuration);
        }

        //Additional delay
        yield return new WaitForSeconds(1.25f);

        //Set visible
        if (monologueCanvasGroup != null)
        {
            monologueCanvasGroup.alpha = 1f;
        }

        //Typing effect
        for (int i = 0; i < message.Length; i++)
        {
            char currentChar = message[i];

            monologueText.text += currentChar;

            //Only play sound on non-space characters
            if (!char.IsWhiteSpace(currentChar))
            {
                charCounter++;

                if (charCounter >= soundInterval)
                {
                    PlayTypeSound();
                    charCounter = 0;
                }
            }

            yield return new WaitForSeconds(typeSpeed);
        }

        //Wait after typing
        yield return new WaitForSeconds(monologueDuration);

        //Fade out over time
        if (monologueCanvasGroup != null)
        {
            float startAlpha = monologueCanvasGroup.alpha;
            float time = 0f;

            while (time < fadeDuration)
            {
                time += Time.deltaTime;

                float t = time / fadeDuration;
                monologueCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);

                yield return null;
            }

            //Ensure it dissapears if fading fails
            monologueCanvasGroup.alpha = 0f;
        }

        typingCoroutine = null;
    }

    void PlayTypeSound()
    {
        if (monologueAudioSource != null && typeSound != null)
        {
            //Slight pitch variation so it doesn't sound repetitive
            monologueAudioSource.pitch = Random.Range(0.9f, 1.1f);
            monologueAudioSource.PlayOneShot(typeSound);
        }
    }
    #endregion
}