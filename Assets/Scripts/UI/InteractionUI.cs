using UnityEngine;
using TMPro;
using System.Collections;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance;
    
    public GameObject interactionPanel;
    public TextMeshProUGUI interactionText;
    
    [Header("Temporary Messages")]
    public GameObject tempMessagePanel;
    public TextMeshProUGUI tempMessageText;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        if (interactionPanel != null)
            interactionPanel.SetActive(false);
            
        if (tempMessagePanel != null)
            tempMessagePanel.SetActive(false);
    }
    
    public void ShowText(string text)
    {
        if (interactionPanel != null && interactionText != null)
        {
            interactionText.text = text;
            interactionPanel.SetActive(true);
        }
    }
    
    public void HideText()
    {
        if (interactionPanel != null)
            interactionPanel.SetActive(false);
    }
    
    public void ShowTempMessage(string message, float duration = 2f)
    {
        if (tempMessagePanel != null && tempMessageText != null)
        {
            StopAllCoroutines();
            StartCoroutine(ShowTempMessageCoroutine(message, duration));
        }
        else
        {
            Debug.LogWarning("TempMessage UI не настроен!");
        }
    }
    
    private IEnumerator ShowTempMessageCoroutine(string message, float duration)
    {
        tempMessageText.text = message;
        tempMessagePanel.SetActive(true);
        
        yield return new WaitForSeconds(duration);
        
        tempMessagePanel.SetActive(false);
    }
}