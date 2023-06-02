using System;
using System.Collections.Generic;
using Dpm.CoreAdapter;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Dpm.Utility.Pool
{
	/// <summary>
	/// 풀링된 GameObject 컴포넌트
	/// TODO : 이펙트 등은 시간이 지나면 알아서 해제되는 것이 편하므로, ObjectPoolSpec.json을 파싱해서 LifeTime을 가져와야 함
	/// </summary>
	public class PooledGameObject : MonoBehaviour, IDisposable
	{
		private GameObjectPool _parent;

		public bool isUsing = false;

		public void AttachToPool(GameObjectPool parent)
		{
			_parent = parent;
		}

		/// <summary>
		/// 풀에 반환. 사용이 끝나면 호출 필요
		/// </summary>
		public void Return()
		{
			_parent.Return(this);
		}

		void IDisposable.Dispose()
		{
			_parent = null;
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
		/// 새 PooledGameObject 생성
		/// </summary>
		private PooledGameObject CreateNew()
		{
			var newGo = Object.Instantiate(_prefab, transform);

			var result = newGo.AddComponent<PooledGameObject>();

			result.AttachToPool(this);

			return result;
		}

		/// <summary>
		/// Position 세팅된 오브젝트를 꺼냄
		/// </summary>
		public PooledGameObject Instantiate(Vector3 position)
		{
			return Instantiate(position, Quaternion.identity);
		}

		/// <summary>
		/// Position 및 Rotation 세팅된 오브젝트를 꺼냄
		/// </summary>
		public PooledGameObject Instantiate(Vector3 position, Quaternion rotation)
		{
			// 풀에 오브젝트가 없으면 새로 생성
			if (!_pool.TryPop(out var result))
			{
				result = CreateNew();
			}

			// Transform 세팅 및 활성화하고 반환
			result.transform.position = position;
			result.transform.rotation = rotation;
			result.gameObject.SetActive(true);

			_usingObjects.Add(result);

			result.isUsing = true;

			return result;
		}

		/// <summary>
		/// 풀에 오브젝트를 직접 리턴
		/// </summary>
		public void Return(PooledGameObject target)
		{
			if (!target.isUsing)
			{
				return;
			}

			target.isUsing = true;

			for (int i = 0; i < _usingObjects.Count; i++)
			{
				if (ReferenceEquals(target, _usingObjects[i]))
				{
					_usingObjects.RemoveAt(i);

					target.gameObject.SetActive(false);

					// 그 동안 부모가 바뀌었을 가능성이 있으므로 ObjectPool을 부모로 다시 세팅한다.
					target.transform.SetParent(transform);

					_pool.Push(target);

					break;
				}
			}
		}

		void IDisposable.Dispose()
		{
			// 오브젝트 풀로 전부 반환함
			while (_usingObjects.Count > 0)
			{
				_usingObjects[0].Return();
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
		/// <param name="specKey">
		/// 프리팹 스펙
		/// TODO : 오브젝트 풀 스펙으로 변경 필요!!
		/// </param>
		/// <returns></returns>
		public static GameObjectPool Get(string specKey)
		{
			if (Pools.TryGetValue(specKey, out var pool))
			{
				return pool;
			}

			if (!CoreService.Asset.TryGet<GameObject>(specKey, out var prefab))
			{
#if UNITY_EDITOR
				Debug.LogError($"AssetManager has no Prefab [SpecKey : { specKey }");
#endif
				return null;
			}

			var go = new GameObject($"{ specKey }Pool");
			var newPool = go.AddComponent<GameObjectPool>();

			newPool._prefab = prefab;

			Pools.Add(specKey, newPool);

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