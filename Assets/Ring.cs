using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring : MonoBehaviour
{
    private Transform player;
    public GameObject[] childRings;

    public float radius = 100f;
    public float force = 500f;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (transform.position.y > player.position.y + 0.1f)
        {
            // Điểm tăng nhẹ theo level
            int level = PlayerPrefs.GetInt("CurrentLevel", 1);
            int scorePerRing = 10 + Mathf.RoundToInt((level - 1) * 1f);

            if (GameManager.instance != null)
                GameManager.instance.AddScore(scorePerRing);

            // COMBO: đăng ký xuyên tầng thành công
            if (ComboManager.instance != null)
                ComboManager.instance.RegisterPass(scorePerRing);

            if (AudioManager.instance != null)
                AudioManager.instance.Play("Whoosh");

            for (int i = 0; i < childRings.Length; i++)
            {
                if (childRings[i] != null)
                {
                    Rigidbody childRb = childRings[i].GetComponent<Rigidbody>();
                    if (childRb != null)
                    {
                        childRb.isKinematic = false;
                        childRb.useGravity = true;
                    }
                    childRings[i].transform.parent = null;
                    Destroy(childRings[i].gameObject, 2f);
                }
            }

            Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
            foreach (Collider newCollider in colliders)
            {
                Rigidbody rb = newCollider.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.AddExplosionForce(force, transform.position, radius);
            }

            Destroy(this.gameObject, 5f);
            this.enabled = false;
        }
    }
}