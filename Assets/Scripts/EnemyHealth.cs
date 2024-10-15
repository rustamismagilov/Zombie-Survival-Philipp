using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] float hitPoints = 100f;

    NavMeshAgent navMeshAgent;

    bool isDead = false;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public bool IsDead()
    {
        return isDead;
    }

    public void TakeDamage(float damage)
    {
        BroadcastMessage("OnDamageTaken");

        hitPoints -= damage;

        if (hitPoints <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;

        navMeshAgent.enabled = false;

        GetComponent<Animator>().SetTrigger("Death");

        GetComponent<CapsuleCollider>().enabled = false;

        Destroy(gameObject, 3f);
    }
}
