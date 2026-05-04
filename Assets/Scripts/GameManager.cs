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

    [Header("Giao diện Tổng Kết (Win Summary)")]
    public TextMeshProUGUI winScoreText;
    public TextMeshProUGUI winCoinText;

    [Header("Hồi sinh")]
    [Tooltip("Đẩy bóng (tag Player) lên sau khi xem video hồi sinh")]
    public float reviveUpwardOffset = 2f;

    private int currentScore = 0;
    private int currentCoins = 0;
    private int coinsGainedThisLevel = 0;

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
        coinsGainedThisLevel = 0;
        UpdateUI();
    }

    // Hàm dùng để gọi từ Ring khi phá vỡ 1 tầng
    public void AddScore(int amountToAdd)
    {
        currentScore += amountToAdd;
        
        // Thưởng 1 Vàng
        AddCoins(1);
    }

    public void AddCoins(int amountToAdd)
    {
        currentCoins += amountToAdd;
        coinsGainedThisLevel += amountToAdd;

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

        // Cập nhật text tổng kết (hoặc tự tạo nếu chưa gán)
        if (winScoreText != null) {
            winScoreText.text = "Score: " + currentScore;
        } else if (levelWinPannal != null) {
            GameObject textObj = new GameObject("DynamicWinScoreText");
            textObj.transform.SetParent(levelWinPannal.transform, false);
            winScoreText = textObj.AddComponent<TextMeshProUGUI>();
            winScoreText.text = "Score: " + currentScore;
            winScoreText.fontSize = 50;
            winScoreText.alignment = TextAlignmentOptions.Center;
            winScoreText.rectTransform.sizeDelta = new Vector2(600, 100); // Thêm size để không bị rớt dòng
            winScoreText.rectTransform.anchoredPosition = new Vector2(0, 100);
        }

        if (winCoinText != null) {
            winCoinText.text = "Earned Coins: +" + coinsGainedThisLevel;
        } else if (levelWinPannal != null) {
            GameObject textObj = new GameObject("DynamicWinCoinText");
            textObj.transform.SetParent(levelWinPannal.transform, false);
            winCoinText = textObj.AddComponent<TextMeshProUGUI>();
            winCoinText.text = "Earned Coins: +" + coinsGainedThisLevel;
            winCoinText.fontSize = 50;
            winCoinText.color = Color.yellow;
            winCoinText.alignment = TextAlignmentOptions.Center;
            winCoinText.rectTransform.sizeDelta = new Vector2(600, 100); // Thêm size để không bị rớt dòng
            winCoinText.rectTransform.anchoredPosition = new Vector2(0, -50);
        }

        // Gửi điểm lên Firebase (nếu đã cấu hình), fallback về PlayerPrefs
        int earnedScore = currentScore;
        FirebaseLeaderboardService firebase = FirebaseLeaderboardService.instance;
        if (firebase != null && firebase.IsConfigured)
        {
            firebase.AddScoreToLocalPlayer(earnedScore, (ok, newTotal, seasonChanged) =>
            {
                if (ok)
                    Debug.Log(seasonChanged
                        ? "[Firebase] Mùa giải mới! Điểm reset, bắt đầu lại với " + newTotal
                        : "[Firebase] Đã cộng " + earnedScore + " điểm. Tổng: " + newTotal);
                else
                    Debug.LogWarning("[Firebase] Không push được điểm.");
            });
        }
        else
        {
            int tempTotalScore = PlayerPrefs.GetInt("TotalLeagueScore", 0);
            tempTotalScore += earnedScore;
            PlayerPrefs.SetInt("TotalLeagueScore", tempTotalScore);
            PlayerPrefs.Save();
            Debug.Log("Tổng điểm xếp hạng (local): " + tempTotalScore);
        }
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
            // KHÔNG dừng vật lý để mọi thứ diễn ra tự nhiên như Helix Jump thật
            // Time.timeScale = 0;

            if (levelWinPannal != null) {
                levelWinPannal.SetActive (true); 
            }

            // Chạm vào màn hình để qua màn mới (Ngoại trừ khu vực nút Home ở góc trên bên trái)
            if (Input.GetMouseButtonDown(0) && !_isProcessingLevel) {
                // Lấy tọa độ chuột
                Vector2 mousePos = Input.mousePosition;
                // Nếu không click vào vùng 20% góc trên bên trái (chỗ nút Home)
                bool isClickingHome = (mousePos.x < Screen.width * 0.2f && mousePos.y > Screen.height * 0.8f);
                if (!isClickingHome) {
                    NextLevelAction();
                }
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
