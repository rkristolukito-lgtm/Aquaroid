using UnityEngine;

public class EliteShield : MonoBehaviour
{
    private EliteEnemy ownerElite;

    public void Initialize(EliteEnemy elite)
    {
        ownerElite = elite;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Bullet")) return;

        BulletBehavior bullet = collision.GetComponent<BulletBehavior>();
        if (bullet != null)
        {
            if (bullet.type == BulletType.Shockwave)
            {
                if (ownerElite != null)
                {
                    ownerElite.BreakShield();
                }
                bullet.OnImpact(collision.transform.position);
                Destroy(collision.gameObject);
                return;
            }
        }

        // Standard bullets: destroy the bullet, do NOT touch the elite enemy!
        Destroy(collision.gameObject);
    }
}