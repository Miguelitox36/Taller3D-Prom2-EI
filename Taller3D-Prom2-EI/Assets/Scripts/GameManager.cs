using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject enemy2Prefab;
    public GameObject tilePrefab;
    public GameObject wallPrefab;

    public int level = 1;
    private int enemiesRemaining;
    private int mapSize;

    private Transform player;
    private bool mapGenerated = false;

    private List<int> waveOrder = new List<int>();
    private int currentWaveIndex = 0;

    private EnemyFormationManager efm;

    private HashSet<Enemy> aliveEnemies = new HashSet<Enemy>();

    private int enemyType2KilledInWave2 = 0;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        StartLevel();
    }

    void StartLevel()
    {
        mapSize = 15 + level * 2;

        if (!mapGenerated)
        {
            GenerateMap(mapSize);
            mapGenerated = true;
        }

        SpawnPlayer();
        SetupEnemyWaves();
        enemyType2KilledInWave2 = 0;
    }

    public int GetMapSize() => mapSize;

    void GenerateMap(int size)
    {
        GameObject tile = Instantiate(tilePrefab, new Vector3(0, -2f, 0), Quaternion.identity);
        tile.tag = "Tile";
        tile.transform.localScale = new Vector3((size + 1) * 2, 1f, (size + 1) * 2);

        float wallHeight = 5f;
        float wallOffset = (size + 1);
        float wallZ = 0;

        GameObject leftWall = Instantiate(wallPrefab, new Vector3(-wallOffset, wallHeight / 2f, wallZ), Quaternion.identity);
        GameObject rightWall = Instantiate(wallPrefab, new Vector3(wallOffset, wallHeight / 2f, wallZ), Quaternion.identity);
        GameObject topWall = Instantiate(wallPrefab, new Vector3(0, wallHeight / 2f, wallOffset), Quaternion.identity);
        GameObject bottomWall = Instantiate(wallPrefab, new Vector3(0, wallHeight / 2f, -wallOffset), Quaternion.identity);

        leftWall.name = "LeftWall";
        rightWall.name = "RightWall";
        topWall.name = "TopWall";
        bottomWall.name = "BottomWall";

        leftWall.transform.localScale = new Vector3(1f, wallHeight, (size + 1) * 5);
        rightWall.transform.localScale = new Vector3(1f, wallHeight, (size + 1) * 5);
        topWall.transform.localScale = new Vector3((size + 1) * 2, wallHeight, 5f);
        bottomWall.transform.localScale = new Vector3((size + 1) * 2, wallHeight, 5f);

        leftWall.tag = rightWall.tag = topWall.tag = bottomWall.tag = "Wall";

        bottomWall.GetComponent<BoxCollider>().isTrigger = true;
    }

    void SpawnPlayer()
    {
        Vector3 startPos = new Vector3(0, 0.6f, -10);
        if (player == null)
        {
            GameObject newPlayer = Instantiate(playerPrefab, startPos, Quaternion.identity);
            player = newPlayer.transform;
            Camera.main.GetComponent<CameraFollow>()?.SetTarget(player);
        }
        else
        {
            player.position = startPos;
        }
    }

    void SetupEnemyWaves()
    {
        waveOrder.Clear();
        waveOrder.AddRange(new List<int> { 0, 1, 2 });
        currentWaveIndex = 0;

        if (efm == null)
        {
            GameObject formationManagerGO = new GameObject("EnemyFormationManager");
            formationManagerGO.transform.position = new Vector3(0, 0.6f, mapSize - 3);
            efm = formationManagerGO.AddComponent<EnemyFormationManager>();
            efm.enemyPrefab = enemyPrefab;
            efm.enemy2Prefab = enemy2Prefab;

            var collider = formationManagerGO.AddComponent<BoxCollider>();
            collider.size = new Vector3(mapSize * 2f, 1f, 1f);
            collider.isTrigger = false;

            var rb = formationManagerGO.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        SpawnNextWave();
    }

    public void SpawnNextWave()
    {
        if (currentWaveIndex < waveOrder.Count)
        {
            efm.transform.position = new Vector3(0, 0.6f, mapSize - 3);
            int waveId = waveOrder[currentWaveIndex];
            currentWaveIndex++;
            efm.SpawnWave(waveId);

            if (waveId == 1) // Segunda oleada (índice 1)
            {
                enemyType2KilledInWave2 = 0;
            }
        }
        else
        {
            GameOver();
        }
    }

    public void RegisterEnemies(List<Enemy> enemyList)
    {
        aliveEnemies.Clear();
        foreach (Enemy e in enemyList)
        {
            if (e != null)
                aliveEnemies.Add(e);
        }
        enemiesRemaining = aliveEnemies.Count;
    }

    public void EnemyKilled(Enemy enemy)
    {
        if (aliveEnemies.Contains(enemy))
        {
            aliveEnemies.Remove(enemy);
            enemiesRemaining = Mathf.Max(0, enemiesRemaining - 1);

            if (currentWaveIndex == 2 && enemy is EnemyType2)
            {
                enemyType2KilledInWave2++;
                if (enemyType2KilledInWave2 == 4)
                {
                    if (enemy.powerUpPrefab != null)
                    {
                        Instantiate(enemy.powerUpPrefab, enemy.transform.position + Vector3.up, Quaternion.identity);
                    }
                }
            }

            if (enemiesRemaining <= 0)
            {
                Invoke(nameof(SpawnNextWave), 2f);
            }
        }
    }

    void GameOver()
    {
        Debug.Log("¡Ganaste el juego!");
    }
}
