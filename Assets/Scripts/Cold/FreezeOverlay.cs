using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Управление визуальным эффектом заморозки (UI Image)
/// </summary>
public class FreezeOverlay : MonoBehaviour
{
    [Header("Компоненты")]
    [SerializeField] public Image _freezeImage;
    
    [Header("Настройки")]
    [SerializeField] private float _maxAlpha = 0.7f;
    [SerializeField] private float _smoothSpeed = 5f;
    
    private void Start()
    {
        // Находим Image на этом объекте, если не назначен
        if (_freezeImage == null)
            _freezeImage = GetComponent<Image>();
        
        // Проверка
        if (_freezeImage == null)
        {
            Debug.LogError("FreezeOverlay: Нет компонента Image на объекте!");
            return;
        }
        
        // Начинаем с полностью прозрачного изображения
        Color color = _freezeImage.color;
        color.a = 0f;
        _freezeImage.color = color;
        
        Debug.Log("FreezeOverlay инициализирован");
    }
    
    private void Update()
    {
        // Проверка наличия FreezeSystem
//        if (FreezeSystem.Instance == null)
//        {
//            Debug.LogWarning("FreezeOverlay: FreezeSystem не найден!");
//            return;
//        }
        
        if (_freezeImage == null)
            return;
        
        // Получаем процент замерзания
        float freezePercent = FreezeSystem.Instance.GetFreezePercent();
        
        // Обновляем прозрачность
        Color color = _freezeImage.color;
        float targetAlpha = freezePercent * _maxAlpha;
        
        // Плавное изменение
        color.a = Mathf.Lerp(color.a, targetAlpha, Time.deltaTime * _smoothSpeed);
        
        _freezeImage.color = color;
    }
}