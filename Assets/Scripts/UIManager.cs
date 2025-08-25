using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource backgroundMusicSource;
    public AudioSource sfxAudioSource;
    public AudioClip buttonClickSound;
    
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
    
    [Header("Heart System")]
    public GameObject heartPrefab;
    public RectTransform heartsParent;
    public ParticleSystem lifeLossEffect;
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
        LoadSettings();
        ShowMainMenu();
        UpdateHeartsDisplay(maxHearts);
        
        StartBackgroundMusic();
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
        PlayButtonClickSound();
        SetActivePanel(mainMenuPanel);
        StartBackgroundMusic();
    }
    
    public void StartGame()
    {
        PlayButtonClickSound();
        SetActivePanel(gamePanel);
        UpdateHeartsDisplay(maxHearts);
        StartBackgroundMusic();
        
        if (gameManager != null)
        {
            gameManager.StartGame();
        }
    }
    
    public void ShowGamePanel()
    {
        PlayButtonClickSound();
        SetActivePanel(gamePanel);
        UpdateHeartsDisplay(maxHearts);
        StartBackgroundMusic();
    }
    
    public void ShowGameOver(int finalScore)
    {
        PauseBackgroundMusic();
        
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (scorePanel != null) scorePanel.SetActive(false);
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (finalScoreText != null)
            {
                finalScoreText.text = "Final Score: " + finalScore.ToString();
            }
        }
    }
    
    public void PlayLifeLossEffect()
    {
        if (lifeLossEffect == null) return;
        
        Vector3 effectPosition = new Vector3(0f, 2f, 0f);
        
        ParticleSystem effect = Instantiate(lifeLossEffect, effectPosition, Quaternion.identity);
        
        effect.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        
        var main = effect.main;
        main.startSize = 0.2f;
        
        effect.Play();
        Destroy(effect.gameObject, 2f);
    }
    
    public void ShowSettings()
    {
        PlayButtonClickSound();
        SetActivePanel(settingsPanel);
        LoadSettings();
    }
    
    public void ShowScore()
    {
        PlayButtonClickSound();
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
        {
            targetColorText.text = targetColor;
            
            if (!string.IsNullOrEmpty(targetColor))
            {
                Color textColor = GetColorFromName(targetColor);
                targetColorText.color = textColor;
            }
        }
        
        UpdateHeartsDisplay(lives);
    }
    
    Color GetColorFromName(string colorName)
    {
        switch (colorName.ToUpper())
        {
            case "KIRMIZI": return Color.red;
            case "MAVİ": return new Color(0f, 0.4f, 1f);
            case "YEŞİL": return Color.green;
            case "PEMBE": return new Color(1f, 0.2f, 0.6f);
            case "MOR": return new Color(0.6f, 0f, 0.8f);
            case "TURUNCU": return new Color(1f, 0.5f, 0f);
            case "SARİ": return Color.yellow;
            case "KAHVERENGİ": return new Color(0.4f, 0.2f, 0f);
            case "ALTIN KÜPE TIKLA": return new Color(1f, 0.8f, 0f);
            default: return Color.white;
        }
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
        PlayButtonClickSound();
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
        PlayButtonClickSound();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public void ShareScore()
    {
        PlayButtonClickSound();
    }
    
    void LoadSettings()
    {
        if (musicSlider != null)
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        
        if (sfxSlider != null)
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        
        if (vibrationToggle != null)
            vibrationToggle.isOn = PlayerPrefs.GetInt("Vibration", 1) == 1;
            
        ApplyAudioSettings();
    }
    
    void ApplyAudioSettings()
    {
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        
        if (backgroundMusicSource != null)
            backgroundMusicSource.volume = musicVolume;
            
        if (sfxAudioSource != null)
            sfxAudioSource.volume = sfxVolume;
    }
    
    void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        
        if (backgroundMusicSource != null)
            backgroundMusicSource.volume = value;
    }
    
    void OnSFXVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        
        if (sfxAudioSource != null)
            sfxAudioSource.volume = value;
    }
    
        public void StartBackgroundMusic()
    {
        if (backgroundMusicSource == null || backgroundMusicSource.clip == null) return;
        
        if (!backgroundMusicSource.isPlaying)
        {
            float savedTime = PlayerPrefs.GetFloat("MusicTime", 0f);
            backgroundMusicSource.time = savedTime;
            backgroundMusicSource.Play();
        }
    }
    
    public void PauseBackgroundMusic()
    {
        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
        {
                    PlayerPrefs.SetFloat("MusicTime", backgroundMusicSource.time);
            backgroundMusicSource.Pause();
        }
    }
    
    public void StopBackgroundMusic()
    {
        if (backgroundMusicSource != null)
        {
                    PlayerPrefs.SetFloat("MusicTime", backgroundMusicSource.time);
            backgroundMusicSource.Stop();
        }
    }
    
    public void ResumeBackgroundMusic()
    {
        if (backgroundMusicSource != null)
        {
            float savedTime = PlayerPrefs.GetFloat("MusicTime", 0f);
            backgroundMusicSource.time = savedTime;
            backgroundMusicSource.Play();
        }
    }
    
    public void PlayUISound(AudioClip clip)
    {
        if (sfxAudioSource != null && clip != null)
        {
                    float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        sfxAudioSource.volume = sfxVolume;
        sfxAudioSource.PlayOneShot(clip);
        }
    }
    
    public void PlayButtonClickSound()
    {
        PlayUISound(buttonClickSound);
    }
    
    public void OnVibrationChanged(bool enabled)
    {
        PlayerPrefs.SetInt("Vibration", enabled ? 1 : 0);
    }
    
    public void ResetSettings()
    {
        PlayButtonClickSound();
        if (musicSlider != null) musicSlider.value = 1f;
        if (sfxSlider != null) sfxSlider.value = 1f;
        if (vibrationToggle != null) vibrationToggle.isOn = true;
        
        PlayerPrefs.SetFloat("MusicVolume", 1f);
        PlayerPrefs.SetFloat("SFXVolume", 1f);
        PlayerPrefs.SetInt("Vibration", 1);
        ApplyAudioSettings();
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
        PlayButtonClickSound();
        PlayerPrefs.DeleteKey("HighScore");
        PlayerPrefs.DeleteKey("LastScore");
        LoadScoreData();
    }
}