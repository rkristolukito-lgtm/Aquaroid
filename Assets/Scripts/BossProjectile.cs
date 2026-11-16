using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 lastVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, 4f); // Auto cleanup after 4 seconds
    }

    void Update()
    {
        lastVelocity = rb.linearVelocity;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Reflect velocity cleanly off walls
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.layer == LayerMask.NameToLayer("Walls"))
        {
            Vector2 normal = collision.contacts[0].normal;
            Vector2 reflected = Vector2.Reflect(lastVelocity, normal);
            rb.linearVelocity = reflected;
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            // Deal damage to player and destroy bullet
            // collision.gameObject.GetComponent<PlayerController>()?.TakeDamage();
            Destroy(gameObject);
        }
    }
}