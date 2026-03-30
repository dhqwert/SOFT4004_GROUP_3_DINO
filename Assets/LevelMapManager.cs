using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class LevelMapManager : MonoBehaviour
{
    [Header("Kéo thả từ bên ngoài vào:")]
    public GameObject nodePrefab; // Kéo file Prefab màu xanh ở Project vào đây
    public Transform container;   // Kéo cái LevelMapContainer vào đây
    
    [Header("Cài đặt Level:")]
    public int totalLevels = 5;   // Tổng số Level game bạn đang có

    void Start()
    {
        // 1. Lấy level hiện tại mà người chơi đã đạt đến (Mặc định là 1)
        int unlockedLevel = PlayerPrefs.GetInt("CurrentLevel", 1);

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
                
                // Ý của bạn là "Có thể chơi level nào cũng được", nên mình vẫn để interactable = true.
                // Nếu muốn khóa không cho chơi "ăn gian" vượt cấp thì đổi chữ true ở dưới thành false nhé!
                nodeButton.interactable = true; 
            }

            // --- LỆNH CHUYỂN MÀN KHI BẤM NÚT ---
            int levelToLoad = i; // Bắt buộc phải có biến tạm này để ghi nhớ đúng số thứ tự
            nodeButton.onClick.AddListener(() => {
                // Đã xóa lệnh SetInt("CurrentLevel") ở đây để không làm mất tiến độ lớn nhất của người chơi
                // Chỉ Load Scene tương ứng thôi (vì LevelManager giờ dựa vào Scene Index thật)
                SceneManager.LoadScene(levelToLoad); 
            });
        }
    }
}