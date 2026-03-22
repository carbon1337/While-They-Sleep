using System.Collections.Generic;
using UnityEngine;

public class TaskUIController : MonoBehaviour
{
    [Header("UI References")]
    public Transform taskListParent;
    public GameObject taskRowPrefab;

    [Header("Runtime")]
    private List<GameObject> spawnedRows = new List<GameObject>();

    public void DisplayTasks(List<TaskData> tasks)
    {
        ClearRows();

        foreach (TaskData task in tasks)
        {
            //Skip displaying this task if it is not active and not completed
            if (task.taskState != TaskState.Active && task.taskState != TaskState.Completed)
                continue;

            //Add new row at the location of tasklist using the taskrow prefab
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
}