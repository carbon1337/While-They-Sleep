using UnityEngine;

public class TaskZoneTrigger : MonoBehaviour
{
    public TaskReporter taskReporter;

    //Report task when player enters trigger
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (taskReporter != null)
        {
            taskReporter.ReportTask();
        }
    }
}