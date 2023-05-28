using System.Collections;
using Core.Interface;
using UnityEngine;

namespace Core.Base.Scene
{
	public class SceneManager : ISceneManager
	{
		private IScene _currentScene;

		public IEnumerator EnterScene(IScene nextScene)
		{
			if (_currentScene != null)
			{
				Debug.LogError($"Previous scene [{ _currentScene }] is still exist");
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