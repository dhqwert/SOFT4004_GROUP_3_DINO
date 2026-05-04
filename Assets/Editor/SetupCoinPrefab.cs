using UnityEngine;
using UnityEditor;

public class SetupCoinPrefab
{
    [MenuItem("Tools/1. Tự Động Cài Đặt Đồng Dollar")]
    public static void Setup()
    {
        string spritePath = "Assets/Sprites/dollar.png";
        
        // Cố gắng đổi Texture thành Sprite nếu người dùng chưa đổi
        TextureImporter ti = AssetImporter.GetAtPath(spritePath) as TextureImporter;
        if (ti != null && ti.textureType != TextureImporterType.Sprite)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.SaveAndReimport();
        }

        Sprite dollarSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (dollarSprite == null)
        {
            Debug.LogError("CẢNH BÁO: Không tìm thấy ảnh tại đường dẫn " + spritePath + ". Bạn hãy kiểm tra lại xem ảnh đã nằm đúng thư mục chưa nhé!");
            return;
        }

        // Tạo Object mẫu
        GameObject go = new GameObject("DollarCoin");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = dollarSprite;

        // Xoay ảnh ngửa lên để phù hợp với góc nhìn 3D từ trên xuống của Helix Jump
        go.transform.rotation = Quaternion.Euler(90, 0, 0);
        // Chỉnh kích thước nhỏ lại rất nhiều vì ảnh gốc có thể có độ phân giải lớn
        go.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);

        SphereCollider col = go.AddComponent<SphereCollider>();
        col.isTrigger = true;
        // Bù trừ lại radius vì scale đã bị thu nhỏ (giảm đi một nửa để bóng phải chạm sát mới ăn)
        col.radius = 3.0f;

        ItemPickup pickup = go.AddComponent<ItemPickup>();
        pickup.itemType = ItemType.Coin;
        pickup.rotationSpeed = 150f; // Xoay nhanh hơn một chút cho đẹp
        
        // Tạo thư mục Prefabs nếu chưa có
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        string prefabPath = "Assets/Prefabs/DollarCoinPrefab.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        GameObject.DestroyImmediate(go); // Xóa cục mồi trên Scene

        // Tự động gán vào Helix Manager
        HelixManager helixManager = GameObject.FindObjectOfType<HelixManager>();
        if (helixManager != null)
        {
            Undo.RecordObject(helixManager, "Assign Coin Prefab");
            helixManager.coinPrefab = prefab;
            PrefabUtility.RecordPrefabInstancePropertyModifications(helixManager);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(helixManager.gameObject.scene);
            Debug.Log("THÀNH CÔNG! Đã tạo Prefab Dollar Coin và gắn tự động vào HelixManager.");
        }
        else
        {
            Debug.LogWarning("Đã tạo xong Prefab tại Assets/Prefabs/DollarCoinPrefab.prefab nhưng không tìm thấy HelixManager trên màn hình để gắn tự động. Bạn hãy tự kéo thả nhé.");
        }
    }
}
