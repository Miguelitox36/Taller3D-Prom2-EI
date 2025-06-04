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
    private bool powerUpDropped = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartLevel();
    }

    void StartLevel()
    {
        mapSize = 20 + level * 3;

        if (!mapGenerated)
        {
            GenerateMap(mapSize);
            mapGenerated = true;
        }

        SpawnPlayer();
        SetupEnemyWaves();
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

        leftWall.tag = rightWall.tag = topWall.tag = "Wall";
        bottomWall.tag = "BottomWall";
                
        BoxCollider bc = bottomWall.GetComponent<BoxCollider>();
        if (bc == null) bc = bottomWall.AddComponent<BoxCollider>();
        bc.isTrigger = true;
    }

    void SpawnPlayer()
    {
        Vector3 startPos = new Vector3(0, 0.6f, -mapSize + 5);
        if (player == null)
        {
            GameObject newPlayer = Instantiate(playerPrefab, startPos, Quaternion.identity);
            player = newPlayer.transform;
            CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
            if (camFollow != null)
            {
                camFollow.SetTarget(player);
            }
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
            formationManagerGO.transform.position = new Vector3(0, 0.6f, mapSize - 5);
            efm = formationManagerGO.AddComponent<EnemyFormationManager>();
            efm.enemyPrefab = enemyPrefab;
            efm.enemy2Prefab = enemy2Prefab;
        }

        SpawnNextWave();
    }

    public void SpawnNextWave()
    {
        if (currentWaveIndex < waveOrder.Count)
        {
            efm.transform.position = new Vector3(0, 0.6f, mapSize - 5);
            int waveId = waveOrder[currentWaveIndex];
            currentWaveIndex++;
            efm.SpawnWave(waveId);
                        
            if (waveId == 1)
            {
                enemyType2KilledInWave2 = 0;
                powerUpDropped = false;
            }

            Debug.Log($"Spawneando oleada {waveId + 1} de {waveOrder.Count}");
        }
        else
        {
            GameWon();
        }
    }

    public void RegisterEnemies(List<Enemy> enemyList)
    {
        aliveEnemies.Clear();
        foreach (Enemy e in enemyList)
        {
            if (e != null && e.gameObject != null)
            {
                aliveEnemies.Add(e);
            }
        }
        enemiesRemaining = aliveEnemies.Count;
        Debug.Log($"Oleada iniciada con {enemiesRemaining} enemigos");
    }

    public void EnemyKilled(Enemy enemy)
    {
        if (enemy == null) return;

        if (aliveEnemies.Contains(enemy))
        {
            aliveEnemies.Remove(enemy);
            enemiesRemaining = Mathf.Max(0, enemiesRemaining - 1);
                        
            if (currentWaveIndex == 2 && enemy is EnemyType2 && !powerUpDropped)
            {
                enemyType2KilledInWave2++;
                Debug.Log($"EnemyType2 eliminado: {enemyType2KilledInWave2}/4");

                if (enemyType2KilledInWave2 == 4 && enemy.powerUpPrefab != null)
                {
                    
                    Vector3 powerUpPos = enemy.transform.position;
                    powerUpPos.y = 2f; 
                    Instantiate(enemy.powerUpPrefab, powerUpPos, Quaternion.identity);
                    powerUpDropped = true;
                    Debug.Log("¡PowerUp generado en posición: " + powerUpPos);
                }
            }

            Debug.Log($"Enemigo eliminado. Quedan: {enemiesRemaining}");

            if (enemiesRemaining <= 0)
            {
                Debug.Log("Oleada completada. Siguiente oleada en 2 segundos...");
                Invoke(nameof(SpawnNextWave), 2f);
            }
        }
    }

    public void EnemyReachedBottom(Enemy enemy)
    {
        if (enemy == null) return;

        if (aliveEnemies.Contains(enemy))
        {
            aliveEnemies.Remove(enemy);
            Destroy(enemy.gameObject);

            enemiesRemaining = Mathf.Max(0, enemiesRemaining - 1);
            Debug.Log($"Enemigo llegó al fondo y fue destruido. Quedan: {enemiesRemaining}");

            if (enemiesRemaining <= 0)
            {
                Debug.Log("Todos los enemigos llegaron al fondo. Siguiente oleada en 2 segundos...");
                Invoke(nameof(SpawnNextWave), 2f);
            }
        }
    }

    public void AllEnemiesReachedBottom()
    {
        Debug.Log("Toda la formación llegó al fondo. Pasando a la siguiente oleada...");
        enemiesRemaining = 0;
        aliveEnemies.Clear();
        Invoke(nameof(SpawnNextWave), 1f);
    }

    void GameWon()
    {
        Debug.Log("¡Felicitaciones! ¡Has completado todas las oleadas y ganaste el juego!");        
    }

    void GameOver()
    {
        Debug.Log("Game Over! Los enemigos han llegado al jugador.");        
    }
}