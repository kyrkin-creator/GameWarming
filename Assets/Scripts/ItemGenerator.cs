using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FloorItemsSet
{
    //Это НАСТРОЙКИ ЭТАЖА!!! В инспекторе настройка
    public int floorLevel = 1;
    public string floorName = "Floor 1";
    public GameObject[] availableItems;
    public int maxItemsToSpawn = 5;
}

public class ItemGenerator : MonoBehaviour
{
    //Генератор: Управляет всеми точками (ItemSpawnPoint) и предметами.
    //Решает, что и где спавнить на каждом этаже\
    [Header("Spawn Points")]
    public ItemSpawnPoint[] spawnPoints;
    
    [Header("Floor Settings")]
    public FloorItemsSet[] floorSettings;
    
    [Header("Spawn Settings")]
    public bool spawnOnStart = true;
    public bool allowDuplicates = false; // Можно ли спавнить одинаковые предметы
    public bool showDebugLogs = true;

    [Header("Auto Discovery")]
    public bool autoFindSpawnPoints = true;
    public string spawnPointTag = "SpawnPoint"; // Тег для точек спавна
    
    
    private Dictionary<int, List<ItemSpawnPoint>> spawnPointsByFloor = new Dictionary<int, List<ItemSpawnPoint>>();
    private List<GameObject> spawnedItems = new List<GameObject>();
    
    void Start()
    {
        if (autoFindSpawnPoints)
        {
            FindAllSpawnPointsInScene();
        }
        
        if (spawnOnStart && spawnPoints.Length > 0)
        {
            GenerateAllItems();
        }
    }
    
    public void GenerateAllItems()
    {
        // Группируем точки по этажам
        GroupSpawnPoints();
        
        foreach (FloorItemsSet floorSet in floorSettings)
        {
            GenerateItemsForFloor(floorSet);
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"Генерация завершена. Создано {spawnedItems.Count} предметов");
        }
    }
    
    void GroupSpawnPoints()
    {
        spawnPointsByFloor.Clear();
        
        foreach (ItemSpawnPoint point in spawnPoints)
        {
            if (point == null) continue;
            
            int floor = point.floorLevel;
            if (!spawnPointsByFloor.ContainsKey(floor))
            {
                spawnPointsByFloor[floor] = new List<ItemSpawnPoint>();
            }
            spawnPointsByFloor[floor].Add(point);
        }
    }
    
    void GenerateItemsForFloor(FloorItemsSet floorSet)
    {
        if (floorSet.availableItems == null || floorSet.availableItems.Length == 0)
        {
            Debug.LogWarning($"Нет предметов для этажа {floorSet.floorLevel}");
            return;
        }
        
        // Получаем точки для этажа
        List<ItemSpawnPoint> floorPoints = GetSpawnPointsForFloor(floorSet.floorLevel);
        
        if (floorPoints == null || floorPoints.Count == 0)
        {
            Debug.LogWarning($"Нет точек спавна для этажа {floorSet.floorLevel}");
            return;
        }
        
        // Сбрасываем использованные точки
        ResetSpawnPoints(floorPoints);
        
        // Определяем сколько спавнить
        int itemsToSpawn = Mathf.Min(floorPoints.Count, floorSet.maxItemsToSpawn, floorSet.availableItems.Length);
        
        if (itemsToSpawn == 0) return;
        
        // Перемешиваем списки
        List<ItemSpawnPoint> shuffledPoints = new List<ItemSpawnPoint>(floorPoints);
        List<GameObject> shuffledPrefabs = new List<GameObject>(floorSet.availableItems);
        ShuffleList(shuffledPoints);
        ShuffleList(shuffledPrefabs);
        
        int spawned = 0;
        
        for (int i = 0; i < itemsToSpawn; i++)
        {
            ItemSpawnPoint point = shuffledPoints[i];
            GameObject prefab = shuffledPrefabs[i % shuffledPrefabs.Count]; // Если дубликаты разрешены
            
            // Если дубликаты запрещены, берем следующий уникальный
            if (!allowDuplicates && i < shuffledPrefabs.Count)
            {
                prefab = shuffledPrefabs[i];
            }
            
            if (point != null && prefab != null)
            {
                SpawnItem(prefab, point);
                spawned++;
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"На этаже {floorSet.floorLevel} заспавнено {spawned} предметов");
        }
    }
    
    void SpawnItem(GameObject prefab, ItemSpawnPoint point)
    {
        // Спавним предмет
        GameObject newItem = Instantiate(prefab, point.transform.position, point.transform.rotation);
        newItem.transform.SetParent(transform);
        
        // Помечаем точку как использованную
        point.isUsed = true;
        
        // Добавляем в список
        spawnedItems.Add(newItem);
        
        // Если у предмета нет компонента Interactable - добавляем
        Interactable interactable = newItem.GetComponent<Interactable>();
        if (interactable == null)
        {
            interactable = newItem.AddComponent<Interactable>();
        }
        
        // Если это ключ, но у него нет ID - генерируем
        if (interactable.itemType == Interactable.ItemType.Key && string.IsNullOrEmpty(interactable.itemID))
        {
            interactable.itemID = $"Key_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"Спавн: {prefab.name} в точке {point.name}");
        }
    }
    
    List<ItemSpawnPoint> GetSpawnPointsForFloor(int floorLevel)
    {
        if (spawnPointsByFloor.ContainsKey(floorLevel))
        {
            return spawnPointsByFloor[floorLevel];
        }
        
        // Если нет в словаре, ищем вручную
        List<ItemSpawnPoint> found = new List<ItemSpawnPoint>();
        foreach (ItemSpawnPoint point in spawnPoints)
        {
            if (point != null && point.floorLevel == floorLevel)
            {
                found.Add(point);
            }
        }
        
        spawnPointsByFloor[floorLevel] = found;
        return found;
    }
    
    void ResetSpawnPoints(List<ItemSpawnPoint> points)
    {
        foreach (ItemSpawnPoint point in points)
        {
            if (point != null)
                point.isUsed = false;
        }
    }
    
    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    // Public методы для управления
    
    public void RegenerateFloor(int floorLevel)
    {
        // Удаляем предметы на этаже
        RemoveFloorItems(floorLevel);
        
        // Находим настройки для этажа
        foreach (FloorItemsSet floorSet in floorSettings)
        {
            if (floorSet.floorLevel == floorLevel)
            {
                GenerateItemsForFloor(floorSet);
                break;
            }
        }
    }
    
    void RemoveFloorItems(int floorLevel)
    {
        // Получаем точки на этаже
        List<ItemSpawnPoint> floorPoints = GetSpawnPointsForFloor(floorLevel);
        
        // Удаляем предметы, которые находятся на этих точках
        List<GameObject> toRemove = new List<GameObject>();
        
        foreach (GameObject item in spawnedItems)
        {
            if (item == null) continue;
            
            // Проверяем, находится ли предмет на точке этого этажа
            foreach (ItemSpawnPoint point in floorPoints)
            {
                if (Vector3.Distance(item.transform.position, point.transform.position) < 0.1f)
                {
                    toRemove.Add(item);
                    break;
                }
            }
        }
        
        // Удаляем
        foreach (GameObject item in toRemove)
        {
            spawnedItems.Remove(item);
            DestroyImmediate(item);
        }
        
        // Сбрасываем точки
        ResetSpawnPoints(floorPoints);
    }
    
    public void ClearAllItems()
    {
        foreach (GameObject item in spawnedItems)
        {
            if (item != null)
                DestroyImmediate(item);
        }
        spawnedItems.Clear();
        
        // Сбрасываем все точки
        foreach (ItemSpawnPoint point in spawnPoints)
        {
            if (point != null)
                point.isUsed = false;
        }
    }
    
    // Визуализация
    void OnDrawGizmos()
    {
        if (spawnPoints != null)
        {
            foreach (ItemSpawnPoint point in spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.color = point.isUsed ? Color.red : Color.green;
                    Gizmos.DrawSphere(point.transform.position, 0.25f);
                }
            }
        }
    }
    public void FindAllSpawnPointsInScene()
    {
        // Ищем все точки в сцене
        ItemSpawnPoint[] foundPoints = FindObjectsOfType<ItemSpawnPoint>();
        
        if (foundPoints.Length > 0)
        {
            spawnPoints = foundPoints;
            
            if (showDebugLogs)
            {
                Debug.Log($"Найдено {foundPoints.Length} точек спавна в сцене");
            }
        }
        else
        {
            // Если точки не найдены, ищем по тегу
            if (!string.IsNullOrEmpty(spawnPointTag))
            {
                GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(spawnPointTag);
                List<ItemSpawnPoint> pointList = new List<ItemSpawnPoint>();
                
                foreach (GameObject obj in taggedObjects)
                {
                    ItemSpawnPoint point = obj.GetComponent<ItemSpawnPoint>();
                    if (point != null)
                    {
                        pointList.Add(point);
                    }
                }
                
                spawnPoints = pointList.ToArray();
                
                if (showDebugLogs)
                {
                    Debug.Log($"Найдено {spawnPoints.Length} точек по тегу {spawnPointTag}");
                }
            }
        }
    }
    
    // Метод для принудительного обновления точек
    public void RefreshSpawnPoints()
    {
        FindAllSpawnPointsInScene();
        
        if (spawnPoints.Length > 0)
        {
            GenerateAllItems();
        }
        else
        {
            Debug.LogWarning("Не найдено точек спавна для генерации предметов!");
        }
    }
}