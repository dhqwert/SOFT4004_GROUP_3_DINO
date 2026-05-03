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

        // Level hiện tại là đúng Scene đang chạy (chứ không phải mốc tiến độ lớn nhất trong máy)
        currentLevel = SceneManager.GetActiveScene().buildIndex;
    }

    // Hàm này sẽ được gọi khi quả bóng chạm đích (Win game)
    public void PassLevelAndLoadNext()
    {
        // 1. Xác định Level tiếp theo dựa trên Scene HIỆN TẠI (để tránh lỗi biến cũ nhớ nhầm)
        int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextLevel = activeSceneIndex + 1;

        // 2. Logic tính toán Scene cần Load
        int totalScenes = SceneManager.sceneCountInBuildSettings;
        int maxPlayableLevel = Mathf.Max(1, totalScenes - 1);
        int safeUnlockedLevel = Mathf.Clamp(nextLevel, 1, maxPlayableLevel);

        // Cập nhật tiến độ: Lưu mốc Level cao nhất mà người chơi đã mở khóa (chỉ lưu nếu lớn hơn mốc đang có)
        int maxUnlocked = PlayerPrefs.GetInt("CurrentLevel", 1);
        if (safeUnlockedLevel > maxUnlocked)
        {
            PlayerPrefs.SetInt("CurrentLevel", safeUnlockedLevel);
        }

        Debug.Log("TỪ MÀN: " + activeSceneIndex + " -> CHUYỂN SANG MÀN: " + nextLevel + " | TỔNG SỐ MÀN: " + totalScenes);
        
        if (nextLevel < totalScenes) 
        {
            // Nếu Level hiện tại VẪN NHỎ HƠN tổng số Scene bạn có -> Load Scene tiếp theo bình thường
            SceneManager.LoadScene(nextLevel);
        }
        else 
        {
            // TÍNH NĂNG VÔ HẠN: Random load lại một màn chơi bất kỳ
            int randomScene = Random.Range(1, totalScenes); 
            SceneManager.LoadScene(randomScene);
        }
    }
}