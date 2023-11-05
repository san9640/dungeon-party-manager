using System;
using System.Collections.Generic;

namespace Dpm.Stage.Buff
{
	public class BuffAddCalculator : IDisposable
	{
		private readonly Dictionary<string, int> _removableRequests = new();

		public int Value => _permanentValue + _removableValue;

		private int _permanentValue = 0;

		private int _removableValue = 0;

		public void Dispose()
		{
			_removableRequests.Clear();
		}

		public void AddBuff(string key, int value)
		{
			if (_removableRequests.TryAdd(key, value))
			{
				_removableValue += value;
			}
			else
			{
				_removableValue -= _removableRequests[key];
				_removableRequests[key] = value;
				_removableValue += value;
			}
		}

		public void RemoveBuff(string key)
		{
			if (_removableRequests.Remove(key, out var value))
			{
				_removableValue -= value;
			}
		}

		public void AddPermanentBuff(int value)
		{
			_permanentValue += value;
		}
	}
}