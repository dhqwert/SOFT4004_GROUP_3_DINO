using UnityEngine;
using System.Collections.Generic;

// Tạo ra cái Mẫu dữ liệu Khai báo Skin để bạn thiết lập
[System.Serializable]
public class SkinData
{
    public Material skinMaterial; 
    [Header("Màu sắc của Mẫu bóng này hiển thị trên bảng UI")]
    public Color previewColor = Color.white; 
    public int price = 100;
    [Header("Loại Hàng: 0=Basic, 1=Rare, 2=Funky, 3=Epic")]
    public int tabCategory = 0;
}

public class SkinManager : MonoBehaviour
{
    [Header("Kéo Cục Material Bóng dùng chung (ballMat) vô đây")]
    public Material globalBallMaterial; 

    [Header("Kho Dữ liệu Mẫu Bóng (Nhập số lượng Skin vào đây)")]
    public SkinData[] database;
    
    [Header("Kéo Cục Prefab SkinNode và Khung chứa (Panel) vào đây")]
    public GameObject skinNodePrefab; 
    public Transform skinContainer;

    [Header("Kéo biến Text Vàng UI trên thanh menu vào")]
    public TMPro.TextMeshProUGUI menuCoinText; 

    // Khu vực chứa rác tạm thời các UI đc sinh ra
    private List<SkinNode> spawnedNodes = new List<SkinNode>();
    private int currentTab = 0; // Ghi nhớ xem đang ở ngăn tủ nào

    void Start()
    {
        // Vừa vào game, máy sẽ ép cái ballMat về đúng màu gốc đã lưu
        if (globalBallMaterial != null)
        {
            float r = PlayerPrefs.GetFloat("SkinColorR", 1f); 
            float g = PlayerPrefs.GetFloat("SkinColorG", 1f);
            float b = PlayerPrefs.GetFloat("SkinColorB", 1f);
            globalBallMaterial.color = new Color(r, g, b, 1f);
        }

        // Khi Shop bật lên thì dọn dẹp cặn bã cũ và tái tạo lại giao diện sạp hàng
        RefreshUI();
    }

    // Hàm Phủi mông sơn lại Giao diện Shop mỗi khi có đứa Mua hàng xong
    public void RefreshUI()
    {
        // Khi SettingManager gọi RefreshUI lúc SkinPanel còn chưa được kích hoạt
        // hoặc Inspector chưa kéo container/database, cần guard để không văng NRE.
        if (skinContainer == null || database == null || skinNodePrefab == null)
        {
            if (menuCoinText != null)
            {
                menuCoinText.text = PlayerPrefs.GetInt("TotalCoins", 0).ToString();
            }
            return;
        }

        foreach (Transform child in skinContainer) 
        {
            Destroy(child.gameObject);
        }
        spawnedNodes.Clear();

        // Lấy thông tin bóng đang mặc ra khỏi trí nhớ máy tính (Hoặc Mặc định là số 0 cho người mới)
        int currentSelected = PlayerPrefs.GetInt("SelectedSkin", 0);

        // Lấy thông tin VÀNG
        int myCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        if (menuCoinText != null) {
            menuCoinText.text = myCoins.ToString();
        }

        // --- CÔNG ĐOẠN ĐÚC LÒ HÀNG LOẠT Ô VUÔNG ---
        for (int i = 0; i < database.Length; i++)
        {
            // Nếu Món hàng này KHÔNG THUỘC Tủ hàng mình đang mở thì BỎ QUA không đúc khuôn
            if (database[i].tabCategory != currentTab) continue;

            // Bơm Cục ô vuông mới vào bảng (Grid Layout Group)
            GameObject newNode = Instantiate(skinNodePrefab, skinContainer);
            SkinNode nodeScript = newNode.GetComponent<SkinNode>();

            // Lấy não máy tính ktra xem Skin thứ [i] này đã tốn mua chưa? 
            // Cú pháp đặc biệt mình đặt là: SkinUnlocked_0, SkinUnlocked_1... Nếu não trả về == 1 là Đã Mua.
            bool isUnlocked = (PlayerPrefs.GetInt("SkinUnlocked_" + i, 0) == 1);
            
            // Ngoại lệ: Quả bóng số 0 (Bộ Mặc định Basic) luôn luôn là Đồ Cho Không nên isUnlocked = true
            if (i == 0) isUnlocked = true; 

            // Cột đèn xanh: Xem thử số [i] này có vô tình TRÙNG đúng bộ mình đang MẶC không?
            bool isSelected = (i == currentSelected);

            // Bơm sinh lực và tham số nạp vào con Node
            nodeScript.Setup(i, this, database[i].previewColor, database[i].price, isUnlocked, isSelected);
            spawnedNodes.Add(nodeScript);
        }
    }

    // Hàm cực kỳ cốt lõi mà con SkinNode sẽ hét lên gọi về báo cáo khi bị khách nhấp chuột
    public void UserClickedOnSkin(int index)
    {
        // Kích hoạt thủ tục xem đã Unlock chưa (Chưa cho tiền là chưa Unlock nghen)
        bool isUnlocked = (PlayerPrefs.GetInt("SkinUnlocked_" + index, 0) == 1);
        if (index == 0) isUnlocked = true; // Hàng chùa 0đ lọt khe

        if (isUnlocked)
        {
            // Lập tức trang bị bộ này lên người
            PlayerPrefs.SetInt("SelectedSkin", index);
            
            // XIN LUÔN MÃ MÀU RGB QUĂNG VÀO NÃO TRÍ NHỚ MÁY TÍNH
            PlayerPrefs.SetFloat("SkinColorR", database[index].previewColor.r);
            PlayerPrefs.SetFloat("SkinColorG", database[index].previewColor.g);
            PlayerPrefs.SetFloat("SkinColorB", database[index].previewColor.b);
            
            // Ép cái Material gốc ballMat nạp màu mới lập tức!
            if (globalBallMaterial != null) {
                globalBallMaterial.color = database[index].previewColor;
            }

            PlayerPrefs.Save();
            RefreshUI(); // Đập nát vẽ lại để cái Dấu tích xanh lục bay vào ô này
            Debug.Log("Thay đồ bộ Skin số: " + index + " OK!");
        }
        else
        {
            // >>> TRƯỜNG HỢP 2: BẤM VÀO Ổ KHÓA CỦA CON NHÀ NGƯỜI TA (Bị khóa)
            int myCoins = PlayerPrefs.GetInt("TotalCoins", 0);
            
            if (myCoins >= database[index].price)
            {
                // -- THANH TOÁN (Trừ tiền, khóa túi)
                myCoins -= database[index].price;
                PlayerPrefs.SetInt("TotalCoins", myCoins);                 
                // -- MUA THÀNH CÔNG, DÁN DẤU ĐÓNG MỘC PlayerPref!
                PlayerPrefs.SetInt("SkinUnlocked_" + index, 1);
                // -- TIỆN TAY THAY ĐỒ LUÔN CHO BAY LIỀN, SAO PHẢI ĐỢI
                PlayerPrefs.SetInt("SelectedSkin", index);
                
                // MUA XONG THÌ SAO CHÉP MÃ MÀU QUĂNG VÀO BỘ NHỚ LUN
                PlayerPrefs.SetFloat("SkinColorR", database[index].previewColor.r);
                PlayerPrefs.SetFloat("SkinColorG", database[index].previewColor.g);
                PlayerPrefs.SetFloat("SkinColorB", database[index].previewColor.b);
                
                // Tiện tay nhuộm màu luôn cục ballMat cho lóng lánh
                if (globalBallMaterial != null) {
                    globalBallMaterial.color = database[index].previewColor;
                }

                PlayerPrefs.Save();
                RefreshUI();
                Debug.Log("Giao dịch ngân hàng hoàn tất. Cám ơn Quý Khách!");
            }
            else
            {
                // THIẾU TIỀN: Ở đây bạn làm thêm hàm Nháy chữ hoặc Rung Rung UI đập vô mặt nha!
                Debug.Log("Nghèo thì đứng dòm! Thiếu tiền gòi bro (Cần " + database[index].price + " vàng). Tài khoản bạn còn: " + myCoins);
            }
        }
    }

    // Hàm gắn vào các Nút Bấm Chuyển Tab (BASIC, RARE, EPIC...)
    public void ClickTab(int tabIndex)
    {
        currentTab = tabIndex;
        RefreshUI(); // Đập toàn bộ tủ đi xếp lại theo ngăn mới
    }
}
