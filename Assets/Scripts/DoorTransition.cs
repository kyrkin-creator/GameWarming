using UnityEngine;

public class DoorTransition : MonoBehaviour
{
    public Transform teleportPoint;
    public Transform player;

    private bool isTransitioning = false;

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что вошел именно игрок, и переход еще не начался
        if (other.CompareTag("Player") && !isTransitioning)
        {
            isTransitioning = true;
            
            // Вызываем переход через GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TransitionToRoom(teleportPoint, player);
            }
        }
    }
}