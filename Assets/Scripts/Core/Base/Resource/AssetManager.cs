﻿using System;
using System.Collections.Generic;
using Core.Interface;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.Base.Resource
{
	public class AssetManager : IAssetManager
	{
		private readonly Dictionary<Type, Dictionary<string, Object>> _resources = new();

		public AssetManager(string[] assetSpecsHolderPaths)
		{
			foreach (var path in assetSpecsHolderPaths)
			{
				var assetSpecsHolder = AssetDatabase.LoadAssetAtPath<AssetSpecsHolder>(path);

				// 타입 맞춰서 미리 로드해둠
				Load(assetSpecsHolder);
			}
		}

		private void Load(AssetSpecsHolder assetSpecsHolder)
		{
			var resourcesHolder = new Dictionary<string, Object>();

			foreach (var assetSpec in assetSpecsHolder.specs)
			{
				var resource = Resources.Load(assetSpec.path);

				resourcesHolder.Add(assetSpec.name, resource);
			}

			_resources.Add(assetSpecsHolder.AssetType, resourcesHolder);
		}

		public bool TryGet<T>(string specName, out T result) where T : Object
		{
			var type = typeof(T);

			if (_resources.TryGetValue(type, out var resHolder) &&
			    resHolder.TryGetValue(specName, out var asset))
			{
				result = asset as T;

				return true;
			}

			result = null;

			return false;
		}

		public T UnsafeGet<T>(string specName) where T : Object
		{
			TryGet<T>(specName, out var result);

			return result;
		}

		public IReadOnlyDictionary<string, Object> GetResourceHolder<T>()
		{
			if (_resources.TryGetValue(typeof(T), out var resHolder))
			{
				return resHolder;
			}

			return null;
		}

		public void Dispose()
		{
			_resources.Clear();
		}
	}
}