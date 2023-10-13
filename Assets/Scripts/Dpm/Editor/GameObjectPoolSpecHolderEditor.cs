using System.Collections.Generic;
using Dpm.Utility.Pool;
using UnityEditor;

namespace Dpm.Editor
{
	[CustomEditor(typeof(GameObjectPoolSpec))]
	public class GameObjectPoolSpecHolderEditor : UnityEditor.Editor
	{
		private GameObjectPoolSpecTable _table;
		private readonly HashSet<string> _specNames = new();

		private void OnEnable()
		{
			_table = target as GameObjectPoolSpecTable;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			string sameSpecName = null;

			foreach (var spec in _table.specs)
			{
				if (_specNames.Add(spec.Name))
					continue;

				sameSpecName = spec.Name;

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