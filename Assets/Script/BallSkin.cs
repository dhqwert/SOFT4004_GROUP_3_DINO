using UnityEngine;

public class BallSkin : MonoBehaviour
{
    void Start()
    {
        // Moi con MeshRenderer của lưới bóng 3D ra
        MeshRenderer myRenderer = GetComponent<MeshRenderer>();
        if (myRenderer != null)
        {
            // Xin cấp phép soi não bộ nhớ máy tính để lấy 3 mã màu R, G, B
            // 1f là giá trị mặc định (Màu Trắng) nếu người chơi mới tải game chưa từng ghé shop
            float r = PlayerPrefs.GetFloat("SkinColorR", 1f); 
            float g = PlayerPrefs.GetFloat("SkinColorG", 1f);
            float b = PlayerPrefs.GetFloat("SkinColorB", 1f);
            
            // Sơn vội lớp sơn mới đè lên cái cũ
            myRenderer.material.color = new Color(r, g, b, 1f);
        }
    }
}
