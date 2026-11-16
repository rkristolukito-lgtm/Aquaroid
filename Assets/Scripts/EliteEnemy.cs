using System.Collections;
using UnityEngine;

public class EliteEnemy : MonoBehaviour
{
    public int health = 1; // Dies in a single hit
    public GameManager gameManager;

    public GameObject shieldPrefab; // Drag your FrontalShield prefab here in the Inspector
    private GameObject spawnedShield;
    public bool hasFrontalShield = true;

    public GameObject enemyProjectilePrefab;
    public float turretFireRate = 1f;

    private Transform player;
    private Rigidbody2D rb;
    private bool isDead = false;
    private bool isShieldBroken = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic; // Locks the turret in place
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        // Spawn the shield dynamically and parent it to this elite enemy so it tracks rotation and position
        if (shieldPrefab != null)
        {
            spawnedShield = Instantiate(shieldPrefab, transform.position, transform.rotation, transform);
            spawnedShield.transform.localPosition = new Vector3(0f, 0.5f, 0f);

            EliteShield shieldScript = spawnedShield.GetComponent<EliteShield>();
            if (shieldScript != null)
            {
                shieldScript.Initialize(this);
            }
        }

        SetShieldActive(true);
        StartCoroutine(TurretShootLoop());
    }

    void Update()
    {
        if (player == null || isDead) return;

        // Rotate to face the player
        Vector2 direction = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    IEnumerator TurretShootLoop()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(turretFireRate);
            ShootAtPlayer();
        }
    }

    void SetShieldActive(bool active)
    {
        hasFrontalShield = active && !isShieldBroken;
        if (spawnedShield != null)
        {
            spawnedShield.SetActive(hasFrontalShield);
        }
    }

    public void BreakShield()
    {
        if (isShieldBroken || !hasFrontalShield) return;

        isShieldBroken = true;
        SetShieldActive(false);
    }

    void ShootAtPlayer()
    {
        if (player == null || enemyProjectilePrefab == null || isDead) return;

        Instantiate(enemyProjectilePrefab, transform.position, transform.rotation);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.CompareTag("Bullet"))
        {
            // If the shield is active, check if the bullet hit from the front half of the enemy.
            // If it did, ignore it completely here so the shield handles it.
            if (hasFrontalShield)
            {
                Vector2 toBullet = ((Vector2)collision.transform.position - (Vector2)transform.position).normalized;
                // transform.up points toward the front of the turret
                if (Vector2.Dot(transform.up, toBullet) > 0f)
                {
                    return; 
                }
            }

            // Only die if the shield is broken or the bullet came from behind/flanked
            Destroy(collision.gameObject);
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (gameManager != null)
        {
            gameManager.enemyCount--;

            if (gameManager.enemyCount <= 0)
            {
                gameManager.NotifyEnemyDefeated();
            }
        }

        Destroy(gameObject);
    }
}