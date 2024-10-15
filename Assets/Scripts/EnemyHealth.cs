using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] float hitPoints = 100f;

    bool isDead = false;
    float delayBeforeDeletion = 5f;

    EnemyController enemyController;
    

    public bool IsDead()
    {
        return isDead;
    }

    public void takeDamage(float damage)
    {
        BroadcastMessage("OnDamageTaken");

        hitPoints -= damage;

        if (hitPoints <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        GetComponent<Animator>().SetTrigger("Death");
        enemyController.enabled = false;
        GetComponent<CapsuleCollider>().enabled = false;
    }
}
