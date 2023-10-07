using System;

namespace Dpm.Stage.Unit.Battle
{
	public interface IBattleAction : IDisposable
	{
		void Attack();
	}
}