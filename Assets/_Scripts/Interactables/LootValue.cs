using UnityEngine;

public class LootValue : MonoBehaviour
{
    [Header("Scoring")]
    public int basePoints = 10;

    [Tooltip("Example: gold = 2.0, junk = 1.0, cursed = 0.5")]
    public float multiplier = 1f;

    [Tooltip("Optional: use if you want unique items or categories later")]
    public string lootId;

    public int GetScoreValue()
    {
        // Score is rounded to int so UI and totals are clean
        return Mathf.RoundToInt(basePoints * multiplier);
    }
}