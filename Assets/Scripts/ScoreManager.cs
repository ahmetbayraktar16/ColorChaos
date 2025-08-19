using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ScoreManager : MonoBehaviour
{
    private const string KeyHighScore = "HighScore";
    private const string KeyLastScore = "LastScore";
    private const string KeyTotalGames = "TotalGames";
    private const string KeyRecentScores = "RecentScores";
    private const int MaxRecentScores = 20;
    
    public static ScoreManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void RecordScore(int score)
    {
        // Update high score
        int currentHighScore = GetHighScore();
        if (score > currentHighScore)
        {
            PlayerPrefs.SetInt(KeyHighScore, score);
        }
        
        // Update last score
        PlayerPrefs.SetInt(KeyLastScore, score);
        
        // Update total games
        int totalGames = GetTotalGames() + 1;
        PlayerPrefs.SetInt(KeyTotalGames, totalGames);
        
        // Add to recent scores
        AddToRecentScores(score);
        
        PlayerPrefs.Save();
    }
    
    public int GetHighScore()
    {
        return PlayerPrefs.GetInt(KeyHighScore, 0);
    }
    
    public int GetLastScore()
    {
        return PlayerPrefs.GetInt(KeyLastScore, 0);
    }
    
    public int GetTotalGames()
    {
        return PlayerPrefs.GetInt(KeyTotalGames, 0);
    }
    
    public List<int> GetRecentScores()
    {
        string recentScores = PlayerPrefs.GetString(KeyRecentScores, "");
        if (string.IsNullOrEmpty(recentScores))
            return new List<int>();
        
        return recentScores.Split(',')
            .Where(s => !string.IsNullOrEmpty(s))
            .Select(s => int.TryParse(s, out int score) ? score : 0)
            .ToList();
    }
    
    void AddToRecentScores(int newScore)
    {
        List<int> recentScores = GetRecentScores();
        recentScores.Insert(0, newScore);
        
        // Keep only the most recent scores
        if (recentScores.Count > MaxRecentScores)
        {
            recentScores = recentScores.Take(MaxRecentScores).ToList();
        }
        
        string scoresString = string.Join(",", recentScores);
        PlayerPrefs.SetString(KeyRecentScores, scoresString);
    }
    
    public void ResetAllProgress()
    {
        PlayerPrefs.DeleteKey(KeyHighScore);
        PlayerPrefs.DeleteKey(KeyLastScore);
        PlayerPrefs.DeleteKey(KeyTotalGames);
        PlayerPrefs.DeleteKey(KeyRecentScores);
        PlayerPrefs.Save();
    }
    
    public void ResetHighScore()
    {
        PlayerPrefs.DeleteKey(KeyHighScore);
        PlayerPrefs.Save();
    }
    
    public void ResetRecentScores()
    {
        PlayerPrefs.DeleteKey(KeyRecentScores);
        PlayerPrefs.Save();
    }
}
