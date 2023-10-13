using System;
using Core.Interface;
using Dpm.Stage.Spec;

namespace Dpm.Stage.Unit.Battle
{
	public interface IBattleAction : IDisposable, IEventListener
	{
		BattleActionSpec Spec { get; }

		void Init(Character character, BattleActionSpec spec);

		void Reset();

		float Dps { get; }
	}
}