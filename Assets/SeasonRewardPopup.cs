using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Popup hiện khi vào game thấy mùa giải đã sang mùa mới (so với mùa lưu cuối cùng
/// trong PlayerPrefs key "LastSeenSeason"). Tính rank cuối mùa cũ + tier để trao
/// thưởng coin, sau đó cập nhật mốc mùa mới.
///
/// Tự build UI runtime, không cần kéo thả prefab.
/// </summary>
public class SeasonRewardPopup : MonoBehaviour
{
    public const string LAST_SEEN_SEASON_KEY = "LastSeenSeason";

    static SeasonRewardPopup instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureSpawned()
    {
        if (instance != null) return;
        if (FindFirstObjectByType<SeasonRewardPopup>() != null) return;
        GameObject go = new GameObject("SeasonRewardPopup");
        DontDestroyOnLoad(go);
        go.AddComponent<SeasonRewardPopup>();
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    Canvas canvas;
    TextMeshProUGUI titleText;
    TextMeshProUGUI tierText;
    TextMeshProUGUI rankText;
    TextMeshProUGUI rewardText;
    Image tierBadge;
    Button claimButton;
    int pendingReward;
    bool rewardClaimed;
    readonly System.Collections.Generic.List<Object> generatedAssets = new System.Collections.Generic.List<Object>();

    void OnDestroy()
    {
        for (int i = 0; i < generatedAssets.Count; i++)
        {
            if (generatedAssets[i] != null) Destroy(generatedAssets[i]);
        }
        generatedAssets.Clear();
    }

    IEnumerator Start()
    {
        // Đợi Firebase ready (tối đa 15s) thay vì chỉ 2s. Nếu offline, sẽ retry
        // mỗi 0.5s — tránh mất reward khi mạng chậm hoặc Bootstrap chưa kịp init.
        float waited = 0f;
        while (waited < 15f)
        {
            FirebaseLeaderboardService fb = FirebaseLeaderboardService.instance;
            if (fb != null && fb.IsConfigured) break;
            yield return new WaitForSeconds(0.5f);
            waited += 0.5f;
        }
        CheckSeasonChange();
    }

    void CheckSeasonChange()
    {
        FirebaseLeaderboardService firebase = FirebaseLeaderboardService.instance;
        if (firebase == null || !firebase.IsConfigured) return;

        string currentSeason = SeasonManager.GetCurrentSeasonId();
        string lastSeen = PlayerPrefs.GetString(LAST_SEEN_SEASON_KEY, "");

        if (string.IsNullOrEmpty(lastSeen))
        {
            PlayerPrefs.SetString(LAST_SEEN_SEASON_KEY, currentSeason);
            PlayerPrefs.Save();
            return;
        }
        if (lastSeen == currentSeason) return;

        StartCoroutine(CalcRewardThenShow(lastSeen, currentSeason));
    }

    IEnumerator CalcRewardThenShow(string oldSeason, string newSeason)
    {
        FirebaseLeaderboardService firebase = FirebaseLeaderboardService.instance;
        bool finished = false;
        List<FirebasePlayerEntry> entries = null;
        firebase.FetchAll(result =>
        {
            entries = result;
            finished = true;
        });

        float waited = 0f;
        while (!finished && waited < 12f)
        {
            waited += Time.unscaledDeltaTime;
            yield return null;
        }
        if (entries == null) yield break;

        List<FirebasePlayerEntry> oldSeasonEntries = new List<FirebasePlayerEntry>();
        for (int i = 0; i < entries.Count; i++)
        {
            FirebasePlayerEntry e = entries[i];
            if (e == null) continue;
            // Coi entry không có season như mùa cũ (giống logic trong LeaderboardPanel).
            string s = string.IsNullOrEmpty(e.season) ? oldSeason : e.season;
            if (s == oldSeason)
            {
                oldSeasonEntries.Add(e);
            }
        }
        oldSeasonEntries.Sort((a, b) => b.score.CompareTo(a.score));

        int rank = -1;
        int finalScore = 0;
        for (int i = 0; i < oldSeasonEntries.Count; i++)
        {
            if (oldSeasonEntries[i].id == firebase.LocalPlayerId)
            {
                rank = i + 1;
                finalScore = oldSeasonEntries[i].score;
                break;
            }
        }

        if (rank < 0)
        {
            // Player không tham gia mùa cũ → bỏ qua, đánh dấu đã thấy.
            PlayerPrefs.SetString(LAST_SEEN_SEASON_KEY, newSeason);
            PlayerPrefs.Save();
            yield break;
        }

        LeagueTier tier = LeagueTierSystem.GetTierForScore(finalScore);
        int reward = LeagueTierSystem.CalcSeasonReward(tier, rank);

        // Lưu newSeason TRƯỚC khi show popup để đảm bảo popup không hiện lại
        // ở lần Play sau dù user có claim hay không. Coin chỉ được cộng khi
        // user bấm CLAIM (tránh double-grant nếu user nhấn CLAIM nhiều lần).
        PlayerPrefs.SetString(LAST_SEEN_SEASON_KEY, newSeason);
        PlayerPrefs.Save();

        pendingReward = reward;
        rewardClaimed = false;
        BuildUIIfNeeded();
        ShowReward(tier, rank, reward, oldSeason);
    }

    void ClaimAndHide()
    {
        if (!rewardClaimed && pendingReward > 0)
        {
            int total = PlayerPrefs.GetInt("TotalCoins", 0) + pendingReward;
            PlayerPrefs.SetInt("TotalCoins", total);
            PlayerPrefs.Save();
            Debug.Log("[SeasonReward] +" + pendingReward + " coins. Tổng: " + total);
            rewardClaimed = true;
        }
        Hide();
    }

    void BuildUIIfNeeded()
    {
        if (canvas != null) return;

        GameObject canvasGo = new GameObject("SeasonRewardCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        DontDestroyOnLoad(canvasGo);
        canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 1f;

        // Backdrop tối
        GameObject backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
        backdrop.transform.SetParent(canvasGo.transform, false);
        RectTransform brt = (RectTransform)backdrop.transform;
        brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
        brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;
        backdrop.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.78f);
        // Backdrop click cũng tính là claim để user không bị mất coin nếu lỡ tap ra ngoài.
        backdrop.GetComponent<Button>().onClick.AddListener(ClaimAndHide);

        // Panel chính
        GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasGo.transform, false);
        RectTransform prt = (RectTransform)panel.transform;
        prt.anchorMin = new Vector2(0.5f, 0.5f); prt.anchorMax = new Vector2(0.5f, 0.5f);
        prt.pivot = new Vector2(0.5f, 0.5f);
        prt.sizeDelta = new Vector2(820f, 1100f);
        Image panelImg = panel.GetComponent<Image>();
        panelImg.color = new Color(0.08f, 0.16f, 0.42f, 1f);

        // Title
        TextMeshProUGUI title = CreateText(panel.transform, "Title", "SEASON ENDED!", 70f, FontStyles.Bold, TextAlignmentOptions.Center);
        RectTransform titleRT = (RectTransform)title.transform;
        titleRT.anchorMin = new Vector2(0f, 1f); titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.sizeDelta = new Vector2(0f, 120f);
        titleRT.anchoredPosition = new Vector2(0f, -40f);
        title.color = new Color(1f, 0.85f, 0.25f, 1f);
        titleText = title;

        // Tier badge
        GameObject badgeGo = new GameObject("TierBadge", typeof(RectTransform), typeof(Image));
        badgeGo.transform.SetParent(panel.transform, false);
        RectTransform brt2 = (RectTransform)badgeGo.transform;
        brt2.anchorMin = new Vector2(0.5f, 1f); brt2.anchorMax = new Vector2(0.5f, 1f);
        brt2.pivot = new Vector2(0.5f, 1f);
        brt2.sizeDelta = new Vector2(360f, 360f);
        brt2.anchoredPosition = new Vector2(0f, -180f);
        Image bImg = badgeGo.GetComponent<Image>();
        Sprite circleSprite = BuildCircle(128);
        bImg.sprite = circleSprite;
        if (circleSprite != null)
        {
            generatedAssets.Add(circleSprite);
            if (circleSprite.texture != null) generatedAssets.Add(circleSprite.texture);
        }
        tierBadge = bImg;

        TextMeshProUGUI tierLabel = CreateText(badgeGo.transform, "TierText", "BRONZE", 56f, FontStyles.Bold, TextAlignmentOptions.Center);
        RectTransform tlRT = (RectTransform)tierLabel.transform;
        tlRT.anchorMin = Vector2.zero; tlRT.anchorMax = Vector2.one;
        tlRT.offsetMin = Vector2.zero; tlRT.offsetMax = Vector2.zero;
        tierLabel.color = Color.white;
        tierText = tierLabel;

        // Rank text
        TextMeshProUGUI rank = CreateText(panel.transform, "Rank", "Rank: -", 56f, FontStyles.Bold, TextAlignmentOptions.Center);
        RectTransform rRT = (RectTransform)rank.transform;
        rRT.anchorMin = new Vector2(0f, 0.5f); rRT.anchorMax = new Vector2(1f, 0.5f);
        rRT.pivot = new Vector2(0.5f, 0.5f);
        rRT.sizeDelta = new Vector2(0f, 100f);
        rRT.anchoredPosition = new Vector2(0f, -120f);
        rank.color = Color.white;
        rankText = rank;

        // Reward text
        TextMeshProUGUI reward = CreateText(panel.transform, "Reward", "+0 Coins", 64f, FontStyles.Bold, TextAlignmentOptions.Center);
        RectTransform rwRT = (RectTransform)reward.transform;
        rwRT.anchorMin = new Vector2(0f, 0.5f); rwRT.anchorMax = new Vector2(1f, 0.5f);
        rwRT.pivot = new Vector2(0.5f, 0.5f);
        rwRT.sizeDelta = new Vector2(0f, 100f);
        rwRT.anchoredPosition = new Vector2(0f, -240f);
        reward.color = new Color(1f, 0.85f, 0.25f, 1f);
        rewardText = reward;

        // Claim button
        GameObject btnGo = new GameObject("ClaimButton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(panel.transform, false);
        RectTransform btnRT = (RectTransform)btnGo.transform;
        btnRT.anchorMin = new Vector2(0.5f, 0f); btnRT.anchorMax = new Vector2(0.5f, 0f);
        btnRT.pivot = new Vector2(0.5f, 0f);
        btnRT.sizeDelta = new Vector2(420f, 130f);
        btnRT.anchoredPosition = new Vector2(0f, 60f);
        btnGo.GetComponent<Image>().color = new Color(0.20f, 0.62f, 0.32f, 1f);
        claimButton = btnGo.GetComponent<Button>();
        claimButton.onClick.AddListener(ClaimAndHide);

        TextMeshProUGUI btnLabel = CreateText(btnGo.transform, "ClaimLabel", "CLAIM", 54f, FontStyles.Bold, TextAlignmentOptions.Center);
        RectTransform blRT = (RectTransform)btnLabel.transform;
        blRT.anchorMin = Vector2.zero; blRT.anchorMax = Vector2.one;
        blRT.offsetMin = Vector2.zero; blRT.offsetMax = Vector2.zero;
        btnLabel.color = Color.white;

        canvasGo.SetActive(false);
    }

    void ShowReward(LeagueTier tier, int rank, int rewardCoins, string oldSeason)
    {
        canvas.gameObject.SetActive(true);
        titleText.text = "SEASON ENDED!\n<size=40><color=#B0C4FF>" + oldSeason + "</color></size>";
        tierText.text = tier.displayName.ToUpperInvariant();
        tierBadge.color = tier.primaryColor;
        rankText.text = "Final rank: <color=#FFD93D>#" + rank + "</color>";
        rewardText.text = "+" + rewardCoins + " coins";
    }

    void Hide()
    {
        if (canvas != null) canvas.gameObject.SetActive(false);
    }

    [ContextMenu("DEBUG: Force trigger season reward popup")]
    public void DebugForceTrigger()
    {
        BuildUIIfNeeded();
        LeagueTier debugTier = LeagueTierSystem.GetTierForScore(2500);
        int reward = LeagueTierSystem.CalcSeasonReward(debugTier, 7);
        pendingReward = reward;
        rewardClaimed = false;
        ShowReward(debugTier, 7, reward, "2026-W17");
    }

    [ContextMenu("DEBUG: Reset LastSeenSeason (sẽ trigger popup ở lần Play kế)")]
    public void DebugResetLastSeen()
    {
        PlayerPrefs.SetString(LAST_SEEN_SEASON_KEY, "2025-W01");
        PlayerPrefs.Save();
        Debug.Log("[SeasonReward] LastSeenSeason reset. Bấm Stop rồi Play lại để xem popup.");
    }

    static TextMeshProUGUI CreateText(Transform parent, string name, string content, float size, FontStyles style, TextAlignmentOptions align)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>();
        t.text = content;
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = align;
        t.color = Color.white;
        t.raycastTarget = false;
        return t;
    }

    static Sprite BuildCircle(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color clear = new Color(0f, 0f, 0f, 0f);
        float r = size * 0.5f;
        Vector2 c = new Vector2(r, r);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c);
            float a = Mathf.Clamp01(r - d);
            tex.SetPixel(x, y, a > 0f ? new Color(1f, 1f, 1f, a) : clear);
        }
        tex.Apply(false, true);
        return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f));
    }
}
