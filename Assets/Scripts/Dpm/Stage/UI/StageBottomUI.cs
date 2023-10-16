using System;
using System.Collections.Generic;
using System.Globalization;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.UI.Calculator;
using Dpm.Stage.UI.Calculator.Attack;
using Dpm.Stage.UI.Calculator.Move;
using Dpm.Stage.Unit;
using Dpm.Stage.Unit.AI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dpm.Stage.UI
{
	public class StageBottomUI : MonoBehaviour, IDisposable
	{
		private int _currentMemberIndex = -1;

		[SerializeField]
		private Image portrait;

		[SerializeField]
		private TextMeshProUGUI hpText;

		[SerializeField]
		private TextMeshProUGUI mpText;

		[SerializeField]
		private TextMeshProUGUI atkText;

		[SerializeField]
		private TextMeshProUGUI asdText;

		[SerializeField]
		private Button moveAIButton;

		[SerializeField]
		private Button attackAIButton;

		[SerializeField]
		private Button abilityAIButton;

		[SerializeField]
		private List<AICalculatorUIBase> aiCalculatorUIs;

		private float _aiCalculatorUIMaxY;

		private const float AICalculatorUIYGap = -10f;

		public Character CurrentMember {
			get
			{
				var members = UnitManager.Instance.AllyParty.Members;

				if (_currentMemberIndex < 0 || members.Count <= _currentMemberIndex)
				{
					return null;
				}

				return UnitManager.Instance.AllyParty.Members[_currentMemberIndex];
			}
		}

		public void Init()
		{
			_currentMemberIndex = 0;

			UpdateMemberInfo();

			_aiCalculatorUIMaxY = float.MinValue;

			foreach (var ui in aiCalculatorUIs)
			{
				ui.Init();

				if (ui.RectTransform.localPosition.y > _aiCalculatorUIMaxY)
				{
					_aiCalculatorUIMaxY = ui.RectTransform.localPosition.y;
				}
			}

			ChangeAIContents(AICalculatorType.Move);

			CoreService.Event.Subscribe<HpChangedEvent>(OnHpChanged);
		}

		public void Dispose()
		{
			CoreService.Event.Unsubscribe<HpChangedEvent>(OnHpChanged);
		}

		public void OnMemberChangeButton(bool isNext)
		{
			if (_currentMemberIndex < 0)
			{
				return;
			}

			var indexAdder = isNext ? 1 : -1;

			_currentMemberIndex += indexAdder;

			var members = UnitManager.Instance.AllyParty.Members;

			// 나머지 연산 써도되지만 계산 검증이 귀찮음
			if (_currentMemberIndex < 0)
			{
				_currentMemberIndex = members.Count - 1;
			}
			else if (_currentMemberIndex >= members.Count)
			{
				_currentMemberIndex = 0;
			}

			UpdateMemberInfo();

			// FIXME : 편의성 아작남
			ChangeAIContents(AICalculatorType.Move);
		}

		public void OnMoveAIButton()
		{
			OnChangeAITabButton(AICalculatorType.Move);
		}

		public void OnAttackAIButton()
		{
			OnChangeAITabButton(AICalculatorType.Attack);
		}

		public void OnAbilityAIButton()
		{
			OnChangeAITabButton(AICalculatorType.Ability);
		}

		private void OnChangeAITabButton(AICalculatorType type)
		{
			ChangeAIContents(type);
		}

		private Button GetAIChangeButton(AICalculatorType type)
		{
			return type switch
			{
				AICalculatorType.Move => moveAIButton,
				AICalculatorType.Attack => attackAIButton,
				AICalculatorType.Ability => abilityAIButton,
				_ => null
			};
		}

		private void ChangeAIContents(AICalculatorType type)
		{
			moveAIButton.interactable = true;
			attackAIButton.interactable = true;
			abilityAIButton.interactable = true;

			var changeButton = GetAIChangeButton(type);

			changeButton.interactable = false;

			// 필요한 Calculator들만 켜주고 나머지는 끈다.
			var nextY = _aiCalculatorUIMaxY;

			var character = StageUIManager.Instance.BottomUI.CurrentMember;

			var decisionMaker = character.DecisionMaker;

			foreach (var ui in aiCalculatorUIs)
			{
				if (ui.AIType == type && decisionMaker.IsUsingTyped(ui.CalculatorType))
				{
					ui.gameObject.SetActive(true);

					var origin = ui.RectTransform.localPosition;

					ui.RectTransform.localPosition = new Vector3(origin.x, nextY, origin.z);

					nextY = nextY - ui.RectTransform.sizeDelta.y + AICalculatorUIYGap;

					var scoreFactor = decisionMaker.GetScoreFactor(ui.CalculatorType);

					ui.FactorValue = scoreFactor;
				}
				else
				{
					ui.gameObject.SetActive(false);
				}
			}
		}

		private void OnHpChanged(Core.Interface.Event e)
		{
			if (!(e is HpChangedEvent hce) || hce.Character != CurrentMember)
			{
				return;
			}

			SetHpText(hce.Character.Hp, hce.Character.MaxHp);
		}

		public void OnMpChanged(Core.Interface.Event e)
		{

		}

		public void OnAttackDamageChanged(Core.Interface.Event e)
		{

		}

		public void OnAttackSpeedChanged(Core.Interface.Event e)
		{

		}

		private void UpdateMemberInfo()
		{
			var character = CurrentMember;

			SetHpText(character.Hp, character.MaxHp);
			SetMpText(character.Mp, character.MaxMp);
			SetAttackDamageText(character.AttackDamage);
			SetAttackSpeedText(character.AttackSpeed);

			portrait.sprite = character.Animator.Portrait;
		}

		private void SetHpText(int current, int max)
		{
			hpText.text = $"{current} / {max}";
		}

		private void SetMpText(int current, int max)
		{
			mpText.text = $"{current} / {max}";
		}

		private void SetAttackDamageText(int value)
		{
			atkText.text = value.ToString();
		}

		private void SetAttackSpeedText(float value)
		{
			asdText.text = value.ToString(CultureInfo.InvariantCulture);
		}
	}
}