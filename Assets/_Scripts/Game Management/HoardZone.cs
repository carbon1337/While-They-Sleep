/*

Trigger zone that counts loot placed inside the goblin hoard.
Adds score when loot enters the zone and optionally removes it
if the item leaves the zone again.

*/
using System.Collections.Generic;
using UnityEngine;

public class HoardZone : MonoBehaviour
{
    [Header("Rules")]
    [Tooltip("If true, score is removed when the item leaves the zone.")]
    [SerializeField] private bool removeScoreOnExit = true;

    //Tracks which loot objects have already been counted
    private readonly HashSet<LootValue> countedLoot = new HashSet<LootValue>();

    #region Trigger Detection
    private void OnTriggerEnter(Collider other)
    {
        LootValue loot = GetLootFromCollider(other);

        if (loot == null)
            return;

        //Prevent double counting the same loot item
        if (countedLoot.Contains(loot))
            return;

        countedLoot.Add(loot);

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(loot.GetScoreValue());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!removeScoreOnExit)
            return;

        LootValue loot = GetLootFromCollider(other);

        if (loot == null)
            return;

        if (!countedLoot.Contains(loot))
            return;

        countedLoot.Remove(loot);

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.RemoveScore(loot.GetScoreValue());
        }
    }
    #endregion

    #region Utility
    private LootValue GetLootFromCollider(Collider collider)
    {
        //Loot may be on a parent object
        return collider.GetComponentInParent<LootValue>();
    }
    #endregion
}