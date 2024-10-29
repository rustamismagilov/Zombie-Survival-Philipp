using UnityEngine;

public class Grenade : MonoBehaviour
{
    private float explosionRadius;
    private float explosionForce;
    private float damage;
    private Vector3 previousPosition;

    private bool hasExploded = false; // Add a flag to track if grenade has exploded

    [SerializeField] private ParticleSystem explosionEffect; // Explosion particle system

    void Start()
    {
        previousPosition = transform.position;
    }

    void Update()
    {
        Debug.DrawLine(previousPosition, transform.position, Color.green, 10f);
        previousPosition = transform.position;
    }

    public void Initialize(float radius, float force, float dmg, float delay)
    {
        explosionRadius = radius;
        explosionForce = force;
        damage = dmg;
        Invoke(nameof(Explode), delay);
    }

    private void Explode()
    {
        if (hasExploded) return; // Ensure Explode is not called more than once

        hasExploded = true;

        // Play explosion particle effect
        if (explosionEffect != null)
        {
            ParticleSystem instantiatedEffect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            instantiatedEffect.Play();
            Destroy(instantiatedEffect.gameObject, instantiatedEffect.main.duration); // Destroy particle system after it finishes
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                float upwardsModifier = 1.0f;
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardsModifier, ForceMode.Impulse);
            }

            EnemyHealth enemyHealth = nearbyObject.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
        }

        Destroy(gameObject); // Immediately destroy grenade after explosion
    }

    void OnDrawGizmos()
    {
        // Visualize the explosion point with a red sphere
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
