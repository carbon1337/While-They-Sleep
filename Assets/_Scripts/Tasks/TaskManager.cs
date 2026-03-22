using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }

    [Header("Current Night Tasks")]
    public List<TaskData> currentTasks = new List<TaskData>();

    [Header("UI")]
    public TaskUIController taskUIController;

    #region Initialization
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        InitializeTasks();
    }
    #endregion

    #region Task Setup
    public void SetTasks(List<TaskData> tasks)
    {
        currentTasks = tasks;
        InitializeTasks();
    }

    void InitializeTasks()
    {
        foreach (TaskData task in currentTasks)
        {
            //Ensure the tasks are at 0% completion
            task.currentAmount = 0;
            task.taskState = TaskState.Locked;

            //Checks if there are prerequisite tasks in taskdata
            bool hasPrerequisites = task.prerequisiteTaskIDs != null && task.prerequisiteTaskIDs.Count > 0;

            //Only set active the not hidden and no prereq tasks
            if (!task.startHidden && !hasPrerequisites)
            {
                task.taskState = TaskState.Active;
            }
        }

        //Update the UI after initializing
        RefreshTaskUI();
    }
    #endregion

    #region Task Reporting
    public void ReportTaskEvent(TaskType eventType, string reportedTargetID, int amount = 1)
    {
        foreach (TaskData task in currentTasks)
        {
            //Cycle through taskstate, tasktype, and the target id to ensure proper the proper task gets affected
            if (task.taskState != TaskState.Active)
                continue;

            if (task.taskType != eventType)
                continue;

            if (task.targetID != reportedTargetID)
                continue;

            //Add the task amount to the proper task
            task.currentAmount += amount;

            //Checks for completion
            if (task.currentAmount >= task.requiredAmount)
            {
                CompleteTask(task.taskID);
            }
        }

        RefreshTaskUI();
    }
    #endregion

    #region Task Completion
    public void CompleteTask(string taskID)
    {
        //Assigns taskdata from referenced taskID
        TaskData task = currentTasks.Find(t => t.taskID == taskID);

        //Check to stop tasks from being completed twice
        if (task == null || task.taskState == TaskState.Completed)
        {
            return;
        }

        //Set the taskstate and required amounts to completion
        task.taskState = TaskState.Completed;
        task.currentAmount = task.requiredAmount;

        //Unlock next tasks if possible
        UnlockAvailableTasks();

        RefreshTaskUI();

        //Debug to check tasks completed
        if (AreAllRequiredTasksCompleted())
        {
            Debug.Log("All required tasks completed for this night.");
        }
    }

    //Unlock and activate the next tasks after completing previous
    void UnlockAvailableTasks()
    {
        foreach (TaskData task in currentTasks)
        {
            if (task.taskState != TaskState.Locked)
                continue;

            if (ArePrerequisitesMet(task))
            {
                task.taskState = TaskState.Active;
            }
        }
    }

    //Returns status of prerequisites for multi-stage tasks
    bool ArePrerequisitesMet(TaskData task)
    {
        //Return true if there are no prerequisite tasks
        if (task.prerequisiteTaskIDs == null || task.prerequisiteTaskIDs.Count == 0)
        {
            return true;
        }

        //Cycle through prerequisite task ids
        foreach (string prerequisiteID in task.prerequisiteTaskIDs)
        {
            //Assign the prerequisite task datas by referencing the prerequisite taskIDs in the current task list
            TaskData prerequisiteTask = currentTasks.Find(t => t.taskID == prerequisiteID);

            //Return false if the new prerequisite task data is not completed
            if (prerequisiteTask == null || prerequisiteTask.taskState != TaskState.Completed)
            {
                return false;
            }
        }

        //If not false (checked above) return true
        return true;
    }

    //Returns status of all required tasks
    public bool AreAllRequiredTasksCompleted()
    {
        foreach (TaskData task in currentTasks)
        {
            if (task.isRequired && task.taskState != TaskState.Completed)
            {
                return false;
            }
        }

        return true;
    }
    #endregion

    #region UI
    public void RefreshTaskUI()
    {
        if (taskUIController != null)
        {
            //Display all current tasks in list
            taskUIController.DisplayTasks(currentTasks);
        }
    }
    #endregion
}