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

        int level = Mathf.Max(SceneManager.GetActiveScene().buildIndex, PlayerPrefs.GetInt("CurrentLevel", 1));
        float t = Mathf.Clamp01((level - 1f) / 30f);

        runtimeTotalRings = noOfRings + Mathf.RoundToInt(Mathf.Lerp(0f, maxExtraRings, t));
        runtimeTotalRings = Mathf.Max(runtimeTotalRings, 4);

        ringDistance = Mathf.Lerp(ringDistance, minRingDistance, t);
        runtimeUnsafeChance = Mathf.Lerp(baseUnsafeChance, maxUnsafeChance, t);

        safeCountdown = Random.Range(minSafeStreak, maxSafeStreak + 1);

        SpawnRings(0);

        for (int i = 1; i < runtimeTotalRings - 1; i++)
        {
            SpawnRings(GetNextMiddleRingIndex());
        }

        SpawnRings(rings.Length - 1);
    }

    int GetNextMiddleRingIndex()
    {
        int maxMiddle = rings.Length - 2;
        if (maxMiddle < 1)
        {
            return 1;
        }

        int[] safePool = BuildPool(false);
        int[] unsafePool = BuildPool(true);

        bool mustSafe = safeCountdown > 0;
        bool pickUnsafe = !mustSafe && unsafePool.Length > 0 && Random.value < runtimeUnsafeChance;

        if (pickUnsafe)
        {
            safeCountdown = Random.Range(minSafeStreak, maxSafeStreak + 1);
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

    void SpawnRings(int index)
    {
        GameObject newRing = Instantiate(rings[index], new Vector3(transform.position.x, yPos, transform.position.z), Quaternion.identity);
        yPos -= ringDistance;
        newRing.transform.parent = transform;
    }
}