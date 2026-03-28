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

    //Calls TaskManager to load the pre-determined tasks of the current scene
    void LoadTasks()
    {
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.SetTasks(nightTasks[FindCurrentLevel()].tasks);
        }

        //Enable/disable noise meter based on level data
        NoiseMeter.Instance.ToggleStealth(nightTasks[FindCurrentLevel()].isStealth);
    }

    //Returns the build index of the current scene, -1 to skip the main menu
    private int FindCurrentLevel()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        return currentScene.buildIndex -1;
    }
}