using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Gắn 3 cái giao diện bự vào đây")]
    public GameObject mapPanel;
    public GameObject skinPanel;
    public GameObject leaguePanel;

    [Header("Panel leaderboard mới (nếu dùng)")]
    public GameObject leaderboardPanel;

    void Awake()
    {
        AutoBindIfMissing();
    }

    void Start()
    {
        AutoBindIfMissing();

        if (leaderboardPanel != null && leaguePanel != null)
        {
            leaguePanel.SetActive(false);
        }
        OpenMapPanel();
    }

    void AutoBindIfMissing()
    {
        if (leaderboardPanel == null)
        {
            LeaderboardPanel[] all = Object.FindObjectsByType<LeaderboardPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (all != null && all.Length > 0)
            {
                leaderboardPanel = all[0].gameObject;
            }
        }

        if (leaguePanel == null)
        {
            leaguePanel = FindByName("LeaguePanel");
        }
        if (mapPanel == null)
        {
            mapPanel = FindByName("MapPanel");
        }
        if (skinPanel == null)
        {
            skinPanel = FindByName("SkinsPanel");
            if (skinPanel == null)
            {
                skinPanel = FindByName("SkinPanel");
            }
        }
    }

    GameObject FindByName(string name)
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        Transform searchRoot = canvas != null ? canvas.transform : null;
        if (searchRoot == null)
        {
            Canvas[] allCanvas = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (allCanvas != null && allCanvas.Length > 0)
            {
                searchRoot = allCanvas[0].transform;
            }
        }
        if (searchRoot == null)
        {
            return null;
        }
        Transform found = FindDescendant(searchRoot, name);
        return found != null ? found.gameObject : null;
    }

    static Transform FindDescendant(Transform root, string name)
    {
        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.name == name)
            {
                return child;
            }
            Transform deeper = FindDescendant(child, name);
            if (deeper != null)
            {
                return deeper;
            }
        }
        return null;
    }

    public void OpenMapPanel()
    {
        AutoBindIfMissing();
        EnsureHomeContainersActive();

        if (mapPanel != null) mapPanel.SetActive(true);
        if (skinPanel != null) skinPanel.SetActive(false);
        if (leaguePanel != null) leaguePanel.SetActive(false);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
    }

    void EnsureHomeContainersActive()
    {
        if (mapPanel != null && mapPanel.transform.parent != null)
        {
            GameObject parent = mapPanel.transform.parent.gameObject;
            if (!parent.activeSelf) parent.SetActive(true);
        }
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            Transform bottom = FindDescendant(canvas.transform, "BottomMenu");
            if (bottom != null && !bottom.gameObject.activeSelf) bottom.gameObject.SetActive(true);
        }
    }

    public void OpenSkinPanel()
    {
        AutoBindIfMissing();

        if (mapPanel != null) mapPanel.SetActive(false);
        if (skinPanel != null) skinPanel.SetActive(true);
        if (leaguePanel != null) leaguePanel.SetActive(false);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
    }

    public void OpenLeaguePanel()
    {
        AutoBindIfMissing();

        if (mapPanel != null) mapPanel.SetActive(false);
        if (skinPanel != null) skinPanel.SetActive(false);

        if (leaderboardPanel != null)
        {
            if (leaguePanel != null) leaguePanel.SetActive(false);
            leaderboardPanel.transform.SetAsLastSibling();
            leaderboardPanel.SetActive(true);
        }
        else if (leaguePanel != null)
        {
            leaguePanel.SetActive(true);
        }
    }
}
