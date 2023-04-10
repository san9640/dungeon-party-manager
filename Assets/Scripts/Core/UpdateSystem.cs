using System;

namespace Core
{
	/// <summary>
	/// Update할 것들을 수집하여 일괄 실행시키는 시스템
	/// MonoBehaviour의 Update는 퍼포먼스 문제가 있으므로 따로 등록/해제 하는 것이 싸게 먹힘
	/// </summary>
	/// TODO : 업데이트 등록 및 해제, 업데이트 일괄 실행 구현
	public class UpdateSystem : ISystem
	{
		public static UpdateSystem Instance { get; } = new UpdateSystem();

		void IDisposable.Dispose()
		{
		}

		void ISystem.Update()
		{
		}
	}
}