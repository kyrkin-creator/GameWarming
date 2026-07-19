using UnityEngine;
using UnityEngine.UI;

public class StaminaBarUI : MonoBehaviour
{
    [Header("References")]
    public Image staminaBar;           // Обычный Image (тип Simple)
    public PlayerController playerController;
    
    [Header("Animation Settings")]
    public float fadeSpeed = 5f;       // Скорость появления/исчезновения
    public float showDelay = 0.5f;     // Задержка перед исчезновением после восстановления
    
    private RectTransform _rectTransform;
    private float _maxWidth;
    private CanvasGroup _canvasGroup;
    private float _hideTimer;
    private bool _isVisible = true;
    
    void Start()
    {
        _rectTransform = staminaBar.GetComponent<RectTransform>();
        _maxWidth = _rectTransform.rect.width;
        
        // Добавляем CanvasGroup для плавного исчезновения, если его нет
        _canvasGroup = staminaBar.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = staminaBar.gameObject.AddComponent<CanvasGroup>();
        
        // Начинаем скрытыми (стамина полная в начале игры)
        if (playerController.GetStaminaPercentage() >= 0.99f)
        {
            _canvasGroup.alpha = 0f;
            _isVisible = false;
        }
        else
        {
            _canvasGroup.alpha = 1f;
            _isVisible = true;
        }
    }
    
    void Update()
    {
        if (playerController == null) return;
        
        float staminaPercentage = playerController.GetStaminaPercentage();
        float targetWidth = _maxWidth * staminaPercentage;
        
        // Меняем ширину полоски
        _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
        
        // Логика появления/исчезновения
        HandleVisibility(staminaPercentage);
    }
    
    void HandleVisibility(float staminaPercentage)
    {
        // Стамина НЕ полная (используется) -> показываем полоску
        if (staminaPercentage < 0.99f)
        {
            ShowBar();
            _hideTimer = showDelay; // Сбрасываем таймер
        }
        // Стамина полная -> запускаем таймер на скрытие
        else if (staminaPercentage >= 0.99f && _isVisible)
        {
            _hideTimer -= Time.deltaTime;
            if (_hideTimer <= 0f)
            {
                HideBar();
            }
        }
    }
    
    void ShowBar()
    {
        if (_isVisible) return;
        
        _isVisible = true;
        StartCoroutine(FadeBar(0f, 1f));
    }
    
    void HideBar()
    {
        if (!_isVisible) return;
        
        _isVisible = false;
        StartCoroutine(FadeBar(1f, 0f));
    }
    
    System.Collections.IEnumerator FadeBar(float fromAlpha, float toAlpha)
    {
        float elapsed = 0f;
        float duration = 1f / fadeSpeed;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            _canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            yield return null;
        }
        
        _canvasGroup.alpha = toAlpha;
    }
}