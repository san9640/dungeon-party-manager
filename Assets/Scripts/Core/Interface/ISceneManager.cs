using System;
using System.Collections;

namespace Core.Interface
{
	public interface ISceneManager
	{
		IScene CurrentScene { get; }

		IEnumerator EnterScene(IScene nextScene);

		void ExitCurrentScene();
	}
}