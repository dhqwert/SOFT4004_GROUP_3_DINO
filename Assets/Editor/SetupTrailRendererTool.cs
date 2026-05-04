using UnityEditor;
using UnityEngine;

public class SetupTrailRendererTool : EditorWindow
{
    [MenuItem("Tools/3. Setup Ball Trail (Vệt sao chổi)")]
    public static void SetupTrail()
    {
        // 1. Tìm quả bóng trong Scene
        GameObject ball = GameObject.FindGameObjectWithTag("Player");
        if (ball == null)
        {
            // Thử tìm theo tên nếu không có tag
            ball = GameObject.Find("Ball");
        }

        if (ball == null)
        {
            Debug.LogError("Không tìm thấy quả bóng (Ball) trong Scene. Vui lòng mở scene GamePlay!");
            return;
        }

        // 2. Thêm TrailRenderer nếu chưa có
        TrailRenderer tr = ball.GetComponent<TrailRenderer>();
        if (tr == null)
        {
            tr = ball.AddComponent<TrailRenderer>();
            Debug.Log("Đã gắn TrailRenderer vào Ball.");
        }

        // 3. Cấu hình hình dáng sao chổi (to ở đầu, vuốt nhọn ở đuôi)
        tr.time = 0.4f; // Thời gian tồn tại của vệt (độ dài)
        
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0.0f, 0.4f); // Chiều rộng ban đầu (bằng cỡ quả bóng)
        curve.AddKey(1.0f, 0.0f); // Chiều rộng đuôi (nhọn hoắt)
        tr.widthCurve = curve;

        // 4. Cấu hình màu sắc mờ dần theo màu của bóng
        Color ballColor = Color.magenta; // Mặc định
        MeshRenderer mr = ball.GetComponent<MeshRenderer>();
        if (mr != null && mr.sharedMaterial != null)
        {
            ballColor = mr.sharedMaterial.color;
        }

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(ballColor, 0.0f), new GradientColorKey(ballColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.7f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) } // Mờ dần về trong suốt
        );
        tr.colorGradient = gradient;

        // 5. Cấu hình Material (dùng loại mượt mà không bị đen viền)
        Material trailMat = new Material(Shader.Find("Particles/Standard Unlit"));
        trailMat.SetFloat("_Mode", 2); // Fade mode
        trailMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        trailMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        trailMat.SetInt("_ZWrite", 0);
        trailMat.DisableKeyword("_ALPHATEST_ON");
        trailMat.EnableKeyword("_ALPHABLEND_ON");
        trailMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        trailMat.renderQueue = 3000;
        
        // Hoặc đơn giản là dùng Sprites-Default nếu Particles không hoạt động tốt
        if (Shader.Find("Sprites/Default") != null) {
            trailMat = new Material(Shader.Find("Sprites/Default"));
        }
        
        tr.material = trailMat;

        // Tối ưu hóa hiệu năng
        tr.minVertexDistance = 0.1f;
        tr.alignment = LineAlignment.View;
        tr.textureMode = LineTextureMode.Tile;

        // Đánh dấu scene đã thay đổi để lưu lại
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(ball.scene);

        Debug.Log("✅ Đã tạo Vệt Sao Chổi cho quả bóng thành công! Bạn có thể ấn Play để xem.");
    }
}
