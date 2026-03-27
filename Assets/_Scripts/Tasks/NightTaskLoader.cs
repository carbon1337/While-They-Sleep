using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NightTaskLoader : MonoBehaviour
{
    public List<NightTaskGroup> nightTasks = new List<NightTaskGroup>();

    private void Start()
    {
        LoadTasks();
    }

    void LoadTasks()
    {
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.SetTasks(nightTasks[FindCurrentLevel()].tasks);
        }
    }

    private int FindCurrentLevel()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        return currentScene.buildIndex;
    }
}