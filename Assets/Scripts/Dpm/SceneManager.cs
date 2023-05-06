using System.Collections;
using UnityEngine;

namespace Dpm
{
	public class SceneManager
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