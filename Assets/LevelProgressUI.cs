using UnityEngine;
using TMPro;

public class LevelProgressUI : MonoBehaviour
{
    public TextMeshProUGUI currentLevelText;
    public TextMeshProUGUI nextLevelText;

    void Start()
    {
        int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        RefreshUI(currentLevel);
    }

    // Gọi từ GameSceneController khi sang level mới
    public void RefreshUI(int level)
    {
        if (currentLevelText != null) currentLevelText.text = level.ToString();
        if (nextLevelText != null) nextLevelText.text = (level + 1).ToString();
    }
}