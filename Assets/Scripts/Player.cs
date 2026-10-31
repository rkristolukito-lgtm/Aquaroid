using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement & Ship Stats")]
    public float acceleration = 10f;
    public float maxSpeed = 10f;
    public float rotationSpeed = 10f;
    public float bulletSpeed = 8f;

    [Header("References")]
    public Transform BulletSpawn;
    public Rigidbody2D bulletPrefab;
    public ParticleSystem destroyedParticles;

    [Header("Firing Cooldown")]
    private float fireCooldown = 0.35f;
    private float lastFireTime = 0f;

    private Rigidbody2D rb;
    private InputSystem_Actions inputActions; // Matches your asset's generated class name

    private bool alive = true;
    private Vector2 moveInput;

    private float lastDamageTime = 0f;
    private float damageCooldown = 0.5f;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        inputActions.Player.Enable(); // Uses the "Player" action map from your JSON[cite: 9]
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!alive) return;

        // Reads movement from WASD or Arrow Keys simultaneously without ghosting limits[cite: 9]
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        // Check if Attack/Shoot is pressed
        if (inputActions.Player.Attack.IsPressed())
        {
            HandleShooting();
        }
    }

    void FixedUpdate()
    {
        if (!alive) return;

        if (moveInput.sqrMagnitude > 0.01f)
        {
            // Move the ship directly in the direction of the joystick[cite: 9]
            Vector2 movement = new Vector2(moveInput.x, moveInput.y);
            rb.AddForce(movement * acceleration);
            rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);

            // Rotate the ship to face the direction you are moving[cite: 9]
            float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void HandleShooting()
    {
        // Check if enough time has passed since the last shot
        if (Time.time < lastFireTime + fireCooldown) return;

        // Pull the next bullet from the 6-slot chamber inventory
        BulletData nextBulletData;
        if (BulletInventory.Instance != null && BulletInventory.Instance.GetNextBullet(out nextBulletData))
        {
            lastFireTime = Time.time; // Record the time we fired

            // Instantiate the specific bullet type's prefab
            GameObject prefabToSpawn = nextBulletData.bulletPrefab != null ? nextBulletData.bulletPrefab : bulletPrefab.gameObject;
            
            Rigidbody2D bullet = Instantiate(prefabToSpawn.GetComponent<Rigidbody2D>(), BulletSpawn.position, Quaternion.identity);
            Vector2 shipVelocity = rb.linearVelocity;
            Vector2 shipDirection = transform.up;
            float shipSpeed = Mathf.Max(0, Vector2.Dot(shipVelocity, shipDirection));
            
            bullet.linearVelocity = shipDirection * shipSpeed;
            bullet.AddForce(bulletSpeed * transform.up, ForceMode2D.Impulse);

            // Configure bullet behavior type if script exists
            BulletBehavior behavior = bullet.GetComponent<BulletBehavior>();
            if (behavior != null)
            {
                behavior.type = nextBulletData.type;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Asteroid") || collision.CompareTag("Projectile") || collision.CompareTag("Enemy"))
        {
            // If we are still in invulnerability cooldown after a hit, ignore
            if (Time.time < lastDamageTime + damageCooldown) return;

            GameManager gameManager = FindAnyObjectByType<GameManager>();
            if (gameManager != null)
            {
                lastDamageTime = Time.time;
                gameManager.LoseLife();

                // Optional: Play a hit particle effect without destroying the ship
                if (destroyedParticles != null)
                {
                    Instantiate(destroyedParticles, transform.position, Quaternion.identity);
                }

                // If health hits 0, GameManager will trigger GameOver, so we can destroy the ship then
                if (gameManager.playerLives <= 0)
                {
                    alive = false;
                    Destroy(gameObject);
                }
            }
        }
    }
}