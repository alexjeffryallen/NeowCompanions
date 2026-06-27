using BaseLib.Config;

namespace AlwaysSoulFyshPip.AlwaysSoulFyshPipCode.Config;

public sealed class AlwaysSoulFyshPipConfig : SimpleModConfig
{
    public static bool StartWithFyshSwoop
    {
        get => ModSettings.StartWithFyshSwoop;
        set => ModSettings.StartWithFyshSwoop = value;
    }
}
