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

    public static bool RandomCompanionNoChoices
    {
        get => ModSettings.RandomCompanionNoChoices;
        set => ModSettings.RandomCompanionNoChoices = value;
    }

    public static bool GrantCompanionCards
    {
        get => ModSettings.GrantCompanionCards;
        set => ModSettings.GrantCompanionCards = value;
    }

    public static bool OfferCompanionsAtEveryAncient
    {
        get => ModSettings.OfferCompanionsAtEveryAncient;
        set => ModSettings.OfferCompanionsAtEveryAncient = value;
    }

    public static bool ChooseMultipleCompanionsAtAncient
    {
        get => ModSettings.ChooseMultipleCompanionsAtAncient;
        set => ModSettings.ChooseMultipleCompanionsAtAncient = value;
    }
}
