using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;
    public StickUI stickUI;

    public int fuel = 0;
    public int fuelOnGenerator = 0;

    public float sticks = 0;
    public float sticksOnFire = 0;
    public Interactable interactable; // огонь как основа
    public bool canFire = true;
    public float priceStick = 0.5f;
    public float timerToBurnStick = 0f;
    public float intervalToBurnStick = 30f;
    
    private HashSet<string> keys = new HashSet<string>();


    [Header("Item Collection")]
    public List<string> collectedItemIDs = new List<string>(); // ID собранных предметов


    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if(sticksOnFire <= 0 && !canFire) return;
        if(sticksOnFire >= 3 && canFire)
        {
            timerToBurnStick += Time.deltaTime;
            if(timerToBurnStick >= intervalToBurnStick)
            {
                timerToBurnStick = 0f;
                BurnStickOnFire(); // fire in interactable ///////
                if(sticksOnFire <= 3)
                {
                    interactable.DeleteFire();
                }
                interactable.CreateStick();
            }
        }

    }
    public void CollectItem(string itemID)
    {
        if (!collectedItemIDs.Contains(itemID))
        {
            collectedItemIDs.Add(itemID);
            Debug.Log($"Предмет {itemID} добавлен в коллекцию");
        }
    }
    public bool HasCollectedItem(string itemID)
    {
        return collectedItemIDs.Contains(itemID);
    }      
    public void AddKey(string keyID)
    {
        keys.Add(keyID);
        CollectItem(keyID); // Добавляем в коллекцию
        Debug.Log($"Подобран ключ: {keyID}");
        
        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.ShowTempMessage("Ключ подобран!", 2f);
        }
    }
    void HideNotification()
    {
        if (InteractionUI.Instance != null)
            InteractionUI.Instance.HideText();
    }
    
    public bool HasKey(string keyID)
    {
        return keys.Contains(keyID);
    }

    public void AddFuel(){
        fuel=fuel+1;
    }

    public void AddStick()
    {
        sticks = sticks+1f;
        stickUI.UpdateStickSum();

    }

    public void AddFuelToGenerator()
    {
        if (fuel>0)
        {
            fuel = fuel-1;
            fuelOnGenerator = fuelOnGenerator+1;
        }
    }    

    public void SpendStickToFire()
    {
        if (sticks > 0)
        {
            
            sticks = sticks-1f;
            sticksOnFire = sticksOnFire +1f;
            interactable.CreateStick();
            if (sticksOnFire >= 3 && canFire)
            {
                interactable.CreateFire();
            }
            stickUI.UpdateStickSum();
        }
        else
        {
            Debug.Log("Палки закончились");
        }
    }

    public void BurnStickOnFire()
    {
        if (sticksOnFire > 0)
        {
            sticksOnFire = sticksOnFire - priceStick;
        }
    }




}