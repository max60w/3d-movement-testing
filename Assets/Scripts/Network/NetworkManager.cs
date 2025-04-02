using System;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;

namespace Network
{
	public class NetworkManager : MonoBehaviour
	{
		#region Singleton
		public static NetworkManager Instance { get; private set; }
		public NetworkRunner NetworkRunner => _networkRunner;
		#endregion

		#region Serialized Fields
		[Header("Network Settings")]
		[SerializeField] private string sessionName = "TestRoom";

		[Header("References")]
		[SerializeField] private SpawnManager spawnManager;
		#endregion

		#region Public Fields

		public NetworkObject LocalCharacter { get; private set; }
		#endregion

		#region Private Fields
		private NetworkRunner _networkRunner;
		private bool _isConnecting;
		#endregion

		#region Events
		public event Action OnConnectionStarted;
		public event Action OnConnectionSuccess;
		public event Action<string> OnConnectionError;
		public event Action OnConnectionEnded;
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

			_networkRunner = gameObject.GetComponent<NetworkRunner>();

			// Initialize SpawnManager with NetworkRunner
			if (spawnManager != null)
			{
				spawnManager.Initialize(_networkRunner);
			}
			else
			{
				Debug.LogError("SpawnManager not assigned to NetworkManager");
			}
		}
		#endregion

		#region Public Methods
		public async Task<bool> ConnectToGame(GameMode gameMode)
		{
			if (_isConnecting) return false;
			_isConnecting = true;

			try
			{
				OnConnectionStarted?.Invoke();

				// Start or join game
				var result = await _networkRunner.StartGame(new StartGameArgs()
				{
					GameMode = gameMode,
					SessionName = sessionName
				});

				if (result.Ok)
				{
					OnConnectionSuccess?.Invoke();
					return true;
				}
				else
				{
					throw new Exception($"Failed to start game: {result.ShutdownReason}");
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"Network error: {e.Message}");
				OnConnectionError?.Invoke(e.Message);
				return false;
			}
			finally
			{
				_isConnecting = false;
				OnConnectionEnded?.Invoke();
			}
		}

		public void StartGame()
		{
			if (spawnManager != null)
			{
				LocalCharacter = spawnManager.SpawnPlayer();
			}
			else
			{
				Debug.LogError("SpawnManager not assigned to NetworkManager");
			}
		}

		public async void RetryConnection(GameMode gameMode)
		{
			OnConnectionError?.Invoke(null); // Clear any previous error
			await ConnectToGame(gameMode);
		}
		#endregion
	}
}