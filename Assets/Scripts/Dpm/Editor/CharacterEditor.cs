using System;
using System.Collections.Generic;
using Dpm.Stage.Unit;
using Dpm.Stage.Unit.AI;
using Dpm.Stage.Unit.AI.Calculator.Attack;
using Dpm.Stage.Unit.AI.Calculator.Move;
using UnityEditor;
using UnityEngine;

namespace Dpm.Editor
{
	[CustomEditor(typeof(Character))]
	public class CharacterEditor : UnitEditor
	{
		private Character _character;

		private static List<Type> _aiTypes = new()
		{
			typeof(ClosestTargetAttackCalculator),
			typeof(HighHpTargetAttackCalculator),
			typeof(LowHpTargetAttackCalculator),
			typeof(WeakestTargetAttackCalculator),
			typeof(MeleeTargetAttackCalculator),
			typeof(RangedTargetAttackCalculator),
			typeof(AwayFromWallMoveCalculator),
			typeof(RetreatMoveCalculator),
		};

		protected override void OnEnable()
		{
			base.OnEnable();

			_character = target as Character;
		}

		protected override void OnDisable()
		{
			_character = null;

			base.OnDisable();
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (!Application.isPlaying || _character.DecisionMaker is not DecisionMaker dm)
			{
				return;
			}

			EditorGUILayout.Space();

			EditorGUILayout.LabelField("MaxHp", _character.MaxHp.ToString());
			EditorGUILayout.LabelField("Hp", _character.Hp.ToString());
			EditorGUILayout.LabelField("AttackSpeed", $"{_character.AttackSpeed:N3}");
			EditorGUILayout.LabelField("AttackDamage", _character.AttackDamage.ToString());

			foreach (var type in _aiTypes)
			{
				if (_character.DecisionMaker.IsUsingTyped(type))
				{
					var calculator = dm.GetCalculatorTyped(type);

					EditorGUILayout.Space();
					EditorGUILayout.LabelField("Type", type.Name);

					if (calculator is IAIMoveCalculator move)
					{
						if (move.TargetPos.HasValue)
						{
							EditorGUILayout.LabelField("TargetDir", (move.TargetPos.Value - _character.Position).normalized.ToString());
						}
						else
						{
							EditorGUILayout.LabelField("TargetDir", Vector2.zero.ToString());
						}
					}
					else if (calculator is IAIAttackCalculator attack)
					{
						EditorGUILayout.LabelField("AttackTarget", attack.CurrentTarget?.Name ?? "None");
					}

					var result = dm.ResultOnThisFrame[calculator];
					var factor = dm.GetScoreFactor(type);

					EditorGUILayout.LabelField("CurrentScore", $"{result.score}");
					EditorGUILayout.LabelField("AverageScore", $"{result.sum / result.frameCount}");
					EditorGUILayout.LabelField("Factor", $"{factor}");
					EditorGUILayout.LabelField("Score X Factor", $"{ result.score * factor}");
				}
			}
		}
	}
}