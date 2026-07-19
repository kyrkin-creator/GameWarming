using UnityEngine;

public class HeatSource : MonoBehaviour
{
    [Header("Heat Settings")]
    public float heatRadius = 5f;        // Радиус действия тепла
    public float warmthAmount = 10f;     // Количество тепла в секунду (или мгновенно)
    public bool instantHeal = true;      // Мгновенно согреваться или постепенно
    
    [Header("Visuals")]
    public bool showGizmo = true;        // Показывать радиус в редакторе
    
    private void OnDrawGizmosSelected()
    {
        if (showGizmo)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, heatRadius);
        }
    }
}