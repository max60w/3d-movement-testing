using Camera;
using Fusion;
using UnityEngine;

namespace Network
{
	public class SpawnManager : MonoBehaviour
	{
		#region Serialized Fields
		[Header("Spawn Settings")]
		[SerializeField] private Vector2 spawnAreaSize = new Vector2(10f, 10f);
		[SerializeField] private float spawnHeight = 1f;
		[SerializeField] private NetworkPrefabRef playerPrefabRef;
		#endregion

		#region Private Fields
		private NetworkRunner _networkRunner;
		#endregion

		#region Public Methods
		public void Initialize(NetworkRunner networkRunner)
		{
			_networkRunner = networkRunner;
		}

		public void SpawnPlayer()
		{
			if (_networkRunner == null)
			{
				Debug.LogError("Cannot spawn player: NetworkRunner not initialized");
				return;
			}

			// Generate random position within spawn area
			float randomX = Random.Range(-spawnAreaSize.x * 0.5f, spawnAreaSize.x * 0.5f);
			float randomZ = Random.Range(-spawnAreaSize.y * 0.5f, spawnAreaSize.y * 0.5f);
			Vector3 spawnPosition = new Vector3(randomX, spawnHeight, randomZ);

			// Random rotation around Y axis
			Quaternion spawnRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

			// Spawn player
			var player = _networkRunner.Spawn(playerPrefabRef, spawnPosition, spawnRotation, _networkRunner.LocalPlayer);
			CameraController.Instance.SetFollowTarget(player.transform);
		}
		#endregion

		#region Unity Lifecycle
		private void OnDrawGizmos()
		{
			// Visualize spawn area in editor
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(Vector3.up * spawnHeight, new Vector3(spawnAreaSize.x, 0.1f, spawnAreaSize.y));
		}
		#endregion
	}
}