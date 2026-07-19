using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;
    
    private HashSet<string> keys = new HashSet<string>();
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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
}