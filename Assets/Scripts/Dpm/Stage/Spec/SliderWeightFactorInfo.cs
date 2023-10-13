using System;
using UnityEngine;

namespace Dpm.Stage.Spec
{
	[Serializable]
	public struct SliderWeightFactorInfo
	{
		[Range(0, 1)]
		public float defaultValue;
	}
}