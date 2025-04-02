using Camera;
using Fusion;
using UnityEngine;

namespace Gameplay
{
    public class PlayerController : NetworkBehaviour
    {
        #region Animation Parameter Hashes
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int GroundedHash = Animator.StringToHash("Grounded");
        private static readonly int FreeFallHash = Animator.StringToHash("FreeFall");
        private static readonly int MotionSpeedHash = Animator.StringToHash("MotionSpeed");
        #endregion

        #region Serialized Fields
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintMultiplier = 1.6f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float movementAnimationMultiplier = 2f; // Multiplier for movement animation speed

        [Header("References")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Animator animator;
        #endregion

        #region Private Fields
        private Vector3 _moveDirection;
        private float _verticalVelocity;
        private UnityEngine.Camera _mainCamera;
        private bool _wasGrounded;

        private const float Gravity = 9.8f;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            if (animator == null)
                animator = GetComponent<Animator>();

            _mainCamera = CameraController.Instance.MainCamera;
        }
        #endregion

        #region Network Behaviour
        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData input))
            {
                Move(input.Movement, input.Sprint);
                Rotate(input.Look);
                if (input.Jump)
                    Jump();

                ApplyGravity();

                characterController.Move(_moveDirection * Runner.DeltaTime);
                UpdateAnimations(input.Movement, input.Sprint);
            }
        }
        #endregion

        #region Private Methods
        private void Move(Vector2 input, bool isSprinting)
        {
            var forward = _mainCamera.transform.forward;
            var right = _mainCamera.transform.right;

            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            var desiredDirection = forward * input.y + right * input.x;

            var currentSpeed = moveSpeed;
            if (isSprinting)
                currentSpeed *= sprintMultiplier;

            _moveDirection.x = desiredDirection.x * currentSpeed;
            _moveDirection.z = desiredDirection.z * currentSpeed;
        }

        private void Rotate(Vector2 lookInput)
        {
            if (lookInput.sqrMagnitude > 0.01f)
            {
                var targetAngle = Mathf.Atan2(lookInput.x, lookInput.y) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
                var targetRotation = Quaternion.Euler(0, targetAngle, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Runner.DeltaTime);
            }
        }

        private void Jump()
        {
            if (characterController.isGrounded)
            {
                _verticalVelocity = jumpForce;
                if (animator != null)
                    animator.SetTrigger(JumpHash);
            }
        }

        private void ApplyGravity()
        {
            if (characterController.isGrounded && _verticalVelocity < 0)
            {
                _verticalVelocity = -0.5f;
            }
            else
            {
                _verticalVelocity -= Gravity * Runner.DeltaTime;
            }

            _moveDirection.y = _verticalVelocity;
        }

        private void UpdateAnimations(Vector2 movement, bool isSprinting)
        {
            if (animator == null) return;

            var speed = movement.magnitude * (isSprinting ? sprintMultiplier : 1f) * movementAnimationMultiplier;
            animator.SetFloat(SpeedHash, speed);
            animator.SetFloat(MotionSpeedHash, isSprinting ? sprintMultiplier : 1f);

            var isGrounded = characterController.isGrounded;
            animator.SetBool(GroundedHash, isGrounded);

            if (!isGrounded && _verticalVelocity < 0)
            {
                animator.SetBool(FreeFallHash, true);
            }
            else if (isGrounded)
            {
                animator.SetBool(FreeFallHash, false);
            }

            animator.SetBool(JumpHash, !isGrounded && _verticalVelocity > 0);
        }
        #endregion
    }
}