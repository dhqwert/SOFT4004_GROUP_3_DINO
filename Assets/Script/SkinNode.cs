using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkinNode : MonoBehaviour
{
    [Header("Kéo các thành phần bên trong cái Cục Prefab Skin vào đây:")]
    public Image skinPreviewImage; // Chỗ hiển thị màu sắc quả bóng
    public GameObject lockedOverlay; // Trọn bộ cái Giao diện Ổ khóa (gồm nền mờ + icon khoá)
    public GameObject selectedCheckmark; // Dấu tích xanh lục khi đang mặc bộ này
    public TextMeshProUGUI priceText; // Hiển thị giá tiền kế bên ổ khóa
    
    private int mySkinIndex; // Lưu số thứ tự của ô này
    private SkinManager manager; // Gọi sếp tổng SkinManager để báo cáo giao dịch

    // Hàm này sẽ được SkinManager gọi tự động để nhồi thông tin vào giao diện
    public void Setup(int index, SkinManager managerRef, Color previewColor, int price, bool isUnlocked, bool isSelected)
    {
        mySkinIndex = index;
        manager = managerRef;
        
        // Tô màu cục Demo thành đúng màu của Skin
        if (skinPreviewImage != null) skinPreviewImage.color = previewColor;
        
        if (isUnlocked)
        {
            // Nếu đã mua rồi thì giấu toàn bộ ổ khóa và giá tiền đi
            if (lockedOverlay != null) lockedOverlay.SetActive(false);
            if (priceText != null) priceText.gameObject.SetActive(false);
        }
        else
        {
            // Nếu chưa mua thì hiện màng nhện ổ khóa lên, show giá tiền để thu hút
            if (lockedOverlay != null) lockedOverlay.SetActive(true);
            if (priceText != null) 
            {
                priceText.gameObject.SetActive(true);
                priceText.text = price.ToString(); // In biến price (ví dụ 100) ra thành chữ
            }
        }

        // Bật tắt dấu tích dựa vào việc có đang chưng diện không
        if (selectedCheckmark != null) selectedCheckmark.SetActive(isSelected);

        // Kích hoạt sự kiện để khi người dùng BẤM VÀO Ô VUÔNG NÀY thì tự kích hoạt hàm OnNodeClicked
        Button myButton = GetComponent<Button>();
        myButton.onClick.RemoveAllListeners();
        myButton.onClick.AddListener(OnNodeClicked);
    }

    void OnNodeClicked()
    {
        // Khi bị bấm, nó réo tên sếp lên và báo "CÓ NGƯỜI BẤM VÀO SỐ THỨ TỰ CỦA EM!"
        manager.UserClickedOnSkin(mySkinIndex);
    }
}
