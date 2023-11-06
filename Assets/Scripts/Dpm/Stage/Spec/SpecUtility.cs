using System;
using System.Collections.Generic;
using Dpm.CoreAdapter;
using Dpm.Utility;
using UnityEngine;

namespace Dpm.Stage.Spec
{
	public static class SpecUtility
	{
		private static Dictionary<Type, string> _typeToAssetName;

		public static void CacheSpecData()
		{
			if (_typeToAssetName != null)
			{
				return;
			}

			_typeToAssetName = new();

			var allSpecs = CoreService.Asset.GetResourceHolder<ScriptableObject>();

			try
			{
				foreach (var kv in allSpecs)
				{
					if (kv.Value is IGameSpecTable and ISpecTable specTable)
					{
						_typeToAssetName.Add(specTable.SpecType, kv.Key);
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogError("Cannot Find ScriptableObject SpecHolder.");
				Debug.LogError(e);
			}
		}

		public static IReadOnlyDictionary<string, T> GetSpecData<T>() where T : struct, IGameSpec
		{
			if (!_typeToAssetName.TryGetValue(typeof(T), out var tableSpecName))
			{
				return null;
			}

			var specTable = CoreService.Asset.UnsafeGet<ScriptableObject>(tableSpecName) as SpecTableBase<T>;

			Debug.Assert(specTable != null, nameof(specTable) + " != null");

			return specTable.NameToSpec;
		}

		public static T GetSpec<T>(string specName) where T : struct, IGameSpec
		{
			if (_typeToAssetName.TryGetValue(typeof(T), out var tableSpecName))
			{
				var specTable = CoreService.Asset.UnsafeGet<ScriptableObject>(tableSpecName) as SpecTableBase<T>;

				Debug.Assert(specTable != null, nameof(specTable) + " != null");

				return specTable.NameToSpec[specName];
			}

			return default;
		}

		public static bool TryGetSpec<T>(string specName, out T result) where T : struct, IGameSpec
		{
			if (_typeToAssetName.TryGetValue(typeof(T), out var tableSpecName))
			{
				var specTable = CoreService.Asset.UnsafeGet<ScriptableObject>(tableSpecName) as SpecTableBase<T>;

				Debug.Assert(specTable != null, nameof(specTable) + " != null");

				if (specTable.NameToSpec.TryGetValue(specName, out result))
				{
					return true;
				}
			}

			result = default;

			return false;
		}
	}
}