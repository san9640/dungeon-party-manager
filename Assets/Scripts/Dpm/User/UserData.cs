using UnityEngine;

namespace Dpm.User
{
	public static class UserData
	{
		private static class Constants
		{
			public const string HighScorePrefName = "highScore";
		}

		public static int HighScore
		{
			get => PlayerPrefs.GetInt(Constants.HighScorePrefName);
			set => PlayerPrefs.SetInt(Constants.HighScorePrefName, value);
		}
	}
}