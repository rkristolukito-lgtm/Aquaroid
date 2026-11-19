using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    public float stoppingDistance;
    public float retreatDistance;
    private float timeBtwShots;
    public float startTimeBtwShots;
    public GameObject projectile;
    public ParticleSystem destroyedParticles;
    private Transform player;
    public GameManager gameManager;
    private bool isDead = false;
    public int health = 1;

    [Header("Facing Settings")]
    public float turnSpeed = 360f;
    public float angleOffset = -90f; // -90f if sprite nose faces +Y (up), 0f if +X (right)

    void Start()
    {
        // Find the player safely
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        timeBtwShots = startTimeBtwShots;

        // Automatically find the GameManager if it's not assigned yet
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
    }

    void Update()
    {
        // If the player has been destroyed (died), stop running the enemy AI
        if (player == null) return;

        FacePlayer();

        // --- MOVEMENT CODE ---
        if (Vector2.Distance(transform.position, player.position) > stoppingDistance)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }
        else if (Vector2.Distance(transform.position, player.position) < stoppingDistance && Vector2.Distance(transform.position, player.position) > retreatDistance)
        {
            transform.position = this.transform.position;
        }
        else if (Vector2.Distance(transform.position, player.position) < retreatDistance)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, -speed * Time.deltaTime);
        }

        // --- SHOOTING CODE ---
        if (timeBtwShots <= 0)
        {
            Instantiate(projectile, transform.position, transform.rotation);
            timeBtwShots = startTimeBtwShots;
        }
        else
        {
            timeBtwShots -= Time.deltaTime;
        }
    }

    void FacePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        if (direction.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + angleOffset;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            Destroy(collision.gameObject);
            Die(); // Use shared death logic
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    // Public method so Explosive bullets (or other systems) can cleanly kill the enemy and update the count
    public void Die()
    {
        if (isDead) return; // Prevent double-counting if multiple bullets hit on the exact same frame
        isDead = true;

        if (gameManager != null)
        {
            gameManager.enemyCount--;
            Debug.Log($"Enemy destroyed. Remaining enemies: {gameManager.enemyCount}");

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