using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "New TaskData", menuName = "Create New TaskData")]
public class TaskData : ScriptableObject
{
    [Header("Basic Info")]
    public string taskID; //Internal Name
    public string taskName; //Display Name
    [TextArea] public string description;
    public AudioClip taskCompletionAudioClip;

    [Header("Task Settings")]
    public TaskType taskType;
    public string targetID; //object it listens for
    public bool isRequired = true; 
    public bool startHidden = false; //For multi-stage tasks

    [Header("Progress")]
    public int requiredAmount = 1; //ex. "Grab food: x/requiredAmount"
    public int currentAmount = 0;

    [Header("Dependencies")]
    public List<string> prerequisiteTaskIDs = new List<string>(); //Tasks unlock after others are done

    [Header("Runtime State")]
    public TaskState taskState = TaskState.Locked;
}

public enum TaskType
{
    Interact,
    Pickup,
    EnterZone
}

public enum TaskState
{
    Locked,
    Active,
    Completed,
    Failed
}