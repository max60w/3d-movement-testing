using UnityEngine;

namespace UI
{
	public abstract class Window : MonoBehaviour
	{
		#region Unity Lifecycle
		protected virtual void Awake() { }
		#endregion

		#region Public Methods
		public virtual void Show()
		{
			gameObject.SetActive(true);
		}

		public virtual void Hide()
		{
			gameObject.SetActive(false);
		}
		#endregion
	}
}