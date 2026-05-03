using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelMapManager : MonoBehaviour
{
    [Header("Kéo thả từ bên ngoài vào:")]
    public GameObject nodePrefab;
    public Transform container;
    public TextMeshProUGUI coinText;

    void Start()
    {
        if (coinText != null)
            coinText.text = PlayerPrefs.GetInt("TotalCoins", 0).ToString();

        // Lấy đúng level cao nhất đã mở, không giới hạn
        int unlockedLevel = Mathf.Max(1, PlayerPrefs.GetInt("CurrentLevel", 1));

        // Chỉ sinh đúng số node = số level đã mở (không bao giờ lag)
        for (int i = unlockedLevel; i >= 1; i--)
        {
            GameObject newNode = Instantiate(nodePrefab, container);

            TextMeshProUGUI levelText = newNode.GetComponentInChildren<TextMeshProUGUI>();
            Image nodeImage = newNode.GetComponent<Image>();
            Button nodeButton = newNode.GetComponent<Button>();
            Transform glowEffect = newNode.transform.Find("Glow");

            levelText.text = i.ToString();

            if (i == unlockedLevel)
            {
                // Level hiện tại: xanh lá + glow
                nodeImage.color = Color.green;
                if (glowEffect != null) glowEffect.gameObject.SetActive(true);
                nodeButton.interactable = true;
            }
            else
            {
                // Đã vượt qua: xanh cyan, cho phép chơi lại
                nodeImage.color = Color.cyan;
                if (glowEffect != null) glowEffect.gameObject.SetActive(false);
                nodeButton.interactable = true;
            }

            // Đường nối
            Transform lineObj = newNode.transform.Find("Line");
            if (lineObj != null)
            {
                lineObj.GetComponent<Image>().color = Color.cyan;
                if (i == 1) lineObj.gameObject.SetActive(false);
            }

            // Bấm node → lưu level → vào Level1
            int levelToLoad = i;
            nodeButton.onClick.AddListener(() =>
            {
                PlayerPrefs.SetInt("CurrentLevel", levelToLoad);
                PlayerPrefs.Save();
                SceneManager.LoadScene("Level1");
            });
        }
    }

    // Nút PLAY to bự — vào thẳng level mới nhất
    public void PlayCurrentLevel()
    {
        int unlockedLevel = Mathf.Max(1, PlayerPrefs.GetInt("CurrentLevel", 1));
        PlayerPrefs.SetInt("CurrentLevel", unlockedLevel);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Level1");
    }

    // Nút Reset — xóa tiến độ chơi lại từ đầu
    public void ResetGameProgress()
    {
        PlayerPrefs.DeleteKey("CurrentLevel");
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // [DEV] Chuột phải vào component → mở khóa nhanh để test
    [ContextMenu("MỞ KHÓA 30 LEVEL (Dev Test)")]
    public void UnlockAllLevels()
    {
        PlayerPrefs.SetInt("CurrentLevel", 30);
        PlayerPrefs.Save();
        Debug.Log("DEV: Đã mở 30 level để test!");
        if (Application.isPlaying)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}