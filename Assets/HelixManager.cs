using UnityEngine;
using UnityEngine.SceneManagement;

public class HelixManager : MonoBehaviour
{
    public GameObject[] rings;

    [Header("Số tầng")]
    public int noOfRings = 10;  // Level 1-5 bắt đầu với 10 tầng
    public int ringsPerTier = 2;   // Thêm 2 tầng mỗi 5 level
    public int maxRings = 30;  // Giới hạn tối đa

    [Header("Khoảng cách tầng")]
    public float ringDistance = 5f;
    public float minRingDistance = 3.4f;

    [Header("Độ khó bẫy")]
    public float baseUnsafeChance = 0.25f;
    public float maxUnsafeChance = 0.7f;
    public int[] unsafeRingIndexes = { 1 };
    public int minSafeStreak = 1;
    public int maxSafeStreak = 3;

    float yPos;
    float runtimeUnsafeChance;
    int runtimeTotalRings;
    int safeCountdown;
    float originalRingDistance;

    void Awake()
    {
        originalRingDistance = ringDistance;
    }

    void Start()
    {
        if (rings == null || rings.Length < 3) return;
        int level = PlayerPrefs.GetInt("CurrentLevel", 1);
        GenerateForLevel(level);
    }

    public void GenerateForLevel(int level)
    {
        if (rings == null || rings.Length < 3)
        {
            Debug.LogWarning("HelixManager: chưa gán đủ rings prefab!");
            return;
        }

        // Xóa tháp cũ
        foreach (Transform child in transform)
            Destroy(child.gameObject);
        yPos = 0f;

        // ── SỐ TẦNG: tăng mỗi 5 level ──────────────────────────────
        // Level 1-5 = 10 tầng, 6-10 = 12, 11-15 = 14, 16-20 = 16...
        int tier = (level - 1) / 5;
        runtimeTotalRings = Mathf.Min(noOfRings + tier * ringsPerTier, maxRings);
        runtimeTotalRings = Mathf.Max(runtimeTotalRings, 4);

        // ── ĐỘ KHÓ THEO CÔNG THỨC SCORE ────────────────────────────
        // t = 0 (level 1) → 1 (level 50+), đường cong ease-in
        float rawT = Mathf.Clamp01((level - 1f) / 50f);
        float t = Mathf.Pow(rawT, 1.8f);

        // Từng tham số
        float gapRatio = Mathf.Lerp(1f, 0.2f, t);   // Khe hở: rộng→hẹp
        float speed = Mathf.Lerp(0f, 1f, t);   // Tốc độ xoay: chậm→nhanh
        float trapRatio = Mathf.Lerp(baseUnsafeChance, maxUnsafeChance, t);
        float fakeGap = level >= 15
                          ? Mathf.Clamp01((t - 0.28f) / 0.72f) * 0.3f
                          : 0f;

        // Công thức điểm độ khó (log để debug)
        float diffScore = (runtimeTotalRings / (float)maxRings) * 0.30f
                        + (1f - gapRatio) * 0.25f
                        + speed * 0.20f
                        + trapRatio * 0.15f
                        + fakeGap * 0.10f;

        // Áp dụng vào runtime
        ringDistance = Mathf.Lerp(originalRingDistance, minRingDistance, t);
        runtimeUnsafeChance = trapRatio;
        safeCountdown = Random.Range(minSafeStreak, maxSafeStreak + 1);

        // Cập nhật tốc độ xoay
        HelixRotator rotator = GetComponent<HelixRotator>();
        if (rotator != null)
            rotator.rotationSpeed = Mathf.Lerp(8f, 40f, speed);

        // Sinh tháp
        SpawnRings(0);
        for (int i = 1; i < runtimeTotalRings - 1; i++)
            SpawnRings(GetNextMiddleRingIndex());
        SpawnRings(rings.Length - 1);

        Debug.Log($"[Lv{level}] tier={tier} rings={runtimeTotalRings} | " +
                  $"gap={gapRatio:F2} speed={speed:F2} trap={trapRatio:F2} " +
                  $"fakeGap={fakeGap:F2} | diffScore={diffScore:F2}");
    }

    int GetNextMiddleRingIndex()
    {
        int maxMiddle = rings.Length - 2;
        if (maxMiddle < 1) return 1;

        int[] safePool = BuildPool(false);
        int[] unsafePool = BuildPool(true);

        bool mustSafe = safeCountdown > 0;
        bool pickUnsafe = !mustSafe && unsafePool.Length > 0
                          && Random.value < runtimeUnsafeChance;

        if (pickUnsafe)
        {
            safeCountdown = Random.Range(minSafeStreak, maxSafeStreak + 1);
            return unsafePool[Random.Range(0, unsafePool.Length)];
        }

        if (safeCountdown > 0) safeCountdown--;

        if (safePool.Length > 0)
            return safePool[Random.Range(0, safePool.Length)];

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
        for (int i = 0; i < count; i++) result[i] = temp[i];
        return result;
    }

    bool IsUnsafeIndex(int index)
    {
        if (unsafeRingIndexes == null) return false;
        for (int i = 0; i < unsafeRingIndexes.Length; i++)
            if (unsafeRingIndexes[i] == index) return true;
        return false;
    }

    void SpawnRings(int index)
    {
        GameObject newRing = Instantiate(
            rings[index],
            new Vector3(transform.position.x, yPos, transform.position.z),
            Quaternion.identity
        );
        yPos -= ringDistance;
        newRing.transform.parent = transform;
    }
}