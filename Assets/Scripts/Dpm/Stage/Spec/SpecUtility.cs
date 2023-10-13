using System.Collections.Generic;
using Dpm.CoreAdapter;
using Dpm.Utility;
using UnityEngine;

namespace Dpm.Stage.Spec
{
	public static class SpecUtility
	{
		// FIXME : Type을 가져오면 알아서 specTable에 접근할 수 있도록 한개로 묶어줘야 함
		public static MoveSpec GetMoveSpec(string specName)
		{
			var specTable = CoreService.Asset.UnsafeGet<ScriptableObject>("MoveSpecTable") as MoveSpecTable;

			return specTable.NameToSpec[specName];
		}

		public static AttackSpec GetAttackSpec(string specName)
		{
			var specTable = CoreService.Asset.UnsafeGet<ScriptableObject>("AttackSpecTable") as AttackSpecTable;

			return specTable.NameToSpec[specName];
		}

		public static BattleActionSpec GetBattleActionSpec(string specName)
		{
			var specTable = CoreService.Asset.UnsafeGet<ScriptableObject>("BattleActionSpecTable") as BattleActionSpecTable;

			return specTable.NameToSpec[specName];
		}

		public static CharacterSpec GetCharacterSpec(string specName)
		{
			var specTable = CoreService.Asset.UnsafeGet<ScriptableObject>("CharacterSpecTable") as CharacterSpecTable;

			return specTable.NameToSpec[specName];
		}
	}
}