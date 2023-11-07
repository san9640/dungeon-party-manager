using System;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.UI.Event;
using TMPro;
using UnityEngine;

namespace Dpm.Stage.UI
{
	public class GameOverUI : MonoBehaviour
	{
		[SerializeField]
		private TextMeshProUGUI scoreText;

		public void Show(int score)
		{
			// TODO : PANEL FADE IN
			scoreText.text = score.ToString();

			gameObject.SetActive(true);
		}

		public void OnExitButtonPressed()
		{
			CoreService.Event.Publish(ExitStageEvent.Instance);
		}

		public void OnRetryButtonPressed()
		{
			CoreService.Event.Publish(RetryButtonPressedEvent.Instance);
		}
	}
}