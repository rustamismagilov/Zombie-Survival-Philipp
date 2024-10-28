using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapom Type", fileName = "Weapon 1")]
public class WeaponSO : ScriptableObject
{
    public float range = 100f;
    public float damage = 40;
    public float timeBetweenShots = 0.5f;
    public int pelletsAmount = 10;

    [Tooltip("Randomized audio clips for shooting sound")]
    public AudioClip[] shootSFX;

    [Tooltip("Randomized audio clips for empty weapon sound")]
    public AudioClip[] emptySFX;
}
