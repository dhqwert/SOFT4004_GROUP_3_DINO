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

        int unlockedLevel = Mathf.Max(1, PlayerPrefs.GetInt("CurrentLevel", 1));

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
                nodeImage.color = Color.green;
                if (glowEffect != null) glowEffect.gameObject.SetActive(true);
                nodeButton.interactable = true;
            }
            else
            {
                nodeImage.color = Color.cyan;
                if (glowEffect != null) glowEffect.gameObject.SetActive(false);
                nodeButton.interactable = true;
            }

            Transform lineObj = newNode.transform.Find("Line");
            if (lineObj != null)
            {
                lineObj.GetComponent<Image>().color = Color.cyan;
                if (i == 1) lineObj.gameObject.SetActive(false);
            }

            int levelToLoad = i;
            nodeButton.onClick.AddListener(() =>
            {
                PlayerPrefs.SetInt("CurrentLevel", levelToLoad);
                PlayerPrefs.Save();
                SceneManager.LoadScene("GamePlay");
            });
        }
    }

    public void PlayCurrentLevel()
    {
        int unlockedLevel = Mathf.Max(1, PlayerPrefs.GetInt("CurrentLevel", 1));
        PlayerPrefs.SetInt("CurrentLevel", unlockedLevel);
        PlayerPrefs.Save();
        SceneManager.LoadScene("GamePlay");
    }

    public void ResetGameProgress()
    {
        PlayerPrefs.DeleteKey("CurrentLevel");
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

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