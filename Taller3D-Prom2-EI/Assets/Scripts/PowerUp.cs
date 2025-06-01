using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerShooting shooter = other.GetComponent<PlayerShooting>();
            if (shooter != null)
                shooter.EnableTripleShot();

            Destroy(gameObject);
        }
    }
}
