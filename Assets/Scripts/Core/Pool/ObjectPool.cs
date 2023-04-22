using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Pool
{
	public class ObjectPool
	{
		private static Dictionary<Type, ObjectPool> pools = new();

		/// <summary>
		/// 원하는 타입의 오브젝트풀을 가져옴
		/// </summary>
		/// <typeparam name="T">풀링될 오브젝트의 타입</typeparam>
		/// <returns>오브젝트 풀</returns>
		public static ObjectPool Get<T>() where T : class, new()
		{
			var type = typeof(T);

			if (!pools.ContainsKey(type))
			{
				pools.Add(type, new ObjectPool<T>());
			}

			return pools[type];
		}

		/// <summary>
		/// 전체 오브젝트 풀 삭제
		/// 일반적인 상황에서는 사용할 일이 없고, 게임을 나갈 때만 사용할 듯...?
		/// </summary>
		public static void Clear()
		{
			foreach (var keyValue in pools)
			{
				(keyValue.Value as IDisposable)?.Dispose();
			}

			pools.Clear();
		}
	}

	public class ObjectPool<T> : ObjectPool, IDisposable where T : class, new()
	{
		private Stack<T> pool = new();

#if UNITY_EDITOR
		private int PoolCount => pool.Count;

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
			if (pool.Count == 0)
			{
				return new();
			}

			return pool.Pop();
		}

		/// <summary>
		/// 오브젝트 다시 풀에 넣음
		/// </summary>
		public void Return(T obj)
		{
#if UNITY_EDITOR
			if (pool.Contains(obj))
			{
				Debug.LogError($"Object already returned to { typeof(T) } object pool.");
				return;
			}
#endif
			pool.Push(obj);
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
			pool.Clear();
		}
	}
}