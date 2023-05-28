using System.Collections;

namespace Core.Interface
{
	public interface ISceneManager
	{
		public IEnumerator EnterScene(IScene nextScene);

		public void ExitCurrentScene();
	}
}