using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring : MonoBehaviour
{
    private Transform player;
    public GameObject[] childRings;

    public float radius = 100f;
    public float force = 500f;

    private void Start () {
        player = GameObject.FindGameObjectWithTag ("Player").transform;
    }

    private void Update () {
        // Không phá vỡ vòng khi đang ở màn hình chiến thắng hoặc game over
        if (GameManager.levelWin || GameManager.gameOver) return;

        if(transform.position.y > player.position.y + 0.1f) {
            
            // --- CỘNG ĐIỂM VÀ VÀNG ---
            // Gọi GameManager và cộng 10 điểm (bên trong hàm AddScore nó đã tự động +1 vàng)
            if (GameManager.instance != null) {
                GameManager.instance.AddScore(10);
            }
            // Phát âm thanh Whoosh khi xuyên qua tầng
            if (AudioManager.instance != null) AudioManager.instance.Play("Whoosh");
            // Cập nhật Combo rơi liên tiếp
            Ball ball = player.GetComponent<Ball>();
            if (ball != null) {
                ball.passCount++;
                
                // Khi đạt mốc >= 3 tầng, kích hoạt hiệu ứng Streak (Chuỗi)
                if (ball.passCount >= 3) {
                    // 1. Cộng vàng thưởng thêm cho Streak
                    int bonusCoins = 2; // Mỗi tầng Streak thưởng 2 vàng
                    if (GameManager.instance != null) {
                        GameManager.instance.AddCoins(bonusCoins);
                    }

                    // 2. Tạo chữ nổi lên (Từ ngữ tăng tiến theo độ dài Streak)
                    string[] words = { "GOOD!", "GREAT!", "AWESOME!", "PERFECT!", "INSANE!", "GODLIKE!" };
                    int wordIndex = Mathf.Min(ball.passCount - 3, words.Length - 1);
                    string streakWord = words[wordIndex] + "\n+" + bonusCoins;

                    // Chữ bay lên giữa màn hình
                    FloatingText.Create(streakWord, new Color(1f, 0.8f, 0f));
                }

                // Hiệu ứng "Hóa lửa đỏ" khi đạt Chuỗi 7
                if (ball.passCount == 7) {
                    ball.SetFireState(true);
                    // Rung nhẹ màn hình hoặc thêm text đặc biệt nếu muốn
                    FloatingText.Create("ON FIRE!!!", Color.red);
                }
            }

            ForceBreak();
        }
    }

    public void ForceBreak() {
        // Xóa ngay lập tức các item đang nằm trên tầng này để không bị lơ lửng
        ItemPickup[] items = GetComponentsInChildren<ItemPickup>();
        foreach(ItemPickup item in items) {
            Destroy(item.gameObject);
        }

        // 1. Duyệt qua các mảnh vỡ: Tách ra khỏi cha và bật vật lý (có kiểm tra an toàn)
        for(int i = 0; i < childRings.Length; i++) {
            if (childRings[i] != null) {
                Rigidbody childRb = childRings[i].GetComponent<Rigidbody> ();
                
                // NẾU mảnh vỡ có Rigidbody thì mới bật
                if (childRb != null) {
                    childRb.isKinematic = false;
                    childRb.useGravity = true;
                }

                childRings[i].transform.parent = null;
                Destroy (childRings[i].gameObject, 2f);
            }
        }

        // 2. TẠO LỰC NỔ (Chỉ gọi 1 lần duy nhất ngoài vòng lặp)
        Collider[] colliders = Physics.OverlapSphere (transform.position, radius);
        foreach(Collider newCollider in colliders) {
            Rigidbody rb = newCollider.GetComponent<Rigidbody> ();
            if(rb != null) {
                rb.AddExplosionForce (force, transform.position, radius);
            }
        }

        // 3. Hủy object cha và tắt script này đi
        Destroy (this.gameObject, 5f);  
        this.enabled = false;
    }
}