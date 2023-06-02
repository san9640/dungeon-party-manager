using Dpm.CoreAdapter;
using Dpm.MainMenu.Event;
using Dpm.Utility.Pool;
using UnityEngine;

namespace Dpm.MainMenu.UI
{
	public class MainMenuUI : MonoBehaviour
	{
		private PooledGameObject go;

		public void OnExitButton()
		{
			CoreService.Event.Publish(ExitButtonEvent.Instance);
		}

		public void OnStartButton()
		{
			CoreService.Event.Publish(StartButtonEvent.Instance);
		}

		public void OnOptionButton()
		{
		}
	}
}