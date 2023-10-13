using Dpm.Stage.Unit;
using UnityEditor;

namespace Dpm.Editor
{
	[CustomEditor(typeof(Character))]
	public class CharacterEditor : UnitEditor
	{
		private Character _character;

		protected override void OnEnable()
		{
			base.OnEnable();

			_character = target as Character;
		}

		protected override void OnDisable()
		{
			_character = null;

			base.OnDisable();
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			// TODO
		}
	}
}