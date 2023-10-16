using System;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Unit.AI;
using UnityEngine;
using UnityEngine.UI;

namespace Dpm.Stage.UI.Calculator
{
	public abstract class AICalculatorUIBase : MonoBehaviour, IDisposable
	{
		[SerializeField]
		private Slider factorSlider;

		public RectTransform RectTransform { get; private set; }

		protected AICalculatorType aiType;

		public AICalculatorType AIType => aiType;

		public Type CalculatorType { get; protected set; }

		protected bool Initialized = false;

		public float FactorValue
		{
			get => factorSlider.value;
			set => factorSlider.value = value;
		}

		public virtual void Init()
		{
			RectTransform = GetComponent<RectTransform>();
		}

		public virtual void Dispose()
		{
			RectTransform = null;
		}

		public void OnFactorChanged(float value)
		{
			var character = StageUIManager.Instance.BottomUI.CurrentMember;

			CoreService.Event.Publish(ChangeAICalculatorFactorEvent.Create(character, CalculatorType, value));
		}
	}
}