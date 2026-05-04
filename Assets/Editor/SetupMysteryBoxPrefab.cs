using UnityEngine;
using UnityEditor;

public class SetupMysteryBoxPrefab
{
    [MenuItem("Tools/3. Tự Động Cài Đặt Hộp Bí Ẩn (Xuyên Phá)")]
    public static void Setup()
    {
        string spritePath = "Assets/Sprites/mysterybox.png";
        
        // Cố gắng đổi Texture thành Sprite nếu người dùng chưa đổi
        TextureImporter ti = AssetImporter.GetAtPath(spritePath) as TextureImporter;
        if (ti != null && ti.textureType != TextureImporterType.Sprite)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.SaveAndReimport();
        }

        Sprite boxSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (boxSprite == null)
        {
            Debug.LogError("CẢNH BÁO: Không tìm thấy ảnh tại đường dẫn " + spritePath + ". Bạn hãy kiểm tra lại xem ảnh đã nằm đúng thư mục chưa nhé!");
            return;
        }

        // Tạo Object mẫu
        GameObject go = new GameObject("MysteryBox");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = boxSprite;

        // Dựng đứng hình ảnh lên (0, 0, 0) thay vì nằm ngang
        go.transform.rotation = Quaternion.Euler(0, 0, 0);
        // Chỉnh kích thước nhỏ lại rất nhiều vì ảnh Mystery Box có độ phân giải lớn
        go.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);

        SphereCollider col = go.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 10.0f; // Thu nhỏ vùng chạm để tránh ăn rùa

        ItemPickup pickup = go.AddComponent<ItemPickup>();
        // Gán đúng chức năng là xuyên phá
        pickup.itemType = ItemType.Piercing;
        pickup.rotationSpeed = 150f; // Xoay vòng quanh trục
        
        // Tạo thư mục Prefabs nếu chưa có
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        string prefabPath = "Assets/Prefabs/MysteryBoxPrefab.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        GameObject.DestroyImmediate(go); // Xóa cục mồi trên Scene

        // Tự động gán vào Helix Manager (vào ô Pierce Prefab)
        HelixManager helixManager = GameObject.FindObjectOfType<HelixManager>();
        if (helixManager != null)
        {
            Undo.RecordObject(helixManager, "Assign Pierce Prefab");
            helixManager.piercePrefab = prefab;
            PrefabUtility.RecordPrefabInstancePropertyModifications(helixManager);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(helixManager.gameObject.scene);
            Debug.Log("THÀNH CÔNG! Đã tạo Prefab Hộp Bí Ẩn và gắn tự động chức năng Xuyên Phá vào HelixManager.");
        }
        else
        {
            Debug.LogWarning("Đã tạo xong Prefab tại Assets/Prefabs/MysteryBoxPrefab.prefab nhưng không tìm thấy HelixManager trên màn hình để gắn tự động. Bạn hãy tự kéo thả nhé.");
        }
    }
}
