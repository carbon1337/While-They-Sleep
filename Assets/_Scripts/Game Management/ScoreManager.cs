/*

Tracks the player's current hoard score and updates the UI
whenever score is added or removed.

*/
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;

    //Current hoard total
    public int Score { get; private set; }

    #region Initialization
    private void Awake()
    {
        //Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        UpdateUI();
    }
    #endregion

    #region Score
    public void AddScore(int amount)
    {
        Score += amount;
        UpdateUI();
    }

    public void RemoveScore(int amount)
    {
        Score -= amount;

        //Prevent score from going below zero
        if (Score < 0)
        {
            Score = 0;
        }

        UpdateUI();
    }
    #endregion

    #region UI
    private void UpdateUI()
    {
        if (scoreText == null)
            return;

        scoreText.text = $"Hoard: {Score}";
    }
    #endregion
}