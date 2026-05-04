using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class FirebasePlayerEntry
{
    public string id;
    public string name;
    public int score;
    public long updatedAt;
    public string season;
    public string tier;
}

/// <summary>
/// Service đọc/ghi leaderboard lên Firebase Realtime Database qua REST API.
/// KHÔNG cần Firebase SDK, KHÔNG cần google-services.json.
/// User chỉ cần: tạo Firebase project, bật Realtime Database (test mode),
/// dán URL database vào field <see cref="databaseUrl"/> trong Inspector.
///
/// Local chỉ lưu UUID + tên người chơi để xác định "ai là tôi" trên DB.
/// Toàn bộ điểm số được lưu trên Firebase.
/// </summary>
public class FirebaseLeaderboardService : MonoBehaviour
{
    public static FirebaseLeaderboardService instance;

    [Header("Firebase Realtime Database")]
    [Tooltip("URL của Realtime Database, dạng https://your-project-default-rtdb.firebaseio.com  (KHÔNG có dấu '/' cuối)")]
    public string databaseUrl = "";
    [Tooltip("Tuỳ chọn: token auth nếu rules KHÔNG ở test mode. Trống nếu test mode.")]
    public string authToken = "";
    [Tooltip("Đường dẫn node chứa leaderboard.")]
    public string leaderboardPath = "leaderboard";

    [Header("Identity")]
    [Tooltip("Tên hiển thị mặc định khi player chưa nhập tên.")]
    public string fallbackPlayerName = "Player";

    public string LocalPlayerId { get; private set; }
    public string LocalPlayerName { get; private set; }

    const string PLAYER_ID_KEY = "FirebasePlayerId";
    const string PLAYER_NAME_KEY = "PlayerName";

    public bool IsConfigured
    {
        get { return !string.IsNullOrWhiteSpace(databaseUrl); }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureLocalIdentity();
    }

    void EnsureLocalIdentity()
    {
        LocalPlayerId = PlayerPrefs.GetString(PLAYER_ID_KEY, "");
        if (string.IsNullOrEmpty(LocalPlayerId))
        {
            LocalPlayerId = Guid.NewGuid().ToString("N");
            PlayerPrefs.SetString(PLAYER_ID_KEY, LocalPlayerId);
            PlayerPrefs.Save();
        }

        LocalPlayerName = PlayerPrefs.GetString(PLAYER_NAME_KEY, "");
        if (string.IsNullOrWhiteSpace(LocalPlayerName))
        {
            LocalPlayerName = fallbackPlayerName + "_" + LocalPlayerId.Substring(0, 4);
            PlayerPrefs.SetString(PLAYER_NAME_KEY, LocalPlayerName);
            PlayerPrefs.Save();
        }
    }

    public void SetLocalPlayerName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            return;
        }
        LocalPlayerName = newName.Trim();
        PlayerPrefs.SetString(PLAYER_NAME_KEY, LocalPlayerName);
        PlayerPrefs.Save();
    }

    string BuildEntryUrl(string playerId)
    {
        return BuildUrl("/" + leaderboardPath + "/" + playerId + ".json");
    }

    string BuildLeaderboardUrl()
    {
        return BuildUrl("/" + leaderboardPath + ".json");
    }

    string BuildUrl(string path)
    {
        string baseUrl = (databaseUrl ?? string.Empty).TrimEnd('/');
        if (!path.StartsWith("/"))
        {
            path = "/" + path;
        }
        string url = baseUrl + path;
        if (!string.IsNullOrEmpty(authToken))
        {
            url += (url.Contains("?") ? "&" : "?") + "auth=" + UnityWebRequest.EscapeURL(authToken);
        }
        return url;
    }

    /// <summary>
    /// Cộng dồn điểm cho player local. Tự reset điểm về 0 nếu mùa hiện tại
    /// khác mùa trong entry trên DB (auto weekly reset).
    /// Callback (ok, totalScoreSauKhiCong, seasonChanged).
    /// </summary>
    public Coroutine AddScoreToLocalPlayer(int addScore, Action<bool, int, bool> onDone = null)
    {
        if (!IsConfigured)
        {
            Debug.LogWarning("[Firebase] AddScore bỏ qua vì chưa cấu hình databaseUrl.");
            if (onDone != null) onDone.Invoke(false, 0, false);
            return null;
        }
        return StartCoroutine(AddScoreRoutine(addScore, onDone));
    }

    IEnumerator AddScoreRoutine(int addScore, Action<bool, int, bool> onDone)
    {
        FirebasePlayerEntry existing = null;
        bool fetched = false;
        yield return FetchEntry(LocalPlayerId, entry =>
        {
            existing = entry;
            fetched = true;
        });

        if (!fetched)
        {
            if (onDone != null) onDone.Invoke(false, 0, false);
            yield break;
        }

        string currentSeason = SeasonManager.GetCurrentSeasonId();
        bool seasonChanged = existing != null && !string.IsNullOrEmpty(existing.season) && existing.season != currentSeason;

        int baseScore = (existing == null || seasonChanged) ? 0 : existing.score;
        int newScore = Mathf.Max(0, baseScore + addScore);
        LeagueTier tier = LeagueTierSystem.GetTierForScore(newScore);

        yield return PushEntryRoutine(LocalPlayerId, LocalPlayerName, newScore, currentSeason, tier.id, ok =>
        {
            if (onDone != null) onDone.Invoke(ok, newScore, seasonChanged);
        });
    }

    /// <summary>Ghi đè entry của player local với điểm tuyệt đối.</summary>
    public Coroutine SetLocalPlayerScore(int score, Action<bool> onDone = null)
    {
        if (!IsConfigured)
        {
            if (onDone != null) onDone.Invoke(false);
            return null;
        }
        string season = SeasonManager.GetCurrentSeasonId();
        string tier = LeagueTierSystem.GetTierForScore(score).id;
        return StartCoroutine(PushEntryRoutine(LocalPlayerId, LocalPlayerName, score, season, tier, onDone));
    }

    /// <summary>Cập nhật tên local lên DB (giữ nguyên điểm, mùa, tier).</summary>
    public Coroutine SyncLocalPlayerName(Action<bool> onDone = null)
    {
        if (!IsConfigured)
        {
            if (onDone != null) onDone.Invoke(false);
            return null;
        }
        return StartCoroutine(SyncNameRoutine(onDone));
    }

    IEnumerator SyncNameRoutine(Action<bool> onDone)
    {
        FirebasePlayerEntry existing = null;
        bool fetched = false;
        yield return FetchEntry(LocalPlayerId, entry =>
        {
            existing = entry;
            fetched = true;
        });
        if (!fetched)
        {
            if (onDone != null) onDone.Invoke(false);
            yield break;
        }

        int score = existing != null ? existing.score : 0;
        string season = existing != null && !string.IsNullOrEmpty(existing.season)
            ? existing.season
            : SeasonManager.GetCurrentSeasonId();
        string tier = existing != null && !string.IsNullOrEmpty(existing.tier)
            ? existing.tier
            : LeagueTierSystem.GetTierForScore(score).id;

        yield return PushEntryRoutine(LocalPlayerId, LocalPlayerName, score, season, tier, onDone);
    }

    /// <summary>Ghi entry tuỳ ý (dùng cho seeder).</summary>
    public Coroutine PushArbitraryEntry(string playerId, string playerName, int score, Action<bool> onDone = null)
    {
        if (!IsConfigured)
        {
            if (onDone != null) onDone.Invoke(false);
            return null;
        }
        string season = SeasonManager.GetCurrentSeasonId();
        string tier = LeagueTierSystem.GetTierForScore(score).id;
        return StartCoroutine(PushEntryRoutine(playerId, playerName, score, season, tier, onDone));
    }

    IEnumerator PushEntryRoutine(string playerId, string playerName, int score, string season, string tier, Action<bool> onDone)
    {
        FirebasePlayerEntry entry = new FirebasePlayerEntry
        {
            id = playerId,
            name = playerName,
            score = Mathf.Max(0, score),
            updatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            season = season,
            tier = tier,
        };
        string json = JsonUtility.ToJson(entry);
        string url = BuildEntryUrl(playerId);

        UnityWebRequest req = new UnityWebRequest(url, "PUT");
        byte[] body = Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        bool ok = req.result == UnityWebRequest.Result.Success;
        if (!ok)
        {
            Debug.LogError("[Firebase] PUT thất bại " + url + " | " + req.error + " | response: " + (req.downloadHandler != null ? req.downloadHandler.text : ""));
        }

        if (onDone != null) onDone.Invoke(ok);
    }

    /// <summary>Lấy entry của 1 player theo id. null nếu chưa tồn tại.</summary>
    public Coroutine FetchEntry(string playerId, Action<FirebasePlayerEntry> onResult)
    {
        if (!IsConfigured)
        {
            if (onResult != null) onResult.Invoke(null);
            return null;
        }
        return StartCoroutine(FetchEntryRoutine(playerId, onResult));
    }

    IEnumerator FetchEntryRoutine(string playerId, Action<FirebasePlayerEntry> onResult)
    {
        string url = BuildEntryUrl(playerId);
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("[Firebase] GET entry thất bại " + url + " | " + req.error);
            if (onResult != null) onResult.Invoke(null);
            yield break;
        }

        string json = req.downloadHandler != null ? req.downloadHandler.text : null;
        if (string.IsNullOrWhiteSpace(json) || json.Trim() == "null")
        {
            if (onResult != null) onResult.Invoke(null);
            yield break;
        }

        FirebasePlayerEntry entry = null;
        try
        {
            entry = JsonUtility.FromJson<FirebasePlayerEntry>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError("[Firebase] Parse entry lỗi: " + ex.Message + " | json: " + json);
        }
        if (onResult != null) onResult.Invoke(entry);
    }

    /// <summary>Lấy toàn bộ entries trong leaderboard. Trả về list rỗng nếu DB trống.</summary>
    public Coroutine FetchAll(Action<List<FirebasePlayerEntry>> onResult)
    {
        if (!IsConfigured)
        {
            if (onResult != null) onResult.Invoke(null);
            return null;
        }
        return StartCoroutine(FetchAllRoutine(onResult));
    }

    IEnumerator FetchAllRoutine(Action<List<FirebasePlayerEntry>> onResult)
    {
        string url = BuildLeaderboardUrl();
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("[Firebase] GET leaderboard thất bại " + url + " | " + req.error);
            if (onResult != null) onResult.Invoke(null);
            yield break;
        }

        string json = req.downloadHandler != null ? req.downloadHandler.text : null;
        List<FirebasePlayerEntry> entries = ParseLeaderboardJson(json);
        if (onResult != null) onResult.Invoke(entries);
    }

    /// <summary>
    /// Parse JSON dạng dictionary của Firebase RTDB:
    /// { "id1": { ...entry... }, "id2": { ...entry... } }
    /// JsonUtility không hỗ trợ dictionary nên cần parse tay từng entry rồi
    /// nạp lại bằng JsonUtility cho object con.
    /// </summary>
    public static List<FirebasePlayerEntry> ParseLeaderboardJson(string json)
    {
        List<FirebasePlayerEntry> result = new List<FirebasePlayerEntry>();
        if (string.IsNullOrWhiteSpace(json))
        {
            return result;
        }
        string trimmed = json.Trim();
        if (trimmed == "null" || trimmed == "{}")
        {
            return result;
        }

        int len = json.Length;
        int i = 0;
        while (i < len && json[i] != '{') i++;
        if (i >= len) return result;
        i++;

        while (i < len)
        {
            while (i < len && (char.IsWhiteSpace(json[i]) || json[i] == ',')) i++;
            if (i >= len || json[i] == '}') break;

            if (json[i] != '"') break;
            int keyEnd = json.IndexOf('"', i + 1);
            if (keyEnd < 0) break;
            i = keyEnd + 1;

            while (i < len && (json[i] == ':' || char.IsWhiteSpace(json[i]))) i++;
            if (i >= len || json[i] != '{') break;

            int objStart = i;
            int depth = 0;
            for (; i < len; i++)
            {
                char c = json[i];
                if (c == '{')
                {
                    depth++;
                }
                else if (c == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        i++;
                        break;
                    }
                }
                else if (c == '"')
                {
                    i++;
                    while (i < len && json[i] != '"')
                    {
                        if (json[i] == '\\' && i + 1 < len)
                        {
                            i++;
                        }
                        i++;
                    }
                }
            }
            int objEnd = i;
            if (objEnd <= objStart) break;

            string entryJson = json.Substring(objStart, objEnd - objStart);
            try
            {
                FirebasePlayerEntry entry = JsonUtility.FromJson<FirebasePlayerEntry>(entryJson);
                if (entry != null && !string.IsNullOrWhiteSpace(entry.name))
                {
                    result.Add(entry);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Firebase] Bỏ qua entry không parse được: " + ex.Message);
            }
        }

        return result;
    }
}
