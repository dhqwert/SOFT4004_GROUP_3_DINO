using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class LevelMapManager : MonoBehaviour
{
    [Header("Kéo thả từ bên ngoài vào:")]
    public GameObject nodePrefab; // Kéo file Prefab màu xanh ở Project vào đây
    public Transform container;   // Kéo cái LevelMapContainer vào đây
    public TextMeshProUGUI coinText;
    [Header("Cài đặt Level:")]
    public int totalLevels = 5;   // Tổng số Level game bạn đang có

    void Start()
    {
        // Hiển thị số Tiền vàng hiện có (nếu bạn đã kéo thả UI xu vào coinText)
        if (coinText != null) 
        {
            coinText.text = PlayerPrefs.GetInt("TotalCoins", 0).ToString();
        }

        // 1. Lấy level hiện tại mà người chơi đã đạt đến (Mặc định là 1)
        int unlockedLevel = Mathf.Max(PlayerPrefs.GetInt("CurrentLevel", 1), 1);

        // 2. Vòng lặp tự động đúc ra các vòng tròn (Sinh từ Level to nhất lùi về Level 1 để level to nằm trên cùng)
        for (int i = totalLevels; i >= 1; i--)
        {
            // Sinh ra 1 vòng tròn mới và nhét nó vào trong Container
            GameObject newNode = Instantiate(nodePrefab, container);
            
            // Tìm các thành phần bên trong cái vòng tròn vừa sinh ra
            TextMeshProUGUI levelText = newNode.GetComponentInChildren<TextMeshProUGUI>();
            Image nodeImage = newNode.GetComponent<Image>();
            Button nodeButton = newNode.GetComponent<Button>();
            
            // Tìm cái hiệu ứng phát sáng (Glow) nếu bạn có tạo một object con tên là "Glow"
            Transform glowEffect = newNode.transform.Find("Glow"); 

            // Gán số cho vòng tròn (Ví dụ: 5, 4, 3, 2, 1)
            levelText.text = i.ToString();

            // --- KIỂM TRA ĐỂ TÔ MÀU VÀ BẬT TẮT NÚT ---
            if (i == unlockedLevel) {
                // ĐANG CHƠI: Tô màu Cam/Xanh lá, bật hiệu ứng sáng
                nodeImage.color = Color.green; 
                if (glowEffect != null) glowEffect.gameObject.SetActive(true);
                nodeButton.interactable = true;
            } 
            else if (i < unlockedLevel) {
                // ĐÃ VƯỢT QUA: Tô màu khác cho cũ đi, tắt phát sáng
                nodeImage.color = Color.cyan; 
                if (glowEffect != null) glowEffect.gameObject.SetActive(false);
                nodeButton.interactable = true; // Cho phép bấm để CHƠI LẠI
            } 
            else {
                // CHƯA CHƠI TỚI: Tô màu xám xịt
                nodeImage.color = Color.gray;
                if (glowEffect != null) glowEffect.gameObject.SetActive(false);
                
                // Khóa lại không cho người chơi bấm lén vượt cấp
                nodeButton.interactable = false; 
            }

            // --- CẬP NHẬT ĐƯỜNG NỐI (LINE TỪ NODE HIỆN TẠI XUỐNG NODE TRƯỚC ĐÓ) ---
            Transform lineObj = newNode.transform.Find("Line");
            if (lineObj != null) {
                Image lineImage = lineObj.GetComponent<Image>();
                // Tô màu đường nối
                if (i <= unlockedLevel) {
                    lineImage.color = Color.cyan; // Bạn có thể đổi Color.cyan thành màu xanh giống vòng tròn (vd: Color.green)
                } else {
                    lineImage.color = Color.gray; // Chưa tới thì để dòng kẻ xám
                }
                
                // Ẩn dòng kẻ của trạm đầu tiên (Lv 1) vì ở dưới Lv 1 không còn Level nào nữa
                if (i == 1) {
                    lineObj.gameObject.SetActive(false);
                }
            }

            // --- LỆNH CHUYỂN MÀN KHI BẤM NÚT ---
            int levelToLoad = i; // Bắt buộc phải có biến tạm này để ghi nhớ đúng số thứ tự
            nodeButton.onClick.AddListener(() => {
                PlayerPrefs.SetInt("PlayingLevel", levelToLoad);
                SceneManager.LoadScene("GamePlay"); // Luôn Load scene GamePlay
            });
        }
    }

    // Gắn hàm này vào sự kiện OnClick của một Nút bấm UI để xoá dữ liệu chơi lại từ đầu
    public void ResetGameProgress()
    {
        // Xoá bỏ mốc lưu bàn chơi hiện tại
        PlayerPrefs.DeleteKey("CurrentLevel");
        // Hoặc bạn có thể dùng PlayerPrefs.DeleteAll(); nếu muốn xoá trắng tải khoản (Cấp, tiền, setting...)
        
        // Tải lại Scene Map hiện tại để tự động cập nhật lại các nút bị khóa và màu sắc
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Gắn hàm này vào cái NÚT PLAY to bự ngoài màn hình để bấm là vô luôn ván mới nhất
    public void PlayCurrentLevel()
    {
        int unlockedLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        PlayerPrefs.SetInt("PlayingLevel", unlockedLevel);
        SceneManager.LoadScene("GamePlay");
    }

    // [DÀNH CHO DEV] Thêm một Nút Bí Mật ngay trên thanh Inspector của Unity để bạn vọc vạch
    [ContextMenu("MỞ KHÓA TOÀN BỘ MAP (Click vào đây)")]
    public void UnlockAllLevels()
    {
        // Phá đảo luôn: Ép mốc lưu trữ thành Max level
        PlayerPrefs.SetInt("CurrentLevel", totalLevels);
        PlayerPrefs.Save();
        Debug.Log("DEV HACK THÀNH CÔNG: Đã cạy khoá toàn bộ " + totalLevels + " Level!");

        // Nếu game đang chạy (Play Mode) thì giựt sập load lại cảnh để áp dụng ngay lập tức
        if (Application.isPlaying)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}