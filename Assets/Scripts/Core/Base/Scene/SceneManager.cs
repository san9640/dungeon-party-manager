using System.Collections;
using Core.Interface;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace Core.Base.Scene
{
	public class SceneManager : ISceneManager
	{
		private IScene _currentScene;

		public IEnumerator EnterScene(IScene nextScene)
		{
			if (_currentScene != null)
			{
#if UNITY_EDITOR
				Debug.LogError($"Previous scene [{ _currentScene }] is still exist");
#endif
				yield break;
			}

			_currentScene = nextScene;
			yield return nextScene.LoadAsync();

			nextScene.Enter();
		}

		public void ExitCurrentScene()
		{
			_currentScene?.Exit();
			_currentScene = null;
		}
	}
}