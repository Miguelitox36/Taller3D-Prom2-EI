using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyType2 : Enemy
{
    private float rotationSpeed = 90f;

    protected override void Move()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}
