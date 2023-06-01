using System;
using System.Collections;

namespace Core.Interface
{
	public interface ISceneManager
	{
		IEnumerator EnterScene(IScene nextScene);

		void ExitCurrentScene();
	}
}