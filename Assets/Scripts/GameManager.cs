using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int maxLives = 3;
    
    [Header("UI References")]
    public UIManager uiManager;
    
    [Header("Cube Spawning")]
    public GameObject cubePrefab;
    public GameObject goldenCubePrefab;
    public Transform spawnArea;
    public float baseSpawnRate = 2.5f;
    public int maxCubesOnScreen = 2;
    
    [Header("Audio")]
    public AudioClip correctClickSound;
    public AudioClip wrongClickSound;
    public AudioClip gameOverSound;
    
    private int currentScore = 0;
    private int currentLives;
    private Color targetColor;
    private bool isGameActive = false;
    private float currentSpawnRate;
    private float currentFallSpeed = 2f;
    
    private Color[] availableColors = {
        new Color(1f, 0f, 0f),     
        new Color(0f, 0.4f, 1f),   
        new Color(0f, 0.7f, 0.2f),
        new Color(1f, 0.2f, 0.6f),
        new Color(1f, 0.4f, 0f),
        new Color(1f, 1f, 0f),    
        new Color(0.4f, 0.2f, 0f)
    };

    private string[] colorNames = { 
        "KIRMIZI", "MAVİ", "YEŞİL", "PEMBE",
        "TURUNCU", "SARI", "KAHVERENGİ"
    };
    
    private Color lastTargetColor;
    private bool isGoldenCubeMode = false;
    private bool hasSpawnedGoldenCubeThisLevel = false;
    
    private int cubesSinceLastTargetColor = 0;
    private int maxCubesWithoutTargetColor = 5;
    
    void Start()
    {
        isGameActive = false;
        currentLives = maxLives;
        currentSpawnRate = baseSpawnRate;
        
        if (uiManager != null)
        {
            uiManager.UpdateGameUI(currentScore, currentLives, "");
        }
    }
    
    void Update()
    {
        if (!isGameActive) return;
    }
    
    void InitializeGame()
    {
        currentScore = 0;
        currentLives = maxLives;
        isGameActive = true;
        currentSpawnRate = baseSpawnRate;
        currentFallSpeed = 2f;
        isGoldenCubeMode = false;
        hasSpawnedGoldenCubeThisLevel = false;
        
        cubesSinceLastTargetColor = 0;
        
        UpdateUI();
        ChangeTargetColor();
        StartCoroutine(SpawnCubes());
    }
    
    void ChangeTargetColor()
    {
        if (currentScore > 0 && currentScore % 100 == 0 && !isGoldenCubeMode)
        {
            isGoldenCubeMode = true;
            lastTargetColor = targetColor;
            
            if (uiManager != null)
            {
                uiManager.UpdateGameUI(currentScore, currentLives, "ALTIN KÜPE TIKLA");
            }
            return;
        }
        
        if (isGoldenCubeMode)
        {
            return;
        }
        
        int randomIndex = Random.Range(0, availableColors.Length);
        targetColor = availableColors[randomIndex];
        string targetColorName = colorNames[randomIndex];
        
        if (uiManager != null)
        {
            uiManager.UpdateGameUI(currentScore, currentLives, targetColorName);
        }
    }
    
    void UpdateDifficulty()
    {
        if (currentScore >= 100)
        {
            currentSpawnRate = 1.0f;
            currentFallSpeed = 4f;
        }
        else if (currentScore >= 75)
        {
            currentSpawnRate = 1.2f;
            currentFallSpeed = 3.5f;
        }
        else if (currentScore >= 50)
        {
            currentSpawnRate = 1.5f;
            currentFallSpeed = 3f;
        }
        else if (currentScore >= 25)
        {
            currentSpawnRate = 2.0f;
            currentFallSpeed = 2.5f;
        }
        else
        {
            currentSpawnRate = baseSpawnRate;
            currentFallSpeed = 2f;
        }
    }

    IEnumerator SpawnCubes()
    {
        while (isGameActive)
        {
            if (GameObject.FindGameObjectsWithTag("Cube").Length < maxCubesOnScreen)
            {
                SpawnCube();
            }
            
            yield return new WaitForSeconds(currentSpawnRate);
        }
    }
    
    void SpawnCube()
    {
        if (cubePrefab == null || spawnArea == null) return;
            
        bool shouldSpawnGolden = (currentScore > 0 && currentScore % 100 == 0 && !hasSpawnedGoldenCubeThisLevel);
        
        Vector3 safeSpawnPosition = GetSafeSpawnPosition();
        
        GameObject cubePrefabToUse;
        
        if (shouldSpawnGolden)
        {
            cubePrefabToUse = goldenCubePrefab != null ? goldenCubePrefab : cubePrefab;
        }
        else
        {
            cubePrefabToUse = cubePrefab;
        }
        
        GameObject cube = Instantiate(cubePrefabToUse, safeSpawnPosition, Quaternion.identity);
        
        CubeController cubeController = cube.GetComponent<CubeController>();
        
        if (cubeController != null)
        {
            if (shouldSpawnGolden)
            {
                cubeController.InitializeAsGolden(this, GetRandomColor());
                hasSpawnedGoldenCubeThisLevel = true; // Bu seviyede altın küp spawn edildi
                
                if (uiManager != null)
                {
                    uiManager.UpdateGameUI(currentScore, currentLives, "ALTIN KÜPE TIKLA");
                }
            }
            else
            {
                Color cubeColor;
                
                if (cubesSinceLastTargetColor >= maxCubesWithoutTargetColor)
                {
                    cubeColor = targetColor;
                    cubesSinceLastTargetColor = 0; 
                }
                else
                {
                    cubeColor = GetRandomColor();
                    cubesSinceLastTargetColor++;
                }
                
                cubeController.Initialize(this, cubeColor);
            }
            
            cubeController.SetFallSpeed(currentFallSpeed);
        }
    }
    
    Vector3 GetSafeSpawnPosition()
    {
        float minX = -1.8f;
        float maxX = 1.8f;
        float minY = 5f;
        float maxY = 8f;
        
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Cube");
        
        if (allObjects.Length == 0)
        {
            float centerX = Random.Range(-1.5f, 1.5f);
            float centerY = Random.Range(6f, 7f);
            return new Vector3(centerX, centerY, 0f);
        }
        
        List<Vector3> possiblePositions = new List<Vector3>();
        
        for (float x = minX; x <= maxX; x += 0.8f)
        {
            for (float y = minY; y <= maxY; y += 1.0f)
            {
                possiblePositions.Add(new Vector3(x, y, 0f));
            }
        }
        
        for (int i = 0; i < possiblePositions.Count; i++)
        {
            Vector3 temp = possiblePositions[i];
            int randomIndex = Random.Range(i, possiblePositions.Count);
            possiblePositions[i] = possiblePositions[randomIndex];
            possiblePositions[randomIndex] = temp;
        }
        
        foreach (Vector3 testPosition in possiblePositions)
        {
            bool isSafe = true;
            
            foreach (GameObject existingObject in allObjects)
            {
                if (existingObject != null)
                {
                    float distance = Vector3.Distance(existingObject.transform.position, testPosition);
                    if (distance < 2.0f) 
                    {
                        isSafe = false;
                        break;
                    }
                }
            }
            
            if (isSafe)
            {
                return testPosition;
            }
        }
        
        float bestX = Random.Range(-1.8f, 1.8f);
        float bestY = Random.Range(6f, 7f);
        float maxDistance = 0f;
        
        for (int attempt = 0; attempt < 30; attempt++)
        {
            float testX = Random.Range(-1.8f, 1.8f);
            float testY = Random.Range(6f, 7f);
            Vector3 testPos = new Vector3(testX, testY, 0f);
            
            float minDistance = float.MaxValue;
            foreach (GameObject existingObject in allObjects)
            {
                if (existingObject != null)
                {
                    float distance = Vector3.Distance(existingObject.transform.position, testPos);
                    if (distance < minDistance) minDistance = distance;
                }
            }
            
            if (minDistance > maxDistance)
            {
                maxDistance = minDistance;
                bestX = testX;
                bestY = testY;
            }
        }
        
        return new Vector3(bestX, bestY, 0f);
    }
    
    Color GetRandomColor()
    {
        return availableColors[Random.Range(0, availableColors.Length)];
    }
    
    public void OnCubeClicked(CubeController cube)
    {
        if (!isGameActive) return;
        
        if (cube.CubeColor == targetColor || (isGoldenCubeMode && cube.isGoldenCube))
        {
            if (cube.isGoldenCube)
            {
                int goldenScore = 20;
                currentScore += goldenScore;
                
                if (currentLives < maxLives)
                {
                    currentLives++;
                }
                
                isGoldenCubeMode = false;
                hasSpawnedGoldenCubeThisLevel = false;
                
                if (Random.Range(0f, 1f) < 0.5f)
                {
                    targetColor = lastTargetColor;
                    string lastColorName = "";
                    for (int i = 0; i < availableColors.Length; i++)
                    {
                        if (availableColors[i] == lastTargetColor)
                        {
                            lastColorName = colorNames[i];
                            break;
                        }
                    }
                    if (uiManager != null)
                    {
                        uiManager.UpdateGameUI(currentScore, currentLives, lastColorName);
                    }
                }
                else
                {
                    ChangeTargetColor();
                }
            }
            else
            {
                int baseScore = 10;
                currentScore += baseScore;
                
                PlaySound(correctClickSound);
                cube.PlayEffectCorrect();
                
                UpdateDifficulty();
                ChangeTargetColor();
            }
        }
        else
        {
            currentLives--;
            PlaySound(wrongClickSound);
            cube.PlayEffectWrong();
            
            if (uiManager != null)
            {
                uiManager.PlayLifeLossEffect();
            }
            
            if (currentLives <= 0)
            {
                GameOver();
            }
        }
        
        cube.PlayDestroyEffect();
        UpdateUI();
        Destroy(cube.gameObject);
    }
    
    public void OnCubeMissed(CubeController cube)
    {
        if (!isGameActive) return;
        
        if (cube.CubeColor == targetColor || (isGoldenCubeMode && cube.isGoldenCube))
        {
            currentLives--;
            PlaySound(wrongClickSound);
            
            if (uiManager != null)
            {
                uiManager.PlayLifeLossEffect();
            }
            
            if (currentLives <= 0)
            {
                GameOver();
                return;
            }
            
            if (isGoldenCubeMode && cube.isGoldenCube)
            {
                isGoldenCubeMode = false;
                
                if (Random.Range(0f, 1f) < 0.5f)
                {
                    targetColor = lastTargetColor;
                    string lastColorName = "";
                    for (int i = 0; i < availableColors.Length; i++)
                    {
                        if (availableColors[i] == lastTargetColor)
                        {
                            lastColorName = colorNames[i];
                            break;
                        }
                    }
                    if (uiManager != null)
                    {
                        uiManager.UpdateGameUI(currentScore, currentLives, lastColorName);
                    }
                }
                else
                {
                    ChangeTargetColor();
                }
            }
            
            UpdateUI();
        }
    }
    
    void UpdateUI()
    {
        if (uiManager != null)
        {
            string targetColorName = "";
            if (isGameActive)
            {
                if (isGoldenCubeMode)
                {
                    targetColorName = "ALTIN KÜPE TIKLA";
                }
                else
                {
                    for (int i = 0; i < availableColors.Length; i++)
                    {
                        if (availableColors[i] == targetColor)
                        {
                            targetColorName = colorNames[i];
                            break;
                        }
                    }
                }
            }
            
            uiManager.UpdateGameUI(currentScore, currentLives, targetColorName);
        }
    }
    
    void GameOver()
    {
        isGameActive = false;
        
        StopAllCoroutines();
        
        GameObject[] existingCubes = GameObject.FindGameObjectsWithTag("Cube");
        foreach (GameObject cube in existingCubes)
        {
            Destroy(cube);
        }
        
        PlaySound(gameOverSound);
        
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.RecordScore(currentScore);
        }
        
        if (uiManager != null)
        {
            uiManager.ShowGameOver(currentScore);
        }
    }
    
    public void RestartGame()
    {
        currentScore = 0;
        currentLives = maxLives;
        isGameActive = false;
        currentSpawnRate = baseSpawnRate;
        currentFallSpeed = 2f;
        isGoldenCubeMode = false;
        hasSpawnedGoldenCubeThisLevel = false;
        
        cubesSinceLastTargetColor = 0;
        
        GameObject[] existingCubes = GameObject.FindGameObjectsWithTag("Cube");
        foreach (GameObject cube in existingCubes)
        {
            if (cube != null)
            {
                Destroy(cube);
            }
        }
        
        StopAllCoroutines();
        
        if (uiManager != null)
        {
            uiManager.UpdateGameUI(currentScore, currentLives, "");
            uiManager.ShowGamePanel();
        }
        
        StartGame();
    }
    
    void PlaySound(AudioClip clip)
    {
        if (uiManager != null && uiManager.sfxAudioSource != null && clip != null)
        {
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            uiManager.sfxAudioSource.volume = sfxVolume;
            uiManager.sfxAudioSource.PlayOneShot(clip);
        }
    }
    
    public void StartGame()
    {
        InitializeGame();
    }
    
    public int GetCurrentScore()
    {
        return currentScore;
    }
}