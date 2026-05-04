using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class SetupBackgroundTool
{
    [MenuItem("Tools/2. Cài Đặt Hình Nền (Background)")]
    public static void SetupBackground()
    {
        string spritePath = "Assets/Sprites/658905970_2186686322156691_1629857610567207382_n.png";

        // Ép kiểu ảnh thành Sprite (2D and UI) nếu người dùng copy thẳng vào mà chưa chỉnh
        TextureImporter ti = AssetImporter.GetAtPath(spritePath) as TextureImporter;
        if (ti != null && ti.textureType != TextureImporterType.Sprite)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.SaveAndReimport();
        }

        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (bgSprite == null)
        {
            Debug.LogError("CẢNH BÁO: Không tìm thấy ảnh nền tại đường dẫn: " + spritePath + ". Bạn hãy kiểm tra lại tên file hoặc đường dẫn nhé!");
            return;
        }

        // Tìm Camera chính trong game
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            mainCam = GameObject.FindObjectOfType<Camera>();
            if (mainCam == null) {
                Debug.LogError("CẢNH BÁO: Không tìm thấy Camera nào trong Scene để đặt hình nền.");
                return;
            }
        }

        // Xóa hình nền cũ (nếu bạn ấn Tool này nhiều lần)
        GameObject oldCanvas = GameObject.Find("BackgroundCanvas_Custom");
        if (oldCanvas != null)
        {
            GameObject.DestroyImmediate(oldCanvas);
        }

        // Tạo Canvas chuyên dụng cho hình nền
        GameObject canvasObj = new GameObject("BackgroundCanvas_Custom");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        
        // Render Mode = ScreenSpaceCamera giúp hình nền luôn bám theo camera nhưng nằm tuốt phía sau các khối 3D
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = mainCam;
        canvas.planeDistance = 50f; // Đẩy ra xa 50 mét để chắc chắn nằm dưới cùng
        canvas.sortingOrder = -100; // Thứ tự vẽ dưới đáy xã hội

        // Thêm các component bắt buộc của UI
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Tạo cục hiển thị ảnh
        GameObject bgObj = new GameObject("BackgroundImage");
        bgObj.transform.SetParent(canvasObj.transform, false);

        Image img = bgObj.AddComponent<Image>();
        img.sprite = bgSprite;

        // Kéo dãn ảnh tràn toàn bộ màn hình
        RectTransform rect = bgObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        // Thêm bộ lọc chống méo ảnh (nếu màn hình dọc/ngang khác tỉ lệ ảnh gốc, nó sẽ tự zoom lên thay vì bóp méo)
        AspectRatioFitter fitter = bgObj.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        if (bgSprite.texture != null) {
            fitter.aspectRatio = (float)bgSprite.texture.width / bgSprite.texture.height;
        }

        // Ghi nhận thay đổi để Unity lưu lại
        Undo.RegisterCreatedObjectUndo(canvasObj, "Tạo Background");
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mainCam.gameObject.scene);

        Debug.Log("THÀNH CÔNG! Đã lót xong bức ảnh " + bgSprite.name + " làm hình nền phía sau các khối trụ.");
    }
}
