using System.Collections;
using UnityEngine;

/// <summary>
/// Bootstrap tự động: spawn FirebaseLeaderboardService + FirebaseSeeder vào scene
/// trước khi scene đầu tiên load. KHÔNG cần thao tác trong Hierarchy.
///
/// Tự seed 30 player giả vào DB nếu DB đang rỗng.
/// </summary>
public static class FirebaseBootstrap
{
    // Sửa URL này nếu bạn dùng project Firebase khác
    public const string DEFAULT_DATABASE_URL = "https://helix-jump-soft4004-default-rtdb.asia-southeast1.firebasedatabase.app";

    public const int AUTO_SEED_COUNT = 30;
    public const bool AUTO_SEED_IF_EMPTY = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        if (FirebaseLeaderboardService.instance != null)
        {
            return;
        }

        GameObject go = new GameObject("FirebaseManager");
        Object.DontDestroyOnLoad(go);

        FirebaseLeaderboardService service = go.AddComponent<FirebaseLeaderboardService>();
        service.databaseUrl = DEFAULT_DATABASE_URL;
        service.leaderboardPath = "leaderboard";
        service.fallbackPlayerName = "Player";

        FirebaseSeeder seeder = go.AddComponent<FirebaseSeeder>();
        seeder.service = service;
        seeder.seedCount = AUTO_SEED_COUNT;

        Debug.Log("[FirebaseBootstrap] Đã tạo FirebaseManager. Database: " + service.databaseUrl);

        if (AUTO_SEED_IF_EMPTY)
        {
            FirebaseAutoSeedRunner runner = go.AddComponent<FirebaseAutoSeedRunner>();
            runner.service = service;
            runner.seeder = seeder;
        }
    }
}

/// <summary>
/// Helper MonoBehaviour: chạy coroutine kiểm tra DB rỗng và tự seed lần đầu.
/// Dùng PlayerPrefs để chỉ seed 1 lần / máy (tránh seed lặp).
/// </summary>
public class FirebaseAutoSeedRunner : MonoBehaviour
{
    public FirebaseLeaderboardService service;
    public FirebaseSeeder seeder;

    const string SEED_DONE_KEY = "FirebaseAutoSeedDone";

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.5f);

        if (service == null || !service.IsConfigured)
        {
            Debug.LogWarning("[AutoSeed] Service chưa cấu hình, bỏ qua seed.");
            yield break;
        }

        if (PlayerPrefs.GetInt(SEED_DONE_KEY, 0) == 1)
        {
            Debug.Log("[AutoSeed] Đã seed trước đây, bỏ qua. Xoá key '" + SEED_DONE_KEY + "' trong PlayerPrefs để seed lại.");
            yield break;
        }

        bool done = false;
        int currentCount = 0;
        service.FetchAll(entries =>
        {
            currentCount = entries != null ? entries.Count : 0;
            done = true;
        });

        float waited = 0f;
        while (!done && waited < 10f)
        {
            waited += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!done)
        {
            Debug.LogWarning("[AutoSeed] Fetch DB timeout, bỏ qua seed lần đầu.");
            yield break;
        }

        if (currentCount > 0)
        {
            Debug.Log("[AutoSeed] DB đã có " + currentCount + " entry, không cần seed.");
            PlayerPrefs.SetInt(SEED_DONE_KEY, 1);
            PlayerPrefs.Save();
            yield break;
        }

        Debug.Log("[AutoSeed] DB rỗng, bắt đầu seed " + seeder.seedCount + " player giả...");
        seeder.SeedFakePlayers();

        PlayerPrefs.SetInt(SEED_DONE_KEY, 1);
        PlayerPrefs.Save();
    }
}
