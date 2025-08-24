using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    [Header("Level Settings")]
    public int currentLevel = 1;
    public float baseSpawnRate = 2f;
    public float baseFallSpeed = 3f;
    public int baseMaxCubes = 2;
    
    [Header("Difficulty Progression")]
    public float speedMultiplier = 1.5f;
    public float spawnRateMultiplier = 0.8f;
    public int cubeIncreaseInterval = 5;
    
    [Header("Special Modes")]
    public bool enableGoldenCubes = true;
    public bool enableRotatingCubes = false;
    public bool enableBlinkingColors = false;
    public bool enableBossMode = false;
    
    [Header("Boss Mode Settings")]
    public float bossSpawnRate = 0.5f;
    public int bossLevelInterval = 10;
    
    private GameManager gameManager;
    private float currentSpawnRate;
    private float currentFallSpeed;
    private int currentMaxCubes;
    
    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        UpdateLevelSettings();
    }
    
    public void SetLevel(int level)
    {
        currentLevel = level;
        UpdateLevelSettings();
        ApplySpecialModes();
    }
    
    void UpdateLevelSettings()
    {
        // Calculate difficulty based on level
        float difficultyMultiplier = 1f + (currentLevel - 1) * 0.2f;
        
        // Update spawn rate (faster spawning)
        currentSpawnRate = baseSpawnRate * Mathf.Pow(spawnRateMultiplier, currentLevel - 1);
        currentSpawnRate = Mathf.Max(currentSpawnRate, 0.5f); // Minimum spawn rate
        
        // Update fall speed
        currentFallSpeed = baseFallSpeed * difficultyMultiplier;
        
        // Update max cubes
        currentMaxCubes = baseMaxCubes + (currentLevel - 1) / cubeIncreaseInterval;
        currentMaxCubes = Mathf.Min(currentMaxCubes, 8); // Maximum cubes limit
        
        // Apply to GameManager
        if (gameManager != null)
        {
            // Update GameManager settings
            // Note: GameManager now handles difficulty internally based on score
        }
    }
    
    void ApplySpecialModes()
    {
        // Enable golden cubes from level 6
        enableGoldenCubes = currentLevel >= 6;
        
        // Enable rotating cubes from level 11
        enableRotatingCubes = currentLevel >= 11;
        
        // Enable blinking colors from level 11
        enableBlinkingColors = currentLevel >= 11;
        
        // Enable boss mode every 10 levels
        enableBossMode = currentLevel % bossLevelInterval == 0;
        
        if (enableBossMode)
        {
            StartBossMode();
        }
    }
    
    void StartBossMode()
    {
        StartCoroutine(BossModeRoutine());
    }
    
    IEnumerator BossModeRoutine()
    {
        // Boss mode: faster spawning
        float originalSpawnRate = currentSpawnRate;
        
        // Set boss mode settings
        currentSpawnRate = bossSpawnRate;
        
        // Boss mode lasts for 30 seconds
        yield return new WaitForSeconds(30f);
        
        // Restore normal settings
        currentSpawnRate = originalSpawnRate;
    }
    
    public float GetCurrentSpawnRate()
    {
        return currentSpawnRate;
    }
    
    public float GetCurrentFallSpeed()
    {
        return currentFallSpeed;
    }
    
    public int GetCurrentMaxCubes()
    {
        return currentMaxCubes;
    }
    
    public bool ShouldSpawnGoldenCube()
    {
        return enableGoldenCubes && Random.Range(0f, 1f) < 0.05f;
    }
    
    public bool ShouldSpawnRotatingCube()
    {
        return enableRotatingCubes && Random.Range(0f, 1f) < 0.1f;
    }
    
    public bool ShouldBlinkColor()
    {
        return enableBlinkingColors && Random.Range(0f, 1f) < 0.3f;
    }
    
    public void LevelUp()
    {
        currentLevel++;
        SetLevel(currentLevel);
        
        // Show level up effect
        StartCoroutine(LevelUpEffect());
    }
    
    IEnumerator LevelUpEffect()
    {
        // TODO: Add visual level up effect
        Debug.Log("Level Up! Level " + currentLevel);
        yield return null;
    }
    
    public string GetLevelDescription()
    {
        string description = "Seviye " + currentLevel;
        
        if (enableBossMode)
        {
            description += " - BOSS MODU!";
        }
        else if (enableBlinkingColors)
        {
            description += " - Yanıp Sönen Renkler";
        }
        else if (enableRotatingCubes)
        {
            description += " - Dönen Küpler";
        }
        else if (enableGoldenCubes)
        {
            description += " - Altın Küpler";
        }
        
        return description;
    }
}