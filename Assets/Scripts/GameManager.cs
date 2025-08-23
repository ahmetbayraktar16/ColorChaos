using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Collections.Generic; // Added for List

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int maxLives = 3;
    public float colorChangeInterval = 5f;
    public float wrongClickPenalty = 1f;
    
    [Header("UI References")]
    public UIManager uiManager;
    
    [Header("Cube Spawning")]
    public GameObject cubePrefab;
    public Transform spawnArea;
    public float spawnRate = 2.5f;
    public int maxCubesOnScreen = 2;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip correctClickSound;
    public AudioClip wrongClickSound;
    public AudioClip gameOverSound;
    
    private int currentScore = 0;
    private int currentLives;
    private Color targetColor;
    private bool isGameActive = false;
    
    private Color[] availableColors = {
        new Color(1f, 0f, 0f),     
        new Color(0f, 0.4f, 1f),   
        new Color(0f, 0.7f, 0.2f),
        new Color(1f, 0.2f, 0.6f),
        new Color(0.6f, 0f, 0.8f),
        new Color(1f, 0.5f, 0f),
        new Color(1f, 1f, 0f),    
        new Color(0.4f, 0.2f, 0f)
    };

    
    private string[] colorNames = { 
        "KIRMIZI", "MAVİ", "YEŞİL", "PEMBE", "MOR",
        "TURUNCU", "SARİ", "KAHVERENGİ"
    };
    
    void Start()
    {
        isGameActive = false;
        currentLives = maxLives;
        
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
        
        UpdateUI();
        ChangeTargetColor();
        StartCoroutine(SpawnCubes());
    }
    
    void ChangeTargetColor()
    {
        int randomIndex = Random.Range(0, availableColors.Length);
        targetColor = availableColors[randomIndex];
        string targetColorName = colorNames[randomIndex];
        
        if (uiManager != null)
        {
            uiManager.UpdateGameUI(currentScore, currentLives, targetColorName);
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
            yield return new WaitForSeconds(spawnRate);
        }
    }
    
    void SpawnCube()
    {
        if (cubePrefab == null || spawnArea == null) return;
        
        Vector3 safeSpawnPosition = GetSafeSpawnPosition();
        GameObject cube = Instantiate(cubePrefab, safeSpawnPosition, Quaternion.identity);
        
        CubeController cubeController = cube.GetComponent<CubeController>();
        
        if (cubeController != null)
        {
            cubeController.Initialize(this, GetRandomColor());
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
        
        // Create a grid of possible spawn positions
        List<Vector3> possiblePositions = new List<Vector3>();
        
        for (float x = minX; x <= maxX; x += 1.2f) // 1.2f spacing between positions
        {
            for (float y = 5f; y <= 8f; y += 1.5f) // 1.5f vertical spacing
            {
                possiblePositions.Add(new Vector3(x, y, 0f));
            }
        }
        
        // Shuffle the positions
        for (int i = 0; i < possiblePositions.Count; i++)
        {
            Vector3 temp = possiblePositions[i];
            int randomIndex = Random.Range(i, possiblePositions.Count);
            possiblePositions[i] = possiblePositions[randomIndex];
            possiblePositions[randomIndex] = temp;
        }
        
        // Check each position for safety
        foreach (Vector3 testPosition in possiblePositions)
        {
            bool isSafe = true;
            
            foreach (GameObject existingCube in existingCubes)
            {
                if (existingCube != null)
                {
                    float distance = Vector3.Distance(existingCube.transform.position, testPosition);
                    if (distance < 2.0f) // Minimum 2.0f distance between cubes
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
        
        // If no safe position found, try to find one with minimal overlap
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
        
        if (cube.CubeColor == targetColor)
        {
            currentScore += 5;
            PlaySound(correctClickSound);
            cube.PlayEffectCorrect();
            
            if (cube.isGoldenCube)
            {
                currentScore += 10;
            }

            ChangeTargetColor();
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
        
        if (cube.CubeColor == targetColor)
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
                for (int i = 0; i < availableColors.Length; i++)
                {
                    if (availableColors[i] == targetColor)
                    {
                        targetColorName = colorNames[i];
                        break;
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
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    public void StartGame()
    {
        InitializeGame();
    }
}