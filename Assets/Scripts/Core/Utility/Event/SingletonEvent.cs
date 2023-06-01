namespace Core.Utility.Event
{
	/// <summary>
	/// 싱글톤 이벤트. Send로만 주고받는 것이 좋음
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class SingletonEvent<T> : Interface.IEvent where T : SingletonEvent<T>, new()
	{
		public static T Instance { get; private set; } = new T();

		public void Dispose()
		{
		}
	}
}