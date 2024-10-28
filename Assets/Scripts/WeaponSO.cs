using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/WeaponSO")]
public class WeaponSO : ScriptableObject
{
    public enum WeaponType
    {
        Pistol,
        SMG,
        Shotgun
    }

    public WeaponType weaponType;
    public float range = 100f;
    public float damage = 40f;
    public float timeBetweenShots = 0.5f;
    public int pelletsAmount = 1;
    public float bulletSpread = 0.1f;

    [Header("Shooting Sounds")]
    public AudioClip[] shootSFX;

    [Header("Empty Weapon Sounds")]
    public AudioClip[] emptyShootSFX;
}
