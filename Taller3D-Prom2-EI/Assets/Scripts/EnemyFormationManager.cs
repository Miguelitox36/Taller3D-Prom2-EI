using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyFormationManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject enemy2Prefab;
    public float followSpeed = 10f;
    public float descentStep = 2f;
    public float spacing = 2f;

    private float direction = 1f;
    private float startX;
    private float mapHalfWidth;
    private List<Enemy> enemies = new List<Enemy>();
    private BoxCollider formationCollider;

    private List<Vector3[]> waveFormations = new List<Vector3[]>
    {
        new Vector3[] {
            new Vector3(-4, 0, 2), new Vector3(-2, 0, 2), new Vector3(0, 0, 2), new Vector3(2, 0, 2), new Vector3(4, 0, 2),
            new Vector3(-4, 0, 0), new Vector3(-2, 0, 0), new Vector3(0, 0, 0), new Vector3(2, 0, 0), new Vector3(4, 0, 0),
        },
        new Vector3[] {
            new Vector3(-2, 0, 2), new Vector3(-1, 0, 1), new Vector3(0, 0, 0), new Vector3(1, 0, 1), new Vector3(2, 0, 2),
            new Vector3(0, 0, 3), new Vector3(2, 0, 1), new Vector3(1, 0, -1), new Vector3(-1, 0, -1), new Vector3(-2, 0, 1),
        },
        new Vector3[] {
            new Vector3(0, 0, 4),
            new Vector3(-1, 0, 3), new Vector3(1, 0, 3),
            new Vector3(-2, 0, 2), new Vector3(0, 0, 2), new Vector3(2, 0, 2),
            new Vector3(-3, 0, 1), new Vector3(-1, 0, 1), new Vector3(1, 0, 1), new Vector3(3, 0, 1),
            new Vector3(-4, 0, 0), new Vector3(-2, 0, 0), new Vector3(0, 0, 0), new Vector3(2, 0, 0), new Vector3(4, 0, 0),
            new Vector3(-2, 0, -1), new Vector3(0, 0, -1), new Vector3(2, 0, -1), new Vector3(0, 0, -3),
        }
    };

    void Start()
    {
        startX = transform.position.x;
        mapHalfWidth = (GameManager.instance.GetMapSize() + 3) * spacing / 2f - 2f;
               
        SetupFormationCollider();
    }

    void SetupFormationCollider()
    {
        formationCollider = gameObject.AddComponent<BoxCollider>();
        formationCollider.isTrigger = true;
        formationCollider.size = new Vector3(spacing * 10f, 2f, spacing * 8f);
        formationCollider.center = Vector3.zero;
    }

    void Update()
    {
        MoveFormation();
    }

    void MoveFormation()
    {
        Vector3 newPosition = transform.position + Vector3.right * direction * followSpeed * Time.deltaTime;
                
        if (Mathf.Abs(newPosition.x) >= mapHalfWidth)
        {
            direction *= -1f;
            newPosition = transform.position + Vector3.back * descentStep;
                       
            float clampedX = Mathf.Clamp(newPosition.x, -mapHalfWidth, mapHalfWidth);
            newPosition = new Vector3(clampedX, newPosition.y, newPosition.z);

            Debug.Log("Formación rebotó en la pared y descendió");
        }

        transform.position = newPosition;
                
        CleanupNullEnemies();
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null && enemy.gameObject != null)
            {
                enemy.UpdateRelativePosition();
            }
        }
    }
        
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            direction *= -1f;
            transform.position += Vector3.back * descentStep;
            Debug.Log("Formación detectó colisión con pared y rebotó");
        }
    }

    public void SpawnWave(int waveIndex)
    {
        ClearEnemies();
        Vector3[] formation = waveFormations[waveIndex];
        enemies.Clear();

        for (int i = 0; i < formation.Length; i++)
        {
            Vector3 relativePos = formation[i];
            Vector3 offset = new Vector3(relativePos.x * spacing, 0, relativePos.z * spacing);
            Vector3 spawnPos = transform.position + offset;

            GameObject prefabToSpawn;
                        
            if (waveIndex == 1 && i >= 5)
                prefabToSpawn = enemy2Prefab;
            else if (waveIndex == 2 && i % 2 == 0)
                prefabToSpawn = enemy2Prefab;
            else
                prefabToSpawn = enemyPrefab;

            GameObject enemyObj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            enemyObj.transform.SetParent(transform);

            if (enemyObj.TryGetComponent<Enemy>(out Enemy enemy))
            {
                enemy.SetFormationManager(this, offset);
                enemies.Add(enemy);
            }
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.RegisterEnemies(enemies);
        }
    }

    public void ClearEnemies()
    {
        foreach (Enemy enemy in enemies.ToList())
        {
            if (enemy != null && enemy.gameObject != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        enemies.Clear();
    }

    public void RemoveEnemy(Enemy enemy)
    {
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
        }
    }

    private void CleanupNullEnemies()
    {
        enemies.RemoveAll(enemy => enemy == null || enemy.gameObject == null);
    }
    
    public bool HasEnemiesLeft()
    {
        CleanupNullEnemies();
        return enemies.Count > 0;
    }
}