using UnityEngine;
using TMPro;

public class LevelProgressUI : MonoBehaviour
{
    public TextMeshProUGUI currentLevelText;
    public TextMeshProUGUI nextLevelText;

    void Start()
    {
        // Phải lấy "PlayingLevel" (level đang chơi thực tế) thay vì "CurrentLevel" (level cao nhất đã mở)
        int currentLevel = PlayerPrefs.GetInt("PlayingLevel", 1);
        RefreshUI(currentLevel);
    }

    public void RefreshUI(int level)
    {
        if (currentLevelText != null) 
        {
            currentLevelText.text = level.ToString();
            currentLevelText.enableWordWrapping = false; // Tắt tự động xuống dòng để số 1000 không bị đứt đoạn
            currentLevelText.enableAutoSizing = true; // Bật tự động thu nhỏ font chữ cho vừa với vòng tròn
        }
        
        if (nextLevelText != null) 
        {
            nextLevelText.text = (level + 1).ToString();
            nextLevelText.enableWordWrapping = false;
            nextLevelText.enableAutoSizing = true;
        }
    }
}