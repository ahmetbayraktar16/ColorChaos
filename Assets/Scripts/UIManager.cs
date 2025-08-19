using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject gamePanel;
    public GameObject gameOverPanel;
    public GameObject settingsPanel;
    public GameObject scorePanel;
    
    [Header("Main Menu UI")]
    public Button playButton;
    public Button settingsButton;
    public Button scoreButton;
    public Button quitButton;
    
    [Header("Game UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI targetColorText;
    public TextMeshProUGUI levelText;
    
    [Header("Heart System")]
    public GameObject heartPrefab;
    public RectTransform heartsParent;
    public bool use3DHearts = true;
    public Vector3 heartScale = new Vector3(0.4f, 0.4f, 0.4f);
    public float heartSpacing = 80f;
    public int maxHearts = 3;
    
    [Header("Game Over UI")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;
    public Button restartButton;
    public Button mainMenuButton;
    public Button shareButton;
    
    [Header("Settings UI")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public Toggle vibrationToggle;
    public Button backButton;
    public Button resetButton;
    
    [Header("Score UI")]
    public TextMeshProUGUI highScoreDisplayText;
    public TextMeshProUGUI lastScoreDisplayText;
    public Button scoreBackButton;
    public Button resetProgressButton;
    
    private GameManager gameManager;
    
    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        SetupUI();
        ShowMainMenu();
        UpdateHeartsDisplay(maxHearts);
    }
    
    void SetupUI()
    {
        if (playButton != null)
            playButton.onClick.AddListener(StartGame);
        
        if (settingsButton != null)
            settingsButton.onClick.AddListener(ShowSettings);
        
        if (scoreButton != null)
            scoreButton.onClick.AddListener(ShowScore);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ShowMainMenu);
        
        if (shareButton != null)
            shareButton.onClick.AddListener(ShareScore);
        
        if (backButton != null)
            backButton.onClick.AddListener(ShowMainMenu);
        
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetSettings);
        
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        
        if (vibrationToggle != null)
            vibrationToggle.onValueChanged.AddListener(OnVibrationChanged);
        
        if (scoreBackButton != null)
            scoreBackButton.onClick.AddListener(ShowMainMenu);
        
        if (resetProgressButton != null)
            resetProgressButton.onClick.AddListener(ResetProgress);
    }
    
    public void ShowMainMenu()
    {
        SetActivePanel(mainMenuPanel);
    }
    
    public void StartGame()
    {
        SetActivePanel(gamePanel);
        UpdateHeartsDisplay(maxHearts);
        
        if (gameManager != null)
        {
            gameManager.StartGame();
        }
    }
    
    public void ShowGamePanel()
    {
        SetActivePanel(gamePanel);
        UpdateHeartsDisplay(maxHearts);
    }
    
    public void ShowGameOver(int finalScore)
    {
        SetActivePanel(gameOverPanel);
        if (finalScoreText != null)
            finalScoreText.text = "Son Puan: " + finalScore.ToString();
        
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (finalScore > highScore)
        {
            highScore = finalScore;
            PlayerPrefs.SetInt("HighScore", highScore);
        }
        
        if (highScoreText != null)
            highScoreText.text = "En Yüksek: " + highScore.ToString();
        
        if (targetColorText != null)
            targetColorText.text = "";
    }
    
    public void ShowSettings()
    {
        SetActivePanel(settingsPanel);
        LoadSettings();
    }
    
    public void ShowScore()
    {
        SetActivePanel(scorePanel);
        LoadScoreData();
    }
    
    void SetActivePanel(GameObject activePanel)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (scorePanel != null) scorePanel.SetActive(false);
        
        if (activePanel != null) activePanel.SetActive(true);
    }
    
    public void UpdateGameUI(int score, int lives, string targetColor)
    {
        if (scoreText != null)
            scoreText.text = "Puan: " + score.ToString();
        
        if (targetColorText != null)
            targetColorText.text = targetColor;
        
        UpdateHeartsDisplay(lives);
    }
    
    public void UpdateHeartsDisplay(int currentLives)
    {
        if (heartsParent == null || heartPrefab == null) return;
        
        if (!heartsParent.gameObject.activeInHierarchy)
        {
            heartsParent.gameObject.SetActive(true);
        }
        
        heartsParent.anchoredPosition = new Vector2(100, -200);
        
        ClearHearts();
        
        for (int i = 0; i < currentLives; i++)
        {
            CreateHeart(i);
        }
    }
    
    void CreateHeart(int heartIndex)
    {
        if (heartsParent == null || heartPrefab == null) return;

        GameObject heart = Instantiate(heartPrefab, heartsParent);
    
        RectTransform rt = heart.GetComponent<RectTransform>();
        if (rt == null)
        {
            rt = heart.AddComponent<RectTransform>();
        }
    
        rt.sizeDelta = new Vector2(80, 80);
    
        float xPos = heartIndex * 100f;
        rt.anchoredPosition = new Vector2(xPos, 0f);
    
        heart.transform.localScale = new Vector3(1f, 1f, 1f);
        heart.SetActive(true);
    
        if (heart.GetComponent<CanvasRenderer>() == null)
        {
            heart.AddComponent<CanvasRenderer>();
        }
    }
    
    void ClearHearts()
    {
        if (heartsParent == null) return;
        
        foreach (Transform child in heartsParent)
        {
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }
    }
    
    public void RestartGame()
    {
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        UpdateHeartsDisplay(maxHearts);
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public void ShareScore()
    {
        Debug.Log("Share Score");
    }
    
    void LoadSettings()
    {
        if (musicSlider != null)
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        
        if (sfxSlider != null)
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        
        if (vibrationToggle != null)
            vibrationToggle.isOn = PlayerPrefs.GetInt("Vibration", 1) == 1;
    }
    
    void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        AudioListener.volume = value;
    }
    
    void OnSFXVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
    }
    
    void OnVibrationChanged(bool enabled)
    {
        PlayerPrefs.SetInt("Vibration", enabled ? 1 : 0);
    }
    
    public void ResetSettings()
    {
        if (musicSlider != null) musicSlider.value = 1f;
        if (sfxSlider != null) sfxSlider.value = 1f;
        if (vibrationToggle != null) vibrationToggle.isOn = true;
        
        OnMusicVolumeChanged(1f);
        OnSFXVolumeChanged(1f);
        OnVibrationChanged(true);
    }
    
    void LoadScoreData()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        int lastScore = PlayerPrefs.GetInt("LastScore", 0);
        
        if (highScoreDisplayText != null)
            highScoreDisplayText.text = "En Yüksek: " + highScore.ToString();
        
        if (lastScoreDisplayText != null)
            lastScoreDisplayText.text = "Son Puan: " + lastScore.ToString();
    }
    
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("HighScore");
        PlayerPrefs.DeleteKey("LastScore");
        LoadScoreData();
    }
}