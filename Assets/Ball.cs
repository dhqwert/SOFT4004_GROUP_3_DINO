using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    Rigidbody rb;
    public float bounceForce = 400f;

    [Header("Cấu hình xoay")]
    public float rotationSpeed = 150f; // Tốc độ xoay
    private Vector3 rotationAxis;     // Trục xoay ngẫu nhiên

    public GameObject splitPrefab;

    private void Start () {
        rb = GetComponent<Rigidbody> ();
        // Tạo một trục xoay ngẫu nhiên để quả bóng xoay trông tự nhiên
        rotationAxis = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    private void Update()
    {
        // Làm cho quả bóng xoay liên tục theo thời gian
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }

    private void OnCollisionEnter (Collision other) {
        if (rb == null) {
            rb = GetComponent<Rigidbody>();
        }
        if (rb != null) {
            rb.velocity = new Vector3 (rb.velocity.x, bounceForce * Time.deltaTime, rb.velocity.z);
        }

        if (splitPrefab != null) {
            GameObject newsplit = Instantiate (splitPrefab, new Vector3 (transform.position.x, other.transform.position.y + 0.19f, transform.position.z), transform.rotation);
            newsplit.transform.localScale = Vector3.one * Random.Range(0.8f, 1.2f);
            newsplit.transform.parent = other.transform;
        }

        if (!TryGetCollisionMaterialName(other, out string materialName)) {
            return;
        }

        if(materialName == "Safe (Instance)") {
            Debug.Log ("Safe");
            if (AudioManager.instance != null) AudioManager.instance.Play("Bounce");
        }
        if(materialName == "Unsafe (Instance)") {
            GameManager.gameOver = true;
            if (AudioManager.instance != null) AudioManager.instance.Play("GameOver");
        }
        if(materialName == "LastRing (Instance)") {
            GameManager.instance?.WinLevel();
            if (AudioManager.instance != null) AudioManager.instance.Play("GameWin");
        }
    }

    static bool TryGetCollisionMaterialName(Collision other, out string materialName)
    {
        materialName = null;
        if (other == null || other.collider == null) {
            return false;
        }

        Renderer rend = other.collider.GetComponentInParent<Renderer>();
        if (rend == null) {
            rend = other.collider.GetComponent<Renderer>();
        }
        if (rend == null) {
            rend = other.gameObject.GetComponentInChildren<Renderer>();
        }

        if (rend == null || rend.material == null) {
            return false;
        }

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
