using System;
using System.Collections.Generic;

namespace Core.Update
{
	/// <summary>
	/// Update할 것들을 수집하여 일괄 실행시키는 시스템
	/// MonoBehaviour의 Update는 퍼포먼스 문제가 있으므로 따로 등록/해제 하는 것이 싸게 먹힘
	/// 싱글톤이 아닌 이유는 Game.cs에서 여러 단계의 업데이트 기능을 제공할 필요성이 있는 경우 여러 UpdateSystem을 이용하려 한 것
	/// </summary>
	/// TODO : 업데이트 등록 및 해제, 업데이트 일괄 실행 구현
	public class UpdateSystem : IDisposable, IUpdatable
	{
		/// <summary>
		/// Updatable 정보
		/// </summary>
		private struct UpdatableInfo
		{
			public int Priority;
			public IUpdatable Updatable;
		}

		/// <summary>
		/// 미리 Capacity를 크게 잡아둠
		/// </summary>
		private readonly List<UpdatableInfo> _updatables = new(256);

		/// <summary>
		/// 등록/해제 판별용
		/// </summary>
		private enum RegisterType
		{
			Add,
			Remove
		}

		/// <summary>
		/// 등록/해제 요청 정보
		/// </summary>
		private struct RegisterRequest
		{
			public RegisterType Type;
			public UpdatableInfo UpdatableInfo;
		}

		private readonly List<RegisterRequest> _registerRequests = new(64);

		void IDisposable.Dispose()
		{
			_registerRequests.Clear();
			_updatables.Clear();
		}

		void IUpdatable.UpdateFrame(float dt)
		{
			for (var i = 0; i < _registerRequests.Count; i++)
			{
				if (_registerRequests[i].Type == RegisterType.Add)
				{
					var insertPos = 0;

					// FIXME : 이분 탐색으로 변경
					for (; insertPos < _updatables.Count; insertPos++)
					{
						// Priority 값이 같은 경우에는 미리 들어가 있는 것들보다 더 늦게 불러줌
						// Updatables가 비어있는 경우도 있기 때문에, for문 안에서 Insert한다면 제대로 처리되지 않을 것임
						if (_registerRequests[i].UpdatableInfo.Priority > _updatables[insertPos].Priority)
							break;
					}

					// 비어있거나 insertPos == _updatables.Count인 경우에도 insert는 유효함
					_updatables.Insert(insertPos, _registerRequests[i].UpdatableInfo);
				}
				else
				{
					// FIXME : 이분 탐색으로 변경
					_updatables.Remove(_registerRequests[i].UpdatableInfo);
				}
			}

			_registerRequests.Clear();

			for (var i = 0; i < _updatables.Count; i++)
			{
				_updatables[i].Updatable.UpdateFrame(dt);
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="updatable">업데이트 등록할 객체</param>
		/// <param name="priority"></param>
		public void Register(IUpdatable updatable, int priority)
		{
			_registerRequests.Add(new RegisterRequest
			{
				Type = RegisterType.Add,
				UpdatableInfo = new UpdatableInfo
				{
					Updatable = updatable,
					Priority = priority
				}
			});
		}

		public void Unregister(IUpdatable updatable, int priority)
		{
			_registerRequests.Add(new RegisterRequest
			{
				Type = RegisterType.Remove,
				UpdatableInfo = new UpdatableInfo
				{
					Updatable = updatable,
					Priority = priority
				}
			});
		}
	}
}