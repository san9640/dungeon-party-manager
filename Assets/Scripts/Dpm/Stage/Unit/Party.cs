using System;
using System.Collections.Generic;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Unit.State;

namespace Dpm.Stage.Unit
{
	public class Party : IDisposable
	{
		public UnitRegion Region { get; private set; }

		public List<Character> Members { get; private set; }

		public Party(UnitRegion region, List<Character> members)
		{
			Region = region;
			Members = members;

			foreach (var member in Members)
			{
				member.Region = region;
			}

			CoreService.Event.Subscribe<CharacterEliminatedEvent>(OnCharacterEliminated);
		}

		public void Dispose()
		{
			CoreService.Event.Unsubscribe<CharacterEliminatedEvent>(OnCharacterEliminated);

			Members?.Clear();
			Members = null;
		}

		private void OnCharacterEliminated(Core.Interface.Event e)
		{
			if (e is not CharacterEliminatedEvent cee)
			{
				return;
			}

			if (cee.Character.Region == Region)
			{
				var allEliminated = true;

				foreach (var member in Members)
				{
					if (member.CurrentState is not CharacterDeadState)
					{
						allEliminated = false;
						break;
					}
				}

				if (allEliminated)
				{
					CoreService.Event.PublishImmediate(PartyEliminatedEvent.Create(this));
				}
			}
		}
	}
}