using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class RankTier
{
    public string rankName = "Tên hạng (VD: Bronze)";
    [Header("Điểm cần có để leo lên mốc này:")]
    public int scoreToReach;
    [Header("Kéo thả thanh UI màu của hạng này vô để mũi tên nó đu theo:")]
    public RectTransform rankUIPosition;
}

[System.Serializable]
public class LeaguePlayerData
{
    public string playerName = "Player";
    public int score = 0;
}

/// <summary>
/// LeagueManager (gọn): chỉ lo phần rank tier (mũi tên + điểm hiện tại) và lưu danh sách player test.
/// Toàn bộ phần render bảng xếp hạng được làm trong <see cref="LeaderboardPanel"/>.
/// </summary>
public class LeagueManager : MonoBehaviour
{
    [Header("Mũi tên rank tier (panel cũ)")]
    public RectTransform arrowIndicator;

    [Header("Text điểm hiện tại của bạn")]
    public TextMeshProUGUI currentScoreText;

    [Header("Khai báo các nấc thang Danh Vọng (Từ Thấp -> Cao)")]
    public RankTier[] rankTiers;

    [Header("Dữ liệu test cho bảng xếp hạng (sẽ được LeaderboardPanel đọc)")]
    public string localPlayerFallbackName = "You";
    public LeaguePlayerData[] localTestPlayers;

    int myTotalLeagueScore;

    void OnEnable()
    {
        StartCoroutine(RefreshAfterLayout());
    }

    IEnumerator RefreshAfterLayout()
    {
        yield return null;
        yield return null;
        Canvas.ForceUpdateCanvases();
        RefreshLeague();
    }

    public void RefreshLeague()
    {
        if (currentScoreText != null)
        {
            currentScoreText.text = "CURRENT POINTS: ...";
        }

        FirebaseLeaderboardService firebase = FirebaseLeaderboardService.instance;
        if (firebase != null && firebase.IsConfigured)
        {
            firebase.FetchEntry(firebase.LocalPlayerId, entry =>
            {
                myTotalLeagueScore = entry != null ? entry.score : 0;
                ApplyRankUI();
            });
        }
        else
        {
            myTotalLeagueScore = 0;
            ApplyRankUI();
        }
    }

    void ApplyRankUI()
    {
        if (currentScoreText != null)
        {
            currentScoreText.text = "CURRENT POINTS: " + myTotalLeagueScore;
        }

        if (rankTiers == null || rankTiers.Length == 0)
        {
            return;
        }

        int currentRankIndex = 0;
        for (int i = 0; i < rankTiers.Length; i++)
        {
            if (myTotalLeagueScore >= rankTiers[i].scoreToReach)
            {
                currentRankIndex = i;
            }
        }

        if (arrowIndicator != null && rankTiers[currentRankIndex].rankUIPosition != null)
        {
            Vector3 targetPos = arrowIndicator.position;
            targetPos.y = rankTiers[currentRankIndex].rankUIPosition.position.y;
            arrowIndicator.position = targetPos;
        }
    }

    [ContextMenu("Reset Hạng (Về 0)")]
    public void ResetLeagueScore()
    {
        FirebaseLeaderboardService firebase = FirebaseLeaderboardService.instance;
        if (firebase != null && firebase.IsConfigured)
        {
            firebase.SetLocalPlayerScore(0, ok =>
            {
                Debug.Log(ok
                    ? "[LeagueManager] Đã reset điểm trên Firebase về 0."
                    : "[LeagueManager] Reset Firebase thất bại.");
                StartCoroutine(RefreshAfterLayout());
            });
        }
        else
        {
            myTotalLeagueScore = 0;
            ApplyRankUI();
        }
    }
}
