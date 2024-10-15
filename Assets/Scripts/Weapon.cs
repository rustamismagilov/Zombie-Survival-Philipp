using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] Camera FPCamera;
    [SerializeField] float range = 100f;
    [SerializeField] float damage = 40;
    [SerializeField] ParticleSystem muzzleFlash;
    [SerializeField] GameObject impactEffect;
    [SerializeField] Ammo ammoSlot;
    [SerializeField] float timeBetweenShots = 0.5f;
    [SerializeField] AmmoType ammoType;
    [SerializeField] TextMeshProUGUI ammoText;
    [SerializeField] int pelletsAmount = 10;
    [SerializeField] float bulletSpread = 0.1f;

    public bool canShoot = true;

    
    Animator animator;

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
    }

    private void OnEnable()
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

        if (Input.GetMouseButtonUp(0) && weaponType == WeaponType.SMG)
        {
            animator.SetBool("isShooting", false);
        }
    }

    IEnumerator Shoot()
    {
        canShoot = false;

        if (ammoSlot.GetCurrentAmmo(ammoType) > 0)
        {
            PlayMuzzleFlash();

            switch (weaponType)
            {
                case WeaponType.Pistol:
                    ProcessRaycast();
                    animator.SetTrigger("Shoot");
                    break;
                case WeaponType.SMG:
                    ProcessRaycast();
                    animator.SetBool("isShooting", true);
                    break;
                case WeaponType.ShotGun:
                    animator.SetTrigger("Shoot");
                    for (int i = 0; i < pelletsAmount; i++)
                    {
                        ProcessRaycast();
                    }
                    break;
            }
            ammoSlot.ReduceCurrentAmmo(ammoType);
            Debug.Log("Ammo " + ammoSlot.GetCurrentAmmo(ammoType));
        }
        yield return new WaitForSeconds(timeBetweenShots);
        canShoot = true;
    }

    void ProcessRaycast()
    {
        RaycastHit hit;

        Vector3 direction = GetShotgunSpread();

        Debug.DrawRay(FPCamera.transform.position, direction * range, Color.red, 5f);

        if (Physics.Raycast(FPCamera.transform.position, FPCamera.transform.forward, out hit, range))
        {
            //Debug.Log("I hit this thing: " + hit.transform.name);

            CreateImpactHit(hit);

            EnemyHealth target = hit.transform.GetComponent<EnemyHealth>();

            if (target == null)
            {
                return;
            }

            target.TakeDamage(damage);
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
    }

    void DispalyAmmo()
    {
        int currentAmmo = ammoSlot.GetCurrentAmmo(ammoType);
        ammoText.text = currentAmmo.ToString();
    }

    Vector3 GetShotgunSpread()
    {
        Vector3 direction = FPCamera.transform.forward;
        direction.x += Random.Range(-bulletSpread, bulletSpread);
        direction.y += Random.Range(-bulletSpread, bulletSpread);
        return direction;
    }
}
