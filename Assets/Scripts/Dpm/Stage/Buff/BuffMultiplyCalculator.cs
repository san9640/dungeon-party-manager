using System;
using System.Collections.Generic;
using Dpm.Utility.Extensions;
using UnityEngine;

namespace Dpm.Stage.Buff
{
	public class BuffMultiplyCalculator : IDisposable
	{
		private readonly Dictionary<string, float> _removableRequests = new();

		public float Value => _permanentValue * _removableValue;

		private float _permanentValue = 1;

		private float _removableValue = 1;

		public void Dispose()
		{
			_removableRequests.Clear();
		}

		public void AddBuff(string key, float value)
		{
			if (_removableRequests.TryAdd(key, value))
			{
				_removableValue *= 1 + value;
			}
		}

		public void RemoveBuff(string key)
		{
			if (_removableRequests.Remove(key, out var value))
			{
				_removableValue /= 1 + value;
			}
		}

		public void AddPermanentBuff(float value)
		{
			_permanentValue *= 1 + value;
		}
	}
}