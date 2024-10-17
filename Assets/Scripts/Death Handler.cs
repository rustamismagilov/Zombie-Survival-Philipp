using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathHandler : MonoBehaviour
{
    [SerializeField] Canvas gameOverCanvas;
    [SerializeField] PlayerController playerController;
    Weapon weapon;
    WeaponSwitcher weaponSwitcher;

    void Start()
    {
        gameOverCanvas.enabled = false;
        weapon = FindObjectOfType<Weapon>();
        weaponSwitcher = FindObjectOfType<WeaponSwitcher>();
    }

    public void HandleDeath()
    {
        gameOverCanvas.enabled = true;
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        playerController.enabled = false;
        weapon.enabled = false;
        weaponSwitcher.enabled = false;
    }
}
