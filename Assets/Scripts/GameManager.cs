using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int maxLives = 3;
    public float wrongClickPenalty = 1f;
    
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
    private bool hasSpawnedGoldenCubeThisScore = false;
    
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
        "TURUNCU", "SARİ", "KAHVERENGİ"
    };
    
    private Color lastTargetColor;
    private bool isGoldenCubeMode = false;
    
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
        hasSpawnedGoldenCubeThisScore = false;
        isGoldenCubeMode = false;
        
        UpdateUI();
        ChangeTargetColor();
        StartCoroutine(SpawnCubes());
    }
    
    void ChangeTargetColor()
    {
        if (currentScore > 0 && currentScore % 50 == 0 && !isGoldenCubeMode)
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
        
        Vector3 safeSpawnPosition = GetSafeSpawnPosition();
        
        bool shouldSpawnGolden = (currentScore > 0 && currentScore % 50 == 0 && !hasSpawnedGoldenCubeThisScore);
        
        GameObject cubePrefabToUse = shouldSpawnGolden && goldenCubePrefab != null ? goldenCubePrefab : cubePrefab;
        GameObject cube = Instantiate(cubePrefabToUse, safeSpawnPosition, Quaternion.identity);
        
        CubeController cubeController = cube.GetComponent<CubeController>();
        
        if (cubeController != null)
        {
            if (shouldSpawnGolden)
            {
                cubeController.InitializeAsGolden(this, GetRandomColor());
                hasSpawnedGoldenCubeThisScore = true;
                
                if (uiManager != null)
                {
                    uiManager.UpdateGameUI(currentScore, currentLives, "ALTIN KÜPE TIKLA");
                }
            }
            else
            {
                cubeController.Initialize(this, GetRandomColor());
            }
            
            cubeController.SetFallSpeed(currentFallSpeed);
        }
    }
    
    Vector3 GetSafeSpawnPosition()
    {
        float minX = -1.8f;
        float maxX = 1.8f;
        float spawnY = Random.Range(5f, 8f);
        
        GameObject[] existingCubes = GameObject.FindGameObjectsWithTag("Cube");
        
        if (existingCubes.Length == 0)
        {
            float centerX = Random.Range(-1f, 1f);
            return new Vector3(centerX, spawnY, 0f);
        }
        
        List<Vector3> possiblePositions = new List<Vector3>();
        
        for (float x = minX; x <= maxX; x += 1.2f)
        {
            for (float y = 5f; y <= 8f; y += 1.5f)
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
            
            foreach (GameObject existingCube in existingCubes)
            {
                if (existingCube != null)
                {
                    float distance = Vector3.Distance(existingCube.transform.position, testPosition);
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
        
        float bestX = Random.Range(minX, maxX);
        float bestY = Random.Range(5f, 8f);
        float minOverlap = float.MaxValue;
        
        for (int attempt = 0; attempt < 50; attempt++)
        {
            float testX = Random.Range(minX, maxX);
            float testY = Random.Range(5f, 8f);
            Vector3 testPos = new Vector3(testX, testY, 0f);
            
            float totalOverlap = 0f;
            foreach (GameObject existingCube in existingCubes)
            {
                if (existingCube != null)
                {
                    float distance = Vector3.Distance(existingCube.transform.position, testPos);
                    if (distance < 2.0f)
                    {
                        totalOverlap += (2.0f - distance);
                    }
                }
            }
            
            if (totalOverlap < minOverlap)
            {
                minOverlap = totalOverlap;
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
            currentScore += 10;
            
            if (cube.isGoldenCube)
            {
                currentScore += 20;
                
                if (currentLives < maxLives)
                {
                    currentLives++;
                }
                
                isGoldenCubeMode = false;
                hasSpawnedGoldenCubeThisScore = false;
                
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
                PlaySound(correctClickSound);
                cube.PlayEffectCorrect();
                
                if (currentScore % 50 == 0)
                {
                    hasSpawnedGoldenCubeThisScore = false;
                }
                
                UpdateDifficulty();
                ChangeTargetColor();
            }
        }
        else
        {
            currentLives--;
            PlaySound(wrongClickSound);
            cube.PlayEffectWrong();
            
            if (PlayerPrefs.GetInt("Vibration", 1) == 1)
            {
                #if UNITY_ANDROID
                    Handheld.Vibrate();
                #endif
            }
            
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
            
            if (PlayerPrefs.GetInt("Vibration", 1) == 1)
            {
                #if UNITY_ANDROID
                    Handheld.Vibrate();
                #endif
            }
            
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
                hasSpawnedGoldenCubeThisScore = false;
                
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
        hasSpawnedGoldenCubeThisScore = false;
        isGoldenCubeMode = false;
        
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
}