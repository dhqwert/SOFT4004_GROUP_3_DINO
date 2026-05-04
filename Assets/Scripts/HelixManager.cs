using UnityEngine;
using UnityEngine.SceneManagement;

public class HelixManager : MonoBehaviour
{
    public GameObject[] rings;

    public int noOfRings = 10;
    public float ringDistance = 5f;
    public int maxExtraRings = 8;
    public float minRingDistance = 3.4f;
    public float baseUnsafeChance = 0.25f;
    public float maxUnsafeChance = 0.7f;
    public int[] unsafeRingIndexes = { 1 };
    public int minSafeStreak = 1;
    public int maxSafeStreak = 3;

    [Header("Item Spawning")]
    public GameObject coinPrefab;
    public GameObject piercePrefab;
    [Range(0f, 1f)] public float itemSpawnChance = 0.5f; // Tăng tỉ lệ ra item lên 50%
    [Range(0f, 1f)] public float pierceSpawnChance = 0.25f; // Trong số item sinh ra, 25% là xuyên phá

    float yPos;
    float runtimeUnsafeChance;
    int runtimeTotalRings;
    int safeCountdown;
    
    // Material được tạo ra để dùng chung cho tất cả các nấc an toàn trong màn chơi này
    Material currentLevelSafeMat;

    private void Start()
    {
        if (rings == null || rings.Length < 3)
        {
            return;
        }

        int level = Mathf.Max(PlayerPrefs.GetInt("PlayingLevel", 1), 1);
        
        // Tạo bảng màu an toàn cho màn chơi này
        CreateLevelSafeMaterial(level);

        // Hệ số giới hạn độ khó (max ở level 100 thay vì 30)
        float t = Mathf.Clamp01((level - 1f) / 100f);
        
        // Hệ số tăng vô hạn
        float t_infinite = (level - 1f) / 50f;

        // Tăng số lượng tầng vô hạn (mỗi 5 level thêm 1 tầng, cộng thêm hệ số mở rộng)
        runtimeTotalRings = noOfRings + Mathf.RoundToInt(maxExtraRings * t_infinite) + (level / 5);
        runtimeTotalRings = Mathf.Max(runtimeTotalRings, 4);

        // Khoảng cách các vòng hẹp dần
        ringDistance = Mathf.Lerp(ringDistance, minRingDistance, t);

        // Tỷ lệ xuất hiện bẫy gai (tăng giới hạn tối đa để game thực sự khó ở level siêu cao)
        float maxUnsafe = Mathf.Max(maxUnsafeChance, 0.85f);
        runtimeUnsafeChance = Mathf.Lerp(baseUnsafeChance, maxUnsafe, t);

        safeCountdown = Random.Range(minSafeStreak, maxSafeStreak + 1);

        SpawnRings(0, true); // Vòng trên cùng luôn được ép an toàn tuyệt đối

        for (int i = 1; i < runtimeTotalRings - 1; i++)
        {
            bool isBeginnerSafe = false;
            if (level <= 10)
            {
                int safeStartRings = Mathf.Max(1, 6 - (level / 2));
                if (i <= safeStartRings) isBeginnerSafe = true;
            }
            SpawnRings(GetNextMiddleRingIndex(level, isBeginnerSafe), isBeginnerSafe);
        }

        SpawnRings(rings.Length - 1, false); // Vòng đích
    }

    int GetNextMiddleRingIndex(int level, bool forceSafe)
    {
        int maxMiddle = rings.Length - 2;
        if (maxMiddle < 1)
        {
            return 1;
        }

        int[] safePool = BuildPool(false);
        int[] unsafePool = BuildPool(true);

        // Ưu tiên 1: Các bậc Beginner không bẫy ở những tầng đầu tiên 
        if (forceSafe && safePool.Length > 0)
        {
            return safePool[Random.Range(0, safePool.Length)];
        }

        bool mustSafe = safeCountdown > 0;
        bool pickUnsafe = !mustSafe && unsafePool.Length > 0 && Random.value < runtimeUnsafeChance;

        if (pickUnsafe)
        {
            // Càng lên level cao, chuỗi vòng an toàn liên tiếp càng ngắn lại
            int dynamicMinSafe = Mathf.Max(1, minSafeStreak - (level / 50));
            int dynamicMaxSafe = Mathf.Max(1, maxSafeStreak - (level / 50));
            if (dynamicMinSafe > dynamicMaxSafe) dynamicMinSafe = dynamicMaxSafe;

            safeCountdown = Random.Range(dynamicMinSafe, dynamicMaxSafe + 1);
            return unsafePool[Random.Range(0, unsafePool.Length)];
        }

        if (safeCountdown > 0)
        {
            safeCountdown--;
        }

        if (safePool.Length > 0)
        {
            return safePool[Random.Range(0, safePool.Length)];
        }

        return Random.Range(1, rings.Length - 1);
    }

    int[] BuildPool(bool unsafePool)
    {
        int maxMiddle = rings.Length - 2;
        int[] temp = new int[Mathf.Max(1, maxMiddle)];
        int count = 0;

        for (int i = 1; i <= maxMiddle; i++)
        {
            bool isUnsafe = IsUnsafeIndex(i);
            if (isUnsafe == unsafePool)
            {
                temp[count] = i;
                count++;
            }
        }

        int[] result = new int[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = temp[i];
        }

        return result;
    }

    bool IsUnsafeIndex(int index)
    {
        if (unsafeRingIndexes == null)
        {
            return false;
        }

        for (int i = 0; i < unsafeRingIndexes.Length; i++)
        {
            if (unsafeRingIndexes[i] == index)
            {
                return true;
            }
        }

        return false;
    }

    void SpawnRings(int index, bool forceSafe)
    {
        GameObject newRing = Instantiate(rings[index], new Vector3(transform.position.x, yPos, transform.position.z), Quaternion.identity);
        yPos -= ringDistance;
        newRing.transform.parent = transform;

        // Quét và sơn lại màu cho các nấc
        Renderer[] renderers = newRing.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (r.sharedMaterial != null)
            {
                if (r.sharedMaterial.name.Contains("Unsafe"))
                {
                    if (forceSafe)
                    {
                        // Nếu vòng này ép an toàn tuyệt đối, bẫy cũng phải sơn màu an toàn
                        if (currentLevelSafeMat != null) r.sharedMaterial = currentLevelSafeMat;
                    }
                    // Ngược lại: Bẫy giữ nguyên màu gốc (đen)
                }
                else if (r.sharedMaterial.name.Contains("Safe"))
                {
                    // Các nấc an toàn thì dùng màu chung của Level
                    if (currentLevelSafeMat != null) r.sharedMaterial = currentLevelSafeMat;
                }
            }
        }

        // --- Spawning Items ---
        if (index != 0 && index != rings.Length - 1)
        {
            if (Random.value < itemSpawnChance)
            {
                bool isPierce = Random.value < pierceSpawnChance;
                GameObject prefabToSpawn = isPierce ? piercePrefab : coinPrefab;
                
                Renderer[] parts = newRing.GetComponentsInChildren<Renderer>();
                if (parts.Length > 0)
                {
                    Renderer randomPart = parts[Random.Range(0, parts.Length)];
                    
                    if (Random.value < 0.5f)
                    {
                        // Đặt trên nấc
                        Vector3 spawnPos = randomPart.bounds.center;
                        spawnPos.y = newRing.transform.position.y + 0.8f; 
                        SpawnItemAt(prefabToSpawn, spawnPos, newRing.transform, isPierce);
                    }
                    else
                    {
                        // Đặt ở khe (hoặc vị trí ngẫu nhiên trên cung tròn)
                        Vector3 partCenter = randomPart.bounds.center;
                        partCenter.y = newRing.transform.position.y;
                        float ringRadius = Vector3.Distance(partCenter, newRing.transform.position);
                        if (ringRadius < 0.5f) ringRadius = 2.5f; // Đề phòng trường hợp lỗi tâm
                        
                        float randomAngle = Random.Range(0f, 360f);
                        Vector3 offset = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward * ringRadius;
                        Vector3 spawnPos = newRing.transform.position + offset;
                        spawnPos.y = newRing.transform.position.y + 0.8f;
                        
                        SpawnItemAt(prefabToSpawn, spawnPos, newRing.transform, isPierce);
                    }
                }
            }
        }
    }

    void SpawnItemAt(GameObject prefab, Vector3 pos, Transform parent, bool isPierce)
    {
        if (prefab != null)
        {
            Instantiate(prefab, pos, prefab.transform.rotation, parent);
        }
        else
        {
            // Tự động tạo item nếu chưa gán prefab (để có thể "chỉ play")
            GameObject item = GameObject.CreatePrimitive(isPierce ? PrimitiveType.Cube : PrimitiveType.Sphere);
            item.transform.position = pos;
            item.transform.parent = parent;
            item.transform.localScale = Vector3.one * 0.5f;
            
            Collider col = item.GetComponent<Collider>();
            if (col != null) {
                col.isTrigger = true;
                // Kích thước vừa phải để tránh chạm ngoài rìa
                if (col is BoxCollider bc) bc.size = new Vector3(1.2f, 1.2f, 1.2f);
                if (col is SphereCollider sc) sc.radius = 1.0f;
            }
            
            ItemPickup pickup = item.AddComponent<ItemPickup>();
            pickup.itemType = isPierce ? ItemType.Piercing : ItemType.Coin;
            
            Renderer rend = item.GetComponent<Renderer>();
            if (rend != null) {
                rend.material.color = isPierce ? Color.red : Color.yellow;
            }
        }
    }

    void CreateLevelSafeMaterial(int level)
    {
        // 1. Tìm 1 material Safe mẫu từ các Prefab rings để copy thuộc tính vật lý, shader,...
        Material baseMat = null;
        foreach (GameObject ring in rings) {
            if (ring == null) continue;
            Renderer[] rs = ring.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in rs) {
                if (r.sharedMaterial != null && r.sharedMaterial.name.Contains("Safe")) {
                    baseMat = r.sharedMaterial;
                    break;
                }
            }
            if (baseMat != null) break;
        }

        if (baseMat != null)
        {
            currentLevelSafeMat = new Material(baseMat);
            
            // Logic màu sắc: Cứ 5 level đổi hệ màu 1 lần
            int colorGroup = (level - 1) / 5;
            int subLevel = (level - 1) % 5;

            // 0.618f là Tỷ lệ vàng (Golden Ratio), nhân vào giúp màu không bao giờ bị trùng và phân bổ đều cầu vồng
            float h = (colorGroup * 0.618034f) % 1.0f; 
            
            // Saturation: Nhạt (0.2) đến Đậm dần (0.9)
            float s = Mathf.Lerp(0.2f, 0.9f, subLevel / 4f);
            
            // Value (Độ sáng): Sáng rực rỡ (1.0) đến tối đi một chút để đậm đà (0.75)
            float v = Mathf.Lerp(1.0f, 0.75f, subLevel / 4f);

            Color newColor = Color.HSVToRGB(h, s, v);
            currentLevelSafeMat.color = newColor;
        }
    }
}