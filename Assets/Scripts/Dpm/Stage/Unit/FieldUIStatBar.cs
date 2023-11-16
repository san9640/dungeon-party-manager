using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Utility.Constants;
using UnityEngine;
using UnityEngine.UI;

namespace Dpm.Stage.Unit
{
	public class FieldUIStatBar : MonoBehaviour
	{
		[SerializeField]
		private GameObject hpBar;

		private float HpValue
		{
			get => hpBar.transform.localScale.x;
			set => hpBar.transform.localScale =
				new Vector3(value, hpBar.transform.localScale.y, hpBar.transform.localScale.z);
		}

		private Character _character;

		public void Init(Character character)
		{
			_character = character;

			HpValue = _character.HpRatio;

			var hpColor = hpBar.GetComponent<SpriteRenderer>().color;

			hpColor = _character.Direction == Direction.Right ? Color.blue : Color.red;
			
			hpBar.GetComponent<SpriteRenderer>().color = hpColor;
			
			CoreService.Event.Subscribe<HpChangedEvent>(OnHpChangedEvent);
		}

		public void Dispose()
		{
			CoreService.Event.Unsubscribe<HpChangedEvent>(OnHpChangedEvent);

			_character = null;
		}

		private void OnHpChangedEvent(Core.Interface.Event e)
		{
			if (e is not HpChangedEvent hce || hce.Character != _character)
			{
				return;
			}

			HpValue = hce.Character.HpRatio;
		}
	}
}