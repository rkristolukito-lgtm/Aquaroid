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

        // --- YOUR EXISTING MOVEMENT CODE ---
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

        // --- YOUR EXISTING SHOOTING CODE ---
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
            
            if (gameManager != null)
            {
                gameManager.enemyCount--; // Simply decrease by 1
                Debug.Log($"Enemy destroyed. Remaining enemies: {gameManager.enemyCount}");

                // Trigger victory the exact moment it hits 0
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
}