namespace Diffraction.Scripting.Globals;

public class Time
{
    public static float TimeScale = 1;
    private static double _deltaTime;
    public static double DeltaTime
    {
        get
        {
            return TimeScale * _deltaTime;
        }
    }

    private static double _timeSinceStart;
    public static double TimeSinceStart
    {
        get
        {
            return _timeSinceStart;
        }
    }
    
    public static void Update(double deltaTime)
    {
        if (!ObjectScene.Instance.Paused)
        {
            _deltaTime = deltaTime;
            _timeSinceStart += DeltaTime;
        }
    }
}