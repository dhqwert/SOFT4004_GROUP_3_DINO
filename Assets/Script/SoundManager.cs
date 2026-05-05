using UnityEngine;
using UnityEngine.UI; // Thêm thư viện để thao tác với UI

public class SoundManager : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Kéo object chứa giao diện nút ON vào đây")]
    [SerializeField] private GameObject onStateObject;
    
    [Tooltip("Kéo object chứa giao diện nút OFF vào đây")]
    [SerializeField] private GameObject offStateObject;

    private bool isMuted = false;

    void Start()
    {
        // Kiểm tra xem trước đó người chơi có đang tắt tiếng không (lưu bằng PlayerPrefs)
        isMuted = PlayerPrefs.GetInt("isMuted", 0) == 1;
        
        UpdateUI();
        ApplySoundState();
    }

    // Gắn hàm này vào sự kiện OnClick() của UI Button
    public void ToggleSound()
    {
        isMuted = !isMuted;
        
        // Lưu lại trạng thái cài đặt âm thanh
        PlayerPrefs.SetInt("isMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();

        UpdateUI();
        ApplySoundState();
    }

    private void UpdateUI()
    {
        // Bật/tắt các Object đại diện cho nút bấm
        if (onStateObject != null) onStateObject.SetActive(!isMuted);
        if (offStateObject != null) offStateObject.SetActive(isMuted);
    }

    private void ApplySoundState()
    {
        // AudioListener.volume điều chỉnh âm lượng tổng của toàn bộ game (0 = tắt, 1 = bật)
        AudioListener.volume = isMuted ? 0f : 1f;
    }
}
