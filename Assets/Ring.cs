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
        if(transform.position.y > player.position.y + 0.1f) {

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
}