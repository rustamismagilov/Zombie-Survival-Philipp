using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Chasing,
        Attacking,
        Investigating
    }

    private EnemyState currentState = EnemyState.Idle;

    [SerializeField] float chaseRange = 10f;
    [SerializeField] float turnSpeed = 5f;

    [Header("Field of View Settings")]
    [SerializeField] float viewRadius = 15;
    [Range(0, 360)]
    [SerializeField] float viewAngle = 110;

    [SerializeField] LayerMask targetMask;
    [SerializeField] LayerMask obstacleMask;

    [SerializeField] float maxInvestigationTime = 5f;

    NavMeshAgent navMeshAgent;
    Animator animator;
    EnemyHealth enemyHealth;
    Transform target;

    float investigationTimer = 0f;
    float distanceToTarget = Mathf.Infinity;

    private Vector3 investigationPoint;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();
        target = FindObjectOfType<PlayerHealth>().transform;

    }

    void Update()
    {
        if (enemyHealth.IsDead())
        {
            navMeshAgent.enabled = false;
            return;
        }

        distanceToTarget = Vector3.Distance(target.position, transform.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                if (CanSeePlayer())
                {
                    currentState = EnemyState.Chasing;
                }
                break;

            case EnemyState.Chasing:
                EngageTarget();
                break;

            case EnemyState.Attacking:
                EngageTarget();
                break;

            case EnemyState.Investigating:
                InvestigateBehaviour();
                break;
        }
        SetMovementAnimation();
    }

    void SetMovementAnimation()
    {
        if (navMeshAgent.enabled && navMeshAgent.velocity.sqrMagnitude > 0.1)
        {
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        if (currentState == EnemyState.Investigating)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(investigationPoint, 0.5f);
        }
    }

    bool CanSeePlayer()
    {
        if (enemyHealth.IsDead()) return false;

        if (distanceToTarget <= viewRadius)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            float angleBetweenEnemyAndTarget = Vector3.Angle(transform.forward, directionToTarget);

            if (angleBetweenEnemyAndTarget <= viewAngle / 2)
            {
                if(!Physics.Linecast(transform.position + Vector3.up * 3f, 
                    target.position + Vector3.up * 3f, obstacleMask))
                {
                    return true;
                }
            }
        }
        return false;
    }

    void InvestigateBehaviour()
    {
        if (navMeshAgent.enabled)
        {
            if (!navMeshAgent.pathPending&&
                navMeshAgent.remainingDistance<=navMeshAgent.stoppingDistance)
            {
                navMeshAgent.isStopped = true;
                animator.SetBool("isMoving", false);

                investigationTimer += Time.deltaTime;
            }
            if (investigationTimer >= maxInvestigationTime)
            {
                navMeshAgent.isStopped = false;
                currentState = EnemyState.Idle;
            }
            else
            {
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(investigationPoint);
                animator.SetBool("isMoving", true);
                //Space for investigation animation
            }
        }
        if (CanSeePlayer())
        {
            currentState = EnemyState.Chasing;
        }
    }

    public void InvestigateSound(Vector3 point)
    {
        if (enemyHealth.IsDead()) return;

        investigationPoint = point;
        currentState = EnemyState.Investigating;
        investigationTimer = 0f;

        if (navMeshAgent.enabled)
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(investigationPoint);
            animator.SetBool("isMoving", true);
        }
    }

    void EngageTarget()
    {
        FaceTarget();

        if (distanceToTarget > chaseRange)
        {
            navMeshAgent.isStopped = false;
            currentState = EnemyState.Idle;
            animator.SetBool("Attack", false);
        }
        else if (distanceToTarget > navMeshAgent.stoppingDistance)
        {
            ChaseTarget();
        }
        else
        {
            AttackTarget();
        }
    }

    void ChaseTarget()
    {
        if (!navMeshAgent.enabled) return;

        animator.SetBool("Attack", false);
        animator.SetBool("isMoving", true);
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(target.position);

        currentState = EnemyState.Chasing;
    }

    void AttackTarget()
    {
        if (distanceToTarget <= navMeshAgent.stoppingDistance)
        {
            navMeshAgent.isStopped = true;
            animator.SetBool("isMoving", false);
            animator.SetBool("Attack", true);

            currentState = EnemyState.Attacking;
        }
        else
        {
            animator.SetBool("Attack", false);
            navMeshAgent.isStopped = false;

            currentState = EnemyState.Chasing;
        }
    }

    void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
    }

    public void OnDamageTaken()
    {
        currentState = EnemyState.Chasing;
    }
}
