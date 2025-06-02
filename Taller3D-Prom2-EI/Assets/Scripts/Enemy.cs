using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    public float speed = 3f;
    public bool isPowerUpHolder = false;
    public GameObject powerUpPrefab; // Agregado

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

        CapsuleCollider col = GetComponent<CapsuleCollider>();
        col.isTrigger = false;
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

        if (isPowerUpHolder && powerUpPrefab != null)
        {
            Instantiate(powerUpPrefab, transform.position, Quaternion.identity);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall") && other.name.ToLower().Contains("bottom"))
        {
            Destroy(gameObject);
        }
    }
}
