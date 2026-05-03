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

    float yPos;
    float runtimeUnsafeChance;
    int runtimeTotalRings;
    int safeCountdown;

    private void Start()
    {
        if (rings == null || rings.Length < 3)
        {
            return;
        }

        int level = Mathf.Max(PlayerPrefs.GetInt("PlayingLevel", 1), 1);
        
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

        // Nếu được yêu cầu là vòng Beginner (an toàn tuyệt đối)
        if (forceSafe)
        {
            // 1. Tìm Material an toàn (màu bình thường) trên chính vòng này
            Material safeMat = null;
            Renderer[] renderers = newRing.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                if (r.sharedMaterial != null && r.sharedMaterial.name.Contains("Safe"))
                {
                    safeMat = r.sharedMaterial;
                    break; // Đã tìm thấy mẫu
                }
            }

            // 2. Tìm tất cả các mảnh bẫy (Unsafe) và sơn lại thành Safe
            if (safeMat != null)
            {
                foreach (Renderer r in renderers)
                {
                    if (r.sharedMaterial != null && r.sharedMaterial.name.Contains("Unsafe"))
                    {
                        r.sharedMaterial = safeMat; 
                    }
                }
            }
        }
    }
}