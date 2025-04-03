using UnityEngine;
using Network;

namespace UI
{
	public class UIController : MonoBehaviour
	{
		#region Unity Lifecycle
		private void Start()
		{
			// Show main menu window when the game starts
			UIManager.Instance.ShowWindow(WindowType.MainMenu);
		}

		private void OnEnable()
		{
			// Subscribe to network events
			NetworkManager.Instance.OnConnectionSuccess += OnConnectionSuccess;
		}

		private void OnDisable()
		{
			// Unsubscribe from network events
			if (NetworkManager.Instance != null)
			{
				NetworkManager.Instance.OnConnectionSuccess -= OnConnectionSuccess;
			}
		}
		#endregion

		#region Event Handlers
		private void OnConnectionSuccess()
		{
			// Show debug window when connected to the game
			UIManager.Instance.ShowWindow(WindowType.Debug);
		}
		#endregion
	}
}