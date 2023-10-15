using Dpm.Utility.Extensions;
using UnityEngine;

namespace Dpm.Stage.Unit.AI
{
	public static class AIDebugUtility
	{
		public static void DrawMoveAIInfo(Character character, Vector2? targetPos)
		{
#if UNITY_EDITOR
			if (targetPos.HasValue)
			{
				var startPos = character.Position.ConvertToVector3();
				var endPos = targetPos.Value.ConvertToVector3();

				Gizmos.color = character.Region == UnitRegion.Ally ? Color.green : Color.magenta;

				Gizmos.DrawLine(startPos, endPos);

				Gizmos.DrawWireSphere(endPos, 0.2f);
			}
#endif
		}

		public static void DrawAttackAIInfo(Character character, IUnit target)
		{
#if UNITY_EDITOR
			if (target != null)
			{
				var startPos = character.Position.ConvertToVector3();
				var endPos = target.Position.ConvertToVector3();

				Gizmos.color = character.Region == UnitRegion.Ally ? Color.blue : Color.red;

				Gizmos.DrawLine(startPos, endPos);
			}
#endif
		}
	}
}