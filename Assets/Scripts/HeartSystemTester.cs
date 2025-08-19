using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeartSystemTester : MonoBehaviour
{
    [Header("Test Controls")]
    public Button testAddHeartButton;
    public Button testRemoveHeartButton;
    public Button testResetHeartsButton;
    public TextMeshProUGUI debugText;
    
    private UIManager uiManager;
    private int currentTestLives = 3;
    
    void Start()
    {
        uiManager = FindFirstObjectByType<UIManager>();
        
        if (testAddHeartButton != null)
            testAddHeartButton.onClick.AddListener(TestAddHeart);
        
        if (testRemoveHeartButton != null)
            testRemoveHeartButton.onClick.AddListener(TestRemoveHeart);
        
        if (testResetHeartsButton != null)
            testResetHeartsButton.onClick.AddListener(TestResetHearts);
        
        UpdateDebugText();
    }
    
    void TestAddHeart()
    {
        if (currentTestLives < 5)
        {
            currentTestLives++;
            if (uiManager != null)
            {
                uiManager.UpdateHeartsDisplay(currentTestLives);
            }
            UpdateDebugText();
        }
    }
    
    void TestRemoveHeart()
    {
        if (currentTestLives > 0)
        {
            currentTestLives--;
            if (uiManager != null)
            {
                uiManager.UpdateHeartsDisplay(currentTestLives);
            }
            UpdateDebugText();
        }
    }
    
    void TestResetHearts()
    {
        currentTestLives = 3;
        if (uiManager != null)
        {
            uiManager.UpdateHeartsDisplay(currentTestLives);
        }
        UpdateDebugText();
    }
    
    void UpdateDebugText()
    {
        if (debugText != null)
        {
            debugText.text = $"Test Lives: {currentTestLives}\nUIManager: {(uiManager != null ? "Found" : "NULL")}";
        }
    }
}
