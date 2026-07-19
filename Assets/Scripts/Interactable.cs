using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public enum ItemType
    {
        Key,      // Ключ
        Door,     // Дверь
        Item      // Обычный предмет
    }
    
    public ItemType itemType = ItemType.Item;
    public string itemID = "";           // ID предмета (для ключей)
    public string interactionText = "Нажмите E"; // Текст в UI
    
    public UnityEvent onInteract; // Событие при взаимодействии
    
    // Для двери
    public bool isLocked = true;
    public string requiredKeyID = "";
    
    public Animator animator;
    public bool isOpen = false;
    
    [Header("Door Settings")]
    public bool canClose = true;  // Можно ли закрыть дверь обратно
    public string openText = "Открыть дверь";
    public string closeText = "Закрыть дверь";
    
    void Start()
    {
        if (isOpen)
        {
            animator.SetBool("isOpen", true);
        }
        
        // Обновляем текст взаимодействия в зависимости от состояния двери
        UpdateInteractionText();
    }
    
    void UpdateInteractionText()
    {
        if (itemType == ItemType.Door)
        {
            if (!isOpen)
                interactionText = openText;
            else if (canClose)
                interactionText = closeText;
            else
                interactionText = "Дверь открыта";
        }
    }
    
    public void Interact()
    {
        // Обрабатываем разные типы предметов
        switch (itemType)
        {
            case ItemType.Door:
                if (!isLocked)
                {
                    // Переключаем состояние двери (открыта/закрыта)
                    ToggleDoor();
                }
                else if (isLocked)
                {
                    Debug.Log($"Дверь {gameObject.name} заперта! Нужен ключ: {requiredKeyID}");
                    if (InteractionUI.Instance != null)
                        InteractionUI.Instance.ShowTempMessage("Door is close", 1.5f);
                }
                break;
                
            case ItemType.Key:
                Debug.Log($"Подобран ключ: {itemID}");
                Destroy(gameObject);
                break;
                
            default:
                onInteract?.Invoke();
                break;
        }
    }
    
    public void ToggleDoor()
    {
        if (isOpen && !canClose) return; // Нельзя закрыть если запрещено
        
        isOpen = !isOpen;
        
        if (isOpen)
        {
            animator.SetBool("isOpen", true);
            Debug.Log($"Дверь {gameObject.name} открыта");
        }
        else
        {
            animator.SetBool("isOpen", false);
            Debug.Log($"Дверь {gameObject.name} закрыта");
        }
        
        // Обновляем текст взаимодействия
        UpdateInteractionText();
    }
    
    // Этот метод оставляем для совместимости со старым кодом
    public void OpenDoor()
    {
        if (!isOpen && !isLocked)
        {
            isOpen = true;
            animator.SetBool("isOpen", true);
            UpdateInteractionText();
        }
    }
    
    // Добавляем метод для закрытия
    public void CloseDoor()
    {
        if (isOpen && canClose)
        {
            isOpen = false;
            animator.SetBool("isOpen", false);
            UpdateInteractionText();
        }
    }

    // Метод для отпирания двери ключом
    public void Unlock(string keyID)
    {
        if (isLocked && requiredKeyID == keyID)
        {
            isLocked = false;
            Debug.Log($"Дверь {gameObject.name} отперта!");
            
            if (InteractionUI.Instance != null)
                InteractionUI.Instance.ShowTempMessage("Дверь отперта!", 1.5f);
                
            UpdateInteractionText();
        }
    }
}