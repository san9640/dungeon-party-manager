namespace Core.Message
{
    /// <summary>
    /// 싱글톤 이벤트. Send로만 주고받는 것이 좋음
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingletonEvent<T> : Event where T : SingletonEvent<T>, new()
    {
        public static T Instance { get; private set; } = new T();

        public override void Dispose()
        {
        }
    }
}