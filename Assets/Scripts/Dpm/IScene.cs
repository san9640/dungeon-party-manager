using System.Collections;

namespace Dpm
{
	public interface IScene
	{
		/// <summary>
		/// 로딩 루틴
		/// </summary>
		IEnumerator LoadAsync();

		/// <summary>
		/// 씬 진입
		/// </summary>
		void Enter();

		/// <summary>
		/// 씬 탈출
		/// </summary>
		void Exit();
	}
}