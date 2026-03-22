using UnityEngine;

public class TaskReporter : MonoBehaviour
{
    [Header("Task Report Settings")]
    public TaskType reportType;
    public string targetID;
    public int amount = 1;
    public bool reportOnlyOnce = true;

    private bool hasReported = false;

    public void ReportTask()
    {
        //Ensures tasks only get reported once
        if (reportOnlyOnce && hasReported)
        {
            return;
        }

        ////Reports task status to taskmanager instance
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.ReportTaskEvent(reportType, targetID, amount);
            hasReported = true;
        }
    }
}