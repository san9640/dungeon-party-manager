using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dpm.Stage.Render
{
	public class StageCamera : MonoBehaviour
	{
		public static StageCamera Instance { get; private set; }

		[Serializable]
		public struct CameraSetting
		{
			public Vector3 position;
			public float size;
		}

		[SerializeField]
		private CameraSetting controlModeSetting;

		private CameraSetting _defaultSetting;

		private Camera _camera;

		private void Awake()
		{
			Instance = this;

			_camera = GetComponent<Camera>();

			_defaultSetting = new CameraSetting
			{
				position = transform.position,
				size = _camera.orthographicSize,
			};
		}

		private void OnDestroy()
		{
			Instance = null;
		}

		public void ChangeToDefaultMode()
		{
			AssignSetting(_defaultSetting);
		}

		public void ChangeToControlMode()
		{
			AssignSetting(controlModeSetting);
		}

		private void AssignSetting(CameraSetting setting)
		{
			_camera.transform.position = setting.position;
			_camera.orthographicSize = setting.size;
		}
	}
}