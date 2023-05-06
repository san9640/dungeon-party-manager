using System;
using System.Collections;
using System.Collections.Generic;
using Core.Interface;
using UnityEngine;

namespace Core.Async
{
	public delegate void OnTaskDone(bool canceled);

	public class CoroutineManager : ICoroutineManager
	{
		/// <summary>
		/// Coroutine Wrapping용 객체
		/// 코루틴 종료 여부 등을 외부에서 트래킹하기 위해 사용
		/// </summary>
		private class Task : IDisposable
        {
        	private enum TaskState
        	{
        		None,
        		Running,
        		Done,
        	}

        	public bool IsDone => _state == TaskState.Done;

        	private TaskState _state = TaskState.None;

        	private IEnumerator _enumerator;

        	private MonoBehaviour _parent;

        	private Coroutine _wrapperRoutine;

        	private OnTaskDone _onDone;

        	public Task(MonoBehaviour parent, IEnumerator enumerator)
        	{
        		_parent = parent;
        		_enumerator = enumerator;
        	}

        	public Task OnDone(OnTaskDone onDone)
        	{
        		_onDone = onDone;

        		return this;
        	}

        	public void Start()
        	{
        		_state = TaskState.Running;

        		_wrapperRoutine = _parent.StartCoroutine(CallWrapper(_enumerator));
        	}

        	public void Stop()
        	{
        		_parent.StopCoroutine(_wrapperRoutine);

        		_state = TaskState.Done;

        		_onDone?.Invoke(true);

        		Dispose();
        	}

        	private IEnumerator CallWrapper(IEnumerator enumerator)
        	{
        		yield return enumerator;

        		_state = TaskState.Done;

        		_onDone?.Invoke(false);

        		Dispose();
        	}

        	public void Dispose()
        	{
        		_parent = null;
        		_enumerator = null;
        		_wrapperRoutine = null;
        		_onDone = null;
        	}
        }

		private MonoBehaviour _coroutineParent;

		private Dictionary<Guid, Task> _tasks = new();

		public CoroutineManager(MonoBehaviour parent)
		{
			_coroutineParent = parent;
		}

		/// <summary>
		/// 코루틴 시작
		/// </summary>
		/// <returns>해당 코루틴의 유니크 아이디</returns>
		public Guid StartCoroutine(IEnumerator enumerator)
		{
			var guid = Guid.NewGuid();
			var task = new Task(_coroutineParent, enumerator);

			if (!_tasks.ContainsKey(guid))
			{
				_tasks.Add(guid, task);
			}
			else
			{
				Debug.LogError($"Coroutine Guid [ {guid} ] crashed.");
			}

			task.OnDone(_ => _tasks.Remove(guid));

			task.Start();

			return guid;
		}

		/// <summary>
		/// 코루틴 중지
		/// </summary>
		/// <param name="guid">중지할 코루틴의 유니크 아이디</param>
		public void StopCoroutine(Guid guid)
		{
			if (_tasks.TryGetValue(guid, out var task))
			{
				if (!task.IsDone)
				{
					task.Stop();
				}
			}
		}

		/// <summary>
		/// 해당 코루틴이 현재 실행 중인가?
		/// </summary>
		/// <param name="guid">확인할 코루틴의 유니크 아이디</param>
		public bool IsRunning(Guid guid)
		{
			// FIXME : 사용한 적 없는 guid를 넣었는지는 어떻게 판단하는가?

			if (_tasks.TryGetValue(guid, out var task))
			{
				return !task.IsDone;
			}

			return false;
		}

		public void Dispose()
		{
			// 돌고있는 코루틴들 강제 종료
			foreach (var guid in _tasks.Keys)
			{
				StopCoroutine(guid);
			}

#if UNITY_EDITOR
			if (_tasks.Count != 0)
			{
				Debug.LogError($"Has running tasks Count : { _tasks.Count }");
			}
#endif

			// 위에서 이미 전부 사라졌을 것이므
			_tasks = null;

			_coroutineParent = null;
		}
	}
}