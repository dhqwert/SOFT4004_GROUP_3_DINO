using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingManager : MonoBehaviour
{
    static SkinManager FindSkinManager()
    {
        SkinManager[] managers = Object.FindObjectsByType<SkinManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        return managers != null && managers.Length > 0 ? managers[0] : null;
    }

    static LeagueManager FindLeagueManager()
    {
        LeagueManager[] managers = Object.FindObjectsByType<LeagueManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        return managers != null && managers.Length > 0 ? managers[0] : null;
    }

    // Bấm Nút này để cháy túi
    public void ResetCoin()
    {
        PlayerPrefs.SetInt("TotalCoins", 0);
        PlayerPrefs.Save();
        Debug.Log(">> Đã Reset sạch số Vàng (Coin)!");
        
        FindSkinManager()?.RefreshUI();
    }

    // Bấm Nút này để rớt đài Xếp hạng
    public void ResetPoint()
    {
        PlayerPrefs.SetInt("TotalLeagueScore", 0);
        PlayerPrefs.Save();
        Debug.Log(">> Đã Reset sạch Điểm Xếp Hạng (League Score)!");
        
        FindLeagueManager()?.RefreshLeague();
    }

    // Bấm Nút này để trả game về Màn 1
    public void ResetLevel()
    {
        PlayerPrefs.SetInt("CurrentLevel", 1);
        PlayerPrefs.Save();
        Debug.Log(">> Đã đẩy Tiến độ về đầu (Màn 1)!");
        
        // Ép Game tải lại chính giữa màn hình để Map văng sạch cục mốc level
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Bấm Nút này để lột sạch Đồ đã mua (Skin)
    public void ResetSkins()
    {
        // Quét ổ cứng khóa chặt lại 100 bộ Skin
        for (int i = 0; i < 100; i++) {
            PlayerPrefs.DeleteKey("SkinUnlocked_" + i);
        }
        
        PlayerPrefs.DeleteKey("SelectedSkin");
        PlayerPrefs.DeleteKey("SkinColorR");
        PlayerPrefs.DeleteKey("SkinColorG");
        PlayerPrefs.DeleteKey("SkinColorB");
        PlayerPrefs.Save();
        
        SkinManager skinMgr = FindSkinManager();
        skinMgr?.RefreshUI();
        
        Material ballMat = skinMgr?.globalBallMaterial;
        if(ballMat != null) ballMat.color = Color.white; 

        Debug.Log(">> Đã lột sạch Skin, trả về trái bóng mặc định!");
    }

    // MUA GÓI KHÔNG QUẢNG CÁO (Dời logic qua đây để sửa lỗi nút hỏng)
    public void PurchaseNoAds()
    {
        PlayerPrefs.SetInt("RemoveAds", 1);
        PlayerPrefs.Save();
        Debug.Log(">> Đã giao dịch: Nâng cấp tài khoản VIP Không Quảng Cáo hoàn tất!");
    }

    // Bấm Nút này để Bật lại Quảng Cáo rớt vào mặt (Trường hợp lỡ bấm mua Không QC)
    public void ResetQC()
    {
        PlayerPrefs.DeleteKey("RemoveAds");
        PlayerPrefs.DeleteKey("NoAds"); // Từ khóa dự phòng
        PlayerPrefs.Save();
        Debug.Log(">> Đã Reset Gói Quảng Cáo. Giờ QC sẽ hiện lại như cũ!");
    }

    // NÚT CLEAR TẤT CẢ (Giống như cài lại Game mới toanh)
    public void NukeEverything()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log(">> ĐÃ XÓA TRẮNG TOÀN BỘ GAME NHƯ LÚC VỪA TẢI VỀ!");
        
        // Quét sạch xong phải vả cho nó load lại game mới thấy kết quả
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
