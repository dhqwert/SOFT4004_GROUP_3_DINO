using UnityEngine;

public enum ItemType {
    Coin,
    Piercing
}

public class ItemPickup : MonoBehaviour
{
    public ItemType itemType;
    public int pierceAmount = 3; // Số tầng xuyên phá
    public float rotationSpeed = 100f;

    private Transform player;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    private void Update()
    {
        // Xoay nhẹ item cho đẹp (xoay quanh trục Y của thế giới để ảnh luôn ngửa lên)
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0, Space.World);

        // Biến mất khi bóng rơi QUÁ vòng (player.y thấp hơn item hẳn 1.5 đơn vị)
        // Điều này đảm bảo bóng nảy trên nấc thì item chưa mất, chỉ mất khi bóng rớt xuống tầng dưới
        if (player != null && transform.position.y > player.position.y + 1.5f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Ball ball = other.GetComponent<Ball>();
            if (ball != null)
            {
                if (itemType == ItemType.Coin)
                {
                    if (GameManager.instance != null)
                    {
                        GameManager.instance.AddCoins(1);
                        Debug.Log("🔔 [Client] Đã nhặt được 1 Coin!");
                    }
                    if (AudioManager.instance != null) AudioManager.instance.Play("Coin");
                }
                else if (itemType == ItemType.Piercing)
                {
                    ball.pierceCount += pierceAmount;
                    Debug.Log("🔥 [Client] Đã nhặt được Xuyên Phá! Xuyên qua " + pierceAmount + " tầng.");
                    if (AudioManager.instance != null) AudioManager.instance.Play("Powerup");
                }
                
                Destroy(gameObject);
            }
        }
    }
}
