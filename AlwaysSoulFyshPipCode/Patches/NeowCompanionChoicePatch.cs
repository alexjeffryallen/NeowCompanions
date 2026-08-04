using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NeowCompanions.NeowCompanionsCode;
using NeowCompanions.NeowCompanionsCode.Assets;
using NeowCompanions.NeowCompanionsCode.Config;
using NeowCompanions.NeowCompanionsCode.Models;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Events;

namespace NeowCompanions.NeowCompanionsCode.Patches;

public static class NeowCompanionChoicePatch
{
    internal sealed record CompanionPoolConfigEntry(string Name, Type CardType);


    private static readonly FieldInfo? EventOptionOnChosenField =
        AccessTools.Field(typeof(EventOption), "<OnChosen>k__BackingField");

    private static readonly HashSet<EventOption> WrappedInitialOptions =
        new(ReferenceEqualityComparer.Instance);

    internal static IReadOnlyList<string>? ActiveCompanionOptionTexts { get; private set; }

    internal static IReadOnlyList<string>? ActiveCompanionIconFiles { get; private set; }

    internal static int ActiveCompanionOptionCount => ActiveCompanionOptionTexts?.Count ?? 0;

    internal sealed class CompanionOption
    {
        public CompanionKind Kind { get; }
        public string DebugName { get; }
        public Type RelicType { get; }
        public Type CardType { get; }

        public CompanionOption(CompanionKind kind, string debugName, Type relicType, Type cardType)
        {
            Kind = kind;
            DebugName = debugName;
            RelicType = relicType;
            CardType = cardType;
        }

        public string TitleKey => Kind switch
        {
            CompanionKind.Byrdpip => "BYRDPIP.title",
            CompanionKind.SoulFysh => "SOUL_FYSH.title",
            CompanionKind.Wriggler => "WRIGGLER.title",
            CompanionKind.CeremonialBeast => "CEREMONIAL_BEAST.title",
            CompanionKind.KinFollower => "KIN_FOLLOWER.title",
            CompanionKind.EyeWithTeeth => "EYE_WITH_TEETH.title",
            CompanionKind.GremlinMerc => "GREMLIN_MERC.title",
            CompanionKind.ThievingHopper => "THIEVING_HOPPER.title",
            CompanionKind.Aeonglass => "AEONGLASS.title",
            CompanionKind.LagavulinMatriarch => "LAGAVULIN_MATRIARCH.title",
            CompanionKind.TheKin => "THE_KIN.title",
            CompanionKind.WaterfallGiant => "WATERFALL_GIANT.title",
            CompanionKind.Vantom => "VANTOM.title",
            CompanionKind.KnowledgeDemon => "KNOWLEDGE_DEMON.title",
            CompanionKind.TheInsatiable => "THE_INSATIABLE.title",
            CompanionKind.Queen => "QUEEN.title",
            CompanionKind.TestSubject => "TEST_SUBJECT.title",
            CompanionKind.Seapunk => "SEAPUNK.title",
            CompanionKind.ShrinkerBeetle => "SHRINKER_BEETLE.title",
            CompanionKind.Operosis => "OPEROSIS.title",
            CompanionKind.Architect => "ARCHITECT.title",
            CompanionKind.Rustclad => "RUSTCLAD.title",
            CompanionKind.Shadeleaf => "SHADELEAF.title",
            CompanionKind.Glitchling => "GLITCHLING.title",
            CompanionKind.Bonebinder => "BONEBINDER.title",
            CompanionKind.GildedPage => "GILDED_PAGE.title",
            CompanionKind.EmberPip => "EMBER_PIP.title",
            CompanionKind.FrostPip => "FROST_PIP.title",
            CompanionKind.StormPip => "STORM_PIP.title",
            CompanionKind.ThornPip => "THORN_PIP.title",
            CompanionKind.KaiserCrab => "KAISER_CRAB.title",
            CompanionKind.BygoneEffigy => "BYGONE_EFFIGY.title",
            CompanionKind.Byrdonis => "BYRDONIS.title",
            CompanionKind.PhrogParasite => "PHROG_PARASITE.title",
            CompanionKind.SkulkingColony => "SKULKING_COLONY.title",
            CompanionKind.PhantasmalGardener => "PHANTASMAL_GARDENER.title",
            CompanionKind.TerrorEel => "TERROR_EEL.title",
            CompanionKind.Decimillipede => "DECIMILLIPEDE.title",
            CompanionKind.Entomancer => "ENTOMANCER.title",
            CompanionKind.InfestedPrism => "INFESTED_PRISM.title",
            CompanionKind.KnightGang => "KNIGHT_GANG.title",
            CompanionKind.MechaKnight => "MECHA_KNIGHT.title",
            CompanionKind.SoulNexus => "SOUL_NEXUS.title",
            CompanionKind.AssassinRubyRaider => "ASSASSIN_RUBY_RAIDER.title",
            CompanionKind.AxeRubyRaider => "AXE_RUBY_RAIDER.title",
            CompanionKind.BruteRubyRaider => "BRUTE_RUBY_RAIDER.title",
            CompanionKind.CrossbowRubyRaider => "CROSSBOW_RUBY_RAIDER.title",
            CompanionKind.Flyconid => "FLYCONID.title",
            CompanionKind.Fogmog => "FOGMOG.title",
            CompanionKind.Mawler => "MAWLER.title",
            CompanionKind.FuzzyWurmCrawler => "FUZZY_WURM_CRAWLER.title",
            CompanionKind.Inklet => "INKLET.title",
            CompanionKind.SnappingJaxfruit => "SNAPPING_JAXFRUIT.title",
            CompanionKind.SlitheringStrangler => "SLITHERING_STRANGLER.title",
            CompanionKind.LeafSlimeS => "LEAF_SLIME_S.title",
            CompanionKind.LeafSlimeM => "LEAF_SLIME_M.title",
            CompanionKind.TwigSlimeS => "TWIG_SLIME_S.title",
            CompanionKind.TwigSlimeM => "TWIG_SLIME_M.title",
            CompanionKind.VineShambler => "VINE_SHAMBLER.title",
            CompanionKind.Chomper => "CHOMPER.title",
            CompanionKind.CubexConstruct => "CUBEX_CONSTRUCT.title",
            CompanionKind.DampCultist => "DAMP_CULTIST.title",
            CompanionKind.CalcifiedCultist => "CALCIFIED_CULTIST.title",
            CompanionKind.CorpseSlug => "CORPSE_SLUG.title",
            CompanionKind.TwoTailedRat => "TWO_TAILED_RAT.title",
            CompanionKind.SewerClam => "SEWER_CLAM.title",
            CompanionKind.HauntedShip => "HAUNTED_SHIP.title",
            CompanionKind.SludgeSpinner => "SLUDGE_SPINNER.title",
            CompanionKind.PunchConstruct => "PUNCH_CONSTRUCT.title",
            CompanionKind.FossilStalker => "FOSSIL_STALKER.title",
            CompanionKind.LivingFog => "LIVING_FOG.title",
            CompanionKind.Parafright => "PARAFRIGHT.title",
            CompanionKind.Tunneler => "TUNNELER.title",
            CompanionKind.SpinyToad => "SPINY_TOAD.title",
            CompanionKind.Stabbot => "STABBOT.title",
            CompanionKind.HunterKiller => "HUNTER_KILLER.title",
            CompanionKind.TorchHeadAmalgam => "TORCH_HEAD_AMALGAM.title",
            CompanionKind.BowlbugEgg => "BOWLBUG_EGG.title",
            CompanionKind.BowlbugNectar => "BOWLBUG_NECTAR.title",
            CompanionKind.BowlbugRock => "BOWLBUG_ROCK.title",
            CompanionKind.BowlbugSilk => "BOWLBUG_SILK.title",
            CompanionKind.LouseProgenitor => "LOUSE_PROGENITOR.title",
            CompanionKind.SlumberingBeetle => "SLUMBERING_BEETLE.title",
            CompanionKind.Axebot => "AXEBOT.title", CompanionKind.BattleFriendV1 => "BATTLE_FRIEND_V1.title", CompanionKind.BattleFriendV2 => "BATTLE_FRIEND_V2.title", CompanionKind.BattleFriendV3 => "BATTLE_FRIEND_V3.title",
            CompanionKind.Crusher => "CRUSHER.title", CompanionKind.DevotedSculptor => "DEVOTED_SCULPTOR.title", CompanionKind.Exoskeleton => "EXOSKELETON.title",
            CompanionKind.Fabricator => "FABRICATOR.title", CompanionKind.FlailKnight => "FLAIL_KNIGHT.title", CompanionKind.FrogKnight => "FROG_KNIGHT.title", CompanionKind.GasBomb => "GAS_BOMB.title",
            CompanionKind.GlobeHead => "GLOBE_HEAD.title", CompanionKind.Guardbot => "GUARDBOT.title", CompanionKind.LivingShield => "LIVING_SHIELD.title", CompanionKind.MagiKnight => "MAGI_KNIGHT.title",
            CompanionKind.MysteriousKnight => "MYSTERIOUS_KNIGHT.title", CompanionKind.Myte => "MYTE.title", CompanionKind.Nibbit => "NIBBIT.title", CompanionKind.Noisebot => "NOISEBOT.title",
            CompanionKind.Ovicopter => "OVICOPTER.title", CompanionKind.OwlMagistrate => "OWL_MAGISTRATE.title", CompanionKind.PaelsLegion => "PAELS_LEGION.title", CompanionKind.Rocket => "ROCKET.title",
            CompanionKind.ScrollOfBiting => "SCROLL_OF_BITING.title", CompanionKind.SlimedBerserker => "SLIMED_BERSERKER.title", CompanionKind.TheForgotten => "THE_FORGOTTEN.title", CompanionKind.TheLost => "THE_LOST.title",
            CompanionKind.TheObscura => "THE_OBSCURA.title", CompanionKind.Toadpole => "TOADPOLE.title", CompanionKind.ToughEgg => "TOUGH_EGG.title", CompanionKind.TrackerRubyRaider => "TRACKER_RUBY_RAIDER.title",
            CompanionKind.TurretOperator => "TURRET_OPERATOR.title", CompanionKind.Zapbot => "ZAPBOT.title",
            _ => "CHOOSE_COMPANION.title"
        };

        public string DescriptionKey => Kind switch
        {
            CompanionKind.Byrdpip => "BYRDPIP.description",
            CompanionKind.SoulFysh => "SOUL_FYSH.description",
            CompanionKind.Wriggler => "WRIGGLER.description",
            CompanionKind.CeremonialBeast => "CEREMONIAL_BEAST.description",
            CompanionKind.KinFollower => "KIN_FOLLOWER.description",
            CompanionKind.EyeWithTeeth => "EYE_WITH_TEETH.description",
            CompanionKind.GremlinMerc => "GREMLIN_MERC.description",
            CompanionKind.ThievingHopper => "THIEVING_HOPPER.description",
            CompanionKind.Aeonglass => "AEONGLASS.description",
            CompanionKind.LagavulinMatriarch => "LAGAVULIN_MATRIARCH.description",
            CompanionKind.TheKin => "THE_KIN.description",
            CompanionKind.WaterfallGiant => "WATERFALL_GIANT.description",
            CompanionKind.Vantom => "VANTOM.description",
            CompanionKind.KnowledgeDemon => "KNOWLEDGE_DEMON.description",
            CompanionKind.TheInsatiable => "THE_INSATIABLE.description",
            CompanionKind.Queen => "QUEEN.description",
            CompanionKind.TestSubject => "TEST_SUBJECT.description",
            CompanionKind.Seapunk => "SEAPUNK.description",
            CompanionKind.ShrinkerBeetle => "SHRINKER_BEETLE.description",
            CompanionKind.Operosis => "OPEROSIS.description",
            CompanionKind.Architect => "ARCHITECT.description",
            CompanionKind.Rustclad => "RUSTCLAD.description",
            CompanionKind.Shadeleaf => "SHADELEAF.description",
            CompanionKind.Glitchling => "GLITCHLING.description",
            CompanionKind.Bonebinder => "BONEBINDER.description",
            CompanionKind.GildedPage => "GILDED_PAGE.description",
            CompanionKind.EmberPip => "EMBER_PIP.description",
            CompanionKind.FrostPip => "FROST_PIP.description",
            CompanionKind.StormPip => "STORM_PIP.description",
            CompanionKind.ThornPip => "THORN_PIP.description",
            CompanionKind.KaiserCrab => "KAISER_CRAB.description",
            CompanionKind.BygoneEffigy => "BYGONE_EFFIGY.description",
            CompanionKind.Byrdonis => "BYRDONIS.description",
            CompanionKind.PhrogParasite => "PHROG_PARASITE.description",
            CompanionKind.SkulkingColony => "SKULKING_COLONY.description",
            CompanionKind.PhantasmalGardener => "PHANTASMAL_GARDENER.description",
            CompanionKind.TerrorEel => "TERROR_EEL.description",
            CompanionKind.Decimillipede => "DECIMILLIPEDE.description",
            CompanionKind.Entomancer => "ENTOMANCER.description",
            CompanionKind.InfestedPrism => "INFESTED_PRISM.description",
            CompanionKind.KnightGang => "KNIGHT_GANG.description",
            CompanionKind.MechaKnight => "MECHA_KNIGHT.description",
            CompanionKind.SoulNexus => "SOUL_NEXUS.description",
            CompanionKind.AssassinRubyRaider => "ASSASSIN_RUBY_RAIDER.description",
            CompanionKind.AxeRubyRaider => "AXE_RUBY_RAIDER.description",
            CompanionKind.BruteRubyRaider => "BRUTE_RUBY_RAIDER.description",
            CompanionKind.CrossbowRubyRaider => "CROSSBOW_RUBY_RAIDER.description",
            CompanionKind.Flyconid => "FLYCONID.description",
            CompanionKind.Fogmog => "FOGMOG.description",
            CompanionKind.Mawler => "MAWLER.description",
            CompanionKind.FuzzyWurmCrawler => "FUZZY_WURM_CRAWLER.description",
            CompanionKind.Inklet => "INKLET.description",
            CompanionKind.SnappingJaxfruit => "SNAPPING_JAXFRUIT.description",
            CompanionKind.SlitheringStrangler => "SLITHERING_STRANGLER.description",
            CompanionKind.LeafSlimeS => "LEAF_SLIME_S.description",
            CompanionKind.LeafSlimeM => "LEAF_SLIME_M.description",
            CompanionKind.TwigSlimeS => "TWIG_SLIME_S.description",
            CompanionKind.TwigSlimeM => "TWIG_SLIME_M.description",
            CompanionKind.VineShambler => "VINE_SHAMBLER.description",
            CompanionKind.Chomper => "CHOMPER.description",
            CompanionKind.CubexConstruct => "CUBEX_CONSTRUCT.description",
            CompanionKind.DampCultist => "DAMP_CULTIST.description",
            CompanionKind.CalcifiedCultist => "CALCIFIED_CULTIST.description",
            CompanionKind.CorpseSlug => "CORPSE_SLUG.description",
            CompanionKind.TwoTailedRat => "TWO_TAILED_RAT.description",
            CompanionKind.SewerClam => "SEWER_CLAM.description",
            CompanionKind.HauntedShip => "HAUNTED_SHIP.description",
            CompanionKind.SludgeSpinner => "SLUDGE_SPINNER.description",
            CompanionKind.PunchConstruct => "PUNCH_CONSTRUCT.description",
            CompanionKind.FossilStalker => "FOSSIL_STALKER.description",
            CompanionKind.LivingFog => "LIVING_FOG.description",
            CompanionKind.Parafright => "PARAFRIGHT.description",
            CompanionKind.Tunneler => "TUNNELER.description",
            CompanionKind.SpinyToad => "SPINY_TOAD.description",
            CompanionKind.Stabbot => "STABBOT.description",
            CompanionKind.HunterKiller => "HUNTER_KILLER.description",
            CompanionKind.TorchHeadAmalgam => "TORCH_HEAD_AMALGAM.description",
            CompanionKind.BowlbugEgg => "BOWLBUG_EGG.description",
            CompanionKind.BowlbugNectar => "BOWLBUG_NECTAR.description",
            CompanionKind.BowlbugRock => "BOWLBUG_ROCK.description",
            CompanionKind.BowlbugSilk => "BOWLBUG_SILK.description",
            CompanionKind.LouseProgenitor => "LOUSE_PROGENITOR.description",
            CompanionKind.SlumberingBeetle => "SLUMBERING_BEETLE.description",
            CompanionKind.Axebot => "AXEBOT.description", CompanionKind.BattleFriendV1 => "BATTLE_FRIEND_V1.description", CompanionKind.BattleFriendV2 => "BATTLE_FRIEND_V2.description", CompanionKind.BattleFriendV3 => "BATTLE_FRIEND_V3.description",
            CompanionKind.Crusher => "CRUSHER.description", CompanionKind.DevotedSculptor => "DEVOTED_SCULPTOR.description", CompanionKind.Exoskeleton => "EXOSKELETON.description",
            CompanionKind.Fabricator => "FABRICATOR.description", CompanionKind.FlailKnight => "FLAIL_KNIGHT.description", CompanionKind.FrogKnight => "FROG_KNIGHT.description", CompanionKind.GasBomb => "GAS_BOMB.description",
            CompanionKind.GlobeHead => "GLOBE_HEAD.description", CompanionKind.Guardbot => "GUARDBOT.description", CompanionKind.LivingShield => "LIVING_SHIELD.description", CompanionKind.MagiKnight => "MAGI_KNIGHT.description",
            CompanionKind.MysteriousKnight => "MYSTERIOUS_KNIGHT.description", CompanionKind.Myte => "MYTE.description", CompanionKind.Nibbit => "NIBBIT.description", CompanionKind.Noisebot => "NOISEBOT.description",
            CompanionKind.Ovicopter => "OVICOPTER.description", CompanionKind.OwlMagistrate => "OWL_MAGISTRATE.description", CompanionKind.PaelsLegion => "PAELS_LEGION.description", CompanionKind.Rocket => "ROCKET.description",
            CompanionKind.ScrollOfBiting => "SCROLL_OF_BITING.description", CompanionKind.SlimedBerserker => "SLIMED_BERSERKER.description", CompanionKind.TheForgotten => "THE_FORGOTTEN.description", CompanionKind.TheLost => "THE_LOST.description",
            CompanionKind.TheObscura => "THE_OBSCURA.description", CompanionKind.Toadpole => "TOADPOLE.description", CompanionKind.ToughEgg => "TOUGH_EGG.description", CompanionKind.TrackerRubyRaider => "TRACKER_RUBY_RAIDER.description",
            CompanionKind.TurretOperator => "TURRET_OPERATOR.description", CompanionKind.Zapbot => "ZAPBOT.description",
            _ => "CHOOSE_COMPANION.description"
        };
    }

    private static readonly List<CompanionOption> CompanionPool =
    [
        new CompanionOption(CompanionKind.Byrdpip, "Byrdpip", typeof(Byrdpip), typeof(ByrdSwoop)),
        new CompanionOption(CompanionKind.SoulFysh, "Soul Fysh Pip", typeof(SoulFyshPipRelic), typeof(FyshSwoop)),
        new CompanionOption(CompanionKind.Wriggler, "Wriggler", typeof(WrigglerRelic), typeof(WrigglerCard)),
        new CompanionOption(CompanionKind.CeremonialBeast, "Ceremonial Beast", typeof(CeremonialBeastRelic), typeof(CeremonialBeastCard)),
        new CompanionOption(CompanionKind.KinFollower, "Kin Follower", typeof(KinFollowerRelic), typeof(KinFollowerCard)),
        new CompanionOption(CompanionKind.EyeWithTeeth, "Eye With Teeth", typeof(EyeWithTeethRelic), typeof(EyeWithTeethCard)),
        new CompanionOption(CompanionKind.GremlinMerc, "Gremlin Merc", typeof(GremlinMercRelic), typeof(GremlinMercCard)),
        new CompanionOption(CompanionKind.ThievingHopper, "Thieving Hopper", typeof(ThievingHopperRelic), typeof(ThievingHopperCard)),
        new CompanionOption(CompanionKind.Aeonglass, "Aeonglass", typeof(AeonglassRelic), typeof(AeonglassCard)),
        new CompanionOption(CompanionKind.LagavulinMatriarch, "Lagavulin Matriarch", typeof(LagavulinMatriarchRelic), typeof(LagavulinMatriarchCard)),
        new CompanionOption(CompanionKind.TheKin, "The Kin", typeof(TheKinRelic), typeof(TheKinCard)),
        new CompanionOption(CompanionKind.WaterfallGiant, "Waterfall Giant", typeof(WaterfallGiantRelic), typeof(WaterfallGiantCard)),
        new CompanionOption(CompanionKind.Vantom, "Vantom", typeof(VantomRelic), typeof(VantomCard)),
        new CompanionOption(CompanionKind.KnowledgeDemon, "Knowledge Demon", typeof(KnowledgeDemonRelic), typeof(KnowledgeDemonCard)),
        new CompanionOption(CompanionKind.TheInsatiable, "The Insatiable", typeof(TheInsatiableRelic), typeof(TheInsatiableCard)),
        new CompanionOption(CompanionKind.Queen, "Queen", typeof(QueenRelic), typeof(QueenCard)),
        new CompanionOption(CompanionKind.TestSubject, "Test Subject", typeof(TestSubjectRelic), typeof(TestSubjectCard)),
        new CompanionOption(CompanionKind.Seapunk, "Seapunk", typeof(SeapunkRelic), typeof(SeapunkCard)),
        new CompanionOption(CompanionKind.ShrinkerBeetle, "Shrinker Beetle", typeof(ShrinkerBeetleRelic), typeof(ShrinkerBeetleCard)),
        new CompanionOption(CompanionKind.Operosis, "Operosis", typeof(OperosisRelic), typeof(OperosisCard)),
        new CompanionOption(CompanionKind.Architect, "The Architect", typeof(ArchitectRelic), typeof(ArchitectCard)),
        new CompanionOption(CompanionKind.Rustclad, "Rustclad", typeof(RustcladRelic), typeof(BuffUpCard)),
        new CompanionOption(CompanionKind.Shadeleaf, "Shadeleaf", typeof(ShadeleafRelic), typeof(NeedleTossCard)),
        new CompanionOption(CompanionKind.Glitchling, "Glitchling", typeof(GlitchlingRelic), typeof(OverclockCard)),
        new CompanionOption(CompanionKind.Bonebinder, "Bonebinder", typeof(BonebinderRelic), typeof(GraveCallCard)),
        new CompanionOption(CompanionKind.GildedPage, "Gilded Page", typeof(GildedPageRelic), typeof(CommandingFlourishCard)),
        new CompanionOption(CompanionKind.EmberPip, "Ember Pip", typeof(EmberPipRelic), typeof(EmberPipCard)),
        new CompanionOption(CompanionKind.FrostPip, "Frost Pip", typeof(FrostPipRelic), typeof(FrostPipCard)),
        new CompanionOption(CompanionKind.StormPip, "Storm Pip", typeof(StormPipRelic), typeof(StormPipCard)),
        new CompanionOption(CompanionKind.ThornPip, "Thorn Pip", typeof(ThornPipRelic), typeof(ThornPipCard)),
        new CompanionOption(CompanionKind.KaiserCrab, "Kaiser Crab", typeof(KaiserCrabRelic), typeof(KaiserCrabCard)),
        new CompanionOption(CompanionKind.BygoneEffigy, "Bygone Effigy", typeof(BygoneEffigyRelic), typeof(BygoneEffigyCard)),
        new CompanionOption(CompanionKind.Byrdonis, "Byrdonis", typeof(ByrdonisRelic), typeof(ByrdonisCard)),
        new CompanionOption(CompanionKind.PhrogParasite, "Phrog Parasite", typeof(PhrogParasiteRelic), typeof(PhrogParasiteCard)),
        new CompanionOption(CompanionKind.SkulkingColony, "Skulking Colony", typeof(SkulkingColonyRelic), typeof(SkulkingColonyCard)),
        new CompanionOption(CompanionKind.PhantasmalGardener, "Phantasmal Gardener", typeof(PhantasmalGardenerRelic), typeof(PhantasmalGardenerCard)),
        new CompanionOption(CompanionKind.TerrorEel, "Terror Eel", typeof(TerrorEelRelic), typeof(TerrorEelCard)),
        new CompanionOption(CompanionKind.Decimillipede, "Decimillipede", typeof(DecimillipedeRelic), typeof(DecimillipedeCard)),
        new CompanionOption(CompanionKind.Entomancer, "Entomancer", typeof(EntomancerRelic), typeof(EntomancerCard)),
        new CompanionOption(CompanionKind.InfestedPrism, "Infested Prism", typeof(InfestedPrismRelic), typeof(InfestedPrismCard)),
        new CompanionOption(CompanionKind.KnightGang, "Knight Gang", typeof(KnightGangRelic), typeof(KnightGangCard)),
        new CompanionOption(CompanionKind.MechaKnight, "Mecha Knight", typeof(MechaKnightRelic), typeof(MechaKnightCard)),
        new CompanionOption(CompanionKind.SoulNexus, "Soul Nexus", typeof(SoulNexusRelic), typeof(SoulNexusCard)),
        new CompanionOption(CompanionKind.AssassinRubyRaider, "Assassin Ruby Raider", typeof(AssassinRubyRaiderRelic), typeof(AssassinRubyRaiderCard)),
        new CompanionOption(CompanionKind.AxeRubyRaider, "Axe Ruby Raider", typeof(AxeRubyRaiderRelic), typeof(AxeRubyRaiderCard)),
        new CompanionOption(CompanionKind.BruteRubyRaider, "Brute Ruby Raider", typeof(BruteRubyRaiderRelic), typeof(BruteRubyRaiderCard)),
        new CompanionOption(CompanionKind.CrossbowRubyRaider, "Crossbow Ruby Raider", typeof(CrossbowRubyRaiderRelic), typeof(CrossbowRubyRaiderCard)),
        new CompanionOption(CompanionKind.Flyconid, "Flyconid", typeof(FlyconidRelic), typeof(FlyconidCard)),
        new CompanionOption(CompanionKind.Fogmog, "Fogmog", typeof(FogmogRelic), typeof(FogmogCard)),
        new CompanionOption(CompanionKind.Mawler, "Mawler", typeof(MawlerRelic), typeof(MawlerCard)),
        new CompanionOption(CompanionKind.FuzzyWurmCrawler, "Fuzzy Wurm Crawler", typeof(FuzzyWurmCrawlerRelic), typeof(FuzzyWurmCrawlerCard)),
        new CompanionOption(CompanionKind.Inklet, "Inklet", typeof(InkletRelic), typeof(InkletCard)),
        new CompanionOption(CompanionKind.SnappingJaxfruit, "Snapping Jaxfruit", typeof(SnappingJaxfruitRelic), typeof(SnappingJaxfruitCard)),
        new CompanionOption(CompanionKind.SlitheringStrangler, "Slithering Strangler", typeof(SlitheringStranglerRelic), typeof(SlitheringStranglerCard)),
        new CompanionOption(CompanionKind.LeafSlimeS, "Small Leaf Slime", typeof(LeafSlimeSRelic), typeof(LeafSlimeSCard)),
        new CompanionOption(CompanionKind.LeafSlimeM, "Medium Leaf Slime", typeof(LeafSlimeMRelic), typeof(LeafSlimeMCard)),
        new CompanionOption(CompanionKind.TwigSlimeS, "Small Twig Slime", typeof(TwigSlimeSRelic), typeof(TwigSlimeSCard)),
        new CompanionOption(CompanionKind.TwigSlimeM, "Medium Twig Slime", typeof(TwigSlimeMRelic), typeof(TwigSlimeMCard)),
        new CompanionOption(CompanionKind.VineShambler, "Vine Shambler", typeof(VineShamblerRelic), typeof(VineShamblerCard)),
        new CompanionOption(CompanionKind.Chomper, "Chomper", typeof(ChomperRelic), typeof(ChomperCard)),
        new CompanionOption(CompanionKind.CubexConstruct, "Cubex Construct", typeof(CubexConstructRelic), typeof(CubexConstructCard)),
        new CompanionOption(CompanionKind.DampCultist, "Damp Cultist", typeof(DampCultistRelic), typeof(DampCultistCard)),
        new CompanionOption(CompanionKind.CalcifiedCultist, "Calcified Cultist", typeof(CalcifiedCultistRelic), typeof(CalcifiedCultistCard)),
        new CompanionOption(CompanionKind.CorpseSlug, "Corpse Slug", typeof(CorpseSlugRelic), typeof(CorpseSlugCard)),
        new CompanionOption(CompanionKind.TwoTailedRat, "Two-Tailed Rat", typeof(TwoTailedRatRelic), typeof(TwoTailedRatCard)),
        new CompanionOption(CompanionKind.SewerClam, "Sewer Clam", typeof(SewerClamRelic), typeof(SewerClamCard)),
        new CompanionOption(CompanionKind.HauntedShip, "Haunted Ship", typeof(HauntedShipRelic), typeof(HauntedShipCard)),
        new CompanionOption(CompanionKind.SludgeSpinner, "Sludge Spinner", typeof(SludgeSpinnerRelic), typeof(SludgeSpinnerCard)),
        new CompanionOption(CompanionKind.PunchConstruct, "Punch Construct", typeof(PunchConstructRelic), typeof(PunchConstructCard)),
        new CompanionOption(CompanionKind.FossilStalker, "Fossil Stalker", typeof(FossilStalkerRelic), typeof(FossilStalkerCard)),
        new CompanionOption(CompanionKind.LivingFog, "Living Fog", typeof(LivingFogRelic), typeof(LivingFogCard)),
        new CompanionOption(CompanionKind.Parafright, "Parafright", typeof(ParafrightRelic), typeof(ParafrightCard)),
        new CompanionOption(CompanionKind.Tunneler, "Tunneler", typeof(TunnelerRelic), typeof(TunnelerCard)),
        new CompanionOption(CompanionKind.SpinyToad, "Spiny Toad", typeof(SpinyToadRelic), typeof(SpinyToadCard)),
        new CompanionOption(CompanionKind.Stabbot, "Stabbot", typeof(StabbotRelic), typeof(StabbotCard)),
        new CompanionOption(CompanionKind.HunterKiller, "Hunter Killer", typeof(HunterKillerRelic), typeof(HunterKillerCard)),
        new CompanionOption(CompanionKind.TorchHeadAmalgam, "Torch Head Amalgam", typeof(TorchHeadAmalgamRelic), typeof(TorchHeadAmalgamCard)),
        new CompanionOption(CompanionKind.BowlbugEgg, "Egg Bowlbug", typeof(BowlbugEggRelic), typeof(BowlbugEggCard)),
        new CompanionOption(CompanionKind.BowlbugNectar, "Nectar Bowlbug", typeof(BowlbugNectarRelic), typeof(BowlbugNectarCard)),
        new CompanionOption(CompanionKind.BowlbugRock, "Rock Bowlbug", typeof(BowlbugRockRelic), typeof(BowlbugRockCard)),
        new CompanionOption(CompanionKind.BowlbugSilk, "Silk Bowlbug", typeof(BowlbugSilkRelic), typeof(BowlbugSilkCard)),
        new CompanionOption(CompanionKind.LouseProgenitor, "Louse Progenitor", typeof(LouseProgenitorRelic), typeof(LouseProgenitorCard)),
        new CompanionOption(CompanionKind.SlumberingBeetle, "Slumbering Beetle", typeof(SlumberingBeetleRelic), typeof(SlumberingBeetleCard)),
        new CompanionOption(CompanionKind.Axebot, "Axebot", typeof(AxebotRelic), typeof(AxebotCard)),
        new CompanionOption(CompanionKind.BattleFriendV1, "Battle Friend V1", typeof(BattleFriendV1Relic), typeof(BattleFriendV1Card)),
        new CompanionOption(CompanionKind.BattleFriendV2, "Battle Friend V2", typeof(BattleFriendV2Relic), typeof(BattleFriendV2Card)),
        new CompanionOption(CompanionKind.BattleFriendV3, "Battle Friend V3", typeof(BattleFriendV3Relic), typeof(BattleFriendV3Card)),
        new CompanionOption(CompanionKind.DevotedSculptor, "Devoted Sculptor", typeof(DevotedSculptorRelic), typeof(DevotedSculptorCard)),
        new CompanionOption(CompanionKind.Exoskeleton, "Exoskeleton", typeof(ExoskeletonRelic), typeof(ExoskeletonCard)),
        new CompanionOption(CompanionKind.Fabricator, "Fabricator", typeof(FabricatorRelic), typeof(FabricatorCard)),
        new CompanionOption(CompanionKind.FlailKnight, "Flail Knight", typeof(FlailKnightRelic), typeof(FlailKnightCard)),
        new CompanionOption(CompanionKind.FrogKnight, "Frog Knight", typeof(FrogKnightRelic), typeof(FrogKnightCard)),
        new CompanionOption(CompanionKind.GasBomb, "Gas Bomb", typeof(GasBombRelic), typeof(GasBombCard)),
        new CompanionOption(CompanionKind.GlobeHead, "Globe Head", typeof(GlobeHeadRelic), typeof(GlobeHeadCard)),
        new CompanionOption(CompanionKind.Guardbot, "Guardbot", typeof(GuardbotRelic), typeof(GuardbotCard)),
        new CompanionOption(CompanionKind.LivingShield, "Living Shield", typeof(LivingShieldRelic), typeof(LivingShieldCard)),
        new CompanionOption(CompanionKind.MagiKnight, "Magi Knight", typeof(MagiKnightRelic), typeof(MagiKnightCard)),
        new CompanionOption(CompanionKind.MysteriousKnight, "Mysterious Knight", typeof(MysteriousKnightRelic), typeof(MysteriousKnightCard)),
        new CompanionOption(CompanionKind.Myte, "Myte", typeof(MyteRelic), typeof(MyteCard)),
        new CompanionOption(CompanionKind.Nibbit, "Nibbit", typeof(NibbitRelic), typeof(NibbitCard)),
        new CompanionOption(CompanionKind.Noisebot, "Noisebot", typeof(NoisebotRelic), typeof(NoisebotCard)),
        new CompanionOption(CompanionKind.Ovicopter, "Ovicopter", typeof(OvicopterRelic), typeof(OvicopterCard)),
        new CompanionOption(CompanionKind.OwlMagistrate, "Owl Magistrate", typeof(OwlMagistrateRelic), typeof(OwlMagistrateCard)),
        new CompanionOption(CompanionKind.PaelsLegion, "Pael's Legion", typeof(PaelsLegionRelic), typeof(PaelsLegionCard)),
        new CompanionOption(CompanionKind.ScrollOfBiting, "Scroll of Biting", typeof(ScrollOfBitingRelic), typeof(ScrollOfBitingCard)),
        new CompanionOption(CompanionKind.SlimedBerserker, "Slimed Berserker", typeof(SlimedBerserkerRelic), typeof(SlimedBerserkerCard)),
        new CompanionOption(CompanionKind.TheForgotten, "The Forgotten", typeof(TheForgottenRelic), typeof(TheForgottenCard)),
        new CompanionOption(CompanionKind.TheLost, "The Lost", typeof(TheLostRelic), typeof(TheLostCard)),
        new CompanionOption(CompanionKind.TheObscura, "The Obscura", typeof(TheObscuraRelic), typeof(TheObscuraCard)),
        new CompanionOption(CompanionKind.Toadpole, "Toadpole", typeof(ToadpoleRelic), typeof(ToadpoleCard)),
        new CompanionOption(CompanionKind.ToughEgg, "Tough Egg", typeof(ToughEggRelic), typeof(ToughEggCard)),
        new CompanionOption(CompanionKind.TrackerRubyRaider, "Tracker Ruby Raider", typeof(TrackerRubyRaiderRelic), typeof(TrackerRubyRaiderCard)),
        new CompanionOption(CompanionKind.TurretOperator, "Turret Operator", typeof(TurretOperatorRelic), typeof(TurretOperatorCard)),
        new CompanionOption(CompanionKind.Zapbot, "Zapbot", typeof(ZapbotRelic), typeof(ZapbotCard))
    ];

    // Enemy companions follow the acts whose encounter tables contain that enemy.
    // Enemies that occur in two acts are deliberately present in both pools.
    private static readonly Dictionary<Type, HashSet<CompanionKind>> CompanionPoolsByAct = new()
    {
        [typeof(Overgrowth)] =
        [
            CompanionKind.Byrdpip,
            CompanionKind.Wriggler,
            CompanionKind.CeremonialBeast,
            CompanionKind.KinFollower,
            CompanionKind.EyeWithTeeth,
            CompanionKind.TheKin,
            CompanionKind.Vantom,
            CompanionKind.BygoneEffigy,
            CompanionKind.Byrdonis,
            CompanionKind.PhrogParasite,
            CompanionKind.ShrinkerBeetle,
            CompanionKind.AssassinRubyRaider,
            CompanionKind.AxeRubyRaider,
            CompanionKind.BruteRubyRaider,
            CompanionKind.CrossbowRubyRaider,
            CompanionKind.Flyconid,
            CompanionKind.Fogmog,
            CompanionKind.FuzzyWurmCrawler,
            CompanionKind.Inklet,
            CompanionKind.Mawler,
            CompanionKind.Nibbit,
            CompanionKind.SnappingJaxfruit,
            CompanionKind.SlitheringStrangler,
            CompanionKind.LeafSlimeS,
            CompanionKind.LeafSlimeM,
            CompanionKind.TwigSlimeS,
            CompanionKind.TwigSlimeM,
            CompanionKind.VineShambler,
            CompanionKind.TrackerRubyRaider,
            CompanionKind.CubexConstruct
        ],
        [typeof(Underdocks)] =
        [
            CompanionKind.SoulFysh,
            CompanionKind.GremlinMerc,
            CompanionKind.LagavulinMatriarch,
            CompanionKind.WaterfallGiant,
            CompanionKind.Seapunk,
            CompanionKind.SkulkingColony,
            CompanionKind.PhantasmalGardener,
            CompanionKind.TerrorEel,
            CompanionKind.DampCultist,
            CompanionKind.CalcifiedCultist,
            CompanionKind.CorpseSlug,
            CompanionKind.TwoTailedRat,
            CompanionKind.SewerClam,
            CompanionKind.HauntedShip,
            CompanionKind.SludgeSpinner,
            CompanionKind.PunchConstruct,
            CompanionKind.FossilStalker,
            CompanionKind.LivingFog,
            CompanionKind.Toadpole,
            CompanionKind.GasBomb
        ],
        [typeof(Hive)] =
        [
            CompanionKind.ThievingHopper,
            CompanionKind.KaiserCrab,
            CompanionKind.KnowledgeDemon,
            CompanionKind.TheInsatiable,
            CompanionKind.Decimillipede,
            CompanionKind.Entomancer,
            CompanionKind.InfestedPrism,
            CompanionKind.Chomper,
            CompanionKind.BowlbugEgg,
            CompanionKind.BowlbugNectar,
            CompanionKind.BowlbugRock,
            CompanionKind.BowlbugSilk,
            CompanionKind.Crusher,
            CompanionKind.Exoskeleton,
            CompanionKind.HunterKiller,
            CompanionKind.LouseProgenitor,
            CompanionKind.Myte,
            CompanionKind.Ovicopter,
            CompanionKind.Parafright,
            CompanionKind.Rocket,
            CompanionKind.SlumberingBeetle,
            CompanionKind.SpinyToad,
            CompanionKind.TheObscura,
            CompanionKind.ToughEgg,
            CompanionKind.Tunneler
        ],
        [typeof(Glory)] =
        [
            CompanionKind.Aeonglass,
            CompanionKind.Queen,
            CompanionKind.TestSubject,
            CompanionKind.KnightGang,
            CompanionKind.MechaKnight,
            CompanionKind.SoulNexus,
            CompanionKind.CubexConstruct,
            CompanionKind.PunchConstruct,
            CompanionKind.Stabbot,
            CompanionKind.BattleFriendV1,
            CompanionKind.BattleFriendV2,
            CompanionKind.BattleFriendV3,
            CompanionKind.Axebot,
            CompanionKind.DevotedSculptor,
            CompanionKind.Fabricator,
            CompanionKind.FlailKnight,
            CompanionKind.FrogKnight,
            CompanionKind.GlobeHead,
            CompanionKind.Guardbot,
            CompanionKind.LivingShield,
            CompanionKind.MagiKnight,
            CompanionKind.MysteriousKnight,
            CompanionKind.Noisebot,
            CompanionKind.OwlMagistrate,
            CompanionKind.PaelsLegion,
            CompanionKind.ScrollOfBiting,
            CompanionKind.SlimedBerserker,
            CompanionKind.TheForgotten,
            CompanionKind.TheLost,
            CompanionKind.TorchHeadAmalgam,
            CompanionKind.TurretOperator,
            CompanionKind.Zapbot
        ]
    };

    // These companions are not sourced from an act encounter, so they remain
    // available in every route instead of being assigned to a fictional spawn.
    private static readonly HashSet<CompanionKind> UniversalCompanionPool =
    [
        CompanionKind.Architect,
        CompanionKind.Operosis,
        CompanionKind.Rustclad,
        CompanionKind.Shadeleaf,
        CompanionKind.Glitchling,
        CompanionKind.Bonebinder,
        CompanionKind.GildedPage,
        CompanionKind.EmberPip,
        CompanionKind.FrostPip,
        CompanionKind.StormPip,
        CompanionKind.ThornPip
    ];

    public static void Postfix(AncientEventModel __instance, ref IReadOnlyList<EventOption> __result)
    {
        if (__result == null
            || __result.Count == 0
            || (__instance is not Neow && !ModSettings.OfferCompanionsAtEveryAncient))
        {
            return;
        }

        GD.Print("[NeowCompanions] GenerateInitialOptionsWrapper patch HIT. Option count: " + __result.Count);

        int wrappedCount = 0;
        foreach (EventOption option in __result)
        {
            if (TryWrapInitialAncientOption(__instance, option))
            {
                wrappedCount++;
            }
        }

        GD.Print("[NeowCompanions] Wrapped Ancient option actions: " + wrappedCount);
    }

    private static bool TryWrapInitialAncientOption(AncientEventModel ancient, EventOption option)
    {
        if (EventOptionOnChosenField == null
            || option.IsLocked
            || option.IsProceed
            || WrappedInitialOptions.Contains(option))
        {
            return false;
        }

        if (EventOptionOnChosenField.GetValue(option) is not Func<Task> originalOnChosen)
        {
            return false;
        }

        Func<Task> wrappedOnChosen = async () =>
        {
            GD.Print("[NeowCompanions] Ancient option selected, delaying original completion for companion flow: " + option.TextKey);
            if (GetAvailableCompanions(ancient).Count == 0)
            {
                GD.Print("[NeowCompanions] No new companions available; finishing original Ancient option.");
                await originalOnChosen();
                return;
            }

            if (ModSettings.RandomCompanionNoChoices)
            {
                await ChooseRandomCompanion(ancient, originalOnChosen);
                return;
            }

            ShowCompanionChoices(ancient, option, originalOnChosen);
        };

        try
        {
            EventOptionOnChosenField.SetValue(option, wrappedOnChosen);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error("[NeowCompanions] Could not wrap Ancient option action: " + ex);
            GD.PrintErr("[NeowCompanions] Could not wrap Ancient option action: " + ex);
            return false;
        }

        WrappedInitialOptions.Add(option);
        return true;
    }

    private static void ShowCompanionChoices(AncientEventModel ancient, EventOption selectedAncientOption, Func<Task> finishOriginalOption, int page = 0)
    {
        GD.Print("[NeowCompanions] Showing companion choices.");

        List<CompanionOption> availableCompanions = GetAvailableCompanions(ancient);
        List<CompanionOption> offeredCompanions = ModSettings.OfferAllCompanions
            ? availableCompanions
            : GetSeededCompanionChoices(ancient, Math.Min(3, availableCompanions.Count), availableCompanions);

        List<CompanionOption> visibleCompanions = offeredCompanions;

        List<EventOption> companionOptions = new();
        List<string> optionTexts = new();
        List<string> iconFiles = new();

        foreach (CompanionOption companion in visibleCompanions)
        {
            GD.Print("[NeowCompanions] Offering companion: " + companion.DebugName);

            RelicModel displayRelic = GetCompanionRelic(companion).ToMutable();
            string? iconFile = displayRelic is CompanionRelicModel companionRelic
                ? companionRelic.IconFileName
                : null;

            if (ancient.Owner != null)
            {
                displayRelic.Owner = ancient.Owner;
            }

            CardModel previewCard = GetCompanionCard(companion);
            IHoverTip[] hoverTips =
            [
                HoverTipFactory.FromCard(previewCard, upgrade: ModSettings.GrantUpgradedCompanionCards)
            ];

            EventOption companionOption = new EventOption(
                ancient,
                async () =>
                {
                    await ChooseCompanion(ancient, companion, finishOriginalOption);
                    if (!ModSettings.ChooseMultipleCompanionsAtAncient)
                    {
                        return;
                    }

                    if (GetAvailableCompanions(ancient).Count == 0)
                    {
                        ActiveCompanionOptionTexts = null;
                        ActiveCompanionIconFiles = null;
                        await finishOriginalOption();
                        return;
                    }

                    ShowCompanionChoices(ancient, selectedAncientOption, finishOriginalOption, page);
                },
                selectedAncientOption.Title,
                selectedAncientOption.Description,
                "COMPANION." + companion.Kind,
                hoverTips);

            companionOptions.Add(companionOption.WithRelic(displayRelic));
            optionTexts.Add(NeowCompanionText.GetText(companion.TitleKey) + "\n" + NeowCompanionText.GetText(companion.DescriptionKey));
            iconFiles.Add(iconFile ?? string.Empty);
        }

        if (ModSettings.ChooseMultipleCompanionsAtAncient)
        {
            AddFinishChoosingOption(
                ancient,
                selectedAncientOption,
                companionOptions,
                optionTexts,
                iconFiles,
                finishOriginalOption);
        }

        ActiveCompanionOptionTexts = optionTexts;
        ActiveCompanionIconFiles = iconFiles;

        InvokeSetEventState(
            ancient,
            selectedAncientOption.Description,
            companionOptions);
    }

    private static async Task ChooseRandomCompanion(AncientEventModel ancient, Func<Task> finishOriginalOption)
    {
        List<CompanionOption> availableCompanions = GetAvailableCompanions(ancient);
        CompanionOption companion = ancient.Rng.NextItem(availableCompanions) ?? availableCompanions[0];
        GD.Print("[NeowCompanions] Random companion selected: " + companion.DebugName);
        await ChooseCompanion(ancient, companion, finishOriginalOption);
        if (ModSettings.ChooseMultipleCompanionsAtAncient)
        {
            await finishOriginalOption();
        }
    }

    private static List<CompanionOption> GetSeededCompanionChoices(AncientEventModel ancient, int count, List<CompanionOption> companions)
    {
        companions = companions.ToList();
        ancient.Rng.Shuffle(companions);
        return companions.Take(count).ToList();
    }

    private static List<CompanionOption> GetAvailableCompanions(AncientEventModel ancient)
    {
        IEnumerable<CompanionOption> ancientPool = GetCompanionPoolForCurrentAct(ancient);
        if (ancient.Owner == null)
        {
            return ancientPool.ToList();
        }

        return ancientPool
            .Where(companion => !ancient.Owner.Relics.Any(relic => companion.RelicType.IsInstanceOfType(relic)))
            .ToList();
    }

    private static IEnumerable<CompanionOption> GetCompanionPoolForCurrentAct(AncientEventModel ancient)
    {
        if (ModSettings.FullStartingCompanionPool
            && ancient is Neow
            && ancient.Owner?.RunState.CurrentActIndex == 0)
        {
            MainFile.Logger.Info("[NeowCompanions] Using the full companion pool for the starting Neow choice.");
            return CompanionPool;
        }

        Type? actType = ancient.Owner?.RunState.Act.GetType();
        if (actType == null || !CompanionPoolsByAct.TryGetValue(actType, out HashSet<CompanionKind>? actPool))
        {
            MainFile.Logger.Info("[NeowCompanions] Current act could not be identified; using the full companion pool.");
            return CompanionPool;
        }

        MainFile.Logger.Info($"[NeowCompanions] Using the {actType.Name} companion pool.");
        return CompanionPool.Where(companion =>
            actPool.Contains(companion.Kind) || UniversalCompanionPool.Contains(companion.Kind));
    }

    internal static IReadOnlyList<(string ActName, IReadOnlyList<CompanionPoolConfigEntry> Companions)>
        GetCompanionPoolsForConfig()
    {
        return CompanionPoolsByAct
            .Select(pair =>
            {
                IReadOnlyList<CompanionPoolConfigEntry> companions = CompanionPool
                    .Where(companion => pair.Value.Contains(companion.Kind)
                        || UniversalCompanionPool.Contains(companion.Kind))
                    .Select(companion => new CompanionPoolConfigEntry(companion.DebugName, companion.CardType))
                    .ToList();
                return (pair.Key.Name, companions);
            })
            .ToList();
    }

    private static async Task ChooseCompanion(AncientEventModel ancient, CompanionOption companion, Func<Task> finishOriginalOption)
    {
        GD.Print("[NeowCompanions] Chose companion: " + companion.DebugName);

        if (ancient.Owner == null)
        {
            GD.PrintErr("[NeowCompanions] ERROR: Ancient Owner was null when choosing companion.");
            await finishOriginalOption();
            return;
        }

        CompanionState.SelectedCompanion = companion.Kind;
        ActiveCompanionOptionTexts = null;
        ActiveCompanionIconFiles = null;

        await RelicCmd.Obtain(GetCompanionRelic(companion).ToMutable(), ancient.Owner);
        if (ModSettings.GrantCompanionCards)
        {
            await AddCompanionCard(companion, ancient.Owner);
        }

        if (ModSettings.ChooseMultipleCompanionsAtAncient)
        {
            GD.Print("[NeowCompanions] Returning to companion choices after multi-pick.");
            return;
        }

        GD.Print("[NeowCompanions] Finishing original Ancient option now.");
        await finishOriginalOption();
    }

    private static void AddFinishChoosingOption(
        AncientEventModel ancient,
        EventOption originalAncientOption,
        List<EventOption> companionOptions,
        List<string> optionTexts,
        List<string> iconFiles,
        Func<Task> finishOriginalOption)
    {
        EventOption finishOption = new EventOption(
            ancient,
            async () =>
            {
                ActiveCompanionOptionTexts = null;
                ActiveCompanionIconFiles = null;
                GD.Print("[NeowCompanions] Finished choosing multiple companions.");
                await finishOriginalOption();
            },
            originalAncientOption.Title,
            originalAncientOption.Description,
            "COMPANION_NAV.DONE_CHOOSING",
            []);

        companionOptions.Add(finishOption);
        optionTexts.Add("Done choosing\nContinue with the Ancient option.");
        iconFiles.Add(string.Empty);
    }

    private static LocString CompanionLoc(string key)
    {
        return new LocString("neow_companions", key);
    }

    private static RelicModel GetCompanionRelic(CompanionOption companion)
    {
        MethodInfo method = AccessTools.Method(typeof(ModelDb), nameof(ModelDb.Relic), Type.EmptyTypes)
            ?? throw new MissingMethodException("Could not find ModelDb.Relic<T>()");

        object? relic = method.MakeGenericMethod(companion.RelicType).Invoke(null, []);

        return relic as RelicModel
            ?? throw new InvalidOperationException($"Companion relic type '{companion.RelicType.FullName}' did not produce a RelicModel.");
    }

    private static async Task AddCompanionCard(CompanionOption companion, MegaCrit.Sts2.Core.Entities.Players.Player owner)
    {
        CardModel canonicalCard = GetCompanionCard(companion);
        CardModel baseCard = owner.RunState.CreateCard(canonicalCard, owner);
        await CardPileCmd.Add(baseCard, PileType.Deck);

        if (ModSettings.GrantUpgradedCompanionCards && baseCard.IsUpgradable)
        {
            CardModel upgradedCard = owner.RunState.CreateCard(canonicalCard, owner);
            CardCmd.Upgrade(upgradedCard);
            await CardPileCmd.Add(upgradedCard, PileType.Deck);
        }
    }

    private static CardModel GetCompanionCard(CompanionOption companion)
    {
        MethodInfo method = AccessTools.Method(typeof(ModelDb), nameof(ModelDb.Card), Type.EmptyTypes)
            ?? throw new MissingMethodException("Could not find ModelDb.Card<T>()");

        object? canonicalCard = method.MakeGenericMethod(companion.CardType).Invoke(null, []);

        return canonicalCard as CardModel
            ?? throw new InvalidOperationException($"Companion card type '{companion.CardType.FullName}' did not produce a CardModel.");
    }


    private static void InvokeSetEventState(AncientEventModel ancient, LocString description, IReadOnlyList<EventOption> options)
    {
        MethodInfo? method = AccessTools.Method(
            typeof(AncientEventModel),
            "SetEventState",
            [typeof(LocString), typeof(IReadOnlyList<EventOption>)]);

        if (method == null)
        {
            throw new MissingMethodException("Could not find AncientEventModel.SetEventState");
        }

        method.Invoke(ancient, [description, options]);
    }
}

[HarmonyPatch(typeof(NAncientEventLayout), "AnimateButtonsIn")]
public static class NeowCompanionScrollableOptionsPatch
{
    public static void Postfix(NAncientEventLayout __instance)
    {
        if (NeowCompanionChoicePatch.ActiveCompanionOptionCount <= 6)
        {
            return;
        }

        if (AccessTools.Field(typeof(NEventLayout), "_optionsContainer")?.GetValue(__instance)
            is not VBoxContainer optionsContainer)
        {
            return;
        }

        if (optionsContainer.GetParent() is ScrollContainer existingScroll)
        {
            // Multi-pick rebuilds the option buttons inside the same scroll container.
            // Keep its current position and prevent the newly focused first button from
            // dragging the view back to the top.
            existingScroll.FollowFocus = false;
            return;
        }

        if (optionsContainer.GetParent() is not Container parent)
        {
            return;
        }

        int childIndex = optionsContainer.GetIndex();
        float viewportHeight = __instance.GetViewportRect().Size.Y;
        float listHeight = Mathf.Clamp(viewportHeight * 0.62f, 360f, 560f);

        ScrollContainer scroll = new()
        {
            Name = "NeowCompanionScrollList",
            CustomMinimumSize = new Vector2(0f, listHeight),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ShrinkBegin,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            VerticalScrollMode = ScrollContainer.ScrollMode.Auto,
            FollowFocus = false
        };

        parent.AddChild(scroll);
        parent.MoveChild(scroll, childIndex);
        optionsContainer.Reparent(scroll);
        optionsContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        optionsContainer.SizeFlagsVertical = Control.SizeFlags.ShrinkBegin;
    }
}

[HarmonyPatch(typeof(NEventOptionButton), "_Ready")]
public static class NeowCompanionOptionButtonTextPatch
{
    public static void Postfix(NEventOptionButton __instance)
    {
        IReadOnlyList<string>? optionTexts = NeowCompanionChoicePatch.ActiveCompanionOptionTexts;
        IReadOnlyList<string>? iconFiles = NeowCompanionChoicePatch.ActiveCompanionIconFiles;
        object? indexValue = AccessTools.Property(typeof(NEventOptionButton), "Index")?.GetValue(__instance);
        if (optionTexts == null || indexValue is not int index || index < 0 || index >= optionTexts.Count)
        {
            return;
        }

        if (AccessTools.Field(typeof(NEventOptionButton), "_label")?.GetValue(__instance) is GodotObject label)
        {
            label.Set("text", optionTexts[index]);
        }

        if (iconFiles == null || index >= iconFiles.Count || string.IsNullOrEmpty(iconFiles[index]))
        {
            return;
        }

        Texture2D? icon = ModTextureLoader.Load(iconFiles[index]);
        if (icon == null)
        {
            MainFile.Logger.Error($"[NeowCompanions] Could not load companion option icon '{iconFiles[index]}'.");
            return;
        }

        if (AccessTools.Field(typeof(NEventOptionButton), "_image")?.GetValue(__instance) is TextureRect image)
        {
            image.Texture = icon;
            image.Visible = true;
        }

        if (AccessTools.Field(typeof(NEventOptionButton), "_outline")?.GetValue(__instance) is TextureRect outline)
        {
            outline.Texture = icon;
            outline.Visible = true;
        }
    }
}
