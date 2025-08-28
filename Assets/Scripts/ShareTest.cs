using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShareTest : MonoBehaviour
{
    [Header("Test UI")]
    public Button testShareButton;
    public InputField scoreInputField;
    public TextMeshProUGUI resultText;
    
    private UIManager uiManager;
    
    void Start()
    {
        uiManager = FindFirstObjectByType<UIManager>();
        
        if (testShareButton != null)
            testShareButton.onClick.AddListener(TestShare);
            
        if (scoreInputField != null)
            scoreInputField.text = "50";
            
        if (resultText != null)
            resultText.text = "Test için skor girin ve PAYLAŞ butonuna tıklayın";
    }
    
    public void TestShare()
    {
        if (uiManager == null)
        {
            Debug.LogError("UIManager bulunamadı!");
            if (resultText != null)
                resultText.text = "HATA: UIManager bulunamadı!";
            return;
        }
        
        // Simulate a score for testing
        int testScore = 50;
        if (scoreInputField != null && !string.IsNullOrEmpty(scoreInputField.text))
        {
            if (int.TryParse(scoreInputField.text, out int parsedScore))
            {
                testScore = parsedScore;
            }
        }
        
        // Create a temporary GameManager for testing
        GameObject tempGameManager = new GameObject("TempGameManager");
        GameManager tempGM = tempGameManager.AddComponent<GameManager>();
        
        // Use reflection to set the private score field for testing
        var scoreField = typeof(GameManager).GetField("currentScore", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (scoreField != null)
        {
            scoreField.SetValue(tempGM, testScore);
        }
        
        // Temporarily replace the UIManager's gameManager reference
        var gameManagerField = typeof(UIManager).GetField("gameManager", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (gameManagerField != null)
        {
            gameManagerField.SetValue(uiManager, tempGM);
        }
        
        // Test the share functionality
        uiManager.ShareScore();
        
        if (resultText != null)
            resultText.text = $"Test tamamlandı! {testScore} puan ile paylaşım test edildi.";
            
        // Clean up
        Destroy(tempGameManager);
        
        // Restore original gameManager reference
        if (gameManagerField != null)
        {
            var originalGM = FindFirstObjectByType<GameManager>();
            gameManagerField.SetValue(uiManager, originalGM);
        }
    }
    
    void OnDestroy()
    {
        if (testShareButton != null)
            testShareButton.onClick.RemoveListener(TestShare);
    }
}
