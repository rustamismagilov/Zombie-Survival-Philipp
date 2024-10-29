using UnityEngine;

public class Grenade : MonoBehaviour
{
    [SerializeField] private float explosionDelay = 2f;
    private float explosionRadius;
    private float explosionForce;
    private float damage;
    private Vector3 previousPosition;

    void Start()
    {
        previousPosition = transform.position;
    }

    void Update()
    {
        Debug.DrawLine(previousPosition, transform.position, Color.green, 1f);
        previousPosition = transform.position;
    }

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
        if (collision.collider != null)
        {
            Explode();
        }
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
