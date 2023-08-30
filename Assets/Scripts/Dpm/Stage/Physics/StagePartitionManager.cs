using System;
using System.Collections.Generic;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Unit;

namespace Dpm.Stage.Physics
{
	/// <summary>
	/// FIXME : 지금은 파티션을 구별하지 않고 충돌체들을 모아두는 역할만 함. 이게 무거워지면 수정 필요
	/// </summary>
	public class StagePartitionManager : IDisposable
	{
		private readonly List<ICustomCollider> _colliders = new();

		public StagePartitionManager()
		{
			CoreService.Event.Subscribe<AddedToPartitionEvent>(OnAddedToPartition);
			CoreService.Event.Subscribe<RemovedFromPartitionEvent>(OnRemovedFromPartition);
		}

		public void Dispose()
		{
			CoreService.Event.Unsubscribe<AddedToPartitionEvent>(OnAddedToPartition);
			CoreService.Event.Unsubscribe<RemovedFromPartitionEvent>(OnRemovedFromPartition);
		}

		private void OnAddedToPartition(Core.Interface.Event e)
		{
			if (e is not AddedToPartitionEvent ape)
			{
				return;
			}

			_colliders.Remove(ape.Collider);
			_colliders.Add(ape.Collider);
		}

		private void OnRemovedFromPartition(Core.Interface.Event e)
		{
			if (e is not RemovedFromPartitionEvent rpe)
			{
				return;
			}

			_colliders.Remove(rpe.Collider);
		}
	}
}