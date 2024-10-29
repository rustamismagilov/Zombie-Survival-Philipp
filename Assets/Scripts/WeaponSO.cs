using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/WeaponSO")]
public class WeaponSO : ScriptableObject
{
    public enum WeaponType
    {
        Pistol,
        SMG,
        Shotgun,
        GrenadeLauncher
    }

    public WeaponType weaponType;
    public AmmoType ammoType;
    public float range = 100f;
    public float damage = 40f;
    public float timeBetweenShots = 0.5f;
    public int pelletsAmount = 1;
    public float bulletSpread = 0.1f;

    [Header("Shooting Sounds")]
    public AudioClip[] shootSFX;

    [Header("Empty Weapon Sounds")]
    public AudioClip[] emptyShootSFX;

    [Header("Grenade Launcher Settings")]
    public GameObject grenadePrefab;
    public float explosionRadius = 10f;
    public float explosionForce = 10f;
    public float launchForce = 40f;
}
