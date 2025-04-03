using UnityEngine;
using System.Collections.Generic;

namespace UI
{
	public class UIManager : MonoBehaviour
	{
		#region Singleton

		public static UIManager Instance { get; private set; }

		#endregion

		#region Serialized Fields

		[Header("Windows")]
		[SerializeField] private MainMenuWindow mainMenuWindow;
		[SerializeField] private DebugUIWindow debugWindow;

		#endregion

		#region Private Fields

		private readonly Dictionary<WindowType, Window> _windows = new();

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

			InitializeWindows();
		}

		#endregion

		#region Private Methods

		private void InitializeWindows()
		{
			if (mainMenuWindow != null)
			{
				_windows[WindowType.MainMenu] = mainMenuWindow;
			}

			if (debugWindow != null)
			{
				_windows[WindowType.Debug] = debugWindow;
			}
		}

		#endregion

		#region Public Methods

		public void ShowWindow(WindowType windowType)
		{
			if (_windows.TryGetValue(windowType, out var window))
			{
				window.Show();
			}
		}

		public void HideWindow(WindowType windowType)
		{
			if (_windows.TryGetValue(windowType, out var window))
			{
				window.Hide();
			}
		}

		public T GetWindow<T>(WindowType windowType) where T : Window
		{
			if (_windows.TryGetValue(windowType, out var window) && window is T typedWindow)
			{
				return typedWindow;
			}

			return null;
		}

		#endregion
	}

	public enum WindowType
	{
		MainMenu,
		Debug
	}
}
