using UnityEngine;

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
		#endregion
	}
}