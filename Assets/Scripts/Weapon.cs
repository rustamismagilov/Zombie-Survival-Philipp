using System.Collections;
using TMPro;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private Camera weaponCamera;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private Ammo ammoSlot;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private WeaponSO weaponData;

    [HideInInspector] public bool canShoot = true;

    private Animator animator;
    private EnemyController enemyController;
    private EnemyHealth enemyHealth;

    private void Start()
    {
        animator = GetComponent<Animator>();
        enemyController = GetComponent<EnemyController>();

        if (audioSource == null)
        {
            audioSource = GetComponentInParent<AudioSource>();
        }
    }

    private void OnEnable()
    {
        canShoot = true;
    }

    private void Update()
    {
        DisplayAmmo();

        if (Input.GetMouseButtonDown(0) && canShoot)
        {
            StartCoroutine(Shoot());
        }

        DisableAnimations();
    }

    private IEnumerator Shoot()
    {
        canShoot = false;
        int currentAmmo = ammoSlot.GetCurrentAmmo(weaponData.ammoType);

        if (currentAmmo > 0)
        {
            PlayMuzzleFlash();
            PlayRandomAudioClip(weaponData.shootSFX);

            animator.SetTrigger("Shoot");

            if (weaponData.weaponType == WeaponSO.WeaponType.GrenadeLauncher)
            {
                LaunchGrenade();
            }
            else
            {
                ProcessRaycast();
            }

            ammoSlot.ReduceCurrentAmmo(weaponData.ammoType);
        }
        else
        {
            PlayRandomAudioClip(weaponData.emptyShootSFX);
        }

        yield return new WaitForSeconds(weaponData.timeBetweenShots);
        canShoot = true;
    }

    private void LaunchGrenade()
    {
        GameObject grenade = Instantiate(weaponData.grenadePrefab, weaponCamera.transform.position, Quaternion.identity);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(weaponCamera.transform.forward * 40f, ForceMode.Impulse);
        }

        // Initialize grenade settings based on WeaponSO data
        Grenade grenadeScript = grenade.GetComponent<Grenade>();
        if (grenadeScript != null)
        {
            grenadeScript.Initialize(weaponData.explosionRadius, weaponData.explosionForce, weaponData.damage);
        }
    }

    private void DisplayAmmo()
    {
        int currentAmmo = ammoSlot.GetCurrentAmmo(weaponData.ammoType);
        ammoText.text = currentAmmo.ToString();
    }

    private void ProcessRaycast()
    {
        RaycastHit hit;
        Vector3 direction = GetShotgunSpread();

        if (Physics.Raycast(weaponCamera.transform.position, direction, out hit, weaponData.range))
        {
            CreateImpactHit(hit);

            enemyHealth = hit.transform.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(weaponData.damage);
                enemyController = hit.transform.GetComponent<EnemyController>();

                if (enemyController != null)
                {
                    enemyController.AlertToPlayer(transform.position);
                    AlertEnemiesNearTarget(hit.point, enemyController);
                }
            }
        }
    }

    private void PlayMuzzleFlash()
    {
        muzzleFlash.Play();
    }

    private void CreateImpactHit(RaycastHit hit)
    {
        GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
        Destroy(impact, 0.1f);
        AlertEnemies(hit.point);
    }

    private void AlertEnemies(Vector3 impactPoint)
    {
        float alertRadius = 20f;
        Collider[] colliders = Physics.OverlapSphere(impactPoint, alertRadius);

        foreach (Collider collider in colliders)
        {
            EnemyController enemyCtrl = collider.GetComponentInParent<EnemyController>();
            if (enemyCtrl != null)
            {
                enemyCtrl.InvestigateSound(impactPoint);
            }
        }
    }

    private void AlertEnemiesNearTarget(Vector3 targetPosition, EnemyController shotEnemy)
    {
        if (shotEnemy == null) return;

        float alertRadius = 15f;
        Collider[] colliders = Physics.OverlapSphere(targetPosition, alertRadius);

        foreach (Collider collider in colliders)
        {
            EnemyController enemyCtrl = collider.GetComponentInParent<EnemyController>();
            if (enemyCtrl != null && enemyCtrl != shotEnemy && !enemyHealth.IsDead())
            {
                enemyCtrl.AlertToPlayer(transform.position);
            }
        }
    }

    private Vector3 GetShotgunSpread()
    {
        Vector3 direction = weaponCamera.transform.forward;
        direction.x += Random.Range(-weaponData.bulletSpread, weaponData.bulletSpread);
        direction.y += Random.Range(-weaponData.bulletSpread, weaponData.bulletSpread);
        return direction;
    }

    private void DisableAnimations()
    {
        if (Input.GetMouseButtonUp(0) && weaponData.weaponType == WeaponSO.WeaponType.SMG)
        {
            animator.SetBool("isShooting", false);
        }
    }

    private void PlayRandomAudioClip(AudioClip[] clips)
    {
        if (clips.Length == 0) return;
        int index = Random.Range(0, clips.Length);
        audioSource.PlayOneShot(clips[index]);
    }
}
