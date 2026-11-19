using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CombatEnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs by Tier")]
    public GameObject[] basicEnemies;     
    public GameObject[] midTierEnemies;   
    public GameObject[] eliteEnemies;     
    public GameObject bossPrefab;         // Keter

    [Header("Spawn Settings")]
    public int targetEnemyCount = 6;
    public float minDistanceFromPlayer = 3.5f; // Minimum safe radius around the player

    void Start()
    {
        StartCoroutine(SpawnWaveRoutine());
    }

    IEnumerator SpawnWaveRoutine()
    {
        if (MapManager.Instance == null)
        {
            Debug.LogWarning("No MapManager found. Creating a temporary default MapManager for standalone testing.");
            GameObject tempManager = new GameObject("MapManager_AutoCreated");
            tempManager.AddComponent<MapManager>();
        }

        int sephira = MapManager.Instance.currentNode != null ? MapManager.Instance.currentNode.sephiraNumber : 10;
        NodeType type = MapManager.Instance.currentNode != null ? MapManager.Instance.currentNode.type : NodeType.Malkhut;

        // 1. Kether Boss Stage
        // 1. Kether Boss Stage
        if (type == NodeType.Keter || sephira == 1)
        {
            if (bossPrefab != null) Instantiate(bossPrefab, transform.position, Quaternion.identity);
            yield break;
        }

        List<GameObject> enemiesToSpawn = new List<GameObject>();

        // 2. Build precise enemy lists based on Sephira number/type
        if (type == NodeType.DaatSecret)
        {
            // Spawn the boss in Da'at (remove this block if Da'at should only spawn the gauntlet below)
            if (bossPrefab != null)
            {
                Instantiate(bossPrefab, transform.position, Quaternion.identity);
            }

            // Da'at secret gauntlet pool
            if (basicEnemies.Length > 0) enemiesToSpawn.Add(basicEnemies[Random.Range(0, basicEnemies.Length)]);
            if (midTierEnemies.Length > 0) enemiesToSpawn.Add(midTierEnemies[Random.Range(0, midTierEnemies.Length)]);
            if (eliteEnemies.Length > 0) enemiesToSpawn.Add(eliteEnemies[Random.Range(0, eliteEnemies.Length)]);
        }
        else
        {
            switch (sephira)
            {
                case 10: // Malkuth: 4 Basics
                    AddRandomEnemies(basicEnemies, 4, enemiesToSpawn);
                    break;
                case 9: // Yesod: 6 Basics
                    AddRandomEnemies(basicEnemies, 6, enemiesToSpawn);
                    break;
                case 8: // Hod: 4 Basics, 2 Mids
                    AddRandomEnemies(basicEnemies, 4, enemiesToSpawn);
                    AddRandomEnemies(midTierEnemies, 2, enemiesToSpawn);
                    break;
                case 7: // Netzach: 2 Basics, 4 Mids
                    AddRandomEnemies(basicEnemies, 2, enemiesToSpawn);
                    AddRandomEnemies(midTierEnemies, 4, enemiesToSpawn);
                    break;
                case 6: // Tiphareth: 6 Mids
                    AddRandomEnemies(midTierEnemies, 6, enemiesToSpawn);
                    break;
                case 5: // Geburah: 4 Basics, 2 Elites
                    AddRandomEnemies(basicEnemies, 4, enemiesToSpawn);
                    AddRandomEnemies(eliteEnemies, 2, enemiesToSpawn);
                    break;
                case 4: // Chesed: 3 Mids, 3 Elites
                    AddRandomEnemies(midTierEnemies, 3, enemiesToSpawn);
                    AddRandomEnemies(eliteEnemies, 3, enemiesToSpawn);
                    break;
                case 3: // Binah: 1 Basic, 1 Mid, 4 Elites
                    AddRandomEnemies(basicEnemies, 1, enemiesToSpawn);
                    AddRandomEnemies(midTierEnemies, 1, enemiesToSpawn);
                    AddRandomEnemies(eliteEnemies, 4, enemiesToSpawn);
                    break;
                case 2: // Chokhmah: 6 Elites
                    AddRandomEnemies(eliteEnemies, 6, enemiesToSpawn);
                    break;
                default:
                    AddRandomEnemies(basicEnemies, 6, enemiesToSpawn);
                    break;
            }
        }

        if (enemiesToSpawn.Count == 0)
        {
            Debug.LogError("Enemy spawn list is empty! Check pool assignments in the Inspector.");
            yield break;
        }

        // Sync target count with the exact generated roster size
        targetEnemyCount = enemiesToSpawn.Count;

        // 3. Staggered Spawning: Spawn initial batch (up to 2), then drip +1 every second
        int spawnedCount = 0;
        int initialSpawnCount = Mathf.Min(2, enemiesToSpawn.Count);

        for (int i = 0; i < initialSpawnCount; i++)
        {
            SpawnEnemyPrefab(enemiesToSpawn[spawnedCount]);
            spawnedCount++;
        }

        while (spawnedCount < enemiesToSpawn.Count)
        {
            yield return new WaitForSeconds(1f);
            SpawnEnemyPrefab(enemiesToSpawn[spawnedCount]);
            spawnedCount++;
        }
    }

    void AddRandomEnemies(GameObject[] sourcePool, int count, List<GameObject> targetList)
    {
        if (sourcePool == null || sourcePool.Length == 0) return;
        for (int i = 0; i < count; i++)
        {
            targetList.Add(sourcePool[Random.Range(0, sourcePool.Length)]);
        }
    }

    void SpawnEnemyPrefab(GameObject prefab)
    {
        Vector3 spawnPos = GetValidSpawnPosition();
        GameObject spawnedEnemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        
        Enemy enemyScript = spawnedEnemy.GetComponent<Enemy>();
        if (enemyScript != null && GameManager.Instance != null)
        {
            enemyScript.gameManager = GameManager.Instance;
        }
    }

    Vector3 GetValidSpawnPosition()
    {
        Vector3 spawnPos = Vector3.zero;
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;

        int safetyAttempts = 0;
        bool positionFound = false;

        // Define strict boundary limits based on your arena walls (inside the box)
        float minX = -7.5f;
        float maxX = 7.5f;
        float minY = -4.0f;
        float maxY = 4.0f;

        while (!positionFound && safetyAttempts < 15)
        {
            safetyAttempts++;

            // Generate a random position strictly within the arena bounds
            float randomX = Random.Range(minX, maxX);
            float randomY = Random.Range(minY, maxY);
            spawnPos = new Vector3(randomX, randomY, 0f);

            // If player exists, ensure the enemy isn't spawning right on top of them
            if (player != null)
            {
                float distanceToPlayer = Vector2.Distance(spawnPos, player.position);
                if (distanceToPlayer >= minDistanceFromPlayer)
                {
                    positionFound = true;
                }
            }
            else
            {
                positionFound = true; // Fallback if player tag isn't set yet
            }
        }

        return spawnPos;
    }
}