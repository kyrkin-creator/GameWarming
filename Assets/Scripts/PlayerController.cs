using Mono.Cecil;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float sprintSpeed = 4f;
    public float jumpSpeed = 5f;
    public float gravity = -9.81f;
    
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina = 100f;
    public float staminaDrainRate = 5f;    // Трата 5 за секунду при беге
    public float staminaRegenRate = 3f;    // Восстановление 3 за секунду
    public float jumpStaminaCost = 5f;     // Стоимость прыжка
    public System.Action<float> OnStaminaChanged;
    
    private CharacterController _characterController;
    private Vector2 _move;
    private Vector3 _velocity;
    private float _currentSpeed;
    private bool _isSprinting;
    private bool _canRegenStamina = true;
    private bool IsFlashlightLightOn = false;
    
    public static bool hasKey = false;
    
    [SerializeField] private GameObject flashlightLight;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private CinemachineCamera _cincam;
    
    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _currentSpeed = walkSpeed;
        currentStamina = maxStamina;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void Update()
    {
        // Обновляем grounded каждый кадр
        bool isGrounded = IsGrounded();
        
        // Сброс вертикальной скорости на земле
        if (isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }
        
        // Управление стаминой
        HandleStamina(isGrounded);
        
        // Обновляем скорость спринта в зависимости от стамины
        UpdateSprintSpeed();
        
        // Движение
        Vector3 moveDirection = (GetForward() * _move.y + GetRight() * _move.x).normalized;
        Vector3 movement = moveDirection * _currentSpeed;
        
        // Гравитация
        _velocity.y += gravity * Time.deltaTime;
        movement.y = _velocity.y;
        
        if (_characterController.enabled)
        {
            _characterController.Move(movement * Time.deltaTime);
        }
    }
    
    private void HandleStamina(bool isGrounded)
    {
        // Проверяем, бежит ли игрок и есть ли движение
        bool isMoving = _move.magnitude > 0.1f;
        bool canSprint = _isSprinting && isMoving && isGrounded && currentStamina > 0;
        
        if (canSprint)
            {
                currentStamina -= staminaDrainRate * Time.deltaTime;
                currentStamina = Mathf.Max(0, currentStamina);
                OnStaminaChanged?.Invoke(currentStamina / maxStamina); // Вызываем событие
                _canRegenStamina = false;
            }
        else
        {
            // Восстановление стамины (стоя или ходьба)
            if (currentStamina < maxStamina && !_canRegenStamina)
            {
                // Небольшая задержка перед восстановлением после бега
                Invoke(nameof(EnableStaminaRegen), 0.5f);
            }

            if (_canRegenStamina && currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(maxStamina, currentStamina);
                OnStaminaChanged?.Invoke(currentStamina / maxStamina); // Вызываем событие
            }
        }
        
        // Если стамина закончилась, принудительно выключаем спринт
        if (currentStamina <= 0)
        {
            _isSprinting = false;
            _currentSpeed = walkSpeed;
        }
    }
    
    private void EnableStaminaRegen()
    {
        _canRegenStamina = true;
    }
    
    private void UpdateSprintSpeed()
    {
        // Обновляем скорость в зависимости от состояния спринта и стамины
        if (_isSprinting && currentStamina > 0  && _move.magnitude > 0.1f)
        {
            _currentSpeed = sprintSpeed;
        }
        else
        {
            _currentSpeed = walkSpeed;
        }
    }
    
    public void OnMove(InputValue val)
    {
        _move = val.Get<Vector2>();
    }

    public void OnFlashlight(InputValue val)
    {
        IsFlashlightLightOn = !IsFlashlightLightOn;
        flashlightLight.SetActive(!IsFlashlightLightOn);
    }
    
    public void OnSprint(InputValue val)
    {
        _isSprinting = val.Get<float>() > 0.5f;
        
        // Если пытаемся начать бег, но нет стамины - запрещаем
        if (_isSprinting && currentStamina <= 0)
        {
            _isSprinting = false;
        }
    }
    
    public void OnJump(InputValue val)
    {
        if (val.isPressed && IsGrounded())
        {
            // Проверяем, хватает ли стамины на прыжок
            if (currentStamina >= jumpStaminaCost)
            {
                _velocity.y = Mathf.Sqrt(jumpSpeed * -2f * gravity);
                currentStamina -= jumpStaminaCost;
                currentStamina = Mathf.Max(0, currentStamina);
            }
            else
            {
                // Можно добавить звук или эффект, что нет стамины для прыжка
                Debug.Log("Недостаточно стамины для прыжка!");
            }
        }
    
        if (currentStamina >= jumpStaminaCost && IsGrounded())
        {
            _velocity.y = Mathf.Sqrt(jumpSpeed * -2f * gravity);
            currentStamina -= jumpStaminaCost;
            OnStaminaChanged?.Invoke(currentStamina / maxStamina);
        }
    }
    
    private bool IsGrounded()
    {
        if (groundCheck == null)
        {
            Debug.LogError("GroundCheck не назначен в инспекторе!");
            return false;
        }
        return Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
    }
    
    private Vector3 GetForward()
    {
        Vector3 forward = _cincam.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }
    
    private Vector3 GetRight()
    {
        Vector3 right = _cincam.transform.right;
        right.y = 0;
        return right.normalized;
    }
    
    // Метод для получения текущей стамины (можно использовать для UI)
    public float GetStaminaPercentage()
    {
        return currentStamina / maxStamina;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}