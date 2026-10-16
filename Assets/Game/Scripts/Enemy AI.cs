
using UnityEngine;
using System;
using Game;

[RequireComponent(typeof(CharacterController))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Line of Sight")]
    [SerializeField] private float sightRadius = 10f;
    [SerializeField, Range(0f, 180f)] private float sightAngle = 45f;
    [SerializeField] private float eyeHeight = 1.5f;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Hearing")]
    [SerializeField] private float hearingRadius = 7f;
    [SerializeField] private float runFactor = 2f;

    // Player refereneces
    private Transform player;
    private PlayerMovement playerMovement;

    // Detection Action
    public Action OnDetection;

    private CharacterController characterController;
    private bool isChasing;
    private float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        FindPlayer();
        verticalVelocity = -2f;
    }

    private void Update()
    {
        if (player == null)
        {
            FindPlayer();

            if (player == null)
            {
                return;
            }
        }

        // Detect the player visually or through running noise.
        bool detectedNow = CanSeePlayer() || CanHearPlayer();

        if (detectedNow)
        {
            isChasing = true;
            GameManager.Instance.ReportPlayerDetected();
        }
        else if (GameManager.Instance.PlayerDetected)
        {
            isChasing = true;
        }
        else
        {
            isChasing = false;
        }

        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            ApplyGravity();
        }
    }

    private bool CanSeePlayer()
    {
        Vector3 eyePosition =
            transform.position + Vector3.up * eyeHeight;

        Vector3 targetPosition =
            player.position + Vector3.up * 1f;

        Vector3 directionToPlayer =
            targetPosition - eyePosition;

        // Check whether the player is within sight range.
        if (directionToPlayer.magnitude > sightRadius)
        {
            return false;
        }

        // Check whether the player is inside the vision cone.
        float angle = Vector3.Angle(
            transform.forward,
            directionToPlayer
        );

        if (angle > sightAngle)
        {
            return false;
        }

        // Check whether a wall blocks the view.
        if (Physics.Raycast(
            eyePosition,
            directionToPlayer.normalized,
            out RaycastHit hit,
            sightRadius,
            ~0,
            QueryTriggerInteraction.Ignore))
        {
            if (hit.transform != player &&
                !hit.transform.IsChildOf(player))
            {
                return false;
            }
        }
        else
        {
            return false;
        }

        return true;
    }

    // Determines if the players in its hearing distance when they are moving
    private bool CanHearPlayer()
    {
        if (playerMovement == null || playerMovement.IsCrouching)
        {
            return false;
        }

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (playerMovement.IsRunning)
        {
            return distance <= hearingRadius + runFactor;
        }

        return distance <= hearingRadius;
    }

    private void ChasePlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            // Face the player smoothly.
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            // Move toward the player.
            Vector3 movement =
                direction.normalized * chaseSpeed;

            ApplyGravity();

            movement.y = verticalVelocity;

            characterController.Move(
                movement * Time.deltaTime
            );
        }
        else
        {
            ApplyGravity();
        }
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        characterController.Move(
            Vector3.up * verticalVelocity * Time.deltaTime
        );
    }

    // Detects if an enemy collides with the player
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.transform == player || hit.transform.IsChildOf(player))
        {
            GameManager.Instance.GameOver();
            Debug.Log("Enemy caught the player!");
        }
    }

    private void FindPlayer()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.Player != null)
        {
            player = GameManager.Instance.Player.transform;
            playerMovement =
                GameManager.Instance.Player.GetComponent<PlayerMovement>();
        }
    }


    private void OnDrawGizmosSelected()
    {
        // Draw the sight radius.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRadius);

        // Draw the hearing radius.
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hearingRadius);

        // Draw the vision cone.
        Vector3 leftBoundary = Quaternion.Euler(
            0f, -sightAngle, 0f
        ) * transform.forward;

        Vector3 rightBoundary = Quaternion.Euler(
            0f, sightAngle, 0f
        ) * transform.forward;

        Gizmos.color = Color.red;

        Gizmos.DrawRay(
            transform.position,
            leftBoundary * sightRadius
        );

        Gizmos.DrawRay(
            transform.position,
            rightBoundary * sightRadius
        );
    }
}
