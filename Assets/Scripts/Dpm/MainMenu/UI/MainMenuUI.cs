using Dpm.MainMenu.Event;
using UnityEngine;

namespace Dpm.MainMenu.UI
{
	public class MainMenuUI : MonoBehaviour
	{
		public void OnExitButton()
		{
			Game.Instance.EventSystem.Publish(ExitButtonEvent.Instance);
		}
	}
}