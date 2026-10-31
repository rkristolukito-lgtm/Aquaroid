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
        Vector2 currentVelocity = rb != null ? rb.linearVelocity : transform.up * 10f;

        // Spawn two split bullets (one angled left, one angled right)
        for (int i = -1; i <= 1; i += 2)
        {
            Quaternion rotation = transform.rotation * Quaternion.Euler(0, 0, i * splitAngle);
            GameObject splitObj = Instantiate(gameObject, transform.position, rotation);

            // Scale down the split bullets to make them look "smaller"
            splitObj.transform.localScale = transform.localScale * 0.7f;

            // Configure the split clone
            BulletBehavior childBehavior = splitObj.GetComponent<BulletBehavior>();
            if (childBehavior != null)
            {
                childBehavior.type = BulletType.Standard; // Downgrade to standard so they don't loop-split infinitely
                childBehavior.hasSplit = true;
            }

            Rigidbody2D childRb = splitObj.GetComponent<Rigidbody2D>();
            if (childRb != null)
            {
                Vector2 skewedDirection = rotation * Vector2.up;
                childRb.linearVelocity = skewedDirection * currentVelocity.magnitude;
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
                Destroy(hit.gameObject); // Clears enemy projectiles[cite: 6]
            }
        }
    }

    void TriggerExplosion(Vector2 position)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Destroy(hit.gameObject); // AOE damage/destroy
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") || collision.CompareTag("Projectile"))
        {
            OnImpact(transform.position);
        }
    }
}