using Dpm.Utility.Event;


public class GameWinEvent : PooledEvent<GameWinEvent>
{
    public int TotalUnits { get; private set; }

    public bool IsHighScore { get; private set; }

    public static GameWinEvent Create(int totalUnits)
    {
        var e = Pool.GetOrCreate();

        e.TotalUnits = totalUnits;

        return e;
    }
}
