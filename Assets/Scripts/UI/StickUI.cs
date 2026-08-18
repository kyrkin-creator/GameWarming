using UnityEngine.UI;
using UnityEngine;
using TMPro;
public class StickUI : MonoBehaviour
{
    public RawImage stickImage;
    public PlayerInventory playerInventory;
    public TextMeshProUGUI stickSumText;
    
    void Start()
    {
        
    }

    public void UpdateStickSum()
    {
        stickSumText.text = playerInventory.sticks.ToString();
        Debug.Log("ОБНОВИЛ ТЕКСТ");
    }
}
