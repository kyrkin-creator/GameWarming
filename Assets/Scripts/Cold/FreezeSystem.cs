using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Система температуры в градусах Цельсия с поддержкой BoxCollider для зон тепла
/// </summary>
public class FreezeSystem : MonoBehaviour
{
    public static FreezeSystem Instance;

    [Header("=== СИСТЕМА ТЕМПЕРАТУРЫ ===")]
    [Header("Единицы измерения")]
    [SerializeField] private float _baseTemperature = 0f;           // Базовая температура (0°C)
    [SerializeField] private float _maxTemperature = -6f;           // Максимальная температура до смерти (-10°C)
    
    [Header("Текущее состояние")]
    [SerializeField] private float _currentTemperature = 0f;        // Текущая температура (0°C = здоров, 10°C = смерть)
    [SerializeField] private bool _isInHeatZone = false;            // Находится ли в зоне тепла
    
    [Header("=== НАСТРОЙКИ СКОРОСТИ ===")]
    [Header("Скорости изменения температуры")]
    [SerializeField] private float _freezeSpeed = -0.1f;             // Скорость замерзания (градусов в секунду)
    [SerializeField] public float _warmSpeed = 0.3f;               // Скорость обогрева (градусов в секунду)
    [SerializeField] private float _warmSpeedMultiplier = 1f;       // Множитель скорости обогрева (для динамического изменения)
    
    [Header("=== ЗОНЫ ТЕПЛА ===")]
    [Header("Проверка через BoxCollider")]
    [SerializeField] private BoxCollider _heatZoneCollider;         // Коллайдер зоны тепла (можно назначить в инспекторе)
    [SerializeField] private bool _useColliderCheck = true;         // Использовать коллайдер или проверку по тегу
    
    [Header("=== ВИЗУАЛЬНЫЕ ЭФФЕКТЫ ===")]
    [Header("UI и анимации")]
    [SerializeField] private Image _freezeOverlayImage;             // Затемнение экрана
    [SerializeField] private Animator _freezeAnimator;              // Аниматор игрока
    [SerializeField] private string _freezeParameter = "freezeLevel"; // Параметр в аниматоре
    [SerializeField] private GameObject _particleSystem;
    
    [Header("=== СОБЫТИЯ ===")]
    [Header("Unity Events")]
    public UnityEvent onFreezeStart;        // Начало замерзания
    public UnityEvent onFreezeProgress;     // Прогресс замерзания (каждый градус)
    public UnityEvent onThawStart;          // Начало оттаивания
    public UnityEvent onPlayerDeath;        // Смерть от холода
    public UnityEvent<float> onTemperatureChanged; // Событие при изменении температуры (передаёт текущую температуру)
    
    // Приватные переменные
    private bool _isDead = false;
    private float _previousTemperature = 0f;
    private Collider _playerCollider;
    private bool _wasInHeatZone = false;
    
    // Публичные свойства для доступа из других скриптов
    public float CurrentTemperature => _currentTemperature;
    public float MaxTemperature => _maxTemperature;
    public bool IsDead => _isDead;
    public bool IsInHeatZone => _isInHeatZone;
    
    // Свойства для изменения скорости динамически
    public float FreezeSpeed 
    { 
        get => _freezeSpeed; 
        set => _freezeSpeed = Mathf.Max(0f, value); 
    }
    
    public float WarmSpeed 
    { 
        get => _warmSpeed; 
        set => _warmSpeed = Mathf.Max(0f, value); 
    }
    
    public float WarmSpeedMultiplier 
    { 
        get => _warmSpeedMultiplier; 
        set => _warmSpeedMultiplier = Mathf.Max(0f, value); 
    }

    // ============================
    // M O N O B E H A V I O U R
    // ============================

    private void Awake()
    {
        // Singleton
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Получаем коллайдер игрока
        _playerCollider = GetComponent<Collider>();
        if (_playerCollider == null)
        {
            Debug.LogWarning("FreezeSystem: На игроке нет Collider! Проверка зон тепла может не работать.");
        }
    }

    private void Start()
    {
        // Ищем FreezeOverlay если не назначен
        if (_freezeOverlayImage == null)
        {
            FreezeOverlay overlay = FindObjectOfType<FreezeOverlay>();
            if (overlay != null && overlay._freezeImage != null)
                _freezeOverlayImage = overlay._freezeImage;
        }

        // Ищем Animator если не назначен
        if (_freezeAnimator == null)
        {
            _freezeAnimator = GetComponent<Animator>();
        }

        // Начинаем с прозрачного экрана
        if (_freezeOverlayImage != null)
        {
            Color color = _freezeOverlayImage.color;
            color.a = 0f;
            _freezeOverlayImage.color = color;
        }

        // Инициализация
        _currentTemperature = _baseTemperature;
        _previousTemperature = _currentTemperature;
        
        Debug.Log($"FreezeSystem инициализирован. Температура: {_currentTemperature}°C. Порог смерти: {_maxTemperature}°C");
    }

    private void Update()
    {

        // Проверяем, находится ли игрок в зоне тепла
        _isInHeatZone = CheckIfInHeatZone();
        
        // Обработка изменения состояния (вход/выход из тепла)
        HandleHeatZoneStateChange();
        
        // Обновляем температуру
        UpdateTemperature();
        
        // Обновляем визуальные эффекты
        UpdateVisualEffects();
        
        // Проверка смерти
        CheckDeath();
    }

    // ============================
    // О С Н О В Н А Я   Л О Г И К А
    // ============================

    /// <summary>
    /// Проверка, находится ли игрок в зоне тепла (через BoxCollider или тег)
    /// </summary>
    private bool CheckIfInHeatZone()
    {
        // Если коллайдер назначен в инспекторе и включена проверка через коллайдер
        if (_useColliderCheck && _heatZoneCollider != null && _playerCollider != null)
        {
            // Простая проверка пересечения коллайдеров (более производительно)
            return _playerCollider.bounds.Intersects(_heatZoneCollider.bounds);
        }
        
        // Альтернативный способ: проверка через тег
        if (!_useColliderCheck)
        {
            // Используем проверку через триггеры (если у тебя есть объекты с тегом "HeatZone")
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.5f);
            foreach (Collider col in hitColliders)
            {
                if (col.CompareTag("HeatZone"))
                    return true;
            }
        }
        
        return false;
    }

    /// <summary>
    /// Обработка входа/выхода из зоны тепла
    /// </summary>
    private void HandleHeatZoneStateChange()
    {
        if (_wasInHeatZone != _isInHeatZone)
        {
            if (_isInHeatZone)
            {
                Debug.Log("🔥 ВОШЕЛ В ЗОНУ ТЕПЛА! Начинаем отогреваться.");
                onThawStart?.Invoke();
                _particleSystem.SetActive(false);
            }
            else
            {
                Debug.Log("❄️ ПОКИНУЛ ЗОНУ ТЕПЛА! Начинаем замерзать.");
                _particleSystem.SetActive(true);
                onFreezeStart?.Invoke();
            }
            _wasInHeatZone = _isInHeatZone;
        }
    }

    /// <summary>
    /// Обновление температуры в зависимости от нахождения в тепле
    /// </summary>
    private void UpdateTemperature()
    {
        float previousTemp = _currentTemperature;
        
        if (!_isInHeatZone && !_isDead)
        {
            // ЗАМЕРЗАНИЕ: температура увеличивается
            _currentTemperature += _freezeSpeed * Time.deltaTime;
            
            // Событие при достижении нового градуса
            CheckTemperatureProgress();
        }
        else if (_isInHeatZone && _currentTemperature > _baseTemperature)
        {
            // ОТОГРЕВ: температура уменьшается
            float effectiveWarmSpeed = _warmSpeed * _warmSpeedMultiplier;
            _currentTemperature -= effectiveWarmSpeed * Time.deltaTime;
            
            // Событие при достижении нового градуса
            CheckTemperatureProgress();
        }
        
        // Ограничиваем температуру в допустимых пределах
        _currentTemperature = Mathf.Clamp(_currentTemperature, _baseTemperature, _maxTemperature);
        
        // Вызываем событие при изменении температуры
        if (Mathf.Abs(_currentTemperature - previousTemp) > 0.01f)
        {
            onTemperatureChanged?.Invoke(_currentTemperature);
        }
    }

    /// <summary>
    /// Проверка прогресса замерзания (каждый градус)
    /// </summary>
    private void CheckTemperatureProgress()
    {
        int currentDegree = Mathf.FloorToInt(_currentTemperature);
        int previousDegree = Mathf.FloorToInt(_previousTemperature);
        
        if (currentDegree > previousDegree && _currentTemperature > _baseTemperature)
        {
            // Достигнут новый градус замерзания
            Debug.Log($"🌡️ ТЕМПЕРАТУРА: {_currentTemperature:F1}°C (Достигнут {currentDegree}°C)");
            onFreezeProgress?.Invoke();
        }
        
        _previousTemperature = _currentTemperature;
    }

    /// <summary>
    /// Обновление визуальных эффектов (UI и анимация)
    /// </summary>
    private void UpdateVisualEffects()
    {
        // Вычисляем процент замерзания (0 = здоров, 1 = смерть)
        float freezePercent = GetFreezePercent();
        
        // Обновляем UI Image (затемнение экрана)
        if (_freezeOverlayImage != null)
        {
            Color color = _freezeOverlayImage.color;
            float targetAlpha = freezePercent * 0.7f;
            color.a = Mathf.Lerp(color.a, targetAlpha, Time.deltaTime * 5f);
            _freezeOverlayImage.color = color;
        }
        
        // Обновляем аниматор
        if (_freezeAnimator != null)
        {
            _freezeAnimator.SetFloat(_freezeParameter, freezePercent);
        }
    }

    /// <summary>
    /// Проверка смерти от холода
    /// </summary>
    private void CheckDeath()
    {
        if (_currentTemperature >= _maxTemperature && !_isDead)
        {
            DieFromCold();
        }
    }

    /// <summary>
    /// Смерть от холода
    /// </summary>
    private void DieFromCold()
    {
        _isDead = true;
        Debug.Log($"💀 ИГРОК ЗАМЕРЗ НАСМЕРТЬ! Температура: {_currentTemperature:F1}°C");
        
        // Анимация смерти
        if (_freezeAnimator != null)
        {
            _freezeAnimator.SetTrigger("die");
        }
        
        // Вызываем событие
        onPlayerDeath?.Invoke();
    }

    // ============================
    // П У Б Л И Ч Н Ы Е   М Е Т О Д Ы
    // ============================

    /// <summary>
    /// Получение процента замерзания (0-1)
    /// </summary>
    public float GetFreezePercent()
    {
        return Mathf.Clamp01((_currentTemperature - _baseTemperature) / (_maxTemperature - _baseTemperature));
    }

    /// <summary>
    /// Получение текущей температуры в градусах Цельсия
    /// </summary>
    public float GetCurrentTemperature()
    {
        return _currentTemperature;
    }

    /// <summary>
    /// Проверка, жив ли игрок
    /// </summary>
    public bool IsPlayerAlive()
    {
        return !_isDead;
    }

    /// <summary>
    /// Сброс состояния системы (при респавне)
    /// </summary>
    public void ResetFreezeState()
    {
        _isDead = false;
        _currentTemperature = _baseTemperature;
        _previousTemperature = _baseTemperature;
        _wasInHeatZone = false;
        
        // Сброс визуала
        if (_freezeOverlayImage != null)
        {
            Color color = _freezeOverlayImage.color;
            color.a = 0f;
            _freezeOverlayImage.color = color;
        }
        
        if (_freezeAnimator != null)
        {
            _freezeAnimator.SetFloat(_freezeParameter, 0f);
        }
        
        Debug.Log($"🔄 Система заморозки сброшена. Температура: {_currentTemperature}°C");
    }

    /// <summary>
    /// Добавление тепла мгновенно (например, от костра или напитка)
    /// </summary>
    public void AddWarmth(float warmthAmount)
    {
        if (_isDead) return;
        
        _currentTemperature = Mathf.Max(_baseTemperature, _currentTemperature - warmthAmount);
        Debug.Log($"🔥 Добавлено тепло: -{warmthAmount}°C. Текущая температура: {_currentTemperature:F1}°C");
    }

    /// <summary>
    /// Установка скорости замерзания (для динамического изменения)
    /// </summary>
    public void SetFreezeSpeed(float speed)
    {
        _freezeSpeed = Mathf.Max(0f, speed);
        Debug.Log($"❄️ Скорость замерзания установлена на {_freezeSpeed}°C/сек");
    }

    /// <summary>
    /// Установка скорости обогрева (для динамического изменения)
    /// </summary>
    public void SetWarmSpeed(float speed)
    {
        _warmSpeed = Mathf.Max(0f, speed);
        Debug.Log($"🔥 Скорость обогрева установлена на {_warmSpeed}°C/сек");
    }

    /// <summary>
    /// Установка множителя скорости обогрева (для временных баффов)
    /// </summary>
    public void SetWarmSpeedMultiplier(float multiplier)
    {
        _warmSpeedMultiplier = Mathf.Max(0f, multiplier);
        Debug.Log($"🔥 Множитель обогрева: {_warmSpeedMultiplier}x");
    }

    // ============================
    // В И З У А Л И З А Ц И Я   В   Р Е Д А К Т О Р Е
    // ============================

    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Визуализация зоны тепла (BoxCollider)
        if (_heatZoneCollider != null && _useColliderCheck)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
            Gizmos.DrawCube(
                _heatZoneCollider.transform.position + _heatZoneCollider.center,
                _heatZoneCollider.size
            );
            
            // Рамка
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            Gizmos.DrawWireCube(
                _heatZoneCollider.transform.position + _heatZoneCollider.center,
                _heatZoneCollider.size
            );
        }
    }
    #endif
}