using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskUIRow : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI taskNameText;
    public Image checkmarkImage;

    //Initializes individual task UI
    public void Setup(TaskData task)
    {
        if (task == null) return;

        //Initialize name
        taskNameText.text = task.taskName;

        //Initialize completion, display checkmark when appropriate
        bool isCompleted = task.taskState == TaskState.Completed;
        checkmarkImage.gameObject.SetActive(isCompleted);

        //Strikethrough when completed
        if (isCompleted)
        {
            taskNameText.text = "<s>" + task.taskName + "</s>";
        }
        else if (task.requiredAmount > 1)
        {
            taskNameText.text = task.taskName + " (" + task.currentAmount + "/" + task.requiredAmount + ")";
        }
        else
        {
            taskNameText.text = task.taskName;
        }
    }
}