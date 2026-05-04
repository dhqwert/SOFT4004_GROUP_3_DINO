using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneController : MonoBehaviour
{
    public static GameSceneController instance;

    [Header("Kéo HelixManager vào đây")]
    public HelixManager helixManager;

    int currentLevel;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
    }

    public void OnLevelComplete()
    {
        currentLevel++; // Tăng mãi, không giới hạn
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.Save();

        GameManager.gameOver = false;
        GameManager.levelWin = false;
        Time.timeScale = 1f;

        if (GameManager.instance != null)
        {
            if (GameManager.instance.levelWinPannal != null)
                GameManager.instance.levelWinPannal.SetActive(false);
            if (GameManager.instance.gameOverPannal != null)
                GameManager.instance.gameOverPannal.SetActive(false);
        }

        helixManager.GenerateForLevel(currentLevel);
        ResetBall();

        LevelProgressUI ui = FindObjectOfType<LevelProgressUI>();
        if (ui != null) ui.RefreshUI(currentLevel);
    }

    public void GoHome()
    {
        GameManager.gameOver = false;
        GameManager.levelWin = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("Home");
    }

    void ResetBall()
    {
        GameObject ball = GameObject.FindGameObjectWithTag("Player");
        if (ball == null) return;

        // Đặt bóng lên đỉnh tháp mới
        ball.transform.position = new Vector3(0f, 2f, 0f);

        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}