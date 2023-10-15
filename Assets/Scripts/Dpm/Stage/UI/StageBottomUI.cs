using System;
using System.Globalization;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Unit;
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

		private Character CurrentMember {
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
		}

		public void OnHpChanged(Core.Interface.Event e)
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