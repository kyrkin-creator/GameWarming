using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Raycast : MonoBehaviour
{
    private Camera mainCamera;
    public float range = 5f;
    [SerializeField] private LayerMask layerMask;

    public bool pressE = false;
    private GameObject currentTarget;  
    private Outline currentOutline;
    private Interactable currentInteractable;

    public bool _doorOpenAnimation = false;

    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openTrigger = "Open";
    public string CloseDoorText;

    // Добавляем переменную для отслеживания нажатия
    private bool isInteracting = false;

    void Start()
    {
        mainCamera = Camera.main;
        
        Outline[] allOutlines = FindObjectsOfType<Outline>();
        foreach (Outline outline in allOutlines)
        {
            outline.enabled = false;
        }
    }
    
    void Update()
    {
        RayCast_Information();
        
        // Ловим нажатие в любом случае
        if (Input.GetKeyDown(KeyCode.E))
        {
            InteractWithCurrentTarget();
        }
    }
    
    private void RayCast_Information()
    {
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width/2, Screen.height/2, 0));
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, range, layerMask))
        {
            GameObject hitObject = hit.collider.gameObject;
            Outline outline = hitObject.GetComponent<Outline>();
            Interactable interactable = hitObject.GetComponent<Interactable>();

            if(currentTarget != hitObject)
            {
                // Сбрасываем старый Outline
                if(currentOutline != null)
                {
                    currentOutline.enabled = false;
                    currentOutline = null;
                }

                currentTarget = hitObject;
                currentInteractable = interactable;
                pressE = true;
                
                // Включаем новый Outline
                if(outline != null)
                {
                    currentOutline = outline;
                    currentOutline.enabled = true;
                }
                
                // Показываем текст
                if (InteractionUI.Instance != null && currentInteractable != null)
                {
                    string textToShow = currentInteractable.interactionText;
                    
                    if (currentInteractable.itemType == Interactable.ItemType.Door && currentInteractable.isLocked)
                    {
                        textToShow = CloseDoorText;
                    }
                    
                    InteractionUI.Instance.ShowText(textToShow);
                }
            }
            return;
        }

        // Если ничего не нашли
        if(currentTarget != null)
        {
            if(currentOutline != null)
            {
                currentOutline.enabled = false;
                currentOutline = null;
            }
            currentTarget = null;
            currentInteractable = null;
            pressE = false;
            
            if (InteractionUI.Instance != null)
                InteractionUI.Instance.HideText();
        }

        Debug.DrawRay(ray.origin, ray.direction*range, Color.red);
    }
    
    private void InteractWithCurrentTarget()
    {
        // Проверяем, есть ли цель и не взаимодействуем ли уже
        if (currentTarget == null || currentInteractable == null)
        {
            Debug.Log("Нет цели для взаимодействия");
            return;
        }
        
        // Предотвращаем множественное взаимодействие
        if (isInteracting) return;
        isInteracting = true;

        try
        {
            // Сохраняем ссылки локально, чтобы они не сбросились во время взаимодействия
            GameObject target = currentTarget;
            Interactable interactable = currentInteractable;
            Outline outline = currentOutline;
            
            // Обрабатываем разные типы предметов
            switch (interactable.itemType)
            {
                case Interactable.ItemType.Key:
                    if (PlayerInventory.Instance != null)
                    {
                        PlayerInventory.Instance.AddKey(interactable.itemID);
                        Destroy(target);
                    }
                    break;
                    
                case Interactable.ItemType.Door:
                    if (interactable.isLocked)
                    {
                        if (PlayerInventory.Instance != null && PlayerInventory.Instance.HasKey(interactable.requiredKeyID))
                        {
                            Debug.Log("Дверь открыта!");
                            interactable.isLocked = false;
                            interactable.Interact();
                            
                            if (InteractionUI.Instance != null)
                                InteractionUI.Instance.ShowText("Open door");
                        }
                        else
                        {
                            Debug.Log("Дверь заперта! Нужен ключ.");
                            if (InteractionUI.Instance != null)
                                InteractionUI.Instance.ShowTempMessage("Нужен ключ!", 1.5f);
                            return; // Выходим, не сбрасывая цель
                        }
                    }
                    else
                    {
                        interactable.Interact();
                    }
                    break;
                    
                default:
                    interactable.Interact();
                    break;
            }
            
            // Сбрасываем только если взаимодействие успешно (кроме случая с запертой дверью)
            if (outline != null)
            {
                outline.enabled = false;
            }
            
            currentTarget = null;
            currentInteractable = null;
            currentOutline = null;
            pressE = false;
            
            if (InteractionUI.Instance != null)
                InteractionUI.Instance.HideText();
        }
        finally
        {
            // Разрешаем следующее взаимодействие
            isInteracting = false;
        }
    }
}