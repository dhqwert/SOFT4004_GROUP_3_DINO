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

        int unlockedLevel = Mathf.Clamp(PlayerPrefs.GetInt("CurrentLevel", 1), 1, 100);
        StartCoroutine(ScrollToTopAfterLayout());

        for (int i = unlockedLevel; i >= 1; i--)
        {
            GameObject newNode = Instantiate(nodePrefab, container);

            TextMeshProUGUI levelText = newNode.GetComponentInChildren<TextMeshProUGUI>();
            Image nodeImage = newNode.GetComponent<Image>();
            Button nodeButton = newNode.GetComponent<Button>();
            Transform glowEffect = newNode.transform.Find("Glow");
            Transform lineObj = newNode.transform.Find("Line");

            if (levelText != null) levelText.text = i.ToString();
            if (lineObj != null)
            {
                lineObj.gameObject.SetActive(i != 1);
                Image lineImg = lineObj.GetComponent<Image>();
                if (lineImg != null) lineImg.color = Color.cyan;
            }

            if (i == unlockedLevel)
            {
                if (nodeImage != null) nodeImage.color = new Color(0.3f, 0.85f, 0.4f);
                if (glowEffect != null) glowEffect.gameObject.SetActive(true);
            }
            else
            {
                if (nodeImage != null) nodeImage.color = new Color(0.25f, 0.7f, 0.85f);
                if (glowEffect != null) glowEffect.gameObject.SetActive(false);
            }

            if (nodeButton != null)
            {
                int levelToLoad = i;
                nodeButton.onClick.AddListener(() =>
                {
                    PlayerPrefs.SetInt("PlayingLevel", levelToLoad);
                    PlayerPrefs.Save();
                    SceneManager.LoadScene("GamePlay");
                });
            }
        }
    }

    System.Collections.IEnumerator ScrollToTopAfterLayout()
    {
        yield return new WaitForEndOfFrame();
        yield return null;
        ScrollRect sr = container.GetComponentInParent<ScrollRect>();
        if (sr != null)
        {
            Canvas.ForceUpdateCanvases();
            sr.verticalNormalizedPosition = 1f; // 1 = top = level cao nhất (tạo đầu tiên)
        }
    }

    public void PlayCurrentLevel()
    {
        int unlockedLevel = Mathf.Clamp(PlayerPrefs.GetInt("CurrentLevel", 1), 1, 100);
        PlayerPrefs.SetInt("PlayingLevel", unlockedLevel);
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
