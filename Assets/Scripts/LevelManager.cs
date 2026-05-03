using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    // Biến lưu số Level hiện tại hiển thị cho người chơi (VD: Level 50)
    public int currentLevel; 

    private void Awake()
    {
        // Khởi tạo Singleton để gọi từ các script khác dễ dàng
        if (instance == null) instance = this;

        // Lấy Level đang chơi từ PlayerPrefs (được set ở LevelMapManager)
        currentLevel = PlayerPrefs.GetInt("PlayingLevel", 1);
    }

    // Hàm này sẽ được gọi khi quả bóng chạm đích (Win game)
    public void PassLevelAndLoadNext()
    {
        int nextLevel = currentLevel + 1;

        // Cập nhật tiến độ: Lưu mốc Level cao nhất mà người chơi đã mở khóa (chỉ lưu nếu lớn hơn mốc đang có)
        int maxUnlocked = PlayerPrefs.GetInt("CurrentLevel", 1);
        if (nextLevel > maxUnlocked)
        {
            PlayerPrefs.SetInt("CurrentLevel", nextLevel);
        }
        
        // Thiết lập level tiếp theo sẽ chơi
        PlayerPrefs.SetInt("PlayingLevel", nextLevel);

        Debug.Log("TỪ MÀN: " + currentLevel + " -> CHUYỂN SANG MÀN: " + nextLevel);
        
        // Load lại Scene GamePlay duy nhất để reset lại màn chơi với độ khó mới
        SceneManager.LoadScene("GamePlay");
    }
}