using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New NightTaskGroup", menuName = "Create New NightTaskGroup")]
public class NightTaskGroup : ScriptableObject
{
    public string nightName;
    public List<TaskData> tasks;
    public bool isStealth;
}
