using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Models;

namespace NeowCompanions.NeowCompanionsCode.Models;

public sealed class NeowCompanionCardPool : CustomCardPoolModel
{
    public override string Title => "NeowCompanions";

    public override bool IsShared => true;

    public override bool IsColorless => true;

    public override bool SeenByDefault => true;

    public override Color DeckEntryCardColor => new("D0B46A");

    protected override CardModel[] GenerateAllCards()
    {
        return
        [
            ModelDb.Card<FyshSwoop>(),
            ModelDb.Card<WrigglerCard>(),
            ModelDb.Card<CeremonialBeastCard>(),
            ModelDb.Card<KinFollowerCard>(),
            ModelDb.Card<EyeWithTeethCard>(),
            ModelDb.Card<GremlinMercCard>(),
            ModelDb.Card<ThievingHopperCard>(),
            ModelDb.Card<AeonglassCard>(),
            ModelDb.Card<LagavulinMatriarchCard>(),
            ModelDb.Card<TheKinCard>(),
            ModelDb.Card<WaterfallGiantCard>(),
            ModelDb.Card<VantomCard>(),
            ModelDb.Card<KnowledgeDemonCard>(),
            ModelDb.Card<TheInsatiableCard>(),
            ModelDb.Card<QueenCard>(),
            ModelDb.Card<TestSubjectCard>()
        ];
    }
}
