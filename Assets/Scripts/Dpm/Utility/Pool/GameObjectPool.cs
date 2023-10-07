using System;
using System.Collections.Generic;
using Core.Interface;
using Dpm.CoreAdapter;
using Dpm.Utility.Constants;
using UnityEngine;

namespace Dpm.Utility.Pool
{
	/// <summary>
	/// 풀링된 GameObject 컴포넌트
	/// </summary>
	public class PooledGameObject : MonoBehaviour, IDisposable, IUpdatable
	{
		private GameObjectPool _parent;

		private bool _isUsing = false;

		/// <summary>
		/// 이 오브젝트의 수명. 0이면 무제한
		/// </summary>
		private float _lifeTime = 0;

		/// <summary>
		/// 이 오브젝트의 지나간 수명. _lifeTime을 넘어가면 자동으로 해제됨
		/// </summary>
		private float _timePassed = 0;

		private readonly Dictionary<Type, object> _cachedComponents = new();

		public void Initialize(GameObjectPool parent, float lifeTime)
		{
			_parent = parent;
			_lifeTime = lifeTime;
		}

		public void Activate()
		{
			if (_lifeTime > 0)
			{
				CoreService.FrameUpdate.RegisterUpdate(this, UpdatePriority.GameObjectPool);
			}

			_isUsing = true;
		}

		/// <summary>
		/// 풀에 반환. 사용이 끝나면 호출 필요
		/// </summary>
		public void Deactivate()
		{
			if (!_isUsing)
			{
				return;
			}

			_isUsing = false;

			if (_lifeTime > 0)
			{
				CoreService.FrameUpdate.UnregisterUpdate(this, UpdatePriority.GameObjectPool);
			}

			_timePassed = 0;

			_parent.Return(this);
		}

		void IDisposable.Dispose()
		{
			_parent = null;
		}

		public new T GetComponent<T>() where T : class
		{
			if (!_cachedComponents.TryGetValue(typeof(T), out var component))
			{
				component = gameObject.GetComponent<T>();

				_cachedComponents.Add(typeof(T), component);

				// 인터페이스나 부모 클래스의 형태로 접근하는 케이스에도 다대일 대응이 되도록 함
				if (!_cachedComponents.ContainsKey(component.GetType()))
				{
					_cachedComponents.Add(component.GetType(), component);
				}
			}

			return component as T;
		}

		public void UpdateFrame(float dt)
		{
			_timePassed += dt;

			// 스펙에 명시된 시간이 지나면 해제
			if (_timePassed >= _lifeTime)
			{
				Deactivate();
			}
		}
	}

	public class GameObjectPool : MonoBehaviour, IDisposable
	{
		private GameObject _prefab;
		private Stack<PooledGameObject> _pool = new();

		/// <summary>
		/// 일반 인스턴스와 다르게 GameObject들은 짜놓은 게임 스크립트 내에서 참조하지 않는다고 GC에서 걷어가는게 아니기 때문에,
		/// Dispose시에 모두 다 해제된다고 보장하기 어려우므로 제대로 해제하기 위해 사용 중인 것들은 담아둔다.
		/// </summary>
		private List<PooledGameObject> _usingObjects = new();

		/// <summary>
		/// 풀링 오브젝트 스펙
		/// </summary>
		private GameObjectPoolSpec _spec;

		/// <summary>
		/// 새 PooledGameObject 생성
		/// </summary>
		private PooledGameObject CreateNew()
		{
			var newGo = Instantiate(_prefab, transform);

			var result = newGo.AddComponent<PooledGameObject>();

			result.Initialize(this, _spec.lifeTime);

			return result;
		}

		/// <summary>
		/// Position 세팅된 오브젝트를 꺼냄
		/// </summary>
		public bool TrySpawn(Vector3 position, out PooledGameObject result)
		{
			return TrySpawn(position, Quaternion.identity, out result);
		}

		/// <summary>
		/// Position 및 Rotation 세팅된 오브젝트를 꺼냄
		/// </summary>
		public bool TrySpawn(Vector3 position, Quaternion rotation, out PooledGameObject result)
		{
			if (_spec.maxCount > 0 && _usingObjects.Count >= _spec.maxCount)
			{
#if UNITY_EDITOR
				Debug.LogError($"[{ _spec.Name } GameObjectPool] already using full of MaxCount.");
#endif
				result = null;
				return false;
			}

			// 풀에 오브젝트가 없으면 새로 생성
			if (!_pool.TryPop(out result))
			{
				result = CreateNew();
			}

			// Transform 세팅 및 활성화하고 반환
			result.transform.position = position;
			result.transform.rotation = rotation;
			result.gameObject.SetActive(true);

			_usingObjects.Add(result);

			result.Activate();

			return true;
		}

		public void Return(PooledGameObject target)
		{
			for (int i = 0; i < _usingObjects.Count; i++)
			{
				if (ReferenceEquals(target, _usingObjects[i]))
				{
					_usingObjects.RemoveAt(i);

					// 그 동안 부모가 바뀌었을 가능성이 있으므로 ObjectPool을 부모로 다시 세팅한다.
					target.transform.SetParent(transform);

					target.gameObject.SetActive(false);

					_pool.Push(target);

					break;
				}
			}

			// usingObjects에 존재하지 않는다면 풀에 반환되지 않을 것임
		}

		void IDisposable.Dispose()
		{
			// 오브젝트 풀로 전부 반환함
			while (_usingObjects.Count > 0)
			{
				_usingObjects[0].Deactivate();
			}

			_usingObjects = null;

			foreach (var instance in _pool)
			{
				((IDisposable)instance).Dispose();
			}

			_pool = null;
			_prefab = null;
		}

		private static readonly Dictionary<string, GameObjectPool> Pools = new();

		/// <summary>
		/// 오브젝트 풀을 가져옴
		/// </summary>
		/// <param name="specName">
		/// 프리팹 스펙
		/// TODO : 오브젝트 풀 스펙으로 변경 필요!!
		/// </param>
		/// <returns></returns>
		public static GameObjectPool Get(string specName)
		{
			if (Pools.TryGetValue(specName, out var pool))
			{
				return pool;
			}

			if (!CoreService.Asset.TryGet<ScriptableObject>("GameObjectPoolSpec", out var specAsset))
			{
#if UNITY_EDITOR
				Debug.LogError("Has no GameObjectPoolSpec.asset.");
#endif
				return null;
			}

			if (!(specAsset is GameObjectPoolSpecHolder specHolder) ||
			    !specHolder.NameToSpec.TryGetValue(specName, out var spec))
			{
#if UNITY_EDITOR
				Debug.LogError($"AssetManager has no GameObjectPoolSpec [SpecName : { specName }");
#endif
				return null;
			}

			if (!CoreService.Asset.TryGet<GameObject>(spec.prefabSpecName, out var prefab))
			{
#if UNITY_EDITOR
				Debug.LogError($"AssetManager has no Prefab [SpecName : { spec.prefabSpecName }");
#endif
				return null;
			}

			var go = new GameObject($"{ specName }Pool");
			var newPool = go.AddComponent<GameObjectPool>();

			newPool._prefab = prefab;
			newPool._spec = spec;

			Pools.Add(specName, newPool);

			return newPool;
		}

		/// <summary>
		/// 모든 오브젝트 풀 전체 해제
		/// </summary>
		public static void Dispose()
		{
			foreach (var pool in Pools)
			{
				(pool.Value as IDisposable).Dispose();
				Destroy(pool.Value.gameObject);
			}

			Pools.Clear();
		}
	}
}