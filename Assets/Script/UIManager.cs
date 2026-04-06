using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Gắn 3 cái giao diện bự vào đây")]
    public GameObject mapPanel;
    public GameObject skinPanel;
    public GameObject leaguePanel;

    void Start()
    {
        // Khi vừa chạy game, luôn luôn ưu tiên mở Map đầu tiên
        OpenMapPanel();
    }

    // Gắn hàm này vào Nút "MAP" dưới đáy màn hình
    public void OpenMapPanel()
    {
        if (mapPanel != null) mapPanel.SetActive(true);
        if (skinPanel != null) skinPanel.SetActive(false);
        if (leaguePanel != null) leaguePanel.SetActive(false);
    }

    // Gắn hàm này vào Nút "SKINS"
    public void OpenSkinPanel()
    {
        if (mapPanel != null) mapPanel.SetActive(false);
        if (skinPanel != null) skinPanel.SetActive(true);
        if (leaguePanel != null) leaguePanel.SetActive(false);
    }

    // Gắn hàm này vào Nút "LEAGUE"
    public void OpenLeaguePanel()
    {
        if (mapPanel != null) mapPanel.SetActive(false);
        if (skinPanel != null) skinPanel.SetActive(false);
        if (leaguePanel != null) leaguePanel.SetActive(true);
    }
}
