using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SharePopup : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI messageText;
    public Button closeButton;
    public Button copyButton;
    
    [Header("Animation")]
    public float animationDuration = 0.3f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 originalScale;
    
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        
        // Set initial state
        canvasGroup.alpha = 0f;
        rectTransform.localScale = Vector3.zero;
        
        // Setup button listeners
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePopup);
            
        if (copyButton != null)
            copyButton.onClick.AddListener(CopyToClipboard);
    }
    
    public void ShowPopup(string message)
    {
        if (messageText != null)
            messageText.text = message;
            
        gameObject.SetActive(true);
        StartCoroutine(AnimateIn());
    }
    
    public void ClosePopup()
    {
        StartCoroutine(AnimateOut());
    }
    
    private void CopyToClipboard()
    {
        if (messageText != null)
        {
            UnityEngine.GUIUtility.systemCopyBuffer = messageText.text;
            
            // Show feedback
            if (copyButton != null)
            {
                TextMeshProUGUI buttonText = copyButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    string originalText = buttonText.text;
                    buttonText.text = "Kopyalandı!";
                    StartCoroutine(ResetButtonText(buttonText, originalText, 1.5f));
                }
            }
        }
    }
    
    private System.Collections.IEnumerator AnimateIn()
    {
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;
            float curveValue = scaleCurve.Evaluate(progress);
            
            canvasGroup.alpha = curveValue;
            rectTransform.localScale = Vector3.Lerp(Vector3.zero, originalScale, curveValue);
            
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        rectTransform.localScale = originalScale;
    }
    
    private System.Collections.IEnumerator AnimateOut()
    {
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;
            float curveValue = 1f - scaleCurve.Evaluate(progress);
            
            canvasGroup.alpha = curveValue;
            rectTransform.localScale = Vector3.Lerp(Vector3.zero, originalScale, curveValue);
            
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        rectTransform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }
    
    private System.Collections.IEnumerator ResetButtonText(TextMeshProUGUI buttonText, string originalText, float delay)
    {
        yield return new WaitForSeconds(delay);
        buttonText.text = originalText;
    }
}
