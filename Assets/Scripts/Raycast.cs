using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Raycast : MonoBehaviour
{
    private Camera mainCamera;
    public float range = 5f;
    [SerializeField] private LayerMask layerMask;

    public bool pressE = false;
    private GameObject currentTarget;  
    private Outline currentOutline;
    private Interactable currentInteractable; // Добавляем ссылку на компонент Interactable

    public bool _doorOpenAnimation = false;

    [SerializeField] private Animator doorAnimator;

    [SerializeField] private string openTrigger = "Open";

    public string CloseDoorText;

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
        
        if (pressE && Input.GetKeyDown(KeyCode.E))
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

        if (outline != null)
        {
            if(currentTarget != hitObject)
            {
                if(currentTarget != null)
                {
                    currentOutline.enabled = false;
                }

                currentTarget = hitObject;
                currentOutline = outline;
                currentInteractable = interactable;
                currentOutline.enabled = true;
                pressE = true;
                
                // Показываем текст в UI
                if (InteractionUI.Instance != null && currentInteractable != null)
                {
                    // Для двери текст может меняться в зависимости от состояния
                    string textToShow = currentInteractable.interactionText;
                    
                    // Если это дверь и она заперта, показываем специальный текст
                    if (currentInteractable.itemType == Interactable.ItemType.Door && currentInteractable.isLocked)
                    {
                        textToShow = CloseDoorText;
                    }
                    
                    InteractionUI.Instance.ShowText(textToShow);
                }
            }
            return;
        }
    }

    if(currentTarget != null)
    {
        currentOutline.enabled = false;
        currentTarget = null;
        currentOutline = null;
        currentInteractable = null;
        pressE = false;
        
        // Скрываем UI текст
        if (InteractionUI.Instance != null)
            InteractionUI.Instance.HideText();
    }

    Debug.DrawRay(ray.origin, ray.direction*range, Color.red);
}
    
    private void InteractWithCurrentTarget()
    {
        if (currentTarget != null && currentInteractable != null)
        {
            // Обрабатываем разные типы предметов
            switch (currentInteractable.itemType)
            {
                case Interactable.ItemType.Key:
                    // Подбираем ключ
                    PlayerInventory.Instance.AddKey(currentInteractable.itemID);
                    Destroy(currentTarget);
                    break;
                    
                case Interactable.ItemType.Door:
                    // Проверяем дверь
                    if (currentInteractable.isLocked)
                    {
                        if (PlayerInventory.Instance.HasKey(currentInteractable.requiredKeyID))
                        {
                            Debug.Log("Дверь открыта!");
                            currentInteractable.isLocked = false;
                            currentInteractable.Interact(); 


                            
                            if (InteractionUI.Instance != null)
                                InteractionUI.Instance.ShowText("Open door");
                        }
                        else
                        {
                            Debug.Log("Дверь заперта! Нужен ключ.");
                            if (InteractionUI.Instance != null)
                                InteractionUI.Instance.ShowText("Нужен ключ!");
                        }
                    }
                    else
                    {
                        currentInteractable.Interact();
                    }
                    break;
                    
                default:
                    currentInteractable.Interact();
                    break;
            }
            
            // Сбрасываем после взаимодействия
            currentOutline.enabled = false;
            currentTarget = null;
            currentOutline = null;
            currentInteractable = null;
            pressE = false;
            
            if (InteractionUI.Instance != null)
                InteractionUI.Instance.HideText();
        }
    }













}