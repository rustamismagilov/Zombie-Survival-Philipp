using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Cinemachine;

public class Weapon : MonoBehaviour
{
    [SerializeField] Camera weaponCamera;
    [SerializeField] ParticleSystem muzzleFlash;
    [SerializeField] GameObject impactEffect;
    [SerializeField] Ammo ammoSlot;
    [SerializeField] TextMeshProUGUI ammoText;
    [SerializeField] AmmoType ammoType;
    [SerializeField] float range = 100f;
    [SerializeField] float damage = 40;
    [SerializeField] float timeBetweenShots = 0.5f;
    [SerializeField] int pelletsAmount = 10;
    [SerializeField] float bulletSpread = 0.1f;
    [SerializeField] AudioSource audiosource;


    [Header("Gun sounds")]
    [SerializeField] AudioClip pistolShootSFX;
    [SerializeField] AudioClip emptyGunSFX;

    [Header("SMG sounds")]
    [SerializeField] AudioClip smgShootSFX;
    [SerializeField] AudioClip emptySmgSFX;

    [Header("Shotgun sounds")]
    [SerializeField] AudioClip shotgunShootSFX;
    [SerializeField] AudioClip emptyShotgunSFX;

    [HideInInspector] public bool canShoot = true;

    Animator animator;
    EnemyController enemyController;
    EnemyHealth enemyHealth;

    public enum WeaponType
    {
        Pistol,
        SMG,
        ShotGun
    }

    public WeaponType weaponType;

    void Start()
    {
        animator = GetComponent<Animator>();
        enemyController = GetComponent<EnemyController>();


        if (audiosource != null)
        {
            audiosource = GetComponentInParent<AudioSource>();
        }
    }


    void OnEnable()
    {
        canShoot = true;
    }

    void Update()
    {
        DispalyAmmo();

        if (Input.GetMouseButtonDown(0) && canShoot == true)
        {
            StartCoroutine(Shoot());
        }

        DisableAnimations();
    }

    IEnumerator Shoot()
    {
        canShoot = false;

        int currentAmmo = ammoSlot.GetCurrentAmmo(ammoType);

        switch (weaponType)
        {
            case WeaponType.Pistol:
                if (currentAmmo > 0)
                {
                    PlayMuzzleFlash();
                    ProcessRaycast();
                    animator.SetTrigger("Shoot");
                    audiosource.PlayOneShot(pistolShootSFX);
                    ammoSlot.ReduceCurrentAmmo(ammoType);
                }
                else
                {
                    audiosource.PlayOneShot(emptyGunSFX);
                }
                break;

            case WeaponType.SMG:
                if (currentAmmo > 0)
                {
                    PlayMuzzleFlash();
                    ProcessRaycast();
                    animator.SetBool("isShooting", true);
                    audiosource.PlayOneShot(smgShootSFX);
                    ammoSlot.ReduceCurrentAmmo(ammoType);
                }
                else
                {
                    audiosource.PlayOneShot(emptySmgSFX);
                }
                break;

            case WeaponType.ShotGun:
                if (currentAmmo > 0)
                {
                    PlayMuzzleFlash();
                    animator.SetTrigger("Shoot");
                    audiosource.PlayOneShot(shotgunShootSFX);
                    for (int i = 0; i < pelletsAmount; i++)
                    {
                        ProcessRaycast();
                    }
                    ammoSlot.ReduceCurrentAmmo(ammoType);
                }
                else
                {
                    audiosource.PlayOneShot(emptyShotgunSFX);
                }
                break;
        }

        yield return new WaitForSeconds(timeBetweenShots);
        canShoot = true;
    }

    void ProcessRaycast()
    {
        RaycastHit hit;
        Vector3 direction = GetShotgunSpread();

        Debug.DrawRay(weaponCamera.transform.position, direction * range, Color.red, 5f);

        if (Physics.Raycast(weaponCamera.transform.position, weaponCamera.transform.forward, out hit, range))
        {
            CreateImpactHit(hit);

            // Initialize enemyHealth here
            enemyHealth = hit.transform.GetComponent<EnemyHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                enemyController = hit.transform.GetComponent<EnemyController>();

                if (enemyController == null)
                {
                    enemyController = hit.transform.GetComponent<EnemyController>();
                }

                if (enemyController != null)
                {
                    // Alert this enemy to the player's position
                    enemyController.AlertToPlayer(transform.position);

                    // Call AlertEnemiesNearTarget to alert nearby enemies
                    AlertEnemiesNearTarget(hit.point, enemyController);
                }
            }
        }
        else
        {
            return;
        }
    }

    void PlayMuzzleFlash()
    {
        muzzleFlash.Play();
    }

    void CreateImpactHit(RaycastHit hit)
    {
        GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
        Destroy(impact, 0.1f);

        AlertEnemies(hit.point);
    }

    void AlertEnemies(Vector3 impactPoint)
    {
        float alertRadius = 20f;

        Collider[] colliders = Physics.OverlapSphere(impactPoint, alertRadius);

        //Debug.Log("AlertEnemies called. Colliders found" + colliders.Length);

        foreach (Collider collider in colliders)
        {
            EnemyController enemyController = collider.GetComponentInParent<EnemyController>();

            if (enemyController != null)
            {
                enemyController.InvestigateSound(impactPoint);
            }
        }
    }

    void AlertEnemiesNearTarget(Vector3 targetPosition, EnemyController shotEnemy)
    {
        if (shotEnemy == null)
        {
            Debug.LogWarning("shotEnemy is null, cannot alert nearby enemies.");
            return; // Exit the method if shotEnemy is null
        }

        float alertRadius = 15f; // Adjust this value as needed
        Collider[] colliders = Physics.OverlapSphere(targetPosition, alertRadius);

        foreach (Collider collider in colliders)
        {
            EnemyController enemyController = collider.GetComponent<EnemyController>();

            if (enemyController == null)
            {
                // Try to get the EnemyController from the parent
                enemyController = collider.GetComponentInParent<EnemyController>();
            }

            if (enemyController != null && !enemyHealth.IsDead())  // Make sure enemyHealth is valid
            {
                // Exclude the enemy that was shot
                if (enemyController != shotEnemy)
                {
                    // Alert the enemy to the player's position
                    enemyController.AlertToPlayer(transform.position);
                }
            }
        }
    }

    void DispalyAmmo()
    {
        int currentAmmo = ammoSlot.GetCurrentAmmo(ammoType);
        ammoText.text = currentAmmo.ToString();
    }

    Vector3 GetShotgunSpread()
    {
        Vector3 direction = weaponCamera.transform.forward;
        direction.x += Random.Range(-bulletSpread, bulletSpread);
        direction.y += Random.Range(-bulletSpread, bulletSpread);
        return direction;
    }

    void DisableAnimations()
    {
        if (Input.GetMouseButtonUp(0) && weaponType == WeaponType.SMG)
        {
            animator.SetBool("isShooting", false);
        }

        /* else if (Input.GetMouseButtonUp(0) && weaponType == WeaponType.Shotgun)
        {
            animator.SetBool("isShotgunShooting", false);
        } */
    }

}
