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
            Instantiate(projectile, transform.position, Quaternion.identity);
            timeBtwShots = startTimeBtwShots;
        }
        else
        {
            timeBtwShots -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            Destroy(collision.gameObject);
            Die(); // Use shared death logic
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
