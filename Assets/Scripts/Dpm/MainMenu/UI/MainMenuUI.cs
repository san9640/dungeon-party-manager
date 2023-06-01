using Core.Interface;
using Dpm.CoreAdapter;
using Dpm.MainMenu.Event;
using UnityEngine;

namespace Dpm.MainMenu.UI
{
	public class MainMenuUI : MonoBehaviour
	{
		public void OnExitButton()
		{
			CoreService.Event.Publish(ExitButtonEvent.Instance);
		}
	}
}