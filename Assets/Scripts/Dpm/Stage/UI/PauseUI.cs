using System;
using Dpm.CoreAdapter;
using Dpm.Stage.UI.Event;
using UnityEngine;

namespace Dpm.Stage.UI
{
	public class PauseUI : MonoBehaviour
	{
		public void OnStageExitButtonPressed()
		{
			// FIXME : 여기서 이렇게 불러주는 게 맞나?
			Game.Instance.MoveToMainMenu();
		}

		public void OnResumeButtonPressed()
		{
			CoreService.Event.Publish(ResumeButtonPressedEvent.Instance);
		}
	}
}