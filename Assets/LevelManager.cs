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

        // Lấy dữ liệu level đã lưu trong máy. Nếu người chơi mới tải game, mặc định là Level 1.
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
    }

    // Hàm này sẽ được gọi khi quả bóng chạm đích (Win game)
    public void PassLevelAndLoadNext()
    {
        // 1. Tăng Level lên 1 và LƯU VÀO MÁY
        currentLevel++;
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);

        // 2. Logic tính toán Scene cần Load
        // Lấy tổng số lượng Scene bạn đã ném vào Build Settings
        int totalScenes = SceneManager.sceneCountInBuildSettings;

        // Giả sử: Scene 0 là Home Menu. 
        // Các Scene chơi thật bắt đầu từ 1.
        
        if (currentLevel < totalScenes) 
        {
            // Nếu Level hiện tại VẪN NHỎ HƠN tổng số Scene bạn có -> Load Scene tiếp theo bình thường
            SceneManager.LoadScene(currentLevel);
        }
        else 
        {
            // TÍNH NĂNG VÔ HẠN: Nếu người chơi đạt level 10 nhưng bạn chỉ có 3 Scene.
            // Game sẽ tự động Random load lại một màn chơi bất kỳ từ màn 1 đến màn cuối.
            // Nhưng UI trên màn hình vẫn sẽ hiển thị là Level 10.
            int randomScene = Random.Range(1, totalScenes); 
            SceneManager.LoadScene(randomScene);
        }
    }
}