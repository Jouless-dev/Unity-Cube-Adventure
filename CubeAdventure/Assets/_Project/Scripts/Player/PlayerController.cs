#pragma warning disable 0649
using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CubeAdventure.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoxCollider))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Horizontal Movement")]
        [Tooltip("Maximum horizontal speed (units/sec).")]
        [SerializeField] private float moveSpeed = 7.5f;

        [Tooltip("Rate of acceleration when speeding up on the ground.")]
        [SerializeField] private float acceleration = 85f;

        [Tooltip("Rate of deceleration when releasing movement keys.")]
        [SerializeField] private float deceleration = 75f;

        [Tooltip("Extra deceleration applied when reversing direction (snappy turnarounds).")]
        [SerializeField] private float turnDeceleration = 130f;

        [Tooltip("Acceleration factor while airborne.")]
        [SerializeField] private float airAccelerationMultiplier = 0.75f;

        [Header("Jump Physics & Feel (Calibrated Jump Arc)")]
        [Tooltip("Upward impulse calibrated to clear exactly one obstacle without overjumping.")]
        [SerializeField] private float jumpForce = 7.2f;

        [Tooltip("Extra gravity applied when jump button is released early (tap = short hop).")]
        [SerializeField] private float jumpCutGravityMultiplier = 2.4f;

        [Tooltip("Fast gravity multiplier when falling downwards.")]
        [SerializeField] private float fallGravityMultiplier = 2.6f;

        [Tooltip("Maximum falling velocity (terminal velocity cap).")]
        [SerializeField] private float maxFallSpeed = 20f;

        [Header("Forgiveness (QoL)")]
        [Tooltip("Grace period (seconds) to jump after leaving a platform.")]
        [SerializeField] private float coyoteTime = 0.15f;

        [Tooltip("Buffer time (seconds) to queue a jump before touching down.")]
        [SerializeField] private float jumpBufferTime = 0.15f;

        [Header("Ground Check")]
        [SerializeField] private float groundCheckRadius = 0.16f;
        [SerializeField] private LayerMask groundLayer = ~0;

        // Components & State
        private Rigidbody rb;
        private BoxCollider boxCollider;
        private Vector3 startPosition;
        private bool isGrounded;
        private float horizontalInput;
        private bool jumpHeld;
        private float coyoteTimeCounter;
        private float jumpBufferCounter;
        private bool isInvulnerable = false;
        private MeshRenderer meshRenderer;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            boxCollider = GetComponent<BoxCollider>();
            meshRenderer = GetComponentInChildren<MeshRenderer>();
            startPosition = transform.position;

            // Strict 2.5D physics constraints: lock all rotations to prevent visual jitter
            rb.constraints = RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationY |
                             RigidbodyConstraints.FreezeRotationZ |
                             RigidbodyConstraints.FreezePositionZ;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            // Maintain perfectly stable, solid cube geometry
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;

            // Zero-friction physics material to prevent snagging on obstacles
            PhysicsMaterial smoothMat = new PhysicsMaterial("Player_CleanMat")
            {
                dynamicFriction = 0f,
                staticFriction = 0f,
                bounciness = 0f,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounceCombine = PhysicsMaterialCombine.Minimum
            };
            boxCollider.sharedMaterial = smoothMat;
        }

        private void Update()
        {
            if (isInvulnerable) return;

            ReadInputs();
            HandleCoyoteTimer();
            HandleJumpTimers();

            // Safety kill plane (Geometry Dash style restart)
            if (transform.position.y < -10f)
            {
                Respawn();
                if (Managers.GameManager.Instance != null)
                {
                    Managers.GameManager.Instance.OnPlayerDeath();
                }
            }
        }

        private void FixedUpdate()
        {
            if (isInvulnerable) return;

            UpdateGroundState();
            ApplyHorizontalMovement();
            ApplyVerticalPhysics();
        }

        private void ReadInputs()
        {
            horizontalInput = 0f;
            bool jumpDownThisFrame = false;
            jumpHeld = false;

            // Quick Restart with R key
            #if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.rKey.wasPressedThisFrame)
                {
                    if (Managers.GameManager.Instance != null)
                        Managers.GameManager.Instance.RestartGame();
                }

                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontalInput -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontalInput += 1f;

                if (keyboard.spaceKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
                {
                    jumpDownThisFrame = true;
                }

                if (keyboard.spaceKey.isPressed || keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                {
                    jumpHeld = true;
                }
            }
            #endif

            // Legacy Input Fallback
            try
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    if (Managers.GameManager.Instance != null)
                        Managers.GameManager.Instance.RestartGame();
                }

                if (horizontalInput == 0f)
                {
                    horizontalInput = Input.GetAxisRaw("Horizontal");
                }

                if (!jumpDownThisFrame)
                {
                    if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
                        jumpDownThisFrame = true;
                }

                if (!jumpHeld)
                {
                    if (Input.GetButton("Jump") || Input.GetKey(KeyCode.Space))
                        jumpHeld = true;
                }
            }
            catch { }

            // Buffer jump input
            if (jumpDownThisFrame)
            {
                jumpBufferCounter = jumpBufferTime;
            }
            else if (jumpBufferCounter > 0f)
            {
                jumpBufferCounter -= Time.deltaTime;
            }
        }

        private void HandleCoyoteTimer()
        {
            if (isGrounded)
            {
                coyoteTimeCounter = coyoteTime;
            }
            else if (coyoteTimeCounter > 0f)
            {
                coyoteTimeCounter -= Time.deltaTime;
            }
        }

        private void UpdateGroundState()
        {
            // Positioned at the bottom face of the 1x1x1 cube
            Vector3 checkPos = transform.position + new Vector3(0f, -0.48f, 0f);
            Collider[] colliders = Physics.OverlapSphere(checkPos, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);
            bool groundDetected = false;

            foreach (var col in colliders)
            {
                if (col.gameObject != gameObject && !col.isTrigger && !col.CompareTag("Obstacle") && !col.CompareTag("Coin") && !col.CompareTag("Goal"))
                {
                    groundDetected = true;
                    break;
                }
            }

            isGrounded = groundDetected;
        }

        private void HandleJumpTimers()
        {
            if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
            {
                ExecuteJump();
                jumpBufferCounter = 0f;
                coyoteTimeCounter = 0f;
            }
        }

        private void ExecuteJump()
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = jumpForce;
            rb.linearVelocity = vel;

            AudioSource audio = GetComponent<AudioSource>();
            if (audio != null && audio.clip != null)
            {
                audio.Play();
            }
        }

        private void ApplyHorizontalMovement()
        {
            float targetSpeed = horizontalInput * moveSpeed;
            float currentSpeed = rb.linearVelocity.x;

            float accelRate;
            if (Mathf.Abs(targetSpeed) > 0.01f)
            {
                if (Mathf.Sign(targetSpeed) != Mathf.Sign(currentSpeed) && Mathf.Abs(currentSpeed) > 0.1f)
                {
                    accelRate = turnDeceleration;
                }
                else
                {
                    accelRate = isGrounded ? acceleration : (acceleration * airAccelerationMultiplier);
                }
            }
            else
            {
                accelRate = isGrounded ? deceleration : (deceleration * airAccelerationMultiplier);
            }

            float newX = Mathf.MoveTowards(currentSpeed, targetSpeed, accelRate * Time.fixedDeltaTime);

            Vector3 vel = rb.linearVelocity;
            vel.x = newX;
            vel.z = 0f;
            rb.linearVelocity = vel;
        }

        private void ApplyVerticalPhysics()
        {
            Vector3 vel = rb.linearVelocity;

            // Variable Jump Cut (release button early for quick low hop)
            if (vel.y > 0f && !jumpHeld)
            {
                vel.y += Physics.gravity.y * (jumpCutGravityMultiplier - 1f) * Time.fixedDeltaTime;
            }
            // Fast, smooth downward fall without discontinuous acceleration cliffs
            else if (vel.y < 0f)
            {
                vel.y += Physics.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
            }

            if (vel.y < -maxFallSpeed)
            {
                vel.y = -maxFallSpeed;
            }

            rb.linearVelocity = vel;
        }

        public void TakeHitAndRespawn()
        {
            if (!isInvulnerable)
            {
                StartCoroutine(HitRespawnRoutine());
            }
        }

        private IEnumerator HitRespawnRoutine()
        {
            isInvulnerable = true;
            rb.linearVelocity = Vector3.zero;

            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
                yield return new WaitForSeconds(0.08f);
                meshRenderer.enabled = true;
                yield return new WaitForSeconds(0.08f);
            }
            else
            {
                yield return new WaitForSeconds(0.16f);
            }

            Respawn();
            isInvulnerable = false;
        }

        public void Respawn()
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.position = startPosition;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position + new Vector3(0f, -0.48f, 0f), groundCheckRadius);
        }
    }
}
