using Dpm.Utility.Event;

namespace Dpm.Stage.Event
{
	public class GameOverEvent : PooledEvent<GameOverEvent>
	{
		public int Score { get; private set; }

		public static GameOverEvent Create(int score)
		{
			var e = Pool.GetOrCreate();

			e.Score = score;

			return e;
		}
	}
}