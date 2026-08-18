using System.Collections.Generic;
using UnityEngine;

public class ProcedualGeneratorRooms : MonoBehaviour
{
    [SerializeField] public GameObject player_;
    
    [Header("Rooms")]
    public Rooms[] roomsPrefabs;
    private List<Rooms> generatedRooms = new List<Rooms>();
    
    [SerializeField] private Transform startRoomStartPoint; // Исправлено имя
    [SerializeField] private GameObject endRoomPrefab; // Переименовано для ясности
    private bool EndGenerated = false;
    
    [Header("Generation Settings")]
    [SerializeField] private int maxRooms = 10; // Лимит комнат для предотвращения бесконечного цикла

    void Awake()
    {
        GenerateRooms();
    }

    public void GenerateRooms()
    {
        // Создаем стартовую комнату
        Rooms startRoom = Instantiate(roomsPrefabs[0], startRoomStartPoint.position, Quaternion.identity);
        generatedRooms.Add(startRoom);
        
        // Генерируем комнаты пока не достигнут лимит или не создана конечная комната
        int attempts = 0;
        while (!EndGenerated && generatedRooms.Count < maxRooms && attempts < 100)
        {
            attempts++;
            
            // Выбираем случайную комнату из доступных префабов
            int randomIndex = Random.Range(0, roomsPrefabs.Length);
            Rooms newRoomPrefab = roomsPrefabs[randomIndex];
            
            // Создаем новую комнату
            Rooms newRoom = Instantiate(newRoomPrefab);
            
            // Позиционируем комнату
            Vector3 lastEndPoint = generatedRooms[generatedRooms.Count - 1].endPoint.position;
            Vector3 offset = newRoom.startPoint.localPosition;
            newRoom.transform.position = lastEndPoint - offset;
            
            // Проверяем, является ли комната конечной
            if (newRoom.IsEndRoom)
            {
                EndGenerated = true;
            }
            
            generatedRooms.Add(newRoom);
            Debug.Log($"Сгенерирована комната #{generatedRooms.Count}. Конечная: {newRoom.IsEndRoom}");
        }
        
        if (EndGenerated)
        {
            Debug.Log($"Генерация завершена! Создано {generatedRooms.Count} комнат.");
        }
        else
        {
            Debug.LogWarning($"Достигнут лимит комнат ({maxRooms}) или превышено количество попыток.");
        }
    }
}