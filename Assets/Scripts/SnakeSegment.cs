using System.Collections;
using UnityEngine;

public class SnakeSegment : MonoBehaviour
{
    public int health = 3;
    public int segmentIndex; // 0 is head, increments down the body
    public SnakeBossController bossController;

    public GameObject bossProjectilePrefab;
    public float fireRate = 4f;

    void Start()
    {
        StartCoroutine(SideFireLoop());
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    IEnumerator SideFireLoop()
    {
        while (health > 0)
        {
            yield return new WaitForSeconds(fireRate);
            FireSideCannons();
        }
    }

    void FireSideCannons()
    {
        if (bossProjectilePrefab == null) return;

        // Fixed: Use transform.right directly instead of Vector2.transform.right
        Vector2 leftPos = (Vector2)transform.position - ((Vector2)transform.right * 0.8f);
        Vector2 rightPos = (Vector2)transform.position + ((Vector2)transform.right * 0.8f);

        GameObject leftProj = Instantiate(bossProjectilePrefab, leftPos, transform.rotation);
        GameObject rightProj = Instantiate(bossProjectilePrefab, rightPos, transform.rotation);

        Rigidbody2D rbL = leftProj.GetComponent<Rigidbody2D>();
        Rigidbody2D rbR = rightProj.GetComponent<Rigidbody2D>();

        if (rbL != null) rbL.linearVelocity = -transform.right * 5f;
        if (rbR != null) rbR.linearVelocity = transform.right * 5f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if hit by the player's bullet[cite: 13]
        if (collision.CompareTag("Bullet"))
        {
            TakeDamage(1); // Take 1 damage per bullet hit[cite: 13]
            Destroy(collision.gameObject); // Destroy the player's bullet[cite: 13]
        }
    }

    void Die()
    {
        if (bossController != null)
        {
            bossController.OnSegmentDestroyed(this); //[cite: 13]
        }
        Destroy(gameObject); //[cite: 13]
    }
}