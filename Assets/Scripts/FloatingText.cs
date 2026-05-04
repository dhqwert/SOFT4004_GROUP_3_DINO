using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float lifeTime = 1.5f;
    private float timer = 0f;
    private TextMeshPro tmp;
    private Color originalColor;

    void Start()
    {
        tmp = GetComponent<TextMeshPro>();
        if (tmp != null) originalColor = tmp.color;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Chữ trôi từ từ lên trên (tính theo tọa độ Local của Camera)
        transform.localPosition += Vector3.up * moveSpeed * Time.deltaTime;

        // Mờ dần (Fade out)
        timer += Time.deltaTime;
        if (tmp != null)
        {
            // Bắt đầu mờ đi sau nửa thời gian đầu
            float alpha = 1f;
            if (timer > lifeTime * 0.5f) {
                alpha = Mathf.Lerp(1f, 0f, (timer - lifeTime * 0.5f) / (lifeTime * 0.5f));
            }
            tmp.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }
    }

    public static void Create(string text, Color color)
    {
        // 1. Dọn dẹp chữ cũ nếu có để tránh đè lên nhau
        GameObject oldText = GameObject.Find("AwesomeText");
        if (oldText != null) {
            Destroy(oldText);
        }

        // 2. Tạo chữ mới
        GameObject obj = new GameObject("AwesomeText");
        
        // Cố định chữ vào Camera để nó bay theo màn hình (không bị trôi lùi lại phía trên)
        if (Camera.main != null) {
            obj.transform.SetParent(Camera.main.transform, false);
            // Đặt vị trí hiển thị: Nằm giữa màn hình, nhích lên trên một chút, cách camera 5 đơn vị
            obj.transform.localPosition = new Vector3(0f, 1.5f, 5f);
            obj.transform.localRotation = Quaternion.identity; // Luôn nhìn thẳng
        }

        TextMeshPro textMesh = obj.AddComponent<TextMeshPro>();
        textMesh.text = text;
        textMesh.color = color;
        textMesh.fontSize = 6;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontStyle = FontStyles.Bold;
        
        // Thêm viền đen (Outline) cho chữ dễ đọc
        textMesh.outlineWidth = 0.2f;
        textMesh.outlineColor = new Color32(0, 0, 0, 255);
        
        // Căn giữa chữ
        textMesh.alignment = TextAlignmentOptions.Center;

        FloatingText ft = obj.AddComponent<FloatingText>();
        ft.moveSpeed = 1f; // Bay chậm lại một chút vì đang bay trên màn hình
        ft.lifeTime = 1.0f; // Tồn tại 1 giây thôi cho gọn
    }
}
