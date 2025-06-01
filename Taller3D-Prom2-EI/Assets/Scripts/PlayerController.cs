using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public GameObject bulletPrefab;
    public Transform firePoint;

    private float limitX, limitZ;

    private void Start()
    {
        transform.position = new Vector3(0, 0.6f, -3);

        int size = GameManager.instance.GetMapSize();
        limitX = (size + 1) - 1f;
        limitZ = (size + 1) - 1f;
    }

    private void Update()
    {
        Move();

        if (Input.GetKeyDown(KeyCode.Space))
            Shoot();
    }

    void Move()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 moveDir = new Vector3(moveX, 0f, moveZ).normalized;
        Vector3 newPos = transform.position + moveDir * moveSpeed * Time.deltaTime;

        newPos.x = Mathf.Clamp(newPos.x, -limitX, limitX);
        newPos.z = Mathf.Clamp(newPos.z, -limitZ, limitZ);

        transform.position = newPos;
    }

    void Shoot()
    {
        if (firePoint != null)
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }
}
