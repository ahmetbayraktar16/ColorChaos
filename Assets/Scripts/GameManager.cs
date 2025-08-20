using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

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
    private float gameTimer = 0f;
    
    private Color[] availableColors = {
        new Color(1f, 0.32f, 0.32f),
        new Color(0.26f, 0.52f, 0.95f),
        new Color(0.20f, 0.66f, 0.33f),
        new Color(1f, 0.41f, 0.71f),
        new Color(0.58f, 0.29f, 0.58f),
        new Color(1f, 0.65f, 0f),
        new Color(1f, 1f, 0f),
        new Color(0.5f, 0.25f, 0f)
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
        // Removed timer-based target color change
    }
    
    void InitializeGame()
    {
        currentScore = 0;
        currentLives = maxLives;
        isGameActive = true;
        gameTimer = 0f;
        
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
        float minX = -2.1f;
        float maxX = 2.1f;
        float spawnY = Random.Range(5f, 8f);
        float minDistance = 2.5f;
        
        GameObject[] existingCubes = GameObject.FindGameObjectsWithTag("Cube");
        
        if (existingCubes.Length == 0)
        {
            float centerX = Random.Range(-1f, 1f);
            return new Vector3(centerX, spawnY, 0f);
        }
        
        for (int attempt = 0; attempt < 20; attempt++)
        {
            float randomX = Random.Range(minX, maxX);
            float randomY = Random.Range(5f, 8f);
            Vector3 testPosition = new Vector3(randomX, randomY, 0f);
            
            bool isSafe = true;
            
            foreach (GameObject existingCube in existingCubes)
            {
                if (existingCube != null)
                {
                    float distanceX = Mathf.Abs(existingCube.transform.position.x - testPosition.x);
                    float distanceY = Mathf.Abs(existingCube.transform.position.y - testPosition.y);
                    
                    if (distanceX < minDistance || distanceY < 1.5f)
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
        
        return new Vector3(Random.Range(minX, maxX), Random.Range(5f, 8f), 0f);
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
            
            if (cube.isGoldenCube)
            {
                currentScore += 10;
            }
            
            // Change target color only after a correct click
            ChangeTargetColor();
        }
        else
        {
            currentLives--;
            PlaySound(wrongClickSound);
            
            if (currentLives <= 0)
            {
                GameOver();
            }
        }
        
        UpdateUI();
        Destroy(cube.gameObject);
    }
    
    // Called when a cube falls off screen without being clicked
    public void OnCubeMissed(CubeController cube)
    {
        if (!isGameActive) return;
        
        if (cube.CubeColor == targetColor)
        {
            currentLives--;
            PlaySound(wrongClickSound);
            
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