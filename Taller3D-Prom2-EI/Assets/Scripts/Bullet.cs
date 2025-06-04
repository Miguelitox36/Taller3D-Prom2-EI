using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && GameManager.instance != null)
            {
                GameManager.instance.EnemyKilled(enemy);
            }
                       
            if (other.gameObject != null)
            {
                Destroy(other.gameObject);
            }

            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }
    }
}