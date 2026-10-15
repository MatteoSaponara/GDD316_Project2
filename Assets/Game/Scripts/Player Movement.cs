using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Speeds")]
        [SerializeField] private float crouchSpeed = 1.5f;
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float runSpeed = 6f;

        [Header("Jumping")]
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -9.81f;

        [Header("Crouching")]
        [SerializeField] private float crouchHeight = 1f;
        [SerializeField] private float standingHeight = 2f;
        [SerializeField] private float crouchCameraHeight = 0.7f;

        [Header("Camera")]
        [SerializeField] private Transform playerCamera;
        [SerializeField] private float lookSensitivity = 0.1f;
        [SerializeField] private float lookUpLimit = 80f;
        [SerializeField] private float lookDownLimit = 80f;

        [Header("Footstep Sounds")]
        [SerializeField] private AudioSource footstepAudioSource;
        [SerializeField] private AudioClip walkFootstepSound;
        [SerializeField] private AudioClip runFootstepSound;
        [SerializeField] private float walkStepInterval = 0.5f;
        [SerializeField] private float runStepInterval = 0.3f;

        public bool IsRunning => isRunning;
        public bool IsCrouching => isCrouching;

        private CharacterController characterController;
        private PlayerControls controls;

        private Vector2 moveInput;
        private Vector2 lookInput;

        private float verticalVelocity;
        private float cameraPitch;
        private float footstepTimer;
        private float defaultCameraHeight;

        // Determines if the player can move the camera
        private bool canLook = true;

        private bool isCrouching;
        private bool isRunning;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            controls = new PlayerControls();

            if (playerCamera != null)
            {
                defaultCameraHeight = playerCamera.localPosition.y;
            }

            characterController.height = standingHeight;
        }

        private void OnEnable()
        {
            controls.Player.Enable();

            controls.Player.Jump.performed += OnJump;
        }

        private void OnDisable()
        {
            controls.Player.Jump.performed -= OnJump;

            controls.Player.Disable();
        }

        private void Start()
        {
            LockCursor();
        }

        private void Update()
        {

            HandleCursor();
            HandleInput();
            HandleMovement();
            if (canLook)
            {
                HandleCamera();
            }
        }

        private void HandleInput()
        {
            moveInput = controls.Player.Move.ReadValue<Vector2>();
            lookInput = controls.Player.Look.ReadValue<Vector2>();

            isCrouching = controls.Player.Crouch.IsPressed();
            isRunning = controls.Player.Sprint.IsPressed() && !isCrouching;
        }

        private void HandleMovement()
        {
            // Set the movement speed.
            float currentSpeed;

            if (isCrouching)
            {
                currentSpeed = crouchSpeed;
            }
            else if (isRunning)
            {
                currentSpeed = runSpeed;
            }
            else
            {
                currentSpeed = walkSpeed;
            }

            // Convert input into movement relative to the player.
            Vector3 moveDirection =
                transform.right * moveInput.x +
                transform.forward * moveInput.y;

            moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

            // Apply gravity.
            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity =
                moveDirection * currentSpeed;

            velocity.y = verticalVelocity;

            characterController.Move(velocity * Time.deltaTime);

            HandleCrouch();
            HandleFootsteps(moveDirection.magnitude);
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            if (characterController.isGrounded && !isCrouching)
            {
                // Calculate the velocity needed to reach the jump height.
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        private void HandleCrouch()
        {
            characterController.height = isCrouching
                ? crouchHeight
                : standingHeight;

            if (playerCamera != null)
            {
                Vector3 cameraPosition = playerCamera.localPosition;

                cameraPosition.y = isCrouching
                    ? crouchCameraHeight
                    : defaultCameraHeight;

                playerCamera.localPosition = cameraPosition;
            }
        }

        private void HandleCamera()
        {
            if (Cursor.lockState != CursorLockMode.Locked || playerCamera == null)
            {
                return;
            }

            // Rotate the player horizontally.
            float mouseX = lookInput.x * lookSensitivity;
            transform.Rotate(Vector3.up * mouseX);

            // Rotate the camera vertically.
            float mouseY = lookInput.y * lookSensitivity;

            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(
                cameraPitch,
                -lookDownLimit,
                lookUpLimit
            );

            playerCamera.localRotation =
                Quaternion.Euler(cameraPitch, 0f, 0f);
        }

        private void HandleFootsteps(float movementAmount)
        {
            if (!characterController.isGrounded ||
                movementAmount < 0.1f ||
                footstepAudioSource == null)
            {
                footstepTimer = 0f;
                return;
            }

            AudioClip currentFootstep;

            if (isRunning)
            {
                currentFootstep = runFootstepSound;
            }
            else
            {
                currentFootstep = walkFootstepSound;
            }

            if (currentFootstep == null)
            {
                return;
            }

            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                footstepAudioSource.PlayOneShot(currentFootstep);

                footstepTimer = isRunning
                    ? runStepInterval
                    : walkStepInterval;
            }
        }

        private void HandleCursor()
        {
            if (Keyboard.current != null &&
                Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (Mouse.current != null &&
                Mouse.current.leftButton.wasPressedThisFrame &&
                Cursor.lockState != CursorLockMode.Locked)
            {
                LockCursor();
            }
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void DisableCameraControl()
        {
            canLook = false;
        }
    }
}
