using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CombatEnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs by Tier")]
    public GameObject[] basicEnemies;     // Malkuth & Yesod
    public GameObject[] midTierEnemies;   // Hod, Netzach, Tiphareth
    public GameObject[] eliteEnemies;     // Geburah, Chesed, Binah, Chokhmah
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
        if (type == NodeType.Keter)
        {
            if (bossPrefab != null) Instantiate(bossPrefab, transform.position, Quaternion.identity);
            yield break;
        }

        // 2. Select the correct pool based on Sephira tier
        GameObject[] activePool = basicEnemies;
        if (type == NodeType.DaatSecret)
        {
            // Daat secret gauntlet pool
            List<GameObject> daatList = new List<GameObject>();
            if (basicEnemies.Length > 0) daatList.Add(basicEnemies[Random.Range(0, basicEnemies.Length)]);
            if (midTierEnemies.Length > 0) daatList.Add(midTierEnemies[Random.Range(0, midTierEnemies.Length)]);
            if (eliteEnemies.Length > 0) daatList.Add(eliteEnemies[Random.Range(0, eliteEnemies.Length)]);
            activePool = daatList.ToArray();
        }
        else if (sephira >= 9) activePool = basicEnemies;
        else if (sephira >= 6) activePool = midTierEnemies;
        else activePool = eliteEnemies;

        if (activePool == null || activePool.Length == 0)
        {
            Debug.LogError("Enemy pool is empty! Assign prefabs in the Inspector.");
            yield break;
        }

        // 3. Staggered Spawning: 2 on start, then +1 every second up to target (6)
        int spawnedCount = 0;

        // Spawn initial 2 enemies immediately
        for (int i = 0; i < 2; i++)
        {
            SpawnSingleEnemy(activePool);
            spawnedCount++;
        }

        // Drip 1 enemy per second until we hit targetEnemyCount (6)
        while (spawnedCount < targetEnemyCount)
        {
            yield return new WaitForSeconds(1f);
            SpawnSingleEnemy(activePool);
            spawnedCount++;
        }
    }

    void SpawnSingleEnemy(GameObject[] enemyPool)
    {
        GameObject randomPrefab = enemyPool[Random.Range(0, enemyPool.Length)];
        Vector3 spawnPos = GetValidSpawnPosition();

        GameObject spawnedEnemy = Instantiate(randomPrefab, spawnPos, Quaternion.identity);
        
        // Link back to GameManager active count
        Enemy enemyScript = spawnedEnemy.GetComponent<Enemy>();
        if (enemyScript != null && GameManager.Instance != null)
        {
            enemyScript.gameManager = GameManager.Instance;
        }
    }

    Vector3 GetValidSpawnPosition()
    {
        Vector3 spawnPos = transform.position;
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;

        int safetyAttempts = 0;
        bool positionFound = false;

        while (!positionFound && safetyAttempts < 15)
        {
            safetyAttempts++;

            // Pick a random point near the edges of the camera viewport or offset area
            Vector2 randomOffset = new Vector2(Random.Range(-6f, 6f), Random.Range(-4f, 4f));
            spawnPos = transform.position + (Vector3)randomOffset;

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