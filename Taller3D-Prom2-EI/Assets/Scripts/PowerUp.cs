using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float rotationSpeed = 90f;
    public float lifetime = 15f; 

    private bool isCollected = false;
    private Transform playerTransform;

    void Start()
    {
        
        Destroy(gameObject, lifetime);
       
        SetupComponents();
       
        FindPlayer();

        Debug.Log("PowerUp creado en posición: " + transform.position);
    }

    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            Debug.Log("PowerUp encontró al jugador en: " + playerTransform.position);
        }
        else
        {
            Debug.LogWarning("PowerUp no pudo encontrar al jugador!");
        }
    }

    void SetupComponents()
    {        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
                
        rb.useGravity = false; 
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
        }
        else
        {
            col.isTrigger = true;
        }
        
        if (gameObject.tag != "PowerUp")
        {
            gameObject.tag = "PowerUp";
        }
    }

    void Update()
    {
        if (!isCollected)
        {
            
            if (playerTransform != null)
            {                
                Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;                
                transform.Translate(directionToPlayer * moveSpeed * Time.deltaTime, Space.World);
            }
            else
            {               
                transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.World);
            }
            
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
                        
            if (transform.position.z < -30f)
            {
                Debug.Log("PowerUp se salió del mapa (parte inferior) y fue destruido");
                Destroy(gameObject);
            }
            
            if (Mathf.Abs(transform.position.x) > 50f || transform.position.y < -10f || transform.position.z > 50f)
            {
                Debug.Log("PowerUp se salió de los límites del juego y fue destruido");
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isCollected && other.CompareTag("Player"))
        {
            isCollected = true;

            PlayerShooting shooter = other.GetComponent<PlayerShooting>();
            if (shooter != null)
            {
                shooter.EnableTripleShot();
                Debug.Log("¡PowerUp recogido! Disparo triple activado.");
                                             
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("El jugador no tiene componente PlayerShooting!");
                isCollected = false;
            }
        }
    }    
        
    void OnDrawGizmos()
    {
        if (!isCollected)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
                       
            if (playerTransform != null)
            {
                Gizmos.color = Color.green;
                Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
                Gizmos.DrawLine(transform.position, transform.position + directionToPlayer * 2f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + Vector3.forward * 2f);
            }
        }
    }
}