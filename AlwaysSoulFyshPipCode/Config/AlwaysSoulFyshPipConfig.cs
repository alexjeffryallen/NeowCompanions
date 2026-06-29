using BaseLib.Config;

namespace NeowCompanions.NeowCompanionsCode.Config;

public sealed class AlwaysSoulFyshPipConfig : SimpleModConfig
{
    public static bool StartWithFyshSwoop
    {
        get => ModSettings.StartWithFyshSwoop;
        set => ModSettings.StartWithFyshSwoop = value;
    }
}
