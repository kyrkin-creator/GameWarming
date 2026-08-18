using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;
    public float sticks = 0;
    public float sticksOnFire = 0;
    public Interactable interactable; // огонь как основа
    public bool canFire = true;
    public float priceStick = 0.5f;
    public float timerToBurnStick = 0f;
    public float intervalToBurnStick = 30f;
    
    private HashSet<string> keys = new HashSet<string>();
    
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

        timerToBurnStick += Time.deltaTime;
        if(timerToBurnStick >= intervalToBurnStick)
        {
            timerToBurnStick = 0f;
            BurnStickOnFire();
            interactable.CreateStick();
        }
    }
    
    public void AddKey(string keyID)
    {
        keys.Add(keyID);
        Debug.Log($"Подобран ключ: {keyID}");
        
        // Показываем уведомление через UI
        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.ShowText("Ключ подобран!");
            Invoke("HideNotification", 2f);
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

    public void AddStick()
    {
        sticks = sticks+1f;

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