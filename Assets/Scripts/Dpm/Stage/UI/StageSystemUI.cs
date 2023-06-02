using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using UnityEngine;

namespace Dpm.Stage.UI
{
	public class StageSystemUI : MonoBehaviour
	{
		public void OnExitButton()
		{
			CoreService.Event.Publish(ExitStageEvent.Instance);
		}
	}
}