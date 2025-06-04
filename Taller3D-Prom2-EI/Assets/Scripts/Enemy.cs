using UnityEngine;

public class Enemy : MonoBehaviour
{
    protected Vector3 relativePosition;
    public GameObject powerUpPrefab;
    protected EnemyFormationManager formationManager;

    public void SetRelativePosition(Vector3 relative)
    {
        relativePosition = relative;
    }

    public void SetFormationManager(EnemyFormationManager manager, Vector3 relative)
    {
        formationManager = manager;
        SetRelativePosition(relative);
    }

    public void UpdateRelativePosition()
    {
        if (this != null && transform != null)
        {
            transform.localPosition = relativePosition;
        }
    }

    void Update()
    {
        Move();
    }

    protected virtual void Move()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {            
            Vector3 incomingVector = transform.forward;
            Vector3 reflectionVector = Vector3.Reflect(incomingVector, collision.contacts[0].normal);
                        
            transform.rotation = Quaternion.LookRotation(reflectionVector);
                        
            transform.position += reflectionVector * 0.1f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BottomWall"))
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.EnemyReachedBottom(this);
            }
        }
    }

    void OnDestroy()
    {       
        if (formationManager != null)
        {
            formationManager.RemoveEnemy(this);
        }
    }
}