using UnityEngine;
using TMPro;
public class LevelProgressUI : MonoBehaviour
{
    // Kéo thả 2 cái Text của 2 vòng tròn vào đây
    public TextMeshProUGUI currentLevelText; 
    public TextMeshProUGUI nextLevelText;

    private void Start()
    {
        // Lấy Level hiện tại từ hệ thống PlayerPrefs mà chúng ta đã làm ở bước trước
        int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);

        // Hiển thị lên giao diện
        currentLevelText.text = currentLevel.ToString(); // Vòng tròn bên trái
        nextLevelText.text = (currentLevel + 1).ToString(); // Vòng tròn bên phải (+1)
    }
}