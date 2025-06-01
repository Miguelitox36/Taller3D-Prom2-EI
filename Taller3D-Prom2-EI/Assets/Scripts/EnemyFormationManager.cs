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
        new Vector3[] {
            new Vector3(-5, 0, 3), new Vector3(-3, 0, 3), new Vector3(-1, 0, 3), new Vector3(1, 0, 3), new Vector3(3, 0, 3), new Vector3(5, 0, 3),
            new Vector3(-5, 0, 1), new Vector3(-3, 0, 1), new Vector3(-1, 0, 1), new Vector3(1, 0, 1), new Vector3(3, 0, 1), new Vector3(5, 0, 1),
        },
        new Vector3[] {
            new Vector3(-4, 0, 2), new Vector3(-3, 0, 1), new Vector3(-2, 0, 0),
            new Vector3(0, 0, -1),
            new Vector3(2, 0, 0), new Vector3(3, 0, 1), new Vector3(4, 0, 2),
            new Vector3(-1, 0, -2), new Vector3(1, 0, -2), new Vector3(0, 0, 1),
        },
        new Vector3[] {
            new Vector3(0, 0, 3),
            new Vector3(-1, 0, 2), new Vector3(1, 0, 2),
            new Vector3(-2, 0, 1), new Vector3(0, 0, 1), new Vector3(2, 0, 1),
            new Vector3(-3, 0, 0), new Vector3(-1, 0, 0), new Vector3(1, 0, 0), new Vector3(3, 0, 0),
            new Vector3(-2, 0, -1), new Vector3(0, 0, -1), new Vector3(2, 0, -1),
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

            GameObject prefabToSpawn = enemyPrefab;

            // Oleada 1: solo tipo 1, Oleada 2: alternar, Oleada 3: cada 3 enemigos es tipo 2
            if (waveIndex == 1 && i % 2 == 1)
                prefabToSpawn = enemy2Prefab;
            else if (waveIndex == 2 && i % 3 == 0)
                prefabToSpawn = enemy2Prefab;

            GameObject enemyObj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            if (enemyObj.TryGetComponent<Enemy>(out Enemy e))
            {
                e.SetFormationManager(this, offset);
                enemies.Add(e);
            }
        }

        GameManager.instance.RegisterEnemies(enemies);

        // Drop power-up al terminar oleada 2 (índice 1)
        if (waveIndex == 1)
        {
            int dropIndex = formation.Length / 2;
            Vector3 dropOffset = new Vector3(formation[dropIndex].x * spacing, 0, formation[dropIndex].z * spacing);
            Vector3 dropPos = transform.position + dropOffset;

            Instantiate(GameManager.instance.powerUpPrefab, dropPos + Vector3.up, Quaternion.identity);
        }
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            direction *= -1f;
            transform.position += Vector3.back * descentStep;
        }
    }
}
