using UnityEngine;

public class Grenade : MonoBehaviour
{
    [SerializeField] private float explosionDelay = 2f;
    private float explosionRadius;
    private float explosionForce;
    private float damage;

    public void Initialize(float radius, float force, float dmg)
    {
        explosionRadius = radius;
        explosionForce = force;
        damage = dmg;
        Invoke(nameof(Explode), explosionDelay);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Explode upon hitting any surface
        Explode();
    }

    private void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }

            EnemyHealth enemyHealth = nearbyObject.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }
}
