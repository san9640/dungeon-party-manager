using System;

namespace Core.Interface
{
	public interface IUpdateSystem : IDisposable, IUpdatable
	{
		public void Register(IUpdatable updatable, int priority);

		public void Unregister(IUpdatable updatable, int priority);
	}
}