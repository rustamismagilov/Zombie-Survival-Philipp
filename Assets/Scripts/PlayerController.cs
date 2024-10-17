using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    public float moveSpeed = 15f;
    public float sprintSpeed = 20f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;
    public float movementSmoothness = 0.1f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Camera Settings")]
    public Transform playerCamera;
    public float cameraClampTop = 89f;
    public float cameraClampBottom = -89f;
    public float mouseSensitivity = 100f;
    public float rotationSmoothness = 0.1f;

    [Header("Crouching Variables")]
    private CapsuleCollider capsuleCollider;
    private Vector3 originalScale;
    public float crouchScaleFactor = 0.5f;
    public float crouchSpeedFactor = 0.5f;
    [HideInInspector] public bool isCrouching = false;

    private float xRotation = 0f;
    private Rigidbody rb;
    private bool isGrounded;

    private float originalMoveSpeed;
    private float originalSprintSpeed;

    private Vector3 velocity;
    private Vector3 movementInput;
    private Vector3 smoothMoveVelocity;

    public bool IsMoving
    {
        get
        {
            return movementInput.magnitude > 0.1f;
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponentInChildren<CapsuleCollider>();
        rb.freezeRotation = true;  

        originalScale = transform.localScale;

        originalMoveSpeed = moveSpeed;
        originalSprintSpeed = sprintSpeed;

        if (playerCamera == null)
        {
            playerCamera = Camera.main.transform;
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        HandleMovementInput();
        HandleCrouch();
        HandleCameraRotation();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovementInput()
    {
        // Capture movement input
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        movementInput = (transform.right * moveX + transform.forward * moveZ).normalized;

        // Adjust speed based on sprinting or crouching
        float currentSpeed = isCrouching ? moveSpeed : (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed);

        // Smooth the movement input
        Vector3 targetVelocity = movementInput * currentSpeed;
        velocity = Vector3.SmoothDamp(velocity, targetVelocity, ref smoothMoveVelocity, movementSmoothness);
    }

    private void HandleMovement()
    {
        // Ground Check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Apply the smoothed velocity
        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Apply gravity manually
        if (!isGrounded)
        {
            rb.AddForce(Vector3.up * gravity, ForceMode.Acceleration);
        }
    }

    private void HandleCameraRotation()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Smooth camera rotation
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, cameraClampBottom, cameraClampTop);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate the player body
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleCrouch()
    {
        if (Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl))
        {
            if (!isCrouching)
            {
                isCrouching = true;

                // Reduce CapsuleCollider height (crouching)
                capsuleCollider.height *= crouchScaleFactor;

                // Reduce movement speed by the crouch speed factor
                moveSpeed *= crouchSpeedFactor;

                // Disable sprinting while crouching
                sprintSpeed = moveSpeed;

                Debug.Log("Crouching");
            }
        }
        else
        {
            if (isCrouching)
            {
                isCrouching = false;

                // Reset CapsuleCollider height
                capsuleCollider.height /= crouchScaleFactor;

                // Reset movement speed to the original speed
                moveSpeed = originalMoveSpeed;
                sprintSpeed = originalSprintSpeed;

                Debug.Log("Not crouching");
            }
        }
    }
}
