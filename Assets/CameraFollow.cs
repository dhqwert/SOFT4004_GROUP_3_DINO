using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public float smoothSpeed = 10f; // Tốc độ bám mượt (cao hơn = nhanh hơn)
    [Header("Nâng camera lên cao hơn bóng bao nhiêu đơn vị")]
    public float extraHeight = 3f; // Chỉnh số này trong Inspector cho vừa mắt

    private void Start () {
        offset = transform.position - target.position;
    }

    private void LateUpdate () {
        Vector3 desiredPos = target.position + offset;
        desiredPos.y += extraHeight; // Nâng camera lên cao hơn
        
        // Nếu bóng đang rơi XUỐNG (camera cần đuổi theo) → bám NGAY LẬP TỨC
        // Nếu bóng nảy LÊN → di chuyển mượt mà
        if (desiredPos.y < transform.position.y)
        {
            // Rơi xuống: Camera bám tức thì, không bao giờ mất bóng
            transform.position = desiredPos;
        }
        else
        {
            // Nảy lên: Camera đuổi theo mượt mà
            Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPos;
        }
    }
}