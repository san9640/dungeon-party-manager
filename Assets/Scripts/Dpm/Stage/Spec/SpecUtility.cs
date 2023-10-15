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

		public static T GetSpec<T>(string specName) where T : struct, IGameSpec
		{
			if (_typeToAssetName.TryGetValue(typeof(T), out var tableSpecName))
			{
				var specTable = CoreService.Asset.UnsafeGet<ScriptableObject>(tableSpecName) as SpecTableBase<T>;

				return specTable.NameToSpec[specName];
			}

			return default;
		}
	}
}