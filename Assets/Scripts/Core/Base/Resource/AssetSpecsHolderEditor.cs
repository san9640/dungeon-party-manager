using System;
using System.Collections.Generic;
using Core.Base.Resource;
using UnityEditor;

namespace Core.Base.Resource
{
	[CustomEditor(typeof(AssetSpecsHolder))]
	public class AssetSpecsHolderEditor : Editor
	{
		private AssetSpecsHolder _holder;
		private readonly HashSet<string> _specNames = new();

		private void OnEnable()
		{
			_holder = target as AssetSpecsHolder;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			string sameSpecName = null;

			foreach (var spec in _holder.specs)
			{
				if (_specNames.Add(spec.name))
					continue;

				sameSpecName = spec.name;

				break;
			}

			if (sameSpecName != null)
			{
				EditorGUILayout.HelpBox($"Has same spec names:[{sameSpecName}].", MessageType.Error);
			}

			_specNames.Clear();
		}
	}
}