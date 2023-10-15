using System;
using Dpm.Common.Event;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.UI.Event;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

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

		[SerializeField]
		private EventSystem eventSystem;

		private bool _canStartBattle = false;

		public StageUIState CurrentState { get; private set; } = StageUIState.None;

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
			ChangeState(StageUIState.NoneInteractable);

			bottomUI.Init();

			CoreService.Event.Subscribe<ScreenFadeInEndEvent>(OnScreenFadeInEnd);
			CoreService.Event.Subscribe<ScreenFadeOutStartEvent>(OnScreenFadeOutStart);
			CoreService.Event.Subscribe<ResumeButtonPressedEvent>(OnResumeButtonPressedEvent);
		}

		public void Dispose()
		{
			CoreService.Event.Unsubscribe<ScreenFadeInEndEvent>(OnScreenFadeInEnd);
			CoreService.Event.Unsubscribe<ScreenFadeOutStartEvent>(OnScreenFadeOutStart);
			CoreService.Event.Unsubscribe<ResumeButtonPressedEvent>(OnResumeButtonPressedEvent);
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
					bottomUI.gameObject.SetActive(false);
					pauseButton.gameObject.SetActive(false);

					break;

				case StageUIState.NoneInteractable:
					eventSystem.enabled = false;
					pauseUI.gameObject.SetActive(false);
					bottomUI.gameObject.SetActive(true);
					pauseButton.gameObject.SetActive(true);

					break;

				case StageUIState.Interactable:
					eventSystem.enabled = true;
					pauseUI.gameObject.SetActive(false);
					bottomUI.gameObject.SetActive(true);
					pauseButton.gameObject.SetActive(true);

					break;

				case StageUIState.Paused:
					eventSystem.enabled = true;
					pauseUI.gameObject.SetActive(true);
					bottomUI.gameObject.SetActive(false);
					pauseButton.gameObject.SetActive(false);

					break;
			}
		}
	}
}