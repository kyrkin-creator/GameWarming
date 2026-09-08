using UnityEngine;

public class ItemSpawnPoint : MonoBehaviour
{
    //Роль: Точка спавна. 
    //Что делает:Отмечает место, где может появиться предмет.
    //Хранит информацию о этаже и статусе (использована/свободна)

    [Header("Spawn Point Settings")]
    public bool isUsed = false;
    public int floorLevel = 1;
    public string spawnPointID = "";
    
    [Header("Visual")]
    public Color gizmoColor = Color.green;
    public float gizmoRadius = 0.5f;
    
    void OnDrawGizmos()
    {
        Gizmos.color = isUsed ? Color.red : gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
        
        // Маленькая стрелка вверх
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.5f);
        Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, transform.position + Vector3.up * 0.3f + Vector3.right * 0.2f);
        Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, transform.position + Vector3.up * 0.3f - Vector3.right * 0.2f);
    }
}