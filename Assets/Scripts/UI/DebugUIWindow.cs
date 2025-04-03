using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Network;
using Gameplay;

namespace UI
{
	public class DebugUIWindow : Window
	{
		#region Serialized Fields
		[Header("Username Settings")]
		[SerializeField] private TMP_InputField usernameField;
		[SerializeField] private Button updateUsernameButton;

		[Header("Color Settings")]
		[SerializeField] private FlexibleColorPicker colorPicker;

		[Header("Window Controls")]
		[SerializeField] private Button collapseButton;
		[SerializeField] private Button expandButton;
		[SerializeField] private CanvasGroup expandedContentGroup;
		[SerializeField] private CanvasGroup collapsedContentGroup;
		#endregion

		#region Private Fields
		private bool isCollapsed;
		private PlayerController localPlayer;
		#endregion

		#region Unity Lifecycle
		protected override void Awake()
		{
			base.Awake();
			InitializeUI();
		}

		private void Update()
		{
			// Try to find the local player if we don't have it yet
			if (localPlayer == null && NetworkManager.Instance != null && NetworkManager.Instance.LocalCharacter != null)
			{
				localPlayer = NetworkManager.Instance.LocalCharacter.GetComponent<PlayerController>();
			}
		}
		#endregion

		#region Private Methods
		private void InitializeUI()
		{
			updateUsernameButton.onClick.AddListener(UpdateUsername);
			colorPicker.onColorChange.AddListener(UpdateCharacterColor);
			collapseButton.onClick.AddListener(CollapseWindow);
			expandButton.onClick.AddListener(ExpandWindow);

			// Set initial state to expanded
			isCollapsed = false;
			UpdateWindowState();
		}

		private void UpdateUsername()
		{
			if (string.IsNullOrEmpty(usernameField.text)) return;

			if (localPlayer != null)
			{
				localPlayer.SetUsername(usernameField.text);
			}
		}

		private void UpdateCharacterColor(Color color)
		{
			if (localPlayer != null)
			{
				localPlayer.SetColor(color);
			}
		}

		private void CollapseWindow()
		{
			isCollapsed = true;
			UpdateWindowState();
		}

		private void ExpandWindow()
		{
			isCollapsed = false;
			UpdateWindowState();
		}

		private void UpdateWindowState()
		{
			// Update expanded content
			expandedContentGroup.alpha = isCollapsed ? 0f : 1f;
			expandedContentGroup.interactable = !isCollapsed;
			expandedContentGroup.blocksRaycasts = !isCollapsed;

			// Update collapsed content
			collapsedContentGroup.alpha = isCollapsed ? 1f : 0f;
			collapsedContentGroup.interactable = isCollapsed;
			collapsedContentGroup.blocksRaycasts = isCollapsed;

			// Update button visibility
			collapseButton.gameObject.SetActive(!isCollapsed);
			expandButton.gameObject.SetActive(isCollapsed);
		}
		#endregion
	}
}