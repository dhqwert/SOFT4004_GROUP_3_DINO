using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class RankTier
{
    public string rankName = "Tên hạng (VD: Bronze)";
    [Header("Điểm cần có để leo lên mốc này:")]
    public int scoreToReach; 
    [Header("Kéo thả thanh UI màu của hạng này vô để mũi tên nó đu theo:")]
    public RectTransform rankUIPosition; 
}

public class LeagueManager : MonoBehaviour
{
    [Header("Kéo thả tấm ảnh Mũi Tên Nhỏ xíu của bạn vào đây:")]
    public RectTransform arrowIndicator;

    [Header("Kéo chữ UI để hiển thị điểm trọn đời vào đây:")]
    public TMPro.TextMeshProUGUI currentScoreText;

    [Header("Khai báo các nấc thang Danh Vọng (Từ Thấp -> Cao)")]
    [Tooltip("Khuyên dùng: Điểm mốc đầu (Bronze) phải bằng 0.")]
    public RankTier[] rankTiers;

    private int myTotalLeagueScore;

    // Mỗi lần mở Panel lên là tự động tính toán lại
    void OnEnable()
    {
        // Dùng Coroutine chờ Layout tính xong vị trí mới di chuyển mũi tên
        StartCoroutine(RefreshAfterLayout());
    }

    // Chờ 2 frame + ép Canvas cập nhật => đảm bảo Layout Group đã xếp xong trên MỌI thiết bị
    IEnumerator RefreshAfterLayout()
    {
        yield return null; // Chờ hết frame 1
        yield return null; // Chờ hết frame 2 (Layout Group chắc chắn đã tính xong)
        Canvas.ForceUpdateCanvases(); // Ép toàn bộ Canvas vẽ lại cho chắc ăn
        RefreshLeague();
    }

    public void RefreshLeague()
    {
        // 1. Lọt hầm bí mật moi điểm xem được bao nhiêu rồi
        myTotalLeagueScore = PlayerPrefs.GetInt("TotalLeagueScore", 0);
        
        // Hiện số điểm lên bảng UI luôn cho người ta thấy
        if (currentScoreText != null) {
            currentScoreText.text = "CURRENT POINTS: " + myTotalLeagueScore;
        }

        Debug.Log("Đang xét hạng... Số điểm trọn đời cày được là: " + myTotalLeagueScore);

        int currentRankIndex = 0;

        // 2. Xét hạng từ dưới lên trên. Xem nó leo lọt đến bậc cao nhất là bao nhiêu
        for (int i = 0; i < rankTiers.Length; i++)
        {
            if (myTotalLeagueScore >= rankTiers[i].scoreToReach)
            {
                currentRankIndex = i; 
            }
        }

        // 3. Phóng mũi tên bay ngang vào biển tên đó
        if (arrowIndicator != null && rankTiers.Length > 0 && rankTiers[currentRankIndex].rankUIPosition != null)
        {
            // Lấy vị trí thế giới của thanh rank mục tiêu
            Vector3 targetPos = arrowIndicator.position;
            targetPos.y = rankTiers[currentRankIndex].rankUIPosition.position.y;
            arrowIndicator.position = targetPos;
            
            Debug.Log("Chúc mừng! Chức danh hiện tại: " + rankTiers[currentRankIndex].rankName 
                     + " | Arrow Y = " + arrowIndicator.position.y 
                     + " | Target Y = " + rankTiers[currentRankIndex].rankUIPosition.position.y);
        }
    }

    // Nút tắt dành riêng cho Dev (Nhấp chuột phải vào tên Script trên Inspector để xài)
    [ContextMenu("THẦN CHÚ: Reset Hạng Về Tương Đàn (Về 0)")]
    public void ResetLeagueScore()
    {
        PlayerPrefs.SetInt("TotalLeagueScore", 0);
        PlayerPrefs.Save();
        StartCoroutine(RefreshAfterLayout());
        Debug.Log("Đã cạo đầu đi tu, rớt hết điểm Rank!");
    }
}
