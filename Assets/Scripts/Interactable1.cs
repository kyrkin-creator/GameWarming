using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public enum ItemType
    {
        Key,      // Ключ
        Door,     // Дверь
        Item,  // Обычный предмет
        DoorBake, 
        LightSwitch,
        Stick,
        Fire,
        TrashDoor
    }
    
    public ItemType itemType = ItemType.Item;
    public string itemID = "";           // ID предмета (для ключей)
    public string interactionText = "Press E"; // Текст в UI
    
    public UnityEvent onInteract; // Событие при взаимодействии
    public PlayerInventory playerInventory;
    public StickUI stickUI;
    
    // Для двери
    public bool isLocked = true;
    public string requiredKeyID = "";
    
    public Animator doorAnimator;

    public Animator doorBakeAnimator;
    public Animator LightSwitchAnimator;
    public Animator trashDoorAnimator;

    public bool isOpen = false;
    public bool isSwitchOn = false;
    public GameObject[] lightObjects ;

    public GameObject fire;
    public GameObject smoke;
    public GameObject houseDoor;

    public GameObject[] sticksArray; 
    

    private bool isOpenDoorBake = false;
    private bool isOpenTrashDoor = false;
    
    [Header("Door Settings")]
    public bool canClose = true;  // Можно ли закрыть дверь обратно
    public string openText = "Open the door";
    public string closeText = "Close the door";

    
    
    void Start()
    {
        if (isOpen)
        {
            doorAnimator.SetBool("isOpen", true);
        }

        FreezeSystem.Instance.SetWarmSpeed(0f);
        
        if(fire!=null) fire.SetActive(false);
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
    
    public void Interact() //ПРОПИСЫВАТЬ В Raycast.cs --> InteractWithCurrentTarget, здесь анимации
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

            case ItemType.DoorBake:
                ToggleDoorBake();
                break;

            case ItemType.LightSwitch:
                ToggleLightSwitch();
                break;
            case ItemType.Fire:
                playerInventory.SpendStickToFire();
                if(playerInventory.sticksOnFire >= 3){
                    CreateFire();
                }
                CreateStick();
                break;
            case ItemType.Stick:
                Destroy(gameObject);
                playerInventory.AddStick();
                break;
            case ItemType.TrashDoor:
                ToggleTrashDoor();
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
            doorAnimator.SetBool("isOpen", true);
            FreezeSystem.Instance.SetWarmSpeed(FreezeSystem.Instance._warmSpeed - 0.1f);
        }
        else
        {
            doorAnimator.SetBool("isOpen", false);
            FreezeSystem.Instance.SetWarmSpeed(FreezeSystem.Instance._warmSpeed + 0.1f);
        }
        
        // Обновляем текст взаимодействия
        UpdateInteractionText();
    }

    public void ToggleDoorBake()
    {
        
        isOpenDoorBake = !isOpenDoorBake;
        
        if (isOpenDoorBake)
        {
            doorBakeAnimator.SetBool("isOpenDoorBake", true);
        }
        else
        {
            doorBakeAnimator.SetBool("isOpenDoorBake", false);
        }
        
    }
    public void ToggleTrashDoor()
    {
        isOpenTrashDoor = !isOpenTrashDoor;
        if (isOpenTrashDoor)
        {
            trashDoorAnimator.SetBool("isOpenTrashDoor", true);
        }

        if (!isOpenTrashDoor)
        {
            trashDoorAnimator.SetBool("isOpenTrashDoor", false);
        }
    }

    public void ToggleLightSwitch()
    {
        isSwitchOn = !isSwitchOn;

        if (isSwitchOn)
        {
            LightSwitchAnimator.SetBool("isSwitchOn", true);
            SetAllLightsActive(true);
        }
        else
        {
            LightSwitchAnimator.SetBool("isSwitchOn", false);
            SetAllLightsActive(false);
        }
    }
    private void SetAllLightsActive(bool isActive)
    {
        if (lightObjects == null || lightObjects.Length == 0)
        {
            Debug.LogWarning("Массив lightObjects пуст!");
            return;
        }
        
        foreach (GameObject light in lightObjects)
        {
            if (light != null)
                light.SetActive(isActive);
        }
    }
    
    // Этот метод оставляем для совместимости со старым кодом
    public void OpenDoor() // не вызывается нигде
    {
        if (!isOpen && !isLocked)
        {
            isOpen = true;
            doorAnimator.SetBool("isOpen", true);
            UpdateInteractionText();
            FreezeSystem.Instance.SetWarmSpeed(FreezeSystem.Instance._warmSpeed - 0.1f);
        }
        
    }
    
    // Добавляем метод для закрытия
    public void CloseDoor()
    {
        if (isOpen && canClose)
        {
            isOpen = false;
            doorAnimator.SetBool("isOpen", false);
            UpdateInteractionText();
            FreezeSystem.Instance.SetWarmSpeed(FreezeSystem.Instance._warmSpeed + 0.1f);
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

    public void CreateFire()
    {
        FreezeSystem.Instance.SetWarmSpeed(0.3f);
        fire.SetActive(true);
        if(smoke!=null) 
        {
            smoke.SetActive(true);
        } 
        Debug.Log("Огонь Активен");
        playerInventory.canFire = true;
    }
    public void DeleteFire()
    {
        FreezeSystem.Instance.SetWarmSpeed(0.1f);
        fire.SetActive(false);
        if(smoke!=null)
        {
            smoke.SetActive(true);
        }
        Debug.Log("Огонь неактивен");
        playerInventory.canFire = false;
    }
    public void CreateStick()
    {
        if (playerInventory.sticksOnFire >= 1)
        {
            sticksArray[0].SetActive(true);
        }
        else sticksArray[0].SetActive(false);


        if (playerInventory.sticksOnFire >= 2)
        {
            sticksArray[1].SetActive(true);
        }
        else sticksArray[1].SetActive(false);


        if (playerInventory.sticksOnFire >= 3)
        {
            sticksArray[2].SetActive(true);
        }
        else sticksArray[2].SetActive(false);


        if (playerInventory.sticksOnFire >= 4)
        {
            sticksArray[3].SetActive(true);
        }
        else sticksArray[3].SetActive(false);


        if (playerInventory.sticksOnFire >= 5)
        {
            sticksArray[4].SetActive(true);
        }
        else sticksArray[4].SetActive(false);


        if (playerInventory.sticksOnFire >= 6)
        {
            sticksArray[5].SetActive(true);
        }
        else sticksArray[5].SetActive(false);


        if (playerInventory.sticksOnFire >= 7)
        {
            sticksArray[6].SetActive(true);
        }
        else sticksArray[6].SetActive(false);


        if (playerInventory.sticksOnFire >= 8)
        {
            sticksArray[7].SetActive(true);
        }
        else sticksArray[7].SetActive(false);


        if (playerInventory.sticksOnFire >= 9)
        {
            sticksArray[8].SetActive(true);
        }
        else sticksArray[8].SetActive(false);


        if (playerInventory.sticksOnFire >= 10)
        {
            sticksArray[9].SetActive(true);
        }
        else sticksArray[9].SetActive(false);


    }
}