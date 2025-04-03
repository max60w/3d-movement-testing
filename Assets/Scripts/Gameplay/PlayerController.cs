using Camera;
using Fusion;
using UnityEngine;

namespace Gameplay
{
    public class PlayerController : NetworkBehaviour
    {
        #region Network Variables

        [Networked] private float NetworkedWeight { get; set; }
        [Networked] private Vector3 NetworkedPosition { get; set; }
        [Networked] private Quaternion NetworkedRotation { get; set; }
        [Networked] private string NetworkedUsername { get; set; }
        [Networked] private Color NetworkedColor { get; set; }

        #endregion

        #region Serialized Fields

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintMultiplier = 1.6f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float movementAnimationMultiplier = 2f;

        [Header("Interpolation Settings")]
        [SerializeField] private float positionInterpolationSpeed = 15f;
        [SerializeField] private float rotationInterpolationSpeed = 15f;
        [SerializeField] private float movementInterpolationSpeed = 15f;

        [Header("Ground Settings")]
        [SerializeField] private float groundOffset = 0.1f;
        [SerializeField] private float groundCheckDistance = 0.2f;
        [SerializeField] private LayerMask groundLayer = -1;

        [Header("Weight Settings")]
        [SerializeField] private float baseWeight = 70f;
        [SerializeField] private float weightInfluenceOnSpeed = 0.5f;
        [SerializeField] private float weightInfluenceOnJump = 0.7f;
        [SerializeField] private float weightInfluenceOnGravity = 0.3f;

        [Header("References")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private PlayerNetworkAnimator networkAnimator;
        [SerializeField] private Renderer characterRenderer;
        [SerializeField] private TMPro.TextMeshPro usernameText;

        #endregion

        #region Private Fields

        private Vector3 _moveDirection;
        private float _verticalVelocity;
        private UnityEngine.Camera _mainCamera;
        private const float Gravity = 9.8f;
        private float _currentWeight;
        private bool _isGrounded;
        private Vector2 _currentMovement;
        private bool _currentSprint;
        private Vector2 _smoothedMovement;
        private float _lastGroundCheckTime;
        private const float GroundCheckInterval = 0.1f;
        private const float GroundCheckRadius = 0.2f;
        private string _username = "Player";
        private Color _color = Color.white;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
            InitializeWeight();
        }

        #endregion

        #region Network Behaviour

        public override void Spawned()
        {
            NetworkedWeight = _currentWeight;
            NetworkedPosition = transform.position;
            NetworkedRotation = transform.rotation;
            NetworkedUsername = _username;
            NetworkedColor = _color;

            // Apply initial values
            UpdateVisuals();
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData input))
            {
                ProcessInput(input);
            }
        }

        public override void Render()
        {
            if (Object.HasStateAuthority)
            {
                NetworkedPosition = transform.position;
                NetworkedRotation = transform.rotation;
            }
            else
            {
                // Use Runner.DeltaTime for network interpolation
                transform.SetPositionAndRotation(
                    Vector3.Lerp(transform.position, NetworkedPosition, positionInterpolationSpeed * Runner.DeltaTime),
                    Quaternion.Slerp(transform.rotation, NetworkedRotation,
                        rotationInterpolationSpeed * Runner.DeltaTime)
                );
            }

            // Update visuals based on networked values
            UpdateVisuals();
        }

        #endregion

        #region Public Methods

        public void SetUsername(string username)
        {
            if (string.IsNullOrEmpty(username)) return;

            _username = username;

            if (Object.HasStateAuthority)
            {
                NetworkedUsername = _username;
            }
        }

        public void SetColor(Color color)
        {
            _color = color;

            if (Object.HasStateAuthority)
            {
                NetworkedColor = _color;
            }
        }

        #endregion

        #region Private Methods

        private void InitializeComponents()
        {
            _mainCamera = CameraController.Instance.MainCamera;
        }

        private void InitializeWeight()
        {
            _currentWeight = baseWeight;
        }

        private void ProcessInput(NetworkInputData input)
        {
            CheckGround();
            Move(input.Movement, input.Sprint);
            Rotate(input.Look);

            if (input.Jump)
            {
                Jump();
            }

            ApplyGravity();
            MoveCharacter();
            UpdateNetworkedState(input);
        }

        private void CheckGround()
        {
            // Only perform ground check at intervals to prevent excessive checks
            if (Time.time - _lastGroundCheckTime < GroundCheckInterval)
                return;

            _lastGroundCheckTime = Time.time;

            // Get the character's bottom position
            var bottomPosition =
                transform.position + Vector3.up * (characterController.radius * transform.localScale.x);

            // Perform a sphere cast with a slightly larger radius for more reliable detection
            _isGrounded = Physics.SphereCast(
                bottomPosition + Vector3.up * 0.1f,
                characterController.radius * transform.localScale.x,
                Vector3.down,
                out _,
                groundCheckDistance + 0.1f,
                groundLayer);
        }

        private void MoveCharacter()
        {
            var weightAdjustedDeltaTime = Runner.DeltaTime * (1f + (NetworkedWeight / 100f) * weightInfluenceOnGravity);
            characterController.Move(_moveDirection * weightAdjustedDeltaTime);
        }

        private void UpdateNetworkedState(NetworkInputData input)
        {
            _currentMovement = input.Movement;
            _currentSprint = input.Sprint;
            NetworkedWeight = _currentWeight;

            // Update network animator with current movement state
            networkAnimator.SetMovement(_currentMovement, _currentSprint);
            networkAnimator.SetGrounded(_isGrounded);
        }

        private void Move(Vector2 input, bool isSprinting)
        {
            // Get the camera's forward and right vectors, but only on the horizontal plane
            var cameraForward = _mainCamera.transform.forward;
            var cameraRight = _mainCamera.transform.right;
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            // Calculate movement direction based on input and camera direction
            var moveDirection = Vector3.zero;

            // Forward/Backward movement
            if (Mathf.Abs(input.y) > 0.01f)
            {
                moveDirection += cameraForward * input.y;
            }

            // Left/Right movement
            if (Mathf.Abs(input.x) > 0.01f)
            {
                moveDirection += cameraRight * input.x;
            }

            // Normalize only if we have movement
            if (moveDirection != Vector3.zero)
            {
                moveDirection.Normalize();
            }

            // Calculate current speed with sprint and weight factors
            var currentSpeed = CalculateCurrentSpeed(isSprinting);

            // Apply movement direction and speed
            _moveDirection.x = moveDirection.x * currentSpeed;
            _moveDirection.z = moveDirection.z * currentSpeed;
        }

        private float CalculateCurrentSpeed(bool isSprinting)
        {
            var baseSpeed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);
            var weightFactor = 1f - (NetworkedWeight / 100f) * weightInfluenceOnSpeed;
            return baseSpeed * weightFactor;
        }

        private void Rotate(Vector2 lookInput)
        {
            if (lookInput.sqrMagnitude <= 0.01f) return;

            var targetAngle = CalculateTargetAngle(lookInput);
            var targetRotation = Quaternion.Euler(0, targetAngle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Runner.DeltaTime);
        }

        private float CalculateTargetAngle(Vector2 lookInput)
        {
            return Mathf.Atan2(lookInput.x, lookInput.y) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
        }

        private void Jump()
        {
            if (!_isGrounded) return;

            var weightFactor = 1f - (NetworkedWeight / 100f) * weightInfluenceOnJump;
            _verticalVelocity = jumpForce * weightFactor;
            _isGrounded = false; // Immediately set as not grounded when jumping
            networkAnimator.TriggerJump();
        }

        private void ApplyGravity()
        {
            if (_isGrounded && _verticalVelocity < 0)
            {
                _verticalVelocity = -groundOffset;
            }
            else
            {
                var weightFactor = 1f + (NetworkedWeight / 100f) * weightInfluenceOnGravity;
                _verticalVelocity -= Gravity * weightFactor * Runner.DeltaTime;
            }

            _moveDirection.y = _verticalVelocity;
        }

        private void UpdateVisuals()
        {
            // Update username text
            if (usernameText != null)
            {
                usernameText.text = NetworkedUsername;
            }

            // Update character color
            if (characterRenderer != null)
            {
                characterRenderer.material.color = NetworkedColor;
            }
        }

        private void OnDrawGizmos()
        {
            // Visualize ground check for debugging
            if (!Application.isPlaying) return;

            var bottomPosition =
                transform.position + Vector3.up * (characterController.radius * transform.localScale.x);
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(bottomPosition, GroundCheckRadius);
            Gizmos.DrawLine(bottomPosition, bottomPosition + Vector3.down * (groundCheckDistance + 0.1f));
        }

        #endregion
    }
}