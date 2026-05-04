using UnityEngine;

public class BallSkin : MonoBehaviour
{
    void Start()
    {
        // Moi con MeshRenderer của lưới bóng 3D ra
        MeshRenderer myRenderer = GetComponent<MeshRenderer>();
        if (myRenderer != null)
        {
            int selectedSkin = PlayerPrefs.GetInt("SelectedSkin", 0);
            
            // Nếu có dữ liệu từ Shop (chạy từ scene Home)
            if (SkinManager.SharedDatabase != null && selectedSkin >= 0 && selectedSkin < SkinManager.SharedDatabase.Length)
            {
                SkinData data = SkinManager.SharedDatabase[selectedSkin];
                
                // Set màu
                myRenderer.material.color = data.previewColor;
                
                // Set texture nếu có
                if (data.skinTexture != null)
                {
                    myRenderer.material.mainTexture = data.skinTexture;
                    myRenderer.material.mainTextureScale = data.tiling;
                    myRenderer.material.mainTextureOffset = data.offset;
                }
            }
            else
            {
                // Fallback nếu chạy trực tiếp từ scene chơi game (Test mode)
                float r = PlayerPrefs.GetFloat("SkinColorR", 1f); 
                float g = PlayerPrefs.GetFloat("SkinColorG", 1f);
                float b = PlayerPrefs.GetFloat("SkinColorB", 1f);
                
                // Sơn vội lớp sơn mới đè lên cái cũ
                myRenderer.material.color = new Color(r, g, b, 1f);
                Debug.LogWarning("Đang chạy trực tiếp từ màn chơi, sẽ không load được Texture. Vui lòng chạy từ Scene Home để thấy Texture.");
            }
        }
    }
}
