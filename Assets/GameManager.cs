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

    private void Update () {
        if(gameOver) {
            Time.timeScale = 0; 
            gameOverPannal.SetActive (true); 
            
            if(Input.GetMouseButtonDown(0)) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        if(levelWin) {
            levelWinPannal.SetActive (true); 

            if(Input.GetMouseButtonDown (0)) {
                LevelManager.instance.PassLevelAndLoadNext();
            }
        }
    }
}