using BaseLib.Config;

namespace NeowCompanions.NeowCompanionsCode.Config;

[ConfigHoverTipsByDefault]
public sealed class NeowCompanionsConfig : SimpleModConfig
{
    [ConfigHideInUI]
    public static bool StartWithFyshSwoop
    {
        get => ModSettings.StartWithFyshSwoop;
        set => ModSettings.StartWithFyshSwoop = value;
    }

    public static bool OfferAllCompanions
    {
        get => ModSettings.OfferAllCompanions;
        set => ModSettings.OfferAllCompanions = value;
    }

    public static bool GrantCompanionCards
    {
        get => ModSettings.GrantCompanionCards;
        set => ModSettings.GrantCompanionCards = value;
    }
}
