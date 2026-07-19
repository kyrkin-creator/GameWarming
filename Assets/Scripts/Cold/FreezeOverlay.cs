using UnityEngine;
using UnityEngine.UI;

public class FreezeOverlay : MonoBehaviour
{
    public Image freezeImage;
    
    void Start()
    {
        // Находим Image на этом объекте, если не назначен
        if (freezeImage == null)
            freezeImage = GetComponent<Image>();
        
        // Убеждаемся, что Image существует
        if (freezeImage == null)
        {
            Debug.LogError("FreezeOverlay: Нет компонента Image на объекте!");
            return;
        }
        
        // Начинаем с полностью прозрачного изображения
        Color color = freezeImage.color;
        color.a = 0f;
        freezeImage.color = color;
        
        Debug.Log("FreezeOverlay инициализирован");
    }
    
    void Update()
    {
        // Проверяем, существует ли FreezeSystem
        if (FreezeSystem.Instance == null)
        {
            Debug.LogWarning("FreezeOverlay: FreezeSystem не найден!");
            return;
        }
        
        if (freezeImage == null)
            return;
        
        // Получаем процент замерзания через метод
        float freezePercent = FreezeSystem.Instance.GetFreezePercent();
        
        // Плавно меняем прозрачность (опционально)
        Color color = freezeImage.color;
        float targetAlpha = freezePercent * 0.7f;
        
        // Плавное изменение (закомментируйте если не нужно)
        color.a = Mathf.Lerp(color.a, targetAlpha, Time.deltaTime * 5f);
        
        // Или мгновенное изменение (раскомментируйте для мгновенного эффекта)
        // color.a = targetAlpha;
        
        freezeImage.color = color;
    }
}