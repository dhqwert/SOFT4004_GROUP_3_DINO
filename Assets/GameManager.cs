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

    [Header("Hồi sinh")]
    [Tooltip("Đẩy bóng (tag Player) lên sau khi xem video hồi sinh")]
    public float reviveUpwardOffset = 2f;

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

    // ====== CHỨC NĂNG LƯU ĐIỂM XẾP HẠNG ======
    public void WinLevel()
    {
        if (instance == null || levelWin) return;
        levelWin = true;

        // --- THƯỞNG CUỐI MÀN theo level ---
        int level = PlayerPrefs.GetInt("CurrentLevel", 1);
        int bonusScore = level * 100; // Level 1=100đ, Level 10=1000đ, Level 20=2000đ
        int bonusCoins = level * 2;   // Level 1=2 vàng, Level 10=20 vàng

        currentScore += bonusScore;
        currentCoins += bonusCoins;
        PlayerPrefs.SetInt("TotalCoins", currentCoins);
        PlayerPrefs.Save();
        UpdateUI();

        Debug.Log($"[Level {level}] Thưởng cuối màn: +{bonusScore} điểm, +{bonusCoins} vàng");

        // Cộng vào bảng xếp hạng
        int tempTotalScore = PlayerPrefs.GetInt("TotalLeagueScore", 0);
        tempTotalScore += currentScore;
        PlayerPrefs.SetInt("TotalLeagueScore", tempTotalScore);
        PlayerPrefs.Save();

        Debug.Log("Tổng điểm xếp hạng: " + tempTotalScore);
    }

    // ====== CHỨC NĂNG HỒI SINH ======
    // (Bấm nút này sẽ mở xem Video, xem xong thực thi hàm Hồi Sinh)
    public void RevivePlayer()
    {
        void DoRevive()
        {
            gameOver = false;
            if (gameOverPannal != null) {
                gameOverPannal.SetActive(false);
            }
            Time.timeScale = 1;

            GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
            if (playerGo != null) {
                Ball ball = playerGo.GetComponent<Ball>();
                if (ball != null) {
                    ball.ApplyReviveNudge(reviveUpwardOffset);
                } else {
                    playerGo.transform.position += Vector3.up * reviveUpwardOffset;
                }
            }
        }

        if (AdManager.instance != null)
        {
            AdManager.instance.ShowRewardedAd(DoRevive);
        }
        else
        {
            DoRevive();
        }
    }

    // Nút Bỏ qua hồi sinh, chơi lại từ đầu
    public void RestartLevel()
    {
        gameOver = false;
        levelWin = false;
        Time.timeScale = 1f;
        currentScore = 0;
        UpdateUI();

        if (gameOverPannal != null) gameOverPannal.SetActive(false);
        if (levelWinPannal != null) levelWinPannal.SetActive(false);

        // Sinh lại đúng level hiện tại (không tăng số)
        int current = PlayerPrefs.GetInt("CurrentLevel", 1);
        if (GameSceneController.instance != null)
            GameSceneController.instance.helixManager.GenerateForLevel(current);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Update () {
        if(gameOver) {
            Time.timeScale = 0; 
            if (gameOverPannal != null) {
                gameOverPannal.SetActive (true); 
            }
            
            // Xóa cái đoạn Bấm chuột nạp lại bàn chơi mặc định (nhường đường cho nút Restart và Revive UI)
            // if(Input.GetMouseButtonDown(0)) {
            //     SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            // }
        }

        if(levelWin) {
            if (levelWinPannal != null) {
                levelWinPannal.SetActive (true); 
            }

            // Cập nhật: Khi nhấp màn hình sang màn mới, kiểm tra chiếu QC trước
            if(Input.GetMouseButtonDown (0)) {

                // THAY BẰNG:
                if (AdManager.instance != null)
                {
                    AdManager.instance.ShowInterstitialAdIfReady(SceneManager.GetActiveScene().buildIndex, () => {
                        GoNextLevel(); // ← gọi hàm mới
                    });
                }
                else
                {
                    GoNextLevel(); // ← gọi hàm mới
                }

                // Chặn không cho gọi hàm nhiều lần
                levelWin = false; 
            }
        }
    }
    public void GoNextLevel()
    {
        int next = PlayerPrefs.GetInt("CurrentLevel", 1) + 1;
        PlayerPrefs.SetInt("CurrentLevel", next);
        PlayerPrefs.Save();

        // Nếu GameSceneController tồn tại → sinh level mới ngay tại chỗ
        if (GameSceneController.instance != null)
        {
            gameOver = false;
            levelWin = false;
            Time.timeScale = 1f;
            currentScore = 0;
            UpdateUI();

            if (levelWinPannal != null) levelWinPannal.SetActive(false);

            GameSceneController.instance.OnLevelComplete();
        }
        else
        {
            // Fallback: load lại scene Level1 như cũ
            SceneManager.LoadScene("Level1");
        }
    }
}
