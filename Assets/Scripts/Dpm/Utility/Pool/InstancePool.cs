using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dpm.Utility.Pool
{
	public class InstancePool
	{
		private static readonly Dictionary<Type, InstancePool> Pools = new();

		/// <summary>
		/// 원하는 타입의 오브젝트풀을 가져옴
		/// </summary>
		/// <typeparam name="T">풀링될 오브젝트의 타입</typeparam>
		/// <returns>오브젝트 풀</returns>
		public static InstancePool Get<T>() where T : class, new()
		{
			var type = typeof(T);

			if (!Pools.ContainsKey(type))
			{
				Pools.Add(type, new InstancePool<T>());
			}

			return Pools[type];
		}

		/// <summary>
		/// 전체 오브젝트 풀 삭제
		/// 일반적인 상황에서는 사용할 일이 없고, 게임을 나갈 때만 사용할 듯...?
		/// </summary>
		public static void Clear()
		{
			foreach (var keyValue in Pools)
			{
				(keyValue.Value as IDisposable)?.Dispose();
			}

			Pools.Clear();
		}
	}

	public class InstancePool<T> : InstancePool, IDisposable where T : class, new()
	{
		private readonly Stack<T> _pool = new();

#if UNITY_EDITOR
		private int PoolCount => _pool.Count;

		public int UsingCount { get; private set; } = 0;
#endif

		/// <summary>
		/// 오브젝트를 풀에서 꺼내옴
		/// </summary>
		public T GetOrCreate()
		{
#if UNITY_EDITOR
			UsingCount++;
#endif
			if (_pool.Count == 0)
			{
				return new();
			}

			return _pool.Pop();
		}

		/// <summary>
		/// 오브젝트 다시 풀에 넣음
		/// </summary>
		public void Return(T obj)
		{
#if UNITY_EDITOR
			if (_pool.Contains(obj))
			{
				Debug.LogError($"Object already returned to { typeof(T) } object pool.");
				return;
			}
#endif
			_pool.Push(obj);
		}

		/// <summary>
		/// 오브젝트 풀 해제
		/// 일반적인 상황에서는 사용할 일이 없고, 게임을 나갈 때만 사용할 듯...?
		/// </summary>
		void IDisposable.Dispose()
		{
#if UNITY_EDITOR
			if (UsingCount > 0)
			{
				Debug.LogError($"{ UsingCount } unreturned items in { typeof(T) } object pool.");
			}

			UsingCount = 0;
#endif
			_pool.Clear();
		}
	}
}