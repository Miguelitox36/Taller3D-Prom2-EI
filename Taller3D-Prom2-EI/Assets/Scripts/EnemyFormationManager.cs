using System.Collections.Generic;
using UnityEngine;

public class EnemyFormationManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject enemy2Prefab;
    public float followSpeed = 3f;
    public float descentStep = 1f;
    public float spacing = 2f;

    private float direction = 1f;
    private float startX;
    private float mapHalfWidth;
    private List<Enemy> enemies = new List<Enemy>();

    private List<Vector3[]> waveFormations = new List<Vector3[]>
    {
        // Oleada 1: 10 enemigos tipo 1
        new Vector3[] {
            new Vector3(-4, 0, 2), new Vector3(-2, 0, 2), new Vector3(0, 0, 2), new Vector3(2, 0, 2), new Vector3(4, 0, 2),
            new Vector3(-4, 0, 0), new Vector3(-2, 0, 0), new Vector3(0, 0, 0), new Vector3(2, 0, 0), new Vector3(4, 0, 0),
        },

        // Oleada 2: 5 enemigos tipo 1 (X) + 5 tipo 2 (círculo)
        new Vector3[] {
            // X shape - tipo 1
            new Vector3(-2, 0, 2), new Vector3(-1, 0, 1), new Vector3(0, 0, 0), new Vector3(1, 0, 1), new Vector3(2, 0, 2),
            // Circle shape - tipo 2
            new Vector3(0, 0, 3), new Vector3(2, 0, 1), new Vector3(1, 0, -1), new Vector3(-1, 0, -1), new Vector3(-2, 0, 1),
        },

        // Oleada 3: 20 enemigos combinados
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
        mapHalfWidth = (GameManager.instance.GetMapSize() + 1) * spacing / 2f - 1f;
    }

    void Update()
    {
        MoveFormation();
    }

    void MoveFormation()
    {
        transform.position += Vector3.right * direction * followSpeed * Time.deltaTime;

        if (Mathf.Abs(transform.position.x) >= mapHalfWidth)
        {
            direction *= -1f;
            transform.position += Vector3.back * descentStep;
        }

        foreach (Enemy enemy in enemies)
        {
            if (enemy != null)
                enemy.UpdateRelativePosition();
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

            if (waveIndex == 1 && i >= 5) // Oleada 2: enemigos circulares son tipo 2
                prefabToSpawn = enemy2Prefab;
            else if (waveIndex == 2 && i % 2 == 0) // Oleada 3: tipo 2 en posiciones pares
                prefabToSpawn = enemy2Prefab;
            else
                prefabToSpawn = enemyPrefab;

            GameObject enemyObj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

            if (enemyObj.TryGetComponent<Enemy>(out Enemy e))
            {
                e.SetFormationManager(this, offset);
                enemies.Add(e);
            }
        }

        GameManager.instance.RegisterEnemies(enemies);
    }

    public void ClearEnemies()
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }
        enemies.Clear();
    }
}
