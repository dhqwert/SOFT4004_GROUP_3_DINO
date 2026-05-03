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

    // Cờ chặn: đang trong quá trình xử lý sang màn mới (chờ ad / chờ scene load)
    private bool _isProcessingLevel = false;

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
        _isProcessingLevel = false; // Reset cờ mỗi khi vào màn mới
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

        Debug.Log("[WinLevel] Đã đánh dấu thắng! levelWin = true. Hiện panel.");

        // Hiện panel chiến thắng ngay lập tức (không cần chờ Update)
        if (levelWinPannal != null) {
            levelWinPannal.SetActive(true);
        }

        // Cộng điểm ván này vào Kho điểm Xếp Hạng Trọn Đời
        int tempTotalScore = PlayerPrefs.GetInt("TotalLeagueScore", 0);
        tempTotalScore += currentScore;
        PlayerPrefs.SetInt("TotalLeagueScore", tempTotalScore);
        PlayerPrefs.Save();
        
        Debug.Log("Tích luỹ thành công! Tổng điểm Xếp Hạng hiện tại: " + tempTotalScore);
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
        SceneManager.LoadScene("GamePlay");
    }

    private void Update () {
        if(gameOver) {
            Time.timeScale = 0; 
            if (gameOverPannal != null) {
                gameOverPannal.SetActive (true); 
            }
        }

        if(levelWin) {
            // Dừng mọi vật lý để ngăn cột tự tan rã trong khi đang ở màn hình chiến thắng
            Time.timeScale = 0;

            if (levelWinPannal != null) {
                levelWinPannal.SetActive (true); 
            }

            // Fallback: nếu "Go to next level" là Text thường (không phải Button),
            // click bất kỳ vị trí nào trên màn hình vẫn sẽ chuyển sang màn mới
            if(Input.GetMouseButtonDown(0) && !_isProcessingLevel) {
                NextLevelAction();
            }
        }
    }

    // Nút Giao diện Next Level gọi hàm này (gắn trực tiếp vào Button OnClick)
    public void NextLevelAction()
    {
        // Chặn bấm đúp - nếu đang xử lý rồi thì bỏ qua hoàn toàn
        if (_isProcessingLevel) {
            Debug.Log("[NextLevel] Đang xử lý, bỏ qua click đúp.");
            return;
        }
        _isProcessingLevel = true;
        levelWin = false;

        if (AdManager.instance != null) {
            int playingLevel = PlayerPrefs.GetInt("PlayingLevel", 1);
            AdManager.instance.ShowInterstitialAdIfReady(playingLevel, () => {
                // Callback này chỉ chạy 1 lần duy nhất sau khi ad đóng hoặc bị bỏ qua
                ProceedToNextLevel();
            });
        } else {
            ProceedToNextLevel();
        }
    }

    // Xử lý nạp cảnh và lưu game an toàn - chỉ được gọi đúng 1 lần
    private void ProceedToNextLevel()
    {
        Debug.Log("[NextLevel] ProceedToNextLevel() được gọi.");
        
        // Tính toán và lưu NGAY LẬP TỨC trước khi load scene
        int current = PlayerPrefs.GetInt("PlayingLevel", 1);
        int next = current + 1;
        int maxUnlocked = PlayerPrefs.GetInt("CurrentLevel", 1);
        if (next > maxUnlocked) PlayerPrefs.SetInt("CurrentLevel", next);
        PlayerPrefs.SetInt("PlayingLevel", next);
        PlayerPrefs.Save();
        
        Debug.Log($"[NextLevel] Đã lưu PlayingLevel = {next}, bắt đầu load scene GamePlay.");
        
        // Load scene - không phụ thuộc vào LevelManager nữa
        SceneManager.LoadScene("GamePlay");
    }
}
