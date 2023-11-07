using System;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.UI.Event;
using UnityEngine;

namespace Dpm.Stage.UI
{
	public class PauseUI : MonoBehaviour
	{
		public void OnStageExitButtonPressed()
		{
			CoreService.Event.Publish(ExitStageEvent.Instance);
		}

		public void OnResumeButtonPressed()
		{
			CoreService.Event.Publish(ResumeButtonPressedEvent.Instance);
		}
	}
}