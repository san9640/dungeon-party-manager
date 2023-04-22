using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Resource
{
	public class ResourceManager
	{
		private readonly string _prefabLootPath = "Prefabs";

		private readonly Dictionary<string, GameObject> _prefabs = new();

		public static ResourceManager Instance => _instance;

		private static ResourceManager _instance;

		public ResourceManager(string prefabLootPath = null)
		{
			_instance = this;

			if (prefabLootPath != null)
			{
				_prefabLootPath = prefabLootPath;
			}
		}

		public IEnumerator LoadAsync(string path, Action<GameObject> doneCallback)
		{
			if (!_prefabs.ContainsKey(path))
			{
				var loadReq = Resources.LoadAsync($"{_prefabLootPath}/{path}");

				yield return loadReq;

				if (loadReq.asset is GameObject assetGo)
				{
					_prefabs.Add(path, assetGo);
				}
#if UNITY_EDITOR
				else
				{
					Debug.LogError($"{ path } load failed.");

					yield break;
				}
#endif
			}

			doneCallback.Invoke(_prefabs[path]);
		}
	}
}