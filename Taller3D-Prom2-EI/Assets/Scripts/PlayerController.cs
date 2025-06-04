using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    private float limitX, limitZ;

    private void Start()
    {
        int size = GameManager.instance.GetMapSize();
                
        transform.position = new Vector3(0, 0.6f, -size + 5);                
        limitX = (size + 1) - 2f; 
        limitZ = (size + 1) - 2f; 
        Debug.Log($"Player spawned. Limits: X=±{limitX}, Z=±{limitZ}");
    }

    private void Update()
    {
        Move();
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
}