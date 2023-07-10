using System;
using System.Collections;
using UnityEngine;

namespace Dpm.Common
{
	public class ScreenTransition : MonoBehaviour
	{
		[SerializeField]
		private new Camera camera;

		[SerializeField]
		private new MeshRenderer renderer;

		public static ScreenTransition Instance { get; private set; }

		private void Awake()
		{
			Instance = this;
		}

		private void OnDestroy()
		{
			Instance = null;
		}

		public void FadeOut()
		{
			renderer.enabled = true;
			camera.enabled = true;
		}

		public void FadeIn()
		{
			renderer.enabled = false;
			camera.enabled = false;
		}
	}
}