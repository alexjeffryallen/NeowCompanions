using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Models;

namespace AlwaysSoulFyshPip.AlwaysSoulFyshPipCode.Models;

public sealed class NeowCompanionCardPool : CustomCardPoolModel
{
    public override string Title => "NeowCompanions";

    public override bool IsShared => false;

    public override bool IsColorless => true;

    public override Color DeckEntryCardColor => new("D0B46A");

    protected override CardModel[] GenerateAllCards()
    {
        return
        [
            ModelDb.Card<FyshSwoop>(),
            ModelDb.Card<WrigglerCard>(),
            ModelDb.Card<CeremonialBeastCard>()
        ];
    }
}
