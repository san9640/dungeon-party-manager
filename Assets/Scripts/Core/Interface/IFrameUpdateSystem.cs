using System;

namespace Core.Interface
{
	public interface IFrameUpdateSystem : IDisposable, IUpdatable, ILateUpdatable
	{
		/// <summary>
		/// Update 콜백 등록
		/// </summary>
		/// <param name="updatable">Update 등록할 객체</param>
		/// <param name="priority">
		/// Update 실행 우선순위. 낮을 수록 먼저 실행되므로, 일반적인 경우 EntityId를 넣는 것이 바람직
		/// </param>
		public void RegisterUpdate(IUpdatable updatable, int priority);

		/// <summary>
		/// Update 콜백 해제
		/// </summary>
		/// <param name="updatable">Update 해제할 객체</param>
		/// <param name="priority">
		/// 등록할 때 기입한 Update 실행 우선순위. 등록할 때랑 달라지면 해제가 안됨!!
		/// </param>
		public void UnregisterUpdate(IUpdatable updatable, int priority);

		/// <summary>
		/// LateUpdate 콜백 등록
		/// </summary>
		/// <param name="lateUpdatable">LateUpdate 등록할 객체</param>
		/// <param name="priority">
		/// LateUpdate 실행 우선순위. 낮을 수록 먼저 실행되므로, 일반적인 경우 EntityId를 넣는 것이 바람직
		/// </param>
		public void RegisterLateUpdate(ILateUpdatable lateUpdatable, int priority);

		/// <summary>
		/// LateUpdate 콜백 해제
		/// </summary>
		/// <param name="lateUpdatable">LateUpdate 해제할 객체</param>
		/// <param name="priority">
		/// 등록할 때 기입한 LateUpdate 실행 우선순위. 등록할 때랑 달라지면 해제가 안됨!!
		/// </param>
		public void UnregisterLateUpdate(ILateUpdatable lateUpdatable, int priority);
	}
}