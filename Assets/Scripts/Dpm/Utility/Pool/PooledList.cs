using System;
using System.Collections.Generic;

namespace Dpm.Utility.Pool
{
	public class PooledList<T> : List<T>, IDisposable
	{
		private static readonly InstancePool<PooledList<T>> Pool = new();

		public void Dispose()
		{
			Clear();

			Pool.Return(this);
		}

		public static PooledList<T> Get()
		{
			return Pool.GetOrCreate();
		}
	}
}