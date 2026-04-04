using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Sử dụng thư viện cho TextMeshPro

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // Tạo trạm trung tâm để gọi từ script khác

    public static bool gameOver;
    public static bool levelWin;

    public GameObject gameOverPannal;
    public GameObject levelWinPannal;

    [Header("Giao diện Điểm và Vàng")]
    public TextMeshProUGUI scoreText; // Kéo thả cái Text hiển thị điểm vào đây
    public TextMeshProUGUI coinText;  // Kéo thả cái Text hiển thị vàng vào đây

    private int currentScore = 0;
    private int currentCoins = 0;

    private void Awake()
    {
        if (instance == null) instance = this;

        // Tải số lượng Vàng lưu trữ từ lần trước lên
        currentCoins = PlayerPrefs.GetInt("TotalCoins", 0);
    }

    private void Start()
    {
        gameOver = false;
        levelWin = false;
        Time.timeScale = 1; 
        
        currentScore = 0; // Bắt đầu màn luôn là 0 điểm
        UpdateUI();
    }

    // Hàm dùng để gọi từ Ring khi phá vỡ 1 tầng
    public void AddScore(int amountToAdd)
    {
        currentScore += amountToAdd;
        
        // Thưởng 1 Vàng
        currentCoins += 1;

        // Lưu Vàng vào bộ nhớ vĩnh viễn (đóng game không bị mất)
        PlayerPrefs.SetInt("TotalCoins", currentCoins);
        
        UpdateUI();
    }

    // Cập nhật lại những dòng chữ hiển thị
    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "" + currentScore;
        if (coinText != null) coinText.text = "Coins: " + currentCoins; 
    }

    // ====== CHỨC NĂNG HỒI SINH ======
    // (Bấm nút này sẽ mở xem Video, xem xong thực thi hàm Hồi Sinh)
    public void RevivePlayer()
    {
        if (AdManager.instance != null)
        {
            AdManager.instance.ShowRewardedAd(() => {
                // LOGIC HỒI SINH SAU KHI XEM XONG VIDEO
                gameOver = false;
                gameOverPannal.SetActive(false);
                Time.timeScale = 1;

                // Ghi chú: Nếu quả bóng đang kẹt vào cái gai màu đỏ, bạn phải nhấc nó lên một chút bằng script Ball
                // Ví dụ (Tuỳ code chướng ngại vật của bạn): 
                // GameObject.FindObjectOfType<Player>().transform.position += Vector3.up * 2f; 
            });
        }
    }

    // Nút Bỏ qua hồi sinh, chơi lại từ đầu
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Update () {
        if(gameOver) {
            Time.timeScale = 0; 
            gameOverPannal.SetActive (true); 
            
            // Xóa cái đoạn Bấm chuột nạp lại bàn chơi mặc định (nhường đường cho nút Restart và Revive UI)
            // if(Input.GetMouseButtonDown(0)) {
            //     SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            // }
        }

        if(levelWin) {
            levelWinPannal.SetActive (true); 

            // Cập nhật: Khi nhấp màn hình sang màn mới, kiểm tra chiếu QC trước
            if(Input.GetMouseButtonDown (0)) {
                
                if (AdManager.instance != null) {
                    // Nhờ AdManager xét duyệt xuất hiện quảng cáo
                    AdManager.instance.ShowInterstitialAdIfReady(SceneManager.GetActiveScene().buildIndex, () => {
                        // Action sau khi đóng quảng cáo (Hoặc không được hiện qc)
                        LevelManager.instance.PassLevelAndLoadNext();
                    });
                } else {
                    // Đề phòng trường hợp lỗi chưa thả AdManager vào Scene
                    LevelManager.instance.PassLevelAndLoadNext();
                }

                // Chặn không cho gọi hàm nhiều lần
                levelWin = false; 
            }
        }
    }
}