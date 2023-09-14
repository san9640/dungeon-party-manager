using System;
using Dpm.Utility.Event;

namespace Dpm.Stage.UI.Event
{
	public class BattleStartButtonPressedEvent : PooledEvent<BattleStartButtonPressedEvent>
	{
		private Action _deactivateButtonCallback;

		public void DeactivateButton()
		{
			// 한 번 실행했으면 지워줌
			_deactivateButtonCallback?.Invoke();
			_deactivateButtonCallback = null;
		}

		public override void Dispose()
		{
			_deactivateButtonCallback = null;

			base.Dispose();
		}

		public static BattleStartButtonPressedEvent Create(Action deactivateButtonCallback)
		{
			var e = Pool.GetOrCreate();

			e._deactivateButtonCallback = deactivateButtonCallback;

			return e;
		}
	}
}