using UnityEngine;
using TMPro;
public class LevelProgressUI : MonoBehaviour
{
    // Kéo thả 2 cái Text của 2 vòng tròn vào đây
    public TextMeshProUGUI currentLevelText; 
    public TextMeshProUGUI nextLevelText;

    private void Start()
    {
        // Lấy Level dựa trên Build Index của Scene hiện tại (thay vì lấy mốc cao nhất từ PlayerPrefs)
        int currentLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;

        // Hiển thị lên giao diện
        currentLevelText.text = currentLevel.ToString(); // Vòng tròn bên trái
        nextLevelText.text = (currentLevel + 1).ToString(); // Vòng tròn bên phải (+1)
    }
}