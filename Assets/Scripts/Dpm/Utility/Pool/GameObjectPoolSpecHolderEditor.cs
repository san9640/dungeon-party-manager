using System.Collections.Generic;
using UnityEditor;

namespace Dpm.Utility.Pool
{
	[CustomEditor(typeof(GameObjectPoolSpec))]
	public class GameObjectPoolSpecHolderEditor : Editor
	{
		private GameObjectPoolSpecHolder _holder;
		private readonly HashSet<string> _specNames = new();

		private void OnEnable()
		{
			_holder = target as GameObjectPoolSpecHolder;
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