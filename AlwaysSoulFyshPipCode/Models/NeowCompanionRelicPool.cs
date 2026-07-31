using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;

namespace NeowCompanions.NeowCompanionsCode.Models;

public sealed class NeowCompanionRelicPool : CustomRelicPoolModel
{
    public override bool IsShared => true;

    public override bool SeenByDefault => true;

    protected override IEnumerable<RelicModel> GenerateAllRelics()
    {
        return
        [
            ModelDb.Relic<SoulFyshPipRelic>(),
            ModelDb.Relic<WrigglerRelic>(),
            ModelDb.Relic<CeremonialBeastRelic>(),
            ModelDb.Relic<KinFollowerRelic>(),
            ModelDb.Relic<EyeWithTeethRelic>(),
            ModelDb.Relic<GremlinMercRelic>(),
            ModelDb.Relic<ThievingHopperRelic>(),
            ModelDb.Relic<AeonglassRelic>(),
            ModelDb.Relic<LagavulinMatriarchRelic>(),
            ModelDb.Relic<TheKinRelic>(),
            ModelDb.Relic<WaterfallGiantRelic>(),
            ModelDb.Relic<VantomRelic>(),
            ModelDb.Relic<KnowledgeDemonRelic>(),
            ModelDb.Relic<TheInsatiableRelic>(),
            ModelDb.Relic<QueenRelic>(),
            ModelDb.Relic<TestSubjectRelic>(),
            ModelDb.Relic<SeapunkRelic>(),
            ModelDb.Relic<ShrinkerBeetleRelic>(),
            ModelDb.Relic<OperosisRelic>(),
            ModelDb.Relic<ArchitectRelic>(),
            ModelDb.Relic<RustcladRelic>(),
            ModelDb.Relic<ShadeleafRelic>(),
            ModelDb.Relic<GlitchlingRelic>(),
            ModelDb.Relic<BonebinderRelic>(),
            ModelDb.Relic<GildedPageRelic>(),
            ModelDb.Relic<EmberPipRelic>(),
            ModelDb.Relic<FrostPipRelic>(),
            ModelDb.Relic<StormPipRelic>(),
            ModelDb.Relic<ThornPipRelic>(),
            ModelDb.Relic<KaiserCrabRelic>(),
            ModelDb.Relic<BygoneEffigyRelic>(),
            ModelDb.Relic<ByrdonisRelic>(),
            ModelDb.Relic<PhrogParasiteRelic>(),
            ModelDb.Relic<SkulkingColonyRelic>(),
            ModelDb.Relic<PhantasmalGardenerRelic>(),
            ModelDb.Relic<TerrorEelRelic>(),
            ModelDb.Relic<DecimillipedeRelic>(),
            ModelDb.Relic<EntomancerRelic>(),
            ModelDb.Relic<InfestedPrismRelic>(),
            ModelDb.Relic<KnightGangRelic>(),
            ModelDb.Relic<MechaKnightRelic>(),
            ModelDb.Relic<SoulNexusRelic>()
        ];
    }
}
