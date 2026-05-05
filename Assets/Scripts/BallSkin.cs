using UnityEngine;

public class BallSkin : MonoBehaviour
{
    void Start()
    {
        MeshRenderer myRenderer = GetComponent<MeshRenderer>();
        if (myRenderer == null) return;

        int selectedSkin = PlayerPrefs.GetInt("SelectedSkin", 0);

        // Ưu tiên dùng SharedDatabase (đã qua Home scene)
        SkinData[] db = SkinManager.SharedDatabase;
        if (db != null && selectedSkin >= 0 && selectedSkin < db.Length)
        {
            ApplySkin(myRenderer, db[selectedSkin]);
            return;
        }

        // Fallback: tìm SkinManager trong scene hiện tại
        SkinManager mgr = Object.FindFirstObjectByType<SkinManager>();
        if (mgr != null && mgr.database != null && selectedSkin < mgr.database.Length)
        {
            SkinManager.SharedDatabase = mgr.database;
            ApplySkin(myRenderer, mgr.database[selectedSkin]);
            return;
        }

        // Fallback cuối: chỉ apply màu từ PlayerPrefs
        float r = PlayerPrefs.GetFloat("SkinColorR", 1f);
        float g = PlayerPrefs.GetFloat("SkinColorG", 1f);
        float b = PlayerPrefs.GetFloat("SkinColorB", 1f);
        myRenderer.material.color = new Color(r, g, b, 1f);
    }

    void ApplySkin(MeshRenderer r, SkinData data)
    {
        Material mat = r.material;
        if (data.skinTexture != null)
        {
            // Có texture → color phải trắng để texture hiện đúng màu gốc
            mat.color = Color.white;
            mat.mainTexture = data.skinTexture;
            mat.mainTextureScale = data.tiling;
            mat.mainTextureOffset = data.offset;
        }
        else
        {
            // Không có texture → dùng màu solid
            mat.color = data.previewColor;
            mat.mainTexture = null;
        }
    }
}
