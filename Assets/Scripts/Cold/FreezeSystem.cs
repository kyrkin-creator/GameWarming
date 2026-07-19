using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class FreezeSystem : MonoBehaviour
{
    public static FreezeSystem Instance;
    
    [Header("Freeze Settings")]
    public float maxColdTime = 10f;
    public float coldGainRate = 1f;
    public float coldLossRate = 2f;
    
    [Header("Current State")]
    public float currentCold = 0f;
    public bool isInHeatZone = false;
    
    [Header("Visual Effects")]
    [SerializeField] private Image freezeOverlayImage;
    public Animator freezeAnimator;
    public string freezeParameter = "freezeLevel";
    
    [Header("Events")]
    public UnityEvent onFreezeStart;
    public UnityEvent onFreezeProgress;
    public UnityEvent onThawStart;
    public UnityEvent onPlayerDeath;
    
    private bool isDead = false;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        // Ищем FreezeOverlay
        if (freezeOverlayImage == null)
        {
            FreezeOverlay overlay = FindObjectOfType<FreezeOverlay>();
            if (overlay != null && overlay.freezeImage != null)
                freezeOverlayImage = overlay.freezeImage;
        }
        
        // Начинаем с прозрачного экрана
        if (freezeOverlayImage != null)
        {
            Color color = freezeOverlayImage.color;
            color.a = 0f;
            freezeOverlayImage.color = color;
        }
    }
    
    void Update()
    {
        if (isDead) return;
        
        // Проверяем, в тепле ли игрок
        bool wasInHeat = isInHeatZone;
        isInHeatZone = CheckIfInHeatZone();
        
        // Если состояние изменилось
        if (wasInHeat != isInHeatZone)
        {
            if (isInHeatZone)
            {
                Debug.Log("Вошел в зону тепла!");
                onThawStart?.Invoke();
            }
            else
            {
                Debug.Log("Покинул зону тепла!");
                onFreezeStart?.Invoke();
            }
        }
        
        // ОСНОВНАЯ ЛОГИКА ХОЛОДА - здесь всё просто
        if (!isInHeatZone && !isDead)
        {
            // Накапливаем холод
            currentCold += coldGainRate * Time.deltaTime;
            Debug.Log($"Холод: {currentCold:F1}/{maxColdTime}"); // Отладка
        }
        else if (isInHeatZone && currentCold > 0)
        {
            // Согреваемся
            currentCold -= coldLossRate * Time.deltaTime;
        }
        
        // Ограничиваем значение
        currentCold = Mathf.Clamp(currentCold, 0, maxColdTime);
        
        // Обновляем визуал
        UpdateVisualEffects();
        
        // Проверка смерти
        if (currentCold >= maxColdTime && !isDead)
        {
            DieFromCold();
        }
    }
    
    private bool CheckIfInHeatZone()
    {
        HeatSource[] heatSources = FindObjectsOfType<HeatSource>();
        
        foreach (HeatSource source in heatSources)
        {
            float distance = Vector3.Distance(transform.position, source.transform.position);
            if (distance <= source.heatRadius)
            {
                return true;
            }
        }
        return false;
    }
    
    private void UpdateVisualEffects()
    {
        float freezePercent = currentCold / maxColdTime;
        
        // Обновляем UI Image
        if (freezeOverlayImage != null)
        {
            Color color = freezeOverlayImage.color;
            color.a = freezePercent * 0.7f;
            freezeOverlayImage.color = color;
        }
        
        // Обновляем аниматор
        if (freezeAnimator != null)
        {
            freezeAnimator.SetFloat(freezeParameter, freezePercent);
        }
    }
    
    private void DieFromCold()
    {
        isDead = true;
        Debug.Log("ИГРОК ЗАМЕРЗ НАСМЕРТЬ!");
        
        if (freezeAnimator != null)
        {
            freezeAnimator.SetTrigger("die");
        }
        
        onPlayerDeath?.Invoke();
    }
    
    public float GetFreezePercent()
    {
        return currentCold / maxColdTime;
    }
    
    public bool IsPlayerAlive()
    {
        return !isDead;
    }
    
    public void ResetFreezeState()
    {
        isDead = false;
        currentCold = 0;
        UpdateVisualEffects();
    }
}