using Cinemachine;
using UnityEngine;

namespace Camera
{
	public class CameraController : MonoBehaviour
	{
		#region Singleton
		public static CameraController Instance { get; private set; }
		#endregion

		#region Serialized Fields
		[field: Header("Camera References")]
		[field: SerializeField] public UnityEngine.Camera MainCamera { get; private set; }
		[field: SerializeField] public CinemachineVirtualCamera VirtualCamera { get; private set; }

		[Header("Follow Settings")]
		[SerializeField] private Vector3 followOffset = new Vector3(0, 5, -10);
		[SerializeField] private float followDamping = 2f;
		[SerializeField] private float positionSmoothTime = 0.1f;
		[SerializeField] private float rotationSmoothTime = 0.1f;
		#endregion

		#region Private Fields
		private float _currentVerticalAngle;
		private float _targetVerticalAngle;
		private CinemachineTransposer _transposer;
		private Vector3 _currentVelocity;
		private float _currentRotationVelocity;
		private Vector3 _currentOffset;
		#endregion

		#region Unity Lifecycle
		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}

			Instance = this;
			DontDestroyOnLoad(gameObject);

			InitializeVirtualCamera();
		}

		private void Update()
		{
			HandleCameraRotation();
		}
		#endregion

		#region Private Methods
		private void InitializeVirtualCamera()
		{
			if (VirtualCamera != null)
			{
				_transposer = VirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
				if (_transposer != null)
				{
					_transposer.m_FollowOffset = followOffset;
					_transposer.m_XDamping = followDamping;
					_transposer.m_YDamping = followDamping;
					_transposer.m_ZDamping = followDamping;
					_currentOffset = followOffset;
				}
			}
		}

		private void HandleCameraRotation()
		{
			if (_transposer == null) return;

			_currentVerticalAngle = Mathf.SmoothDamp(
				_currentVerticalAngle,
				_targetVerticalAngle,
				ref _currentRotationVelocity,
				rotationSmoothTime
			);

			var rotation = Quaternion.Euler(_currentVerticalAngle, 0, 0);
			var targetOffset = rotation * new Vector3(0, followOffset.y, followOffset.z);

			_currentOffset = Vector3.SmoothDamp(
				_currentOffset,
				targetOffset,
				ref _currentVelocity,
				positionSmoothTime
			);
			_transposer.m_FollowOffset = _currentOffset;
		}
		#endregion

		#region Public Methods
		public void SetFollowTarget(Transform target)
		{
			if (VirtualCamera == null)
			{
				Debug.LogError("Cannot set follow target: Virtual Camera not initialized!");
				return;
			}

			if (target == null)
			{
				Debug.LogError("Cannot set follow target: Target is null!");
				return;
			}

			VirtualCamera.Follow = target;
			VirtualCamera.LookAt = target;
			Debug.Log($"Camera now following: {target.name}");
		}
		#endregion
	}
}