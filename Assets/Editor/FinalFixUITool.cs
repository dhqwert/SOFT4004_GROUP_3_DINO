using UnityEditor;
using UnityEngine;
using TMPro;

public class FinalFixUITool : EditorWindow
{
    [MenuItem("Tools/6. Xóa chữ BACK trong thanh Level")]
    public static void CleanUpLevelText()
    {
        LevelProgressUI progressUI = GameObject.FindObjectOfType<LevelProgressUI>(true);
        if (progressUI != null)
        {
            // Trả lại chữ gốc cho 2 vòng tròn
            if (progressUI.currentLevelText != null) {
                progressUI.currentLevelText.text = "1";
                progressUI.currentLevelText.fontSize = 36;
            }
            if (progressUI.nextLevelText != null) {
                progressUI.nextLevelText.text = "2";
                progressUI.nextLevelText.fontSize = 36;
            }

            // Đưa thanh level về chính giữa (đề phòng bạn chưa bấm Fix lần trước)
            RectTransform rect = progressUI.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0f, -50f);
            }
            
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(progressUI.gameObject.scene);
            Debug.Log("✅ Đã xóa chữ BACK rác và trả lại giao diện chuẩn!");
        }
    }
}
