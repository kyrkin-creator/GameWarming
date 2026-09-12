using System.Collections.Generic;
using UnityEngine;

public class ProcedualGeneratorRooms : MonoBehaviour
{
    [SerializeField] public GameObject player_;

    [Header("ItemGenerator")]
    [SerializeField] public ItemGenerator itemGenerator;

    [Header("1st floor 'Rooms' ")]
    public Rooms[] roomsPrefabs;
    public List<Rooms> generatedRooms = new List<Rooms>();
    
    [SerializeField] private Transform startRoomStartPoint;
    [SerializeField] private GameObject endRoomPrefab;
    private bool EndGenerated = false;
    
    [Header("Generation Rooms Settings")]
    [SerializeField] private int maxRooms = 10;

    [Header("2nd floor 'Forest' ")]
    [SerializeField] private GameObject forestMap;

    [Header("3rd floor 'DangerForest' ")]
    [SerializeField] private GameObject dangerMap;

    [Header(" 'The Fog Settings' ")]
    public bool isTheFog = false;
    public float theFogStart = 2f;    
    public float theFogEnd = 15f;
   // public float theFogColor = 606060;



    void Awake()
    {
        GenerateRooms();
        RenderSettings.fog = isTheFog; // off/on theFog
    }


    public void GenerateRooms()
    {
        // Создаем стартовую комнату
        Rooms startRoom = Instantiate(roomsPrefabs[0], startRoomStartPoint.position, Quaternion.identity);
        generatedRooms.Add(startRoom);
        
        // Генерируем комнаты
        int attempts = 0;
        bool endRoomSpawned = false;
        
        while (!endRoomSpawned && generatedRooms.Count < maxRooms && attempts < 100)
        {
            attempts++;
            
            // Выбираем случайную комнату
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
                endRoomSpawned = true;
                EndGenerated = true;
            }
            
            generatedRooms.Add(newRoom);
            Debug.Log($"Сгенерирована комната #{generatedRooms.Count}. Конечная: {newRoom.IsEndRoom}");
        }
        
        // Если конечная комната не была заспавнена, создаем её принудительно
        if (!endRoomSpawned)
        {
            Debug.Log("Конечная комната не найдена. Создаем принудительно...");
            SpawnEndRoom();
        }
        
        Debug.Log($"Генерация завершена! Создано {generatedRooms.Count} комнат.");
    }
    
    void SpawnEndRoom()
    {
        // Находим префаб конечной комнаты
        Rooms endRoomPrefab = null;
        
        foreach (Rooms room in roomsPrefabs)
        {
            if (room.IsEndRoom)
            {
                endRoomPrefab = room;
                break;
            }
        }
        
        // Если префаб конечной комнаты не найден, создаем из endRoomPrefab (переменная в инспекторе)
        if (endRoomPrefab == null && this.endRoomPrefab != null)
        {
            endRoomPrefab = this.endRoomPrefab.GetComponent<Rooms>();
        }
        
        // Если всё еще нет - создаем простую комнату и помечаем как конечную
        if (endRoomPrefab == null)
        {
            Debug.LogWarning("Префаб конечной комнаты не найден! Создаем последнюю комнату как конечную.");
            endRoomPrefab = roomsPrefabs[Random.Range(0, roomsPrefabs.Length)];
            
            // Создаем комнату
            Rooms lastRoom = Instantiate(endRoomPrefab);
            
            // Позиционируем
            Vector3 lastEndPoint = generatedRooms[generatedRooms.Count - 1].endPoint.position;
            Vector3 offset = lastRoom.startPoint.localPosition;
            lastRoom.transform.position = lastEndPoint - offset;
            
            // Помечаем как конечную (добавляем компонент если нужно)
            lastRoom.IsEndRoom = true;
            generatedRooms.Add(lastRoom);
            
            Debug.Log($"Создана принудительная конечная комната #{generatedRooms.Count}");
            return;
        }
        
        // Создаем конечную комнату из префаба
        Rooms endRoom = Instantiate(endRoomPrefab);
        
        // Позиционируем
        Vector3 lastPoint = generatedRooms[generatedRooms.Count - 1].endPoint.position;
        Vector3 endOffset = endRoom.startPoint.localPosition;
        endRoom.transform.position = lastPoint - endOffset;
        
        // Помечаем как конечную (на всякий случай)
        endRoom.IsEndRoom = true;
        EndGenerated = true;
        
        generatedRooms.Add(endRoom);
        Debug.Log($"Создана конечная комната #{generatedRooms.Count}");
    }


    public void ClearAllRooms()
    {
        if (generatedRooms == null || generatedRooms.Count == 0)
        {
            Debug.Log("Нет комнат для удаления");
            return;
        }

        Debug.Log($"Начинаем удаление {generatedRooms.Count} комнат...");
        
        // Сохраняем список для удаления (чтобы избежать проблем с изменением коллекции)
        List<Rooms> roomsToDelete = new List<Rooms>(generatedRooms);
        
        foreach (Rooms room in roomsToDelete)
        {
            if (room != null && room.gameObject != null)
            {
                // Если у комнаты есть дети с точками спавна, они удалятся вместе с ней
                Destroy(room.gameObject);
            }
        }
        
        // Очищаем список
        generatedRooms.Clear();
        
        // Сбрасываем флаги
        EndGenerated = false;
        
        Debug.Log("Все комнаты удалены!");
    }


    void EnterTheFogSettings()
    {
        RenderSettings.fog = true;
        RenderSettings.fogStartDistance = theFogStart;
        RenderSettings.fogEndDistance = theFogEnd;
       // RenderSettings.fogColor = theFogColor;   
    }

    public void SpawnFloorTwo()
    {
        ClearAllRooms();
        forestMap.SetActive(true);
        EnterTheFogSettings();
        itemGenerator.ClearAllItems();
        itemGenerator.RemoveFloorItems(1);
    }
    public void SpawnFloorThree(){
        Destroy(forestMap);
        dangerMap.SetActive(true);
        
    }

}