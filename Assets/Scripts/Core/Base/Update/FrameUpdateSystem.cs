using System;
using System.Collections.Generic;
using Core.Interface;

namespace Core.Base.Update
{
	/// <summary>
	/// Update할 것들을 수집하여 일괄 실행시키는 시스템
	/// MonoBehaviour의 Update는 퍼포먼스 문제가 있으므로 따로 등록/해제 하는 것이 싸게 먹힘
	/// </summary>
	public class FrameUpdateSystem : IFrameUpdateSystem
	{
		/// <summary>
		/// Updatable 정보
		/// </summary>
		private struct FrameUpdatableInfo<T> where T : IFrameUpdatable
		{
			public int Priority;
			public T FrameUpdatable;
		}

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
		private struct RegisterRequest<T> where T : IFrameUpdatable
		{
			public RegisterType Type;
			public FrameUpdatableInfo<T> FrameUpdatableInfo;
		}

		private class FrameUpdatableContainer<T> : IDisposable where T : IFrameUpdatable
		{
			/// <summary>
			/// 미리 Capacity를 크게 잡아둠
			/// </summary>
			public readonly List<FrameUpdatableInfo<T>> FrameUpdatables = new(256);

			/// <summary>
			/// 전체 요청
			/// </summary>
			public readonly List<RegisterRequest<T>> RegisterRequests = new(64);

			public void ProcessRequests()
			{
				for (var i = 0; i < RegisterRequests.Count; i++)
				{
					if (RegisterRequests[i].Type == RegisterType.Add)
					{
						var insertPos = 0;

						// FIXME : 이분 탐색으로 변경
						for (; insertPos < FrameUpdatables.Count; insertPos++)
						{
							// Priority 값이 같은 경우에는 미리 들어가 있는 것들보다 더 늦게 불러줌
							// Updatables가 비어있는 경우도 있기 때문에, for문 안에서 Insert한다면 제대로 처리되지 않을 것임
							if (RegisterRequests[i].FrameUpdatableInfo.Priority > FrameUpdatables[insertPos].Priority)
								break;
						}

						// 비어있거나 insertPos == _updatables.Count인 경우에도 insert는 유효함
						FrameUpdatables.Insert(insertPos, RegisterRequests[i].FrameUpdatableInfo);
					}
					else
					{
						// FIXME : 이분 탐색으로 변경
						FrameUpdatables.Remove(RegisterRequests[i].FrameUpdatableInfo);
					}
				}

				RegisterRequests.Clear();
			}

			public void Add(T frameUpdatable, int priority)
			{

			}

			public void Remove(T frameUpdatable, int priority)
			{

			}

			public void Dispose()
			{
				FrameUpdatables.Clear();
				RegisterRequests.Clear();
			}
		}

		private readonly FrameUpdatableContainer<IUpdatable> _updatableContainer = new();
		private readonly FrameUpdatableContainer<ILateUpdatable> _lateUpdatableContainer = new();

		public void Dispose()
		{
			_updatableContainer.Dispose();
			_lateUpdatableContainer.Dispose();
		}

		void IUpdatable.UpdateFrame(float dt)
		{
			_updatableContainer.ProcessRequests();

			for (var i = 0; i < _updatableContainer.FrameUpdatables.Count; i++)
			{
				_updatableContainer.FrameUpdatables[i].FrameUpdatable.UpdateFrame(dt);
			}
		}

		public void LateUpdateFrame(float dt)
		{
			_lateUpdatableContainer.ProcessRequests();

			for (var i = 0; i < _lateUpdatableContainer.FrameUpdatables.Count; i++)
			{
				_lateUpdatableContainer.FrameUpdatables[i].FrameUpdatable.LateUpdateFrame(dt);
			}
		}

		public void RegisterUpdate(IUpdatable updatable, int priority)
		{
			_updatableContainer.RegisterRequests.Add(new RegisterRequest<IUpdatable>
			{
				Type = RegisterType.Add,
				FrameUpdatableInfo = new FrameUpdatableInfo<IUpdatable>
				{
					FrameUpdatable = updatable,
					Priority = priority
				}
			});
		}

		public void UnregisterUpdate(IUpdatable updatable, int priority)
		{
			_updatableContainer.RegisterRequests.Add(new RegisterRequest<IUpdatable>
			{
				Type = RegisterType.Remove,
				FrameUpdatableInfo = new FrameUpdatableInfo<IUpdatable>
				{
					FrameUpdatable = updatable,
					Priority = priority
				}
			});
		}

		public void RegisterLateUpdate(ILateUpdatable lateUpdatable, int priority)
		{
			_lateUpdatableContainer.RegisterRequests.Add(new RegisterRequest<ILateUpdatable>
			{
				Type = RegisterType.Add,
				FrameUpdatableInfo = new FrameUpdatableInfo<ILateUpdatable>
				{
					FrameUpdatable = lateUpdatable,
					Priority = priority
				}
			});
		}

		public void UnregisterLateUpdate(ILateUpdatable lateUpdatable, int priority)
		{
			_lateUpdatableContainer.RegisterRequests.Add(new RegisterRequest<ILateUpdatable>
			{
				Type = RegisterType.Remove,
				FrameUpdatableInfo = new FrameUpdatableInfo<ILateUpdatable>
				{
					FrameUpdatable = lateUpdatable,
					Priority = priority
				}
			});
		}
	}
}