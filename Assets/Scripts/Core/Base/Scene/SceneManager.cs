using System.Collections;
using Core.Interface;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace Core.Base.Scene
{
	public class SceneManager : ISceneManager
	{
		public IScene CurrentScene { get; private set; }

		public IEnumerator EnterScene(IScene nextScene)
		{
			if (CurrentScene != null)
			{
#if UNITY_EDITOR
				Debug.LogError($"Previous scene [{ CurrentScene }] is still exist");
#endif
				yield break;
			}

			CurrentScene = nextScene;
			yield return nextScene.LoadAsync();

			nextScene.Enter();
		}

		public void ExitCurrentScene()
		{
			CurrentScene?.Exit();
			CurrentScene = null;
		}
	}
}