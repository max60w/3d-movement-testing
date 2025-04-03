using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System.Collections;
using Network;

namespace UI
{
	public class MainMenuWindow : Window
	{
		#region Serialized Fields
		[Header("UI References")]
		[SerializeField] private GameObject loadingUI;
		[SerializeField] private GameObject errorUI;
		[SerializeField] private TMP_Text errorText;
		[SerializeField] private Button joinButton;
		[SerializeField] private TMP_InputField roomIdInput;

		[Header("Game Settings")]
		[SerializeField] private GameMode gameMode = GameMode.Shared;
		[SerializeField] private string defaultRoomId = "Test";
		#endregion

		#region Private Fields
		private NetworkManager _networkManager;
		private bool _isInitialized;
		#endregion

		#region Unity Lifecycle
		protected override void Awake()
		{
			base.Awake();

			// Setup join button
			if (joinButton != null)
			{
				joinButton.onClick.AddListener(HandleJoinClick);
			}

			// Setup room ID input
			if (roomIdInput != null)
			{
				roomIdInput.text = defaultRoomId;
			}

			// Start waiting for NetworkManager
			StartCoroutine(WaitForNetworkManager());
		}

		private IEnumerator WaitForNetworkManager()
		{
			// Wait until NetworkManager is available
			while (_networkManager == null)
			{
				_networkManager = NetworkManager.Instance;
				if (_networkManager == null)
				{
					yield return new WaitForSeconds(0.1f);
				}
			}

			// Subscribe to NetworkManager events
			_networkManager.OnConnectionStarted += HandleConnectionStarted;
			_networkManager.OnConnectionSuccess += HandleConnectionSuccess;
			_networkManager.OnConnectionError += HandleConnectionError;
			_networkManager.OnConnectionEnded += HandleConnectionEnded;

			_isInitialized = true;
			Debug.Log("MainMenuWindow initialized with NetworkManager");
		}

		public override void Show()
		{
			base.Show();
			EnableJoinButton(true);

			// Hide error and loading UI when showing window
			if (errorUI != null) errorUI.SetActive(false);
			if (loadingUI != null) loadingUI.SetActive(false);
		}

		private void EnableJoinButton(bool enable)
		{
			if (joinButton != null)
				joinButton.interactable = enable;
		}

		private void OnDestroy()
		{
			if (_networkManager != null)
			{
				_networkManager.OnConnectionStarted -= HandleConnectionStarted;
				_networkManager.OnConnectionSuccess -= HandleConnectionSuccess;
				_networkManager.OnConnectionError -= HandleConnectionError;
				_networkManager.OnConnectionEnded -= HandleConnectionEnded;
			}
		}
		#endregion

		#region Private Methods
		private void HandleConnectionStarted()
		{
			if (loadingUI != null)
				loadingUI.SetActive(true);
			if (errorUI != null)
				errorUI.SetActive(false);
			EnableJoinButton(false);
		}

		private void HandleConnectionSuccess()
		{
			if (loadingUI != null)
				loadingUI.SetActive(false);
			if (errorUI != null)
				errorUI.SetActive(false);

			// Hide main menu and start game
			Hide();
			if (_networkManager != null)
			{
				_networkManager.StartGame();
			}
		}

		private void HandleConnectionError(string errorMessage)
		{
			if (loadingUI != null)
				loadingUI.SetActive(false);
			if (errorUI != null)
			{
				errorUI.SetActive(true);
				if (errorText != null)
					errorText.text = errorMessage ?? "Connection failed";
			}
			EnableJoinButton(true);
		}

		private void HandleConnectionEnded()
		{
			EnableJoinButton(true);
		}

		private async void HandleJoinClick()
		{
			if (!_isInitialized)
			{
				Debug.LogWarning("Cannot join game: NetworkManager not initialized yet");
				return;
			}

			if (_networkManager == null)
			{
				Debug.LogError("Cannot join game: NetworkManager not available");
				return;
			}

			string roomId = roomIdInput != null ? roomIdInput.text : defaultRoomId;
			if (string.IsNullOrEmpty(roomId))
			{
				roomId = defaultRoomId;
			}

			bool success = await _networkManager.ConnectToGame(gameMode, roomId);
			if (!success)
			{
				Debug.Log("Failed to connect to game");
			}
		}
		#endregion
	}
}