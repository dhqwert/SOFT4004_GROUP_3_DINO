using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdManager : MonoBehaviour
{
    public static AdManager instance;

    [Header("Mã Quảng Cáo (Đang dùng mã Test Google)")]
    public string interstitialId = "ca-app-pub-3940256099942544/1033173712"; // Quảng cáo khi qua màn
    public string rewardedId = "ca-app-pub-3940256099942544/5224354917";     // Quay video nhận thưởng

    [Header("Cài đặt Mức độ xuất hiện (Tùy chỉnh)")]
    public int minLevelToShowAds = 3; // Từ level 3 mới bắt đầu bị hiện quảng cáo
    public int interstitialFrequency = 2; // Cứ qua 2 màn thì hiện 1 lần quảng cáo

    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;

    private int levelWinCount = 0; // Đếm số lần để xem bao giờ đến lượt hiện QC

    private Action onRewardedCallback;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Cái này giúp AdManager sống bất tử qua mọi màn
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Quan trọng: Bắt buộc gọi event QC trên luồng chính của Unity để tránh lỗi văng game
        MobileAds.RaiseAdEventsOnUnityMainThread = true;

        // Báo cho Google khởi động hệ thống QC
        MobileAds.Initialize(initStatus => {
            // Sau khi khởi động xong, bắt đầu âm thầm tải Video QC ở chế độ chạy ngầm
            LoadInterstitialAd();
            LoadRewardedAd();
        });
    }

    #region 1. CHUYÊN MỤC QUẢNG CÁO CHUYỂN MÀN (INTERSTITIAL AD)

    [Header("Quyền lợi của Gói Gỡ QC (VIP)")]
    public bool blockInterstitialAds = true; // Mua VIP xong sẽ chặn QC chuyển màn
    public bool blockRewardedAds = false;    // Mua VIP xong cho hồi sinh/nhận đồ miễn phí KHÔNG CẦN CHỜ XEM VIDEO
    
    // Hàm gắn vào Nút "No Ads", bấm cái Mãi Mãi Không Gặp QC Chuyển Màn Nữa
    public void PurchaseRemoveAds()
    {
        // Ghi vào não bộ máy tính: Chủ nhân đã nạp vip!
        PlayerPrefs.SetInt("RemoveAds", 1);
        PlayerPrefs.Save();
        Debug.Log("Giao dịch mua VIP Xoá quảng cáo thành công!");
        
        // Bạn có thể Ẩn cái nút NoAds đi (tùy bạn code ở UI sau)
    }

    // Hàm kiểm tra xem đã nạp tiền gỡ QC chưa
    public bool IsAdsRemoved()
    {
        return PlayerPrefs.GetInt("RemoveAds", 0) == 1; // == 1 là đã gỡ, == 0 là chưa gỡ
    }

    // Hàm DÀNH RIÊNG CHO DEV CỨU HỘ TEST GAME (Nút Reset QC)
    public void ResetAdsPurchase()
    {
        PlayerPrefs.DeleteKey("RemoveAds");
        PlayerPrefs.Save();
        Debug.Log("Khôi phục nhân phẩm Tài khoản thường: Toàn bộ quảng cáo đã bị gọi về!");
    }

    public void LoadInterstitialAd()
    {
        // Xóa đồ cũ đi trước khi chứa đồ mới
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        var adRequest = new AdRequest();
        InterstitialAd.Load(interstitialId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Máy chưa tải kịp quảng cáo, bỏ qua...");
                    return;
                }
                interstitialAd = ad;
            });
    }

    // Hàm do GameManager gọi khi đập thủng tầng cuối cùng
    public void ShowInterstitialAdIfReady(int currentLevel, Action onAdFinished)
    {
        // Đặc quyền VIP: Đã mua gói gỡ quảng cáo VÀ Cấu hình cho phép chặn QC lướt màn
        if (IsAdsRemoved() && blockInterstitialAds)
        {
            onAdFinished?.Invoke(); 
            return;
        }

        // Thỏa hiệp 1: Đã tới level muốn chèn QC chưa?
        if (currentLevel < minLevelToShowAds)
        {
            onAdFinished?.Invoke(); // Nhảy màn game luôn
            return;
        }

        levelWinCount++; // Qua thêm 1 màn

        // Thỏa hiệp 2: Đã tải xong video VÀ đếm đến định mức (ví dụ cách 2 ván hiện 1 lần)
        if (interstitialAd != null && interstitialAd.CanShowAd() && levelWinCount >= interstitialFrequency)
        {
            levelWinCount = 0; // Trả đồng hồ đếm về số 0

            // Bắt sự kiện khi người dùng BẤM DẤU [X] TẮT QUẢNG CÁO
            interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                LoadInterstitialAd(); // Chuẩn bị cuốn chiếu tải săn QC mới
                onAdFinished?.Invoke(); // Tiếp tục ném ng chơi sang Scene tiếp theo
            };

            // Hoặc lỡ lỗi sập QC
            interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                LoadInterstitialAd();
                onAdFinished?.Invoke();
            };

            // HÚ KO QUẢNG CÁO BAY VÀO MẶTTTT
            interstitialAd.Show();
        }
        else
        {
            onAdFinished?.Invoke(); // Trình độ hụt thì qua màn tự nhiên
        }
    }
    private bool isRewardEarnedTemporary = false; // Biến nhớ cờ: Lát nữa tắt ads là phải hồi sinh

    #endregion

    #region 2. CHUYÊN MỤC XEM VIDEO NHẬN QUÀ (REWARDED AD)
    public void LoadRewardedAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        var adRequest = new AdRequest();
        RewardedAd.Load(rewardedId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Chưa tải được Video thưởng");
                    return;
                }
                rewardedAd = ad;
            });
    }

    public void ShowRewardedAd(Action onRewardEarned)
    {
        // 1. Đặc quyền VIP: Chạm vào hồi sinh luôn khỏi mất công nạp xem video
        if (IsAdsRemoved() && blockRewardedAds)
        {
            Debug.Log("Tài khoản VIP được Hồi sinh vượt rào Video miễn phí!");
            onRewardEarned?.Invoke();
            return;
        }

        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            onRewardedCallback = onRewardEarned;
            isRewardEarnedTemporary = false; // Ban đầu là chưa có quà

            // Sự kiện khi bấm DẤU X ĐÓNG Video
            rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                LoadRewardedAd(); // Lên đạn sẵn cho ván sau
                
                // NẾU LÚC NÃY XEM ĐỦ GIỜ RỒI THÌ BÂY GIỜ LÚC ĐÓNG ADS MỚI LÀ LÚC THỰC THI HỒI SINH
                if (isRewardEarnedTemporary) 
                {
                    onRewardedCallback?.Invoke();
                    isRewardEarnedTemporary = false; // Làm sạch trí nhớ
                }
            };
            
            rewardedAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                LoadRewardedAd();
            };

            // Sự kiện NGẦM khi người chơi XEM ĐỦ SỐ GIÂY (Chưa tắt Ads đâu nhé)
            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log($"Ngon, người chơi đã xem đủ số phút quy định!");
                isRewardEarnedTemporary = true; // Chỉ đánh dấu cờ nhớ là "Đáng được cứu"
            });
        }
        else
        {
            Debug.Log("Video đang tải chưa xong, mạng lag nên bấm xem không nhúc nhích.");
        }
    }
    #endregion
}
