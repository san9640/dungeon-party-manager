using Core.Interface;

namespace Dpm.Utility.Event
{
	/// <summary>
	/// 싱글톤 이벤트
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class SingletonEvent<T> : Core.Interface.Event where T : SingletonEvent<T>, new()
	{
		public static T Instance { get; private set; } = new T();

		public new void Dispose()
		{
			base.Dispose();
		}
	}
}