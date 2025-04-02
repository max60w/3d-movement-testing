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

			// Set initial virtual camera settings
			if (VirtualCamera != null)
			{
				var transposer = VirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
				if (transposer != null)
				{
					transposer.m_FollowOffset = followOffset;
					transposer.m_XDamping = followDamping;
					transposer.m_YDamping = followDamping;
					transposer.m_ZDamping = followDamping;
				}
			}
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