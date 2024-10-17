using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [SerializeField] float chaseRange = 20f;
    public float soundReactionRange = 10f;
    [SerializeField] float turnSpeed = 5f;
    [SerializeField] float investigationTime = 5f; // How long zombie will investigate the impact

    [SerializeField] float viewRadius = 21f; // Field of view radius.
    [SerializeField] float viewAngle = 90f;  // Field of view angle (e.g., 90 degrees).

    NavMeshAgent navMeshAgent;
    EnemyHealth enemyHealth;
    Transform target;
    float distanceToTarget = Mathf.Infinity;
    bool isProvoked = false;
    bool isInvestigating = false;
    Vector3 investigationPoint;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyHealth = GetComponent<EnemyHealth>();
        target = FindObjectOfType<PlayerHealth>().transform;
    }

    void Update()
    {
        if (enemyHealth.IsDead())
        {
            enemyHealth.Die();
            return;
        }

        distanceToTarget = Vector3.Distance(target.position, transform.position);
        PlayerController playerController = target.GetComponent<PlayerController>();

        // If the zombie is investigating a bullet impact, continue that first
        if (isInvestigating)
        {
            InvestigateImpact();
        }
        else if (PlayerInFieldOfView()) // If the player is within the viewAngle
        {
            // Engage if the player is within the viewAngle, no matter if standing, moving, or crouching
            isProvoked = true;
        }
        else if (distanceToTarget <= viewRadius) // If player is within viewRadius but NOT in viewAngle
        {
            if (!playerController.isCrouching && playerController.IsMoving)
            {
                // Engage if the player is moving and not crouching in the viewRadius (but outside the viewAngle)
                isProvoked = true;
            }
            else
            {
                // Ignore if the player is standing still or crouching outside the viewAngle
                isProvoked = false;
            }
        }
        else
        {
            // Player is outside viewRadius and not in viewAngle
            isProvoked = false;
        }

        // Engage the player if provoked
        if (isProvoked)
        {
            EngageTarget();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, soundReactionRange); // Visualize sound radius


        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);

        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);
    }

    public void OnDamageTaken()
    {
        isProvoked = true; // Zombie gets provoked when taking damage
        StopAllCoroutines(); // Stop any ongoing investigation
        ChaseTarget(); // Engage the player

        // Alert nearby zombies when this zombie is hit
        AlertNearbyZombies();
    }

    void AlertNearbyZombies()
    {
        Collider[] nearbyZombies = Physics.OverlapSphere(transform.position, soundReactionRange);
        foreach (Collider nearbyObject in nearbyZombies)
        {
            EnemyController nearbyZombie = nearbyObject.GetComponent<EnemyController>();
            if (nearbyZombie != null && !nearbyZombie.isProvoked)
            {
                nearbyZombie.isProvoked = true;
            }
        }
    }

    public void Investigate(Vector3 impactPoint)
    {
        if (isProvoked) return; // If already provoked, don't investigate

        StopAllCoroutines(); // Stop any ongoing investigations
        investigationPoint = impactPoint;
        isInvestigating = true;

        navMeshAgent.SetDestination(investigationPoint);
        GetComponent<Animator>().SetBool("isMoving", true); // Start Move animation
    }

    void InvestigateImpact()
    {
        // If the zombie has reached the investigation point
        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f)
            {
                // Stop moving and play Idle animation
                GetComponent<Animator>().SetBool("isMoving", false); // Stop Move animation
                StartCoroutine(StopInvestigation());
            }
        }
    }

    IEnumerator StopInvestigation()
    {
        // Wait at the impact point for a bit
        yield return new WaitForSeconds(investigationTime);

        isInvestigating = false;

        // Ensure the NavMeshAgent is stopped
        navMeshAgent.ResetPath();

        // The zombie remains idle after investigation
        GetComponent<Animator>().SetBool("isMoving", false); // Ensure Move animation is stopped
    }

    void EngageTarget()
    {
        FaceTarget();

        if (distanceToTarget >= navMeshAgent.stoppingDistance && navMeshAgent.enabled)
        {
            ChaseTarget();
        }
        else if (distanceToTarget <= navMeshAgent.stoppingDistance)
        {
            AttackTarget();
        }
    }

    void ChaseTarget()
    {
        if (!navMeshAgent.enabled) return;
        GetComponent<Animator>().SetBool("isMoving", true); // Start Move animation
        navMeshAgent.SetDestination(target.position);
    }

    void AttackTarget()
    {
        GetComponent<Animator>().SetBool("isMoving", false); // Stop Move animation
        GetComponent<Animator>().SetTrigger("Attack");
    }


    bool PlayerInFieldOfView()
    {
        // Perform an overlap sphere to find all colliders within the view radius
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius);

        foreach (Collider targetCollider in targetsInViewRadius)
        {
            // Check if the collider has the "Player" tag
            if (targetCollider.CompareTag("Player"))
            {
                Transform targetTransform = targetCollider.transform;
                Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;

                // Calculate the angle between the zombie's forward direction and the direction to the target
                float angleBetweenZombieAndTarget = Vector3.Angle(transform.forward, directionToTarget);

                if (angleBetweenZombieAndTarget < viewAngle / 2f)
                {
                    // Optional: Perform a Linecast to check for obstacles between the zombie and the player
                    if (!Physics.Linecast(transform.position, targetTransform.position, out RaycastHit hit))
                    {
                        // The player is within the cone of vision and there are no obstacles
                        return true;
                    }
                    else if (hit.transform.CompareTag("Player"))
                    {
                        // The Linecast hit the player directly
                        return true;
                    }
                }
            }
        }
        return false;
    }

    void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
    }

    Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
