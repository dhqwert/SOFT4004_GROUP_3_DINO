using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    Rigidbody rb;
    public float bounceForce = 400f;

    [Header("Cấu hình xoay")]
    public float rotationSpeed = 150f;
    private Vector3 rotationAxis;

    public GameObject splitPrefab;
    
    [Header("Item Effects")]
    public int pierceCount = 0; // Số tầng có thể xuyên qua
    [HideInInspector]
    public int passCount = 0; // Đếm số tầng đã rơi qua liên tiếp không chạm

    // Material dùng chung cho Projector vết bóng
    private static Material projectorMat;

    private Color originalBallColor;
    private Gradient originalTrailGradient;
    private MeshRenderer mr;
    private TrailRenderer tr;

    private void Start () {
        rb = GetComponent<Rigidbody> ();
        mr = GetComponent<MeshRenderer>();
        tr = GetComponent<TrailRenderer>();

        rotationAxis = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

        if (mr != null) originalBallColor = mr.sharedMaterial.color;
        if (tr != null) originalTrailGradient = tr.colorGradient;
    }

    // Đổi màu bóng và vệt sao chổi sang rực lửa đỏ khi đạt siêu combo
    public void SetFireState(bool isFire)
    {
        if (mr == null || tr == null) return;

        if (isFire)
        {
            mr.material.color = Color.red;
            
            Gradient fireGradient = new Gradient();
            fireGradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.yellow, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            tr.colorGradient = fireGradient;
        }
        else
        {
            mr.material.color = originalBallColor;
            tr.colorGradient = originalTrailGradient;
        }
    }

    private void Update()
    {
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);

        // Khi có combo xuyên phá, bóng rơi một mạch và phá các vòng phía dưới
        if (pierceCount > 0 && !GameManager.levelWin && !GameManager.gameOver)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position + Vector3.down * 0.3f, 0.3f);
            foreach (var hit in hits)
            {
                if (hit.isTrigger || hit.CompareTag("Player")) continue;

                if (TryGetColliderMaterialName(hit, out string matName))
                {
                    if (matName.Contains("LastRing"))
                    {
                        GameManager.instance?.WinLevel();
                        if (AudioManager.instance != null) AudioManager.instance.Play("GameWin");
                        pierceCount = 0; // Dừng xuyên phá ở tầng đích
                        continue;
                    }
                }

                Ring ring = hit.GetComponentInParent<Ring>();
                if (ring != null && ring.enabled)
                {
                    ring.ForceBreak();
                    pierceCount--;
                    if (pierceCount <= 0) {
                        pierceCount = 0;
                        SetFireState(false); // Hết xuyên phá thì tắt lửa
                    }
                }
            }

            // Ép tốc độ rơi xuống hợp lý để càn quét nhưng người chơi vẫn kịp nhìn (không quá nhanh)
            if (rb != null)
            {
                rb.velocity = new Vector3(rb.velocity.x, -15f, rb.velocity.z);
            }
        }
        else if (passCount >= 7 && rb != null && rb.velocity.y < 0)
        {
            // Khi đang ở trạng thái On Fire (lách qua 7 lỗ), lực hút trái đất sẽ mạnh hơn 1 tí
            // Giúp quả bóng rơi qua các khe hở nhanh hơn bình thường
            rb.AddForce(Vector3.down * 15f, ForceMode.Acceleration);
        }
    }

    private void OnCollisionEnter (Collision other) {
        if (rb == null) {
            rb = GetComponent<Rigidbody>();
        }

        // Đã thắng game rồi thì chỉ cho bóng nảy lên nảy xuống để ăn mừng, không xét bẫy bủng gì nữa
        if (GameManager.levelWin) {
            if (rb != null) rb.velocity = new Vector3 (rb.velocity.x, bounceForce * Time.deltaTime, rb.velocity.z);
            return; 
        }

        if (!TryGetCollisionMaterialName(other, out string materialName)) {
            return;
        }

        // ƯU TIÊN SỐ 1: NẾU CHẠM VÀO ĐÍCH (LASTRING), BẤT CHẤP ĐANG CÓ COMBO HAY KHÔNG
        if(materialName.Contains("LastRing")) {
            GameManager.instance?.WinLevel();
            if (AudioManager.instance != null) AudioManager.instance.Play("GameWin");
            // Quả bóng nảy lên để ăn mừng
            if (rb != null) rb.velocity = new Vector3 (rb.velocity.x, bounceForce * Time.deltaTime, rb.velocity.z);
            return;
        }

        // --- SỬA LỖI CHẾT OAN (QUẸT NHẸ VÀO BẪY) ---
        if (materialName.Contains("Unsafe"))
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.6f))
            {
                if (TryGetColliderMaterialName(hit.collider, out string hitMat))
                {
                    if (hitMat.Contains("Safe"))
                    {
                        materialName = hitMat; 
                    }
                }
            }
        }

        // Xử lý xuyên phá (Combo Item) - Fallback nếu va chạm vật lý vẫn xảy ra
        if (pierceCount > 0) {
            Ring ring = other.gameObject.GetComponentInParent<Ring>();
            if (ring != null && ring.enabled) {
                ring.ForceBreak();
                pierceCount--;
                if (pierceCount < 0) pierceCount = 0;
            }
            // Giữ tốc độ rơi đâm xuyên vừa phải, không nảy lên
            if (rb != null) {
                rb.velocity = new Vector3(rb.velocity.x, -15f, rb.velocity.z);
            }
            return; // Bỏ qua xử lý va chạm bình thường (không Game Over nếu trúng bẫy)
        }

        // --- LOGIC COMBO: XUYÊN PHÁ TOÀN TẬP ---
        if (passCount >= 3) {
            // Cân bằng lại sức mạnh xuyên phá:
            // Lách 3, 4 tầng -> Xuyên nát 2 tầng
            // Lách 5, 6 tầng -> Xuyên nát 3 tầng
            // Lách 7+ tầng   -> Xuyên nát 4 tầng
            if (passCount < 5) {
                pierceCount = 2;
            } else if (passCount < 7) {
                pierceCount = 3;
            } else {
                pierceCount = 4;
            }
            
            // Xóa trạng thái đếm combo
            passCount = 0;
            
            // Ép quả bóng rớt đâm thủng (Tốc độ giảm lại một chút để an toàn cho người chơi)
            if (rb != null) {
                rb.velocity = new Vector3(rb.velocity.x, -15f, rb.velocity.z);
            }
            
            if (AudioManager.instance != null) AudioManager.instance.Play("Powerup");
            
            // Giữ nguyên lửa cho đến khi hết pierceCount (xử lý tắt lửa ở Update)
            return; 
        }

        // Bình thường chạm đất là mất combo
        passCount = 0;
        SetFireState(false);

        // Quả bóng nảy lên bình thường
        if (rb != null) {
            rb.velocity = new Vector3 (rb.velocity.x, bounceForce * Time.deltaTime, rb.velocity.z);
        }

        // --- VẼ VẾT BÓNG (NẾU KHÔNG CÓ COMBO VÀ KHÔNG ITEM XUYÊN PHÁ) ---
        if (splitPrefab != null) {
            Vector3 splashPos = new Vector3 (transform.position.x, other.transform.position.y + 0.5f, transform.position.z);
            Quaternion splashRot = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);
            GameObject newsplit = Instantiate (splitPrefab, splashPos, splashRot);
            newsplit.transform.parent = other.transform;
            
            Texture splashTex = null;
            Renderer[] rends = newsplit.GetComponentsInChildren<Renderer>();
            foreach(var r in rends) {
                if (splashTex == null && r.sharedMaterial != null) splashTex = r.sharedMaterial.mainTexture;
                if (r is SpriteRenderer sr && sr.sprite != null) splashTex = sr.sprite.texture;
                r.enabled = false; 
            }

            if (projectorMat == null) {
                Shader projShader = Shader.Find("Custom/SplashProjector");
                if (projShader != null) {
                    projectorMat = new Material(projShader);
                    if (splashTex != null) {
                        splashTex.wrapMode = TextureWrapMode.Clamp;
                        projectorMat.SetTexture("_ShadowTex", splashTex);
                    }
                }
            }

            Projector proj = newsplit.AddComponent<Projector>();
            proj.orthographic = true;
            proj.orthographicSize = Random.Range(0.4f, 0.6f);
            proj.nearClipPlane = 0.1f;
            proj.farClipPlane = 2.0f; 

            if (projectorMat != null) {
                Material mat = new Material(projectorMat);
                MeshRenderer ballRenderer = GetComponent<MeshRenderer>();
                if (ballRenderer != null) mat.color = ballRenderer.material.color;
                proj.material = mat;
            }
        }

        // Xử lý chết hoặc sống
        if(materialName.Contains("Safe")) {
            Debug.Log ("Safe");
            if (AudioManager.instance != null) AudioManager.instance.Play("Bounce");
        }
        else if(materialName.Contains("Unsafe")) {
            GameManager.gameOver = true;
            if (AudioManager.instance != null) AudioManager.instance.Play("GameOver");
        }
    }

    static bool TryGetCollisionMaterialName(Collision other, out string materialName)
    {
        materialName = null;
        if (other == null || other.collider == null) return false;
        return TryGetColliderMaterialName(other.collider, out materialName);
    }

    static bool TryGetColliderMaterialName(Collider col, out string materialName)
    {
        materialName = null;
        if (col == null) return false;

        Renderer rend = col.GetComponentInParent<Renderer>();
        if (rend == null) rend = col.GetComponent<Renderer>();
        if (rend == null) rend = col.gameObject.GetComponentInChildren<Renderer>();

        if (rend == null || rend.material == null) return false;

        materialName = rend.material.name;
        return true;
    }

    /// <summary>Gọi sau hồi sinh: đẩy bóng lên và xóa vận tốc để tránh kẹt/chết ngay.</summary>
    public void ApplyReviveNudge(float upwardDistance)
    {
        if (rb == null) {
            rb = GetComponent<Rigidbody>();
        }
        transform.position += Vector3.up * upwardDistance;
        if (rb != null) {
            rb.velocity = new Vector3(rb.velocity.x, Mathf.Max(rb.velocity.y, 0f), rb.velocity.z);
            rb.angularVelocity = Vector3.zero;
        }
    }
}
