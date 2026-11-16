using UnityEngine;

public class BulletBehavior : MonoBehaviour
{
    public BulletType type;
    public float bulletLifetime = 3f;
    public float explosionRadius = 2f;
    public LayerMask enemyLayer;

    private bool hasSplit = false; // Prevents recursive or double-triggering

    void Awake()
    {
        Destroy(gameObject, bulletLifetime);
    }

    public void OnImpact(Vector2 hitPosition)
    {
        switch (type)
        {
            case BulletType.Splitter:
                SplitBullet();
                break;
            case BulletType.Shockwave:
                TriggerShockwave(hitPosition);
                break;
            case BulletType.Explosive:
                TriggerExplosion(hitPosition);
                break;
        }
        Destroy(gameObject);
    }

    void SplitBullet()
    {
        if (hasSplit) return;
        hasSplit = true;

        float splitAngle = 20f; // Angle offset for the two split bullets
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        // Get current velocity, or fallback to transform.up if physics hasn't initialized velocity yet
        Vector2 currentVelocity = rb != null && rb.linearVelocity != Vector2.zero ? rb.linearVelocity : transform.up * 10f;
        float currentSpeed = currentVelocity.magnitude;

        // Convert velocity to an angle in degrees
        float baseAngle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;

        // Spawn two split bullets angled relative to the original flight direction
        for (int i = -1; i <= 1; i += 2)
        {
            // Calculate offset angle for left (-20) and right (+20) spreads
            float skewedAngle = baseAngle + (i * splitAngle);
            Quaternion rotation = Quaternion.Euler(0, 0, skewedAngle - 90f); // -90 offset if your bullet sprite faces "up"

            GameObject splitObj = Instantiate(gameObject, transform.position, rotation);

            // Scale down the split bullets to make them look "smaller"
            splitObj.transform.localScale = transform.localScale * 0.7f;

            // Configure the split clone
            BulletBehavior childBehavior = splitObj.GetComponent<BulletBehavior>();
            if (childBehavior != null)
            {
                childBehavior.type = BulletType.Standard; // Downgrade so they don't loop-split infinitely
                childBehavior.hasSplit = true;
            }

            // Apply velocity along the new angled direction while preserving speed
            Rigidbody2D childRb = splitObj.GetComponent<Rigidbody2D>();
            if (childRb != null)
            {
                Vector2 skewedDirection = new Vector2(Mathf.Cos(skewedAngle * Mathf.Deg2Rad), Mathf.Sin(skewedAngle * Mathf.Deg2Rad));
                childRb.linearVelocity = skewedDirection * currentSpeed;
            }
        }
    }

    void TriggerShockwave(Vector2 position)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Projectile"))
            {
                Destroy(hit.gameObject);
            }

            EliteShield shield = hit.GetComponent<EliteShield>();
            if (shield != null)
            {
                EliteEnemy elite = shield.GetComponentInParent<EliteEnemy>();
                if (elite != null) elite.BreakShield();
            }

            EliteEnemy directElite = hit.GetComponent<EliteEnemy>();
            if (directElite != null)
            {
                directElite.BreakShield();
            }
        }
    }

    void TriggerExplosion(Vector2 position)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, explosionRadius);
        foreach (var hit in hits)
        {
            // Do not let explosions trigger on the shield object directly
            if (hit.CompareTag("Shield")) continue;

            if (hit.CompareTag("Enemy"))
            {
                EliteEnemy elite = hit.GetComponent<EliteEnemy>();
                if (elite != null)
                {
                    elite.Die();
                    continue;
                }

                // MidTierEnemy is intentionally omitted here so explosions pass through them safely

                Enemy enemyScript = hit.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.Die();
                    continue;
                }

                Destroy(hit.gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignore shield objects completely so bullets don't treat them like a direct enemy body hit
        if (collision.CompareTag("Shield")) return;

        if (type != BulletType.Shockwave)
        {
            if (collision.CompareTag("Enemy"))
            {
                OnImpact(transform.position);
            }
        }
        else
        {
            if (collision.CompareTag("Enemy"))
            {
                OnImpact(transform.position);
            }
            else if (collision.CompareTag("Projectile"))
            {
                Destroy(collision.gameObject);
            }
        }
    }
}