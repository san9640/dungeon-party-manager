using System;
using System.Collections;
using Dpm.Common.Event;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Render;
using Dpm.Stage.UI.Event;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Dpm.Stage.UI
{
	public enum StageUIState
	{
		None,
		// 보이긴 하나 모든 UI를 사용할 수 없음
		NoneInteractable,
		// Pause되지 않고, StageUI를 사용할 수 있음
		Interactable,
		// Pause된 상태에서는 OptionUI만 사용할 수 있음
		Paused,
		// 게임 오버 시
		GameOver,
	}

	public class StageUIManager : MonoBehaviour, IDisposable
	{
		[SerializeField]
		private PauseUI pauseUI;

		[SerializeField]
		private GameObject pauseButton;

		[SerializeField]
		private GameObject battleStartButton;

		[SerializeField]
		private StageBottomUI bottomUI;

		public StageBottomUI BottomUI => bottomUI;

		[SerializeField]
		private Button bottomUIShowButton;

		[SerializeField]
		private Button bottomUIHideButton;

		[SerializeField]
		private GameOverUI gameOverUI;

		[SerializeField]
		private EventSystem eventSystem;

		private bool _canStartBattle = false;

		public StageUIState CurrentState { get; private set; } = StageUIState.None;

		private enum BottomUIState
		{
			Hidden,
			Showing,
			Disabled,
		}

		private BottomUIState _currentBottomUIState = BottomUIState.Disabled;

		/// <summary>
		/// FIXME : 싱글톤이 아닌 다른 방식을 쓰는 게 낫지 않을까?
		/// </summary>
		public static StageUIManager Instance { get; private set; }

		private void Awake()
		{
			Instance = this;
		}

		private void OnDestroy()
		{
			Instance = null;
		}

		public void Init()
		{
			bottomUI.Init();

			ChangeState(StageUIState.NoneInteractable);

			CoreService.Event.Subscribe<ScreenFadeInEndEvent>(OnScreenFadeInEnd);
			CoreService.Event.Subscribe<ScreenFadeOutStartEvent>(OnScreenFadeOutStart);
			CoreService.Event.Subscribe<ResumeButtonPressedEvent>(OnResumeButtonPressedEvent);
			CoreService.Event.Subscribe<GameOverEvent>(OnGameOverEvent);
		}

		public void Dispose()
		{
			CoreService.Event.Unsubscribe<ScreenFadeInEndEvent>(OnScreenFadeInEnd);
			CoreService.Event.Unsubscribe<ScreenFadeOutStartEvent>(OnScreenFadeOutStart);
			CoreService.Event.Unsubscribe<ResumeButtonPressedEvent>(OnResumeButtonPressedEvent);
			CoreService.Event.Unsubscribe<GameOverEvent>(OnGameOverEvent);

			bottomUI.Dispose();
		}

		public void OnBattleStartButtonPressed()
		{
			CoreService.Event.Publish(BattleStartButtonPressedEvent.Create(() =>
			{
				battleStartButton.SetActive(false);
				_canStartBattle = false;
			}));
		}

		public void OnPauseButtonPressed()
		{
			if (CurrentState != StageUIState.Interactable)
			{
				return;
			}

			ChangeState(StageUIState.Paused);

			battleStartButton.SetActive(false);

			CoreService.Event.Publish(PauseButtonPressedEvent.Instance);
		}

		public void OnBottomUIShowButtonPressed()
		{
			ChangeBottomUIState(BottomUIState.Showing);
		}

		public void OnBottomUIHideButtonPressed()
		{
			ChangeBottomUIState(BottomUIState.Hidden);
		}

		private void OnScreenFadeInEnd(Core.Interface.Event e)
		{
			if (e is not ScreenFadeInEndEvent)
			{
				return;
			}

			ChangeState(StageUIState.Interactable);

			_canStartBattle = true;
			battleStartButton.SetActive(true);
		}

		private void OnScreenFadeOutStart(Core.Interface.Event e)
		{
			if (e is not ScreenFadeOutStartEvent)
			{
				return;
			}

			ChangeState(StageUIState.NoneInteractable);
		}

		private void OnResumeButtonPressedEvent(Core.Interface.Event e)
		{
			if (CurrentState != StageUIState.Paused)
			{
				return;
			}

			ChangeState(StageUIState.Interactable);

			battleStartButton.SetActive(_canStartBattle);
		}

		private void OnGameOverEvent(Core.Interface.Event e)
		{
			if (e is not GameOverEvent goe)
			{
				return;
			}

			ChangeState(StageUIState.GameOver);

			CoreService.Coroutine.StartCoroutine(ShowGameOverUI(goe.Score));
		}

		private void ChangeState(StageUIState nextState)
		{
			if (CurrentState == nextState)
			{
				return;
			}

			CurrentState = nextState;

			switch (CurrentState)
			{
				case StageUIState.None:
					eventSystem.enabled = false;
					pauseUI.gameObject.SetActive(false);
					pauseButton.gameObject.SetActive(false);

					ChangeBottomUIState(BottomUIState.Disabled);

					break;

				case StageUIState.NoneInteractable:
					eventSystem.enabled = false;
					pauseUI.gameObject.SetActive(false);
					pauseButton.gameObject.SetActive(true);

					ChangeBottomUIState(BottomUIState.Hidden);

					break;

				case StageUIState.Interactable:
					eventSystem.enabled = true;
					pauseUI.gameObject.SetActive(false);
					pauseButton.gameObject.SetActive(true);

					ChangeBottomUIState(BottomUIState.Hidden);

					break;

				case StageUIState.Paused:
					eventSystem.enabled = true;
					pauseUI.gameObject.SetActive(true);
					pauseButton.gameObject.SetActive(false);

					ChangeBottomUIState(BottomUIState.Disabled);

					break;

				case StageUIState.GameOver:
					eventSystem.enabled = true;
					pauseUI.gameObject.SetActive(false);
					pauseButton.gameObject.SetActive(false);

					ChangeBottomUIState(BottomUIState.Disabled);

					break;
			}
		}

		private void ChangeBottomUIState(BottomUIState nextState)
		{
			if (_currentBottomUIState == nextState)
			{
				return;
			}

			_currentBottomUIState = nextState;

			switch (_currentBottomUIState)
			{
				case BottomUIState.Hidden:
					bottomUI.gameObject.SetActive(false);
					bottomUIShowButton.gameObject.SetActive(true);
					bottomUIHideButton.gameObject.SetActive(false);

					StageCamera.Instance.ChangeToDefaultMode();

					break;
				case BottomUIState.Showing:
					bottomUI.gameObject.SetActive(true);
					bottomUIShowButton.gameObject.SetActive(false);
					bottomUIHideButton.gameObject.SetActive(true);

					StageCamera.Instance.ChangeToControlMode();

					break;
				case BottomUIState.Disabled:
					bottomUI.gameObject.SetActive(false);
					bottomUIShowButton.gameObject.SetActive(false);
					bottomUIHideButton.gameObject.SetActive(false);

					StageCamera.Instance.ChangeToDefaultMode();

					break;
			}
		}

		private IEnumerator ShowGameOverUI(int score)
		{
			yield return new WaitForSeconds(1.0f);

			gameOverUI.Show(score);
		}
	}
}