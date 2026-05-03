using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPrefsMigration
{
    private const int MigrationVersion = 1;
    private const string MigrationKey = "PlayerDataMigrationVersion";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RunMigration()
    {
        int currentVersion = PlayerPrefs.GetInt(MigrationKey, 0);
        if (currentVersion >= MigrationVersion)
        {
            return;
        }

        int totalScenes = SceneManager.sceneCountInBuildSettings;
        int maxPlayableLevel = Mathf.Max(1, totalScenes - 1);

        int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        currentLevel = Mathf.Clamp(currentLevel, 1, maxPlayableLevel);
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);

        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        if (totalCoins < 0)
        {
            PlayerPrefs.SetInt("TotalCoins", 0);
        }

        // League score giờ lưu trên Firebase (FirebaseLeaderboardService),
        // dọn key cũ "TotalLeagueScore" để tránh nhầm lẫn khi debug.
        if (PlayerPrefs.HasKey("TotalLeagueScore"))
        {
            PlayerPrefs.DeleteKey("TotalLeagueScore");
        }

        int removeAds = PlayerPrefs.GetInt("RemoveAds", 0);
        PlayerPrefs.SetInt("RemoveAds", removeAds == 1 ? 1 : 0);

        int isMuted = PlayerPrefs.GetInt("isMuted", 0);
        PlayerPrefs.SetInt("isMuted", isMuted == 1 ? 1 : 0);

        PlayerPrefs.SetInt(MigrationKey, MigrationVersion);
        PlayerPrefs.Save();
    }
}
