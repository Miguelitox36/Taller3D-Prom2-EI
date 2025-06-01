using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public float bulletSpeed = 10f;
    public float fireRate = 0.3f;

    private float nextFireTime;
    private bool tripleShotEnabled = false;

    void Update()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Fire();
        }
    }

    void Fire()
    {
        InstantiateBullet(shootPoint.forward);

        if (tripleShotEnabled)
        {
            InstantiateBullet(Quaternion.Euler(0, 15, 0) * shootPoint.forward);
            InstantiateBullet(Quaternion.Euler(0, -15, 0) * shootPoint.forward);
        }
    }

    void InstantiateBullet(Vector3 dir)
    {
        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody>().velocity = dir * bulletSpeed;
    }

    public void EnableTripleShot()
    {
        tripleShotEnabled = true;
    }
}
