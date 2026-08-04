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
            ModelDb.Card<TestSubjectCard>(),
            ModelDb.Card<SeapunkCard>(),
            ModelDb.Card<ShrinkerBeetleCard>(),
            ModelDb.Card<OperosisCard>(),
            ModelDb.Card<ArchitectCard>(),
            ModelDb.Card<BuffUpCard>(),
            ModelDb.Card<NeedleTossCard>(),
            ModelDb.Card<OverclockCard>(),
            ModelDb.Card<GraveCallCard>(),
            ModelDb.Card<CommandingFlourishCard>(),
            ModelDb.Card<EmberPipCard>(),
            ModelDb.Card<FrostPipCard>(),
            ModelDb.Card<StormPipCard>(),
            ModelDb.Card<ThornPipCard>(),
            ModelDb.Card<KaiserCrabCard>(),
            ModelDb.Card<BygoneEffigyCard>(),
            ModelDb.Card<ByrdonisCard>(),
            ModelDb.Card<PhrogParasiteCard>(),
            ModelDb.Card<SkulkingColonyCard>(),
            ModelDb.Card<PhantasmalGardenerCard>(),
            ModelDb.Card<TerrorEelCard>(),
            ModelDb.Card<DecimillipedeCard>(),
            ModelDb.Card<EntomancerCard>(),
            ModelDb.Card<InfestedPrismCard>(),
            ModelDb.Card<KnightGangCard>(),
            ModelDb.Card<MechaKnightCard>(),
            ModelDb.Card<SoulNexusCard>(),
            ModelDb.Card<AssassinRubyRaiderCard>(),
            ModelDb.Card<AxeRubyRaiderCard>(),
            ModelDb.Card<BruteRubyRaiderCard>(),
            ModelDb.Card<CrossbowRubyRaiderCard>(),
            ModelDb.Card<FlyconidCard>(),
            ModelDb.Card<FogmogCard>(),
            ModelDb.Card<MawlerCard>(),
            ModelDb.Card<FuzzyWurmCrawlerCard>(),
            ModelDb.Card<InkletCard>(),
            ModelDb.Card<SnappingJaxfruitCard>(),
            ModelDb.Card<SlitheringStranglerCard>(),
            ModelDb.Card<LeafSlimeSCard>(),
            ModelDb.Card<LeafSlimeMCard>(),
            ModelDb.Card<TwigSlimeSCard>(),
            ModelDb.Card<TwigSlimeMCard>(),
            ModelDb.Card<VineShamblerCard>(),
            ModelDb.Card<ChomperCard>(),
            ModelDb.Card<CubexConstructCard>(),
            ModelDb.Card<DampCultistCard>(),
            ModelDb.Card<CalcifiedCultistCard>(),
            ModelDb.Card<CorpseSlugCard>(),
            ModelDb.Card<TwoTailedRatCard>(),
            ModelDb.Card<SewerClamCard>(),
            ModelDb.Card<HauntedShipCard>(),
            ModelDb.Card<SludgeSpinnerCard>(),
            ModelDb.Card<PunchConstructCard>(),
            ModelDb.Card<FossilStalkerCard>(),
            ModelDb.Card<LivingFogCard>(),
            ModelDb.Card<ParafrightCard>(),
            ModelDb.Card<TunnelerCard>(),
            ModelDb.Card<SpinyToadCard>(),
            ModelDb.Card<StabbotCard>(),
            ModelDb.Card<HunterKillerCard>(),
            ModelDb.Card<TorchHeadAmalgamCard>(),
            ModelDb.Card<BowlbugEggCard>(),
            ModelDb.Card<BowlbugNectarCard>(),
            ModelDb.Card<BowlbugRockCard>(),
            ModelDb.Card<BowlbugSilkCard>(),
            ModelDb.Card<LouseProgenitorCard>(),
            ModelDb.Card<SlumberingBeetleCard>(),
            ModelDb.Card<AxebotCard>(), ModelDb.Card<BattleFriendV1Card>(), ModelDb.Card<BattleFriendV2Card>(), ModelDb.Card<BattleFriendV3Card>(),
            ModelDb.Card<DevotedSculptorCard>(), ModelDb.Card<ExoskeletonCard>(),
            ModelDb.Card<FabricatorCard>(), ModelDb.Card<FlailKnightCard>(), ModelDb.Card<FrogKnightCard>(), ModelDb.Card<GasBombCard>(),
            ModelDb.Card<GlobeHeadCard>(), ModelDb.Card<GuardbotCard>(), ModelDb.Card<LivingShieldCard>(), ModelDb.Card<MagiKnightCard>(),
            ModelDb.Card<MysteriousKnightCard>(), ModelDb.Card<MyteCard>(), ModelDb.Card<NibbitCard>(), ModelDb.Card<NoisebotCard>(),
            ModelDb.Card<OvicopterCard>(), ModelDb.Card<OwlMagistrateCard>(), ModelDb.Card<PaelsLegionCard>(),
            ModelDb.Card<ScrollOfBitingCard>(), ModelDb.Card<SlimedBerserkerCard>(), ModelDb.Card<TheForgottenCard>(), ModelDb.Card<TheLostCard>(),
            ModelDb.Card<TheObscuraCard>(), ModelDb.Card<ToadpoleCard>(), ModelDb.Card<ToughEggCard>(), ModelDb.Card<TrackerRubyRaiderCard>(),
            ModelDb.Card<TurretOperatorCard>(), ModelDb.Card<ZapbotCard>()
        ];
    }
}
