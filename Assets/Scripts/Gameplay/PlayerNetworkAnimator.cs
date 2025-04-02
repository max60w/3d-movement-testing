using Fusion;
using UnityEngine;

namespace Gameplay
{
	public class PlayerNetworkAnimator : NetworkBehaviour
	{
		#region Animation Parameter Hashes
		private static readonly int ForwardHash = Animator.StringToHash("Forward");
		private static readonly int SidewaysHash = Animator.StringToHash("Sideways");
		private static readonly int JumpHash = Animator.StringToHash("Jump");
		private static readonly int GroundedHash = Animator.StringToHash("Grounded");
		private static readonly int SprintHash = Animator.StringToHash("Sprint");
		#endregion

		#region Network Variables
		[Networked] private Vector2 NetworkedMovement { get; set; }
		[Networked] private NetworkBool NetworkedSprint { get; set; }
		[Networked] private NetworkBool NetworkedIsGrounded { get; set; }
		[Networked] private TickTimer JumpTimer { get; set; }
		#endregion

		#region Serialized Fields
		[Header("Animation Settings")]
		[SerializeField] private Animator animator;
		[SerializeField] private float movementSmoothSpeed = 8f;
		#endregion

		#region Private Fields
		private Vector2 _currentMovement;
		private bool _currentSprint;
		private bool _currentGrounded;
		private bool _hasTriggeredJump;
		#endregion

		#region Network Behaviour
		public override void Spawned()
		{
			if (Object.HasStateAuthority)
			{
				_currentMovement = Vector2.zero;
				_currentSprint = false;
				_currentGrounded = true;
			}
		}

		public override void FixedUpdateNetwork()
		{
			if (!Object.HasStateAuthority) return;

			NetworkedMovement = _currentMovement;
			NetworkedSprint = _currentSprint;
			NetworkedIsGrounded = _currentGrounded;
		}

		public override void Render()
		{
			if (animator == null) return;

			if (Object.HasStateAuthority)
			{
				UpdateAnimations(_currentMovement, _currentSprint, _currentGrounded);
			}
			else
			{
				UpdateAnimations(NetworkedMovement, NetworkedSprint, NetworkedIsGrounded);
			}

			if (JumpTimer.IsRunning && !_hasTriggeredJump)
			{
				animator.SetTrigger(JumpHash);
				_hasTriggeredJump = true;
			}
			else if (!JumpTimer.IsRunning)
			{
				_hasTriggeredJump = false;
			}
		}
		#endregion

		#region Public Methods
		public void SetMovement(Vector2 movement, bool isSprinting)
		{
			_currentMovement = movement;
			_currentSprint = isSprinting;
		}

		public void SetGrounded(bool isGrounded)
		{
			_currentGrounded = isGrounded;
		}

		public void TriggerJump()
		{
			JumpTimer = TickTimer.CreateFromSeconds(Runner, 0.1f);
			_hasTriggeredJump = false;
		}
		#endregion

		#region Private Methods
		private void UpdateAnimations(Vector2 movement, bool isSprinting, bool isGrounded)
		{
			var magnitude = movement.magnitude;
			if (magnitude > 0.01f)
			{
				movement /= magnitude;
			}

			// Update animation parameters
			animator.SetFloat(ForwardHash, movement.y);
			animator.SetFloat(SidewaysHash, movement.x);
			animator.SetBool(SprintHash, isSprinting);
			animator.SetBool(GroundedHash, isGrounded);
		}
		#endregion
	}
}