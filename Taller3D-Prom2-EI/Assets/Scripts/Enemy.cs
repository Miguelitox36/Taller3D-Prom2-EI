using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public abstract class Enemy : MonoBehaviour
{
    public float speed = 3f;
    protected Transform player;
    private EnemyFormationManager formationManager;
    private Vector3 relativePosition;
    private bool isRegistered = false;

    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        isRegistered = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

        BoxCollider col = GetComponent<BoxCollider>();
        col.isTrigger = false; // Necesitamos colisión sólida para OnTrigger en pared
    }

    public void SetFormationManager(EnemyFormationManager efm, Vector3 relPos)
    {
        formationManager = efm;
        relativePosition = relPos;
    }

    public void UpdateRelativePosition()
    {
        if (formationManager != null)
            transform.position = formationManager.transform.position + relativePosition;
    }

    protected abstract void Move();

    protected virtual void Update() { }

    private void OnDestroy()
    {
        if (isRegistered && GameManager.instance != null)
            GameManager.instance.EnemyKilled(this);
    }

    // Cambio: Usamos OnTriggerEnter para detectar la pared inferior
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall") && other.name.ToLower().Contains("bottom"))
        {
            Destroy(gameObject);
        }
    }
}

