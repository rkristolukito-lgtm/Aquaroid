using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MidTierEnemy : MonoBehaviour
{
    [Header("Stats & Movement Physics")]
    public int health = 2; // Takes 2 hits to defeat
    public float passiveDriftSpeed = 3f; 
    public float ramSpeed = 10f;
    public float ramTriggerDistance = 6f;
    public float ramCooldown = 3f;

    [Header("Visuals & Effects")]
    public Color damagedColor = Color.red; 
    public ParticleSystem destroyedParticles;
    public GameManager gameManager;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float ramTimer;
    private bool isRamming = false;
    private Vector2 cachedRamDirection;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        ramTimer = ramCooldown;

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        rb.linearVelocity = randomDir * passiveDriftSpeed;
    }

    void Update()
    {
        if (player == null || isDead) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!isRamming)
        {
            if (ramTimer > 0)
            {
                ramTimer -= Time.deltaTime;
            }
            else if (distanceToPlayer <= ramTriggerDistance)
            {
                StartCoroutine(ExecuteRamAttack());
            }
        }
    }

    void FixedUpdate()
    {
        if (player == null || isDead || rb == null) return;

        if (!isRamming)
        {
            if (rb.linearVelocity.sqrMagnitude > 0.1f)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * passiveDriftSpeed;
            }
            else
            {
                rb.linearVelocity = Random.insideUnitCircle.normalized * passiveDriftSpeed;
            }

            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            rb.linearVelocity = cachedRamDirection * ramSpeed;

            float angle = Mathf.Atan2(cachedRamDirection.y, cachedRamDirection.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    IEnumerator ExecuteRamAttack()
    {
        isRamming = true;
        
        if (player != null)
        {
            cachedRamDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
        }
        else
        {
            cachedRamDirection = Vector2.down;
        }

        yield return new WaitForSeconds(0.8f);

        isRamming = false;
        ramTimer = ramCooldown; 
        
        rb.linearVelocity = cachedRamDirection * passiveDriftSpeed;
    }

    // Catches Trigger objects like Bullets
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.CompareTag("Bullet"))
        {
            Destroy(collision.gameObject);
            TakeDamage(1);
        }
    }

    // Catches Physical collisions like the Player
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            GameManager gm = gameManager != null ? gameManager : FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                gm.LoseLife();
            }

            isRamming = false;
            ramTimer = ramCooldown;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        health -= damageAmount;
        Debug.Log($"Mid-tier enemy took damage. Remaining health: {health}");

        if (health == 1 && spriteRenderer != null)
        {
            spriteRenderer.color = damagedColor;
        }

        if (health <= 0)
        {
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
            Debug.Log($"Mid-tier enemy destroyed. Remaining enemies: {gameManager.enemyCount}");

            if (gameManager.enemyCount <= 0)
            {
                gameManager.NotifyEnemyDefeated();
            }
        }

        if (destroyedParticles != null)
        {
            Instantiate(destroyedParticles, transform.position, Quaternion.identity);
        }
        
        Destroy(gameObject);
    }
}