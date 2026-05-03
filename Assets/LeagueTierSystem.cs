using UnityEngine;

[System.Serializable]
public class LeagueTier
{
    public string id;
    public string displayName;
    public int minScore;
    public Color primaryColor;
    public Color secondaryColor;
    public int seasonRewardCoins;
}

/// <summary>
/// 6 tier giống mobile game phổ biến: Bronze → Silver → Gold → Platinum → Diamond → Master.
/// Score → Tier dựa vào ngưỡng minScore.
/// </summary>
public static class LeagueTierSystem
{
    public static readonly LeagueTier[] Tiers = new LeagueTier[]
    {
        new LeagueTier
        {
            id = "bronze", displayName = "Bronze",
            minScore = 0, seasonRewardCoins = 50,
            primaryColor = new Color(0.80f, 0.50f, 0.20f, 1f),
            secondaryColor = new Color(0.55f, 0.32f, 0.10f, 1f),
        },
        new LeagueTier
        {
            id = "silver", displayName = "Silver",
            minScore = 500, seasonRewardCoins = 100,
            primaryColor = new Color(0.83f, 0.86f, 0.92f, 1f),
            secondaryColor = new Color(0.55f, 0.60f, 0.70f, 1f),
        },
        new LeagueTier
        {
            id = "gold", displayName = "Gold",
            minScore = 1500, seasonRewardCoins = 200,
            primaryColor = new Color(1f, 0.84f, 0.20f, 1f),
            secondaryColor = new Color(0.78f, 0.60f, 0.10f, 1f),
        },
        new LeagueTier
        {
            id = "platinum", displayName = "Platinum",
            minScore = 3500, seasonRewardCoins = 350,
            primaryColor = new Color(0.65f, 0.92f, 0.95f, 1f),
            secondaryColor = new Color(0.30f, 0.65f, 0.75f, 1f),
        },
        new LeagueTier
        {
            id = "diamond", displayName = "Diamond",
            minScore = 8000, seasonRewardCoins = 500,
            primaryColor = new Color(0.55f, 0.85f, 1f, 1f),
            secondaryColor = new Color(0.25f, 0.55f, 0.95f, 1f),
        },
        new LeagueTier
        {
            id = "master", displayName = "Master",
            minScore = 15000, seasonRewardCoins = 1000,
            primaryColor = new Color(0.85f, 0.40f, 1f, 1f),
            secondaryColor = new Color(0.55f, 0.10f, 0.80f, 1f),
        },
    };

    public static LeagueTier GetTierForScore(int score)
    {
        LeagueTier current = Tiers[0];
        for (int i = 0; i < Tiers.Length; i++)
        {
            if (score >= Tiers[i].minScore)
            {
                current = Tiers[i];
            }
        }
        return current;
    }

    public static LeagueTier GetTierById(string id)
    {
        if (string.IsNullOrEmpty(id)) return Tiers[0];
        for (int i = 0; i < Tiers.Length; i++)
        {
            if (Tiers[i].id == id) return Tiers[i];
        }
        return Tiers[0];
    }

    /// <summary>Trả về tier kế tiếp + điểm còn cần để lên hạng (null nếu đã max tier).</summary>
    public static bool TryGetNextTier(int score, out LeagueTier next, out int scoreNeeded)
    {
        for (int i = 0; i < Tiers.Length; i++)
        {
            if (score < Tiers[i].minScore)
            {
                next = Tiers[i];
                scoreNeeded = Tiers[i].minScore - score;
                return true;
            }
        }
        next = null;
        scoreNeeded = 0;
        return false;
    }

    /// <summary>
    /// Tính reward coin cho 1 player cuối mùa dựa vào tier + rank.
    /// Top 1: x3, Top 2: x2, Top 3: x1.5, Top 10: x1.2, còn lại x1.
    /// </summary>
    public static int CalcSeasonReward(LeagueTier tier, int rank)
    {
        if (tier == null) tier = Tiers[0];
        float multiplier = 1f;
        if (rank == 1) multiplier = 3f;
        else if (rank == 2) multiplier = 2f;
        else if (rank == 3) multiplier = 1.5f;
        else if (rank <= 10) multiplier = 1.2f;
        return Mathf.RoundToInt(tier.seasonRewardCoins * multiplier);
    }
}
