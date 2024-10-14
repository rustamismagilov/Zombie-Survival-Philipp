using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathHandler : MonoBehaviour
{
    [SerializeField] Canvas gameOverCanvas;
    [SerializeField] FirstPersonController playerController;
    Weapon weapon;

    void Start()
    {
        gameOverCanvas.enabled = false;
        weapon = FindObjectOfType<Weapon>();
    }

    public void HandleDeath()
    {
        gameOverCanvas.enabled = true;
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        playerController.enabled = false;
    }
}
