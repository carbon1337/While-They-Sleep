using System.Collections.Generic;
using UnityEngine;

public class NightTaskLoader : MonoBehaviour
{
    private void Start()
    {
        LoadNightOneTasks();
    }

    void LoadNightOneTasks()
    {
        List<TaskData> nightOneTasks = new List<TaskData>();

        TaskData grabFoodTask = new TaskData
        {
            taskID = "grab_food",
            taskName = "Grab Food",
            description = "Sneak upstairs and grab food from the kitchen.",
            taskType = TaskType.Pickup,
            targetID = "KitchenFood",
            isRequired = true,
            startHidden = false,
            requiredAmount = 3
        };

        TaskData fillWaterTask = new TaskData
        {
            taskID = "fill_water",
            taskName = "Fill Water",
            description = "Fill your water at the sink.",
            taskType = TaskType.Interact,
            targetID = "KitchenSink",
            isRequired = true,
            startHidden = false,
            requiredAmount = 1
        };

        TaskData returnToBasementTask = new TaskData
        {
            taskID = "go_to_kitchen",
            taskName = "Go To Kitchen",
            description = "Go explore the kitchen.",
            taskType = TaskType.EnterZone,
            targetID = "KitchenZone",
            isRequired = true,
            startHidden = false,
            requiredAmount = 1,
        };

        nightOneTasks.Add(grabFoodTask);
        nightOneTasks.Add(fillWaterTask);
        nightOneTasks.Add(returnToBasementTask);

        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.SetTasks(nightOneTasks);
        }
    }
}