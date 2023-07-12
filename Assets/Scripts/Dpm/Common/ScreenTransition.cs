using System;
using System.Collections;
using Dpm.CoreAdapter;
using UnityEngine;

namespace Dpm.Common
{
	public class ScreenTransition : MonoBehaviour
	{
		[SerializeField]
		private new Camera camera;

		[SerializeField]
		private new SpriteRenderer renderer;

		public static ScreenTransition Instance { get; private set; }

		private bool Enable
		{
			set
			{
				renderer.enabled = value;
				camera.enabled = value;
			}
		}

		private float Alpha
		{
			get => renderer.color.a;
			set => renderer.color = new Color(0, 0, 0, value);
		}

		private uint _reqId = 0;

		private void Awake()
		{
			Instance = this;
		}

		private void OnDestroy()
		{
			Instance = null;
		}

		public void FadeOut(float duration)
		{
			CoreService.Coroutine.StartCoroutine(FadeOutAsync(duration));
		}

		public void FadeIn(float duration)
		{
			CoreService.Coroutine.StartCoroutine(FadeInAsync(duration));
		}

		public IEnumerator FadeOutAsync(float duration)
		{
			Enable = true;

			var reqId = unchecked(++_reqId);

			yield return FadeAlphaAsync(1, duration, reqId);
		}

		public IEnumerator FadeInAsync(float duration)
		{
			Enable = true;

			var reqId = unchecked(++_reqId);

			yield return FadeAlphaAsync(0, duration, reqId);

			if (reqId == _reqId)
			{
				Enable = false;
			}
		}

		private IEnumerator FadeAlphaAsync(float targetAlpha, float duration, uint reqId)
		{
			var timePassed = 0f;
			var startAlpha = Alpha;

			while (reqId == _reqId && timePassed < duration)
			{
				var progress = timePassed / duration;
				var curAlpha = startAlpha * (1 - progress) + targetAlpha * progress;

				Alpha = curAlpha;

				yield return null;

				timePassed += Time.deltaTime;
			}

			if (reqId == _reqId)
			{
				Alpha = targetAlpha;
			}
		}
	}
}