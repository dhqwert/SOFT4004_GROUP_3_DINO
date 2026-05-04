using System.Collections;
using UnityEngine;

/// <summary>
/// Seeder gieo player giả lên Firebase Realtime Database để test bảng xếp hạng.
/// Cách dùng:
///   1. Đảm bảo trong scene có GameObject mang <see cref="FirebaseLeaderboardService"/>
///      đã điền databaseUrl.
///   2. Add component này vào cùng GameObject (hoặc 1 GameObject riêng).
///   3. Bấm Play, rồi chuột phải vào component → "Seed Fake Players".
///   4. Mở Firebase Console kiểm tra node leaderboard có 30 entry là OK.
/// </summary>
public class FirebaseSeeder : MonoBehaviour
{
    [Tooltip("Để trống sẽ tự lấy FirebaseLeaderboardService.instance.")]
    public FirebaseLeaderboardService service;

    [Tooltip("Số player giả muốn gieo.")]
    public int seedCount = 30;

    [Tooltip("Seed ngẫu nhiên ổn định.")]
    public int randomSeed = 4242;

    [Tooltip("Khoảng điểm min của player giả.")]
    public int minScore = 200;
    [Tooltip("Khoảng điểm max của player giả.")]
    public int maxScore = 9000;

    static readonly string[] NamePrefixes =
    {
        "Player", "Helix", "Bouncy", "Drop", "Spin", "Jump",
        "Crash", "Neon", "Pixel", "Comet", "Nova", "Astro",
        "Rocket", "Vortex", "Echo", "Lyra"
    };

    [ContextMenu("Seed Fake Players")]
    public void SeedFakePlayers()
    {
        if (!Application.isPlaying)
        {
            Debug.LogError("[Seeder] Cần BẤM PLAY trước rồi mới chạy seeder (Coroutine cần runtime).");
            return;
        }

        if (service == null)
        {
            service = FirebaseLeaderboardService.instance;
        }
        if (service == null)
        {
            service = FindFirstObjectByType<FirebaseLeaderboardService>();
        }
        if (service == null || !service.IsConfigured)
        {
            Debug.LogError("[Seeder] Không tìm thấy FirebaseLeaderboardService đã cấu hình databaseUrl.");
            return;
        }

        StartCoroutine(SeedRoutine());
    }

    [ContextMenu("Wipe Leaderboard (DELETE ALL)")]
    public void WipeLeaderboard()
    {
        if (!Application.isPlaying)
        {
            Debug.LogError("[Seeder] Cần BẤM PLAY trước.");
            return;
        }
        if (service == null) service = FirebaseLeaderboardService.instance;
        if (service == null || !service.IsConfigured)
        {
            Debug.LogError("[Seeder] Service chưa cấu hình.");
            return;
        }
        StartCoroutine(WipeRoutine());
    }

    IEnumerator SeedRoutine()
    {
        System.Random rng = new System.Random(randomSeed);
        int success = 0;

        Debug.Log("[Seeder] Bắt đầu seed " + seedCount + " player...");

        for (int i = 0; i < seedCount; i++)
        {
            string id = "seed_" + i.ToString("D3") + "_" + rng.Next(0x10000, 0xFFFFF).ToString("X");
            string name = NamePrefixes[rng.Next(NamePrefixes.Length)] + rng.Next(10, 999);
            int score = rng.Next(Mathf.Min(minScore, maxScore), Mathf.Max(minScore, maxScore));

            bool done = false;
            bool ok = false;
            yield return service.PushArbitraryEntry(id, name, score, result =>
            {
                ok = result;
                done = true;
            });

            float waited = 0f;
            while (!done && waited < 5f)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }
            if (ok) success++;
        }

        Debug.Log("[Seeder] Hoàn tất: " + success + "/" + seedCount + " player được ghi lên Firebase.");
    }

    IEnumerator WipeRoutine()
    {
        string url = service.databaseUrl.TrimEnd('/') + "/" + service.leaderboardPath + ".json";
        if (!string.IsNullOrEmpty(service.authToken))
        {
            url += "?auth=" + UnityEngine.Networking.UnityWebRequest.EscapeURL(service.authToken);
        }

        UnityEngine.Networking.UnityWebRequest req = UnityEngine.Networking.UnityWebRequest.Delete(url);
        yield return req.SendWebRequest();

        if (req.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            Debug.Log("[Seeder] Đã xoá toàn bộ leaderboard trên Firebase.");
        }
        else
        {
            Debug.LogError("[Seeder] Xoá leaderboard thất bại: " + req.error);
        }
    }
}
