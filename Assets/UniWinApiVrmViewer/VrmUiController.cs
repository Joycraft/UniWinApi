using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kirurobo
{

	public class VrmUiController : MonoBehaviour
	{

		public WindowController windowController;

		public RectTransform panel;
		public Text informationText;
		public Text warningText;
		public Button closeButton;
		public Toggle transparentToggle;
		public Toggle topmostToggle;
		public Toggle maximizeToggle;
		public Button quitButton;
		public Text titleText;

		public Dropdown languageDropdown;

		public Button tabButtonModel;
		public Button tabButtonControl;
		public RectTransform modelPanel;
		public RectTransform controlPanel;

		private float mouseMoveSS = 0f;             // Sum of mouse trajectory squares. [px^2]
		private float mouseMoveSSThreshold = 16f;   // Threshold to be regarded as not moving. [px^2]
		private Vector3 lastMousePosition;

		private bool isDebugMode = false;   // Show debug Info.

		private VRMLoader.VRMPreviewLocale locale;
		private VRMLoader.VRMPreviewUI vrmLoaderUI;

		private static readonly Color UnfocusedTabColor = new Color(0.75f, 0.75f, 0.75f, 1f);
		private static readonly Color FocusedTabColor = Color.white;

		/// <summary>
		/// Use this for initialization
		/// </summary>
		void Start()
		{

			windowController = FindObjectOfType<WindowController>();
			windowController.OnStateChanged += windowController_OnStateChanged;

			locale = this.GetComponentInChildren<VRMLoader.VRMPreviewLocale>();
			vrmLoaderUI = this.GetComponentInChildren<VRMLoader.VRMPreviewUI>();

			// Initialize toggles.
			UpdateUI();

			// Set event listeners.
			if (closeButton) { closeButton.onClick.AddListener(Close);  }
			if (quitButton) { quitButton.onClick.AddListener(Quit); }
			if (transparentToggle) { transparentToggle.onValueChanged.AddListener(windowController.SetTransparent); }
			if (maximizeToggle) { maximizeToggle.onValueChanged.AddListener(windowController.SetMaximized); }
			if (topmostToggle) { topmostToggle.onValueChanged.AddListener(windowController.SetTopmost); }
			if (tabButtonModel) { tabButtonModel.onClick.AddListener(ShowModelTab); }
			if (tabButtonControl) { tabButtonControl.onClick.AddListener(ShowControlTab); }
			if (languageDropdown) { languageDropdown.onValueChanged.AddListener(val => SetLanguage(val)); }

			ShowModelTab();

			// Show menu on startup.
			Show(null);
		}

		private void SetLanguage(int no)
		{
			if (!locale) return;

			switch (no)
			{
				case 0:
					locale.SetLocale("en");
					break;
				case 1:
					locale.SetLocale("ja");
					break;
			}
		}

		private void windowController_OnStateChanged()
		{
			UpdateUI();
		}

		/// <summary>
		/// UIの状況を現在のウィンドウ状態に合わせて更新
		/// </summary>
		public void UpdateUI()
		{
			if (!windowController) return;

			if (transparentToggle) { transparentToggle.isOn = windowController.isTransparent; }
			if (maximizeToggle) { maximizeToggle.isOn = windowController.isMaximized; }
			if (topmostToggle) { topmostToggle.isOn = windowController.isTopmost; }
		}


		/// <summary>
		/// メニューを閉じる
		/// </summary>
		private void Close()
		{
			panel.gameObject.SetActive(false);
		}

		/// <summary>
		/// 終了ボタンが押された時の処理。エディタ上であれば再生停止とする。
		/// </summary>
		private void Quit()
		{
#if UNITY_EDITOR
			// Stop playing for the editor
			UnityEditor.EditorApplication.isPlaying = false;
#else
		// Quit application for the standalone player
		Application.Quit();
#endif
		}

		private void ShowModelTab()
		{
			if (modelPanel) { modelPanel.gameObject.SetActive(true); }
			if (controlPanel) { controlPanel.gameObject.SetActive(false); }

			if (tabButtonControl) { tabButtonControl.image.color = UnfocusedTabColor; }
			if (tabButtonModel) { tabButtonModel.image.color = FocusedTabColor; }
		}

		private void ShowControlTab()
		{
			if (controlPanel) { controlPanel.gameObject.SetActive(true); }
			if (modelPanel) { modelPanel.gameObject.SetActive(false); }

			if (tabButtonControl) { tabButtonControl.image.color = FocusedTabColor; }
			if (tabButtonModel) { tabButtonModel.image.color = UnfocusedTabColor; }
		}

		/// <summary>
		/// Update is called once per frame
		/// </summary>
		void Update()
		{
			// マウス右ボタンクリックでメニューを表示させる。閾値以下の移動ならクリックとみなす。
			if (Input.GetMouseButtonDown(1))
			{
				lastMousePosition = Input.mousePosition;
			}
			if (Input.GetMouseButton(1))
			{
				mouseMoveSS += (Input.mousePosition - lastMousePosition).sqrMagnitude;
			}
			if (Input.GetMouseButtonUp(1))
			{
				if (mouseMoveSS < mouseMoveSSThreshold)
				{
					Show();
				}
				mouseMoveSS = 0f;
			}

			// [ESC] でメニューを閉じる
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				Close();
			}

			// 裏機能
			// メニューのタイトルがマウスカーソル直下の色になる
			if (isDebugMode && windowController && titleText)
			{
				titleText.color = windowController.pickedColor;
				//Debug.Log(windowController.pickedColor);
			}
		}

		/// <summary>
		/// フォーカスが外れたときの処理
		/// </summary>
		/// <param name="focus"></param>
		private void OnApplicationFocus(bool focus)
		{
			// フォーカスが外れたらメニューを閉じる
			if (!focus)
			{
				Close();
			}
		}

		/// <summary>
		/// メニューを表示する
		/// </summary>
		public void Show()
		{
			if (panel)
			{
				panel.gameObject.SetActive(true);
			}

			// 裏機能
			// メニューが表示されるとき、[Shift]が押されていればデバッグ表示を有効にする
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			{
				isDebugMode = true;
			}
			else
			{
				isDebugMode = false;
			}
		}

		/// <summary>
		/// Show the meta information
		/// </summary>
		/// <param name="meta"></param>
		public void Show(VRM.VRMMetaObject meta)
		{
			if (meta)
			{
				if (vrmLoaderUI) vrmLoaderUI.setMeta(meta);
			}

			Show();
		}

		/// <summary>
		/// Set the warning text
		/// </summary>
		/// <param name="message"></param>
		public void SetWarning(string message)
		{
			if (warningText)
			{
				warningText.text = message;
			}
		}
	}
}