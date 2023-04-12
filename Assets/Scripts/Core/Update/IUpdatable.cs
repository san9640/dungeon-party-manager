namespace Core.Update
{
	/// <summary>
	/// MonoBehaviour의 Update를 사용하는 대신 이 인스턴스를 구현해서 쓸 것
	/// Game.cs는 예외
	/// </summary>
	public interface IUpdatable
	{
		void UpdateFrame(float dt);
	}
}