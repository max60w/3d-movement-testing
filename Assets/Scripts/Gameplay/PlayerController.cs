using Camera;
using Fusion;
using UnityEngine;

namespace Gameplay
{
    public class PlayerController : NetworkBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintMultiplier = 1.6f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float jumpForce = 5f;

        [Header("References")]
        [SerializeField] private CharacterController characterController;

        private Vector3 _moveDirection;
        private float _verticalVelocity;
        private UnityEngine.Camera _mainCamera;

        private const float Gravity = 9.8f;

        private void Awake()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            _mainCamera = CameraController.Instance.MainCamera;
        }

        public override void FixedUpdateNetwork()
        {
            // Only move if we have input
            if (GetInput(out NetworkInputData input))
            {
                // Handle movement
                Move(input.Movement, input.Sprint);

                // Handle rotation (for third-person)
                Rotate(input.Look);

                // Handle jumping
                if (input.Jump)
                    Jump();

                // Apply gravity
                ApplyGravity();

                // Apply final movement
                characterController.Move(_moveDirection * Runner.DeltaTime);
            }
        }

        private void Move(Vector2 input, bool isSprinting)
        {
            // Convert input to world-space direction based on camera
            Vector3 forward = _mainCamera.transform.forward;
            Vector3 right = _mainCamera.transform.right;

            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            Vector3 desiredDirection = forward * input.y + right * input.x;

            // Apply speed and sprint multiplier
            float currentSpeed = moveSpeed;
            if (isSprinting)
                currentSpeed *= sprintMultiplier;

            _moveDirection.x = desiredDirection.x * currentSpeed;
            _moveDirection.z = desiredDirection.z * currentSpeed;
        }

        private void Rotate(Vector2 lookInput)
        {
            if (lookInput.sqrMagnitude > 0.01f)
            {
                float targetAngle = Mathf.Atan2(lookInput.x, lookInput.y) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
                Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Runner.DeltaTime);
            }
        }

        private void Jump()
        {
            if (characterController.isGrounded)
            {
                _verticalVelocity = jumpForce;
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
    }
}