using System;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.UI.Event;
using Dpm.User;
using TMPro;
using UnityEngine;

namespace Dpm.Stage.UI
{
	public class GameOverUI : MonoBehaviour
	{
		[SerializeField]
		private TextMeshProUGUI scoreText;

		[SerializeField]
		private TextMeshProUGUI highScoreText;

		[SerializeField]
		private GameObject newHighScore;

		public void Show(int score, bool isHighScore)
		{
			newHighScore.SetActive(isHighScore);

			scoreText.text = score.ToString();
			highScoreText.text = UserData.HighScore.ToString();

			// TODO : PANEL FADE IN
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