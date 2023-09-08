using Dpm.Stage.Unit;
using UnityEditor;

namespace Dpm.Editor
{
	[CustomEditor(typeof(Unit), true)]
	public class UnitEditor : UnityEditor.Editor
	{
		private IUnit _unit;

		protected virtual void OnEnable()
		{
			_unit = target as IUnit;
		}

		protected virtual void OnDisable()
		{
			_unit = null;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			EditorGUILayout.LabelField("CurrentState", _unit.CurrentState.ToString());
		}
	}
}