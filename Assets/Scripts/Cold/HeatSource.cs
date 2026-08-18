using UnityEngine;

/// <summary>
/// Компонент для указания зоны тепла через BoxCollider
/// (Может использоваться как маркер для визуализации)
/// </summary>
public class HeatSource : MonoBehaviour
{
    [Header("Настройки тепла")]
    [SerializeField] private float _heatIntensity = 1f;  // Интенсивность тепла (влияет на скорость обогрева)
    [SerializeField] private bool _isActive = true;      // Активен ли источник
    
    [Header("Визуализация")]
    [SerializeField] private bool _showGizmo = true;     // Показывать зону в редакторе
    [SerializeField] private Color _gizmoColor = new Color(1f, 0.5f, 0f, 0.3f);
    
    public float HeatIntensity => _heatIntensity;
    public bool IsActive => _isActive;
    
    // Ссылка на BoxCollider (если есть)
    private BoxCollider _boxCollider;
    
    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
    }
    
    // Получение границ зоны тепла
    public Bounds GetHeatBounds()
    {
        if (_boxCollider != null)
        {
            return _boxCollider.bounds;
        }
        return new Bounds(transform.position, Vector3.one);
    }
    
    // Для обратной совместимости (если используется старый код)
    public float heatRadius 
    { 
        get 
        {
            if (_boxCollider != null)
            {
                // Возвращаем наибольший размер коллайдера как радиус
                Vector3 size = _boxCollider.size;
                return Mathf.Max(size.x, size.z) / 2f;
            }
            return 5f;
        }
        set { } // Заглушка
    }
    
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!_showGizmo) return;
        
        Gizmos.color = _gizmoColor;
        
        // Если есть BoxCollider - визуализируем его
        if (_boxCollider != null)
        {
            Gizmos.DrawCube(
                _boxCollider.transform.position + _boxCollider.center,
                _boxCollider.size
            );
            
            Gizmos.color = new Color(_gizmoColor.r, _gizmoColor.g, _gizmoColor.b, 0.7f);
            Gizmos.DrawWireCube(
                _boxCollider.transform.position + _boxCollider.center,
                _boxCollider.size
            );
        }
        else
        {
            // Запасной вариант: сфера на месте объекта
            Gizmos.DrawSphere(transform.position, 2f);
        }
    }
    #endif
}