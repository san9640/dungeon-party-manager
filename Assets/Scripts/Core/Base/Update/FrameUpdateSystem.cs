using System;
using System.Collections;
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

		private class FrameUpdatableHolder<T> : IEnumerable, IDisposable where T : class, IFrameUpdatable
		{
			/// <summary>
			/// 미리 Capacity를 크게 잡아둠
			/// </summary>
			private readonly List<FrameUpdatableInfo<T>> _container = new(256);

			private int _walkingIndex = -1;

			public void Add(T frameUpdatable, int priority)
			{
				var insertPos = 0;

				// FIXME : 이분 탐색으로 변경
				for (; insertPos < _container.Count; insertPos++)
				{
					// Priority 값이 같은 경우에는 미리 들어가 있는 것들보다 더 늦게 불러줌
					// Updatables가 비어있는 경우도 있기 때문에, for문 안에서 Insert한다면 제대로 처리되지 않을 것임
					if (priority > _container[insertPos].Priority)
						break;
				}

				// 비어있거나 insertPos == _updatables.Count인 경우에도 insert는 유효함
				_container.Insert(insertPos, new FrameUpdatableInfo<T>
				{
					Priority = priority,
					FrameUpdatable = frameUpdatable
				});

				if (insertPos <= _walkingIndex)
				{
					_walkingIndex++;
				}
			}

			public void Remove(T frameUpdatable, int priority)
			{
				// FIXME : 이분 탐색으로 변경
				for (int i = 0; i < _container.Count; i++)
				{
					if (_container[i].Priority == priority &&
					    _container[i].FrameUpdatable == frameUpdatable)
					{
						_container.RemoveAt(i);

						if (i <= _walkingIndex)
						{
							_walkingIndex--;
						}
					}
				}
			}

			public void Dispose()
			{
				_container.Clear();
			}

			/// <summary>
			/// 외부에서 각 Updatable 객체에 접근하기 위해 IEnumerable 구현
			/// </summary>
			/// <returns></returns>
			public IEnumerator GetEnumerator()
			{
				for (_walkingIndex = 0; _walkingIndex < _container.Count; _walkingIndex++)
				{
					yield return _container[_walkingIndex].FrameUpdatable;
				}

				_walkingIndex = -1;
			}
		}

		private readonly FrameUpdatableHolder<IUpdatable> _updatableHolder = new();
		private readonly FrameUpdatableHolder<ILateUpdatable> _lateUpdatableHolder = new();

		public void Dispose()
		{
			_updatableHolder.Dispose();
			_lateUpdatableHolder.Dispose();
		}

		public void UpdateFrame(float dt)
		{
			foreach (IUpdatable updatable in _updatableHolder)
			{
				updatable.UpdateFrame(dt);
			}
		}

		public void LateUpdateFrame(float dt)
		{
			foreach (ILateUpdatable lateUpdatable in _lateUpdatableHolder)
			{
				lateUpdatable.LateUpdateFrame(dt);
			}
		}

		public void RegisterUpdate(IUpdatable updatable, int priority)
		{
			_updatableHolder.Add(updatable, priority);
		}

		public void UnregisterUpdate(IUpdatable updatable, int priority)
		{
			_updatableHolder.Remove(updatable, priority);
		}

		public void RegisterLateUpdate(ILateUpdatable lateUpdatable, int priority)
		{
			_lateUpdatableHolder.Add(lateUpdatable, priority);
		}

		public void UnregisterLateUpdate(ILateUpdatable lateUpdatable, int priority)
		{
			_lateUpdatableHolder.Remove(lateUpdatable, priority);
		}
	}
}