using System;
using System.Collections;
using System.Collections.Generic;
using Core.Interface;
using UnityEngine;

namespace Core.Base.Resource
{
	public class ResourceManager : IResourceManager
	{
		private ICoroutineManager _coroutineManager;

		private readonly Dictionary<Type, Dictionary<string, object>> _resources = new();

		public ResourceManager(ICoroutineManager coroutineManager)
		{
			_coroutineManager = coroutineManager;
		}

		/// <summary>
		/// 비동기 로드
		/// </summary>
		/// <param name="path"></param>
		/// <param name="doneCallback"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IEnumerator LoadAsync<T>(string path, Action<T> doneCallback) where T : class
		{
			var type = typeof(T);

			if (!_resources.ContainsKey(type))
			{
				_resources.Add(type, new());
			}

			if (!_resources[type].ContainsKey(path))
			{
				var loadReq = Resources.LoadAsync(path);

				yield return loadReq;

				if (loadReq.asset is T)
				{
					_resources[type].Add(path, loadReq.asset);
				}
#if UNITY_EDITOR
				else
				{
					Debug.LogError($"Resource { path } [ type { type } ] load failed.");

					yield break;
				}
#endif
			}

			doneCallback.Invoke(_resources[type][path] as T);
		}

		/// <summary>
		/// 비동기 로드 실행
		/// </summary>
		/// <param name="path">에셋 경로 및 이름</param>
		/// <param name="doneCallback"></param>
		/// <typeparam name="T"></typeparam>
		public void Load<T>(string path, Action<T> doneCallback) where T : class
		{
			_coroutineManager.StartCoroutine(LoadAsync(path, doneCallback));
		}

		public void ClearAll()
		{
			_resources.Clear();
		}

		public void Dispose()
		{
			ClearAll();

			_coroutineManager = null;
		}
	}
}