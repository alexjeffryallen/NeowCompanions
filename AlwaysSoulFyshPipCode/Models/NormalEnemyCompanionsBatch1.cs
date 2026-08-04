using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace NeowCompanions.NeowCompanionsCode.Models;

public abstract class NormalEnemyCard<TPet> : BossCompanionCard<TPet> where TPet : MonsterModel
{
    private enum EnemyTheme
    {
        Poison, Weak, Vulnerable, Block, Vigor, Strength, Thorns, Slow, Doom
    }

    protected abstract decimal BaseDamage { get; }
    protected virtual int HitCount => 1;
    protected virtual string AnimationName => "Attack";

    protected virtual Task TriggerAttackAnimation()
        => EliteCompanionAnimation.Trigger<TPet>(Owner, AnimationName);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, DamageProps.card),
        new RepeatVar(HitCount),
        new PowerVar<PoisonPower>(3m),
        new PowerVar<WeakPower>(1m),
        new PowerVar<VulnerablePower>(1m),
        new SelfAwareBlockVar(5m, ValueProp.Move),
        new PowerVar<VigorPower>(3m),
        new PowerVar<StrengthPower>(1m),
        new PowerVar<ThornsPower>(2m),
        new PowerVar<SlowPower>(1m),
        new PowerVar<DoomPower>(4m)
    ];

    public override bool GainsBlock => Theme == EnemyTheme.Block;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        Theme is EnemyTheme.Strength or EnemyTheme.Thorns ? [CardKeyword.Exhaust] : [];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => Theme switch
    {
        EnemyTheme.Poison => [HoverTipFactory.FromPower<PoisonPower>()],
        EnemyTheme.Weak => [HoverTipFactory.FromPower<WeakPower>()],
        EnemyTheme.Vulnerable => [HoverTipFactory.FromPower<VulnerablePower>()],
        EnemyTheme.Vigor => [HoverTipFactory.FromPower<VigorPower>()],
        EnemyTheme.Strength => [HoverTipFactory.FromPower<StrengthPower>()],
        EnemyTheme.Thorns => [HoverTipFactory.FromPower<ThornsPower>()],
        EnemyTheme.Slow => [HoverTipFactory.FromPower<SlowPower>()],
        EnemyTheme.Doom => [HoverTipFactory.FromPower<DoomPower>()],
        _ => []
    };

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", AttackDescription + " " + ThemeDescription),
        ("flavor", $"{CompanionName} joins the fray.")
    ];

    protected NormalEnemyCard() : base(1, CardType.Attack, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await TriggerAttackAnimation();
        for (int i = 0; i < DynamicVars.Repeat.IntValue && cardPlay.Target.IsAlive; i++)
            await CreatureCmd.Damage(choiceContext, cardPlay.Target, DynamicVars.Damage, Owner.Creature, this, cardPlay);

        await ApplyTheme(choiceContext, cardPlay);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(HitCount > 1 ? 1m : 3m);

    private string AttackDescription => HitCount == 1
        ? "Deal {Damage:diff} damage."
        : "Deal {Damage:diff} damage {Repeat:diff} times.";

    private string ThemeDescription => Theme switch
    {
        EnemyTheme.Poison => "Apply {PoisonPower:diff} Poison.",
        EnemyTheme.Weak => "Apply {WeakPower:diff} Weak.",
        EnemyTheme.Vulnerable => "Apply {VulnerablePower:diff} Vulnerable.",
        EnemyTheme.Block => "Gain {Block:diff} Block.",
        EnemyTheme.Vigor => "Gain {VigorPower:diff} Vigor.",
        EnemyTheme.Strength => "Gain {StrengthPower:diff} Strength. Exhaust.",
        EnemyTheme.Thorns => "Gain {ThornsPower:diff} Thorns. Exhaust.",
        EnemyTheme.Slow => "Apply Slow.",
        EnemyTheme.Doom => "Apply {DoomPower:diff} Doom.",
        _ => string.Empty
    };

    private async Task ApplyTheme(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Creature? target = cardPlay.Target;
        switch (Theme)
        {
            case EnemyTheme.Poison when target?.IsAlive == true:
                await PowerCmd.Apply<PoisonPower>(choiceContext, target, DynamicVars["PoisonPower"].BaseValue, Owner.Creature, this);
                break;
            case EnemyTheme.Weak when target?.IsAlive == true:
                await PowerCmd.Apply<WeakPower>(choiceContext, target, DynamicVars["WeakPower"].BaseValue, Owner.Creature, this);
                break;
            case EnemyTheme.Vulnerable when target?.IsAlive == true:
                await PowerCmd.Apply<VulnerablePower>(choiceContext, target, DynamicVars["VulnerablePower"].BaseValue, Owner.Creature, this);
                break;
            case EnemyTheme.Block:
                await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay, false);
                break;
            case EnemyTheme.Vigor:
                await PowerCmd.Apply<VigorPower>(choiceContext, Owner.Creature, DynamicVars["VigorPower"].BaseValue, Owner.Creature, this);
                break;
            case EnemyTheme.Strength:
                await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, DynamicVars["StrengthPower"].BaseValue, Owner.Creature, this);
                break;
            case EnemyTheme.Thorns:
                await PowerCmd.Apply<ThornsPower>(choiceContext, Owner.Creature, DynamicVars["ThornsPower"].BaseValue, Owner.Creature, this);
                break;
            case EnemyTheme.Slow when target?.IsAlive == true:
                await PowerCmd.Apply<SlowPower>(choiceContext, target, 1m, Owner.Creature, this);
                break;
            case EnemyTheme.Doom when target?.IsAlive == true:
                await PowerCmd.Apply<DoomPower>(choiceContext, target, DynamicVars["DoomPower"].BaseValue, Owner.Creature, this);
                break;
        }
    }

    private static EnemyTheme Theme
    {
        get
        {
            Type pet = typeof(TPet);
            if (pet == typeof(FlyconidPet) || pet == typeof(FuzzyWurmCrawlerPet) || pet == typeof(TwoTailedRatPet)
                || pet == typeof(SludgeSpinnerPet) || pet == typeof(MytePet) || pet == typeof(GasBombPet)
                || pet == typeof(ToadpolePet) || pet == typeof(BowlbugNectarPet))
                return EnemyTheme.Poison;
            if (pet == typeof(FogmogPet) || pet == typeof(InkletPet) || pet == typeof(HauntedShipPet)
                || pet == typeof(LivingFogPet) || pet == typeof(NoisebotPet) || pet == typeof(TheForgottenPet)
                || pet == typeof(LeafSlimeSPet) || pet == typeof(LeafSlimeMPet))
                return EnemyTheme.Weak;
            if (pet == typeof(BruteRubyRaiderPet) || pet == typeof(MawlerPet) || pet == typeof(VineShamblerPet)
                || pet == typeof(ChomperPet) || pet == typeof(CorpseSlugPet) || pet == typeof(FossilStalkerPet)
                || pet == typeof(HunterKillerPet) || pet == typeof(CrusherPet) || pet == typeof(ExoskeletonPet)
                || pet == typeof(FlailKnightPet) || pet == typeof(FrogKnightPet) || pet == typeof(MysteriousKnightPet)
                || pet == typeof(RocketPet) || pet == typeof(ScrollOfBitingPet) || pet == typeof(TrackerRubyRaiderPet))
                return EnemyTheme.Vulnerable;
            if (pet == typeof(AxeRubyRaiderPet) || pet == typeof(SlitheringStranglerPet) || pet == typeof(SewerClamPet)
                || pet == typeof(PunchConstructPet) || pet == typeof(BowlbugRockPet) || pet == typeof(SlumberingBeetlePet)
                || pet == typeof(GuardbotPet) || pet == typeof(LivingShieldPet) || pet == typeof(TheObscuraPet)
                || pet == typeof(ToughEggPet))
                return EnemyTheme.Block;
            if (pet == typeof(DampCultistPet) || pet == typeof(CalcifiedCultistPet) || pet == typeof(StabbotPet)
                || pet == typeof(TorchHeadAmalgamPet) || pet == typeof(SlimedBerserkerPet) || pet == typeof(PaelsLegionPet))
                return EnemyTheme.Strength;
            if (pet == typeof(TwigSlimeSPet) || pet == typeof(TwigSlimeMPet) || pet == typeof(SpinyToadPet)
                || pet == typeof(BowlbugSilkPet) || pet == typeof(LouseProgenitorPet))
                return EnemyTheme.Thorns;
            if (pet == typeof(CubexConstructPet) || pet == typeof(AxebotPet) || pet == typeof(BattleFriendV1Pet)
                || pet == typeof(BattleFriendV2Pet) || pet == typeof(BattleFriendV3Pet) || pet == typeof(FabricatorPet)
                || pet == typeof(TurretOperatorPet) || pet == typeof(ZapbotPet) || pet == typeof(ParafrightPet))
                return EnemyTheme.Slow;
            if (pet == typeof(AssassinRubyRaiderPet) || pet == typeof(CrossbowRubyRaiderPet)
                || pet == typeof(SnappingJaxfruitPet) || pet == typeof(NibbitPet) || pet == typeof(OwlMagistratePet)
                || pet == typeof(OvicopterPet))
                return EnemyTheme.Vigor;
            return EnemyTheme.Doom;
        }
    }
}

#region Ruby Raiders
[Pool(typeof(NeowCompanionRelicPool))] public sealed class AssassinRubyRaiderRelic : BossCompanionRelic<AssassinRubyRaiderPet> { protected override string CompanionName => "Assassin Ruby Raider"; protected override string RelicIconFileName => "relic_assassin_ruby_raider.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class AssassinRubyRaiderCard : NormalEnemyCard<AssassinRubyRaiderPet> { protected override string CompanionName => "Assassin Ruby Raider"; protected override string CardTitle => "Killshot"; protected override string CardArtFileName => "card_assassin_ruby_raider.png"; protected override decimal BaseDamage => 11m; }
public sealed class AssassinRubyRaiderPet : BossCompanionPet<AssassinRubyRaider> { protected override float PetScale => 0.32f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class AxeRubyRaiderRelic : BossCompanionRelic<AxeRubyRaiderPet> { protected override string CompanionName => "Axe Ruby Raider"; protected override string RelicIconFileName => "relic_axe_ruby_raider.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class AxeRubyRaiderCard : NormalEnemyCard<AxeRubyRaiderPet> { protected override string CompanionName => "Axe Ruby Raider"; protected override string CardTitle => "Wide Swing"; protected override string CardArtFileName => "card_axe_ruby_raider.png"; protected override decimal BaseDamage => 10m; }
public sealed class AxeRubyRaiderPet : BossCompanionPet<AxeRubyRaider> { protected override float PetScale => 0.32f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class BruteRubyRaiderRelic : BossCompanionRelic<BruteRubyRaiderPet> { protected override string CompanionName => "Brute Ruby Raider"; protected override string RelicIconFileName => "relic_brute_ruby_raider.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class BruteRubyRaiderCard : NormalEnemyCard<BruteRubyRaiderPet> { protected override string CompanionName => "Brute Ruby Raider"; protected override string CardTitle => "Brutal Beating"; protected override string CardArtFileName => "card_brute_ruby_raider.png"; protected override decimal BaseDamage => 12m; }
public sealed class BruteRubyRaiderPet : BossCompanionPet<BruteRubyRaider> { protected override float PetScale => 0.32f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class CrossbowRubyRaiderRelic : BossCompanionRelic<CrossbowRubyRaiderPet> { protected override string CompanionName => "Crossbow Ruby Raider"; protected override string RelicIconFileName => "relic_crossbow_ruby_raider.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class CrossbowRubyRaiderCard : NormalEnemyCard<CrossbowRubyRaiderPet> { protected override string CompanionName => "Crossbow Ruby Raider"; protected override string CardTitle => "Ruby Volley"; protected override string CardArtFileName => "card_crossbow_ruby_raider.png"; protected override decimal BaseDamage => 4m; protected override int HitCount => 2; }
public sealed class CrossbowRubyRaiderPet : BossCompanionPet<CrossbowRubyRaider> { protected override float PetScale => 0.32f; }
#endregion

#region Overgrowth
[Pool(typeof(NeowCompanionRelicPool))] public sealed class FlyconidRelic : BossCompanionRelic<FlyconidPet> { protected override string CompanionName => "Flyconid"; protected override string RelicIconFileName => "relic_flyconid.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class FlyconidCard : NormalEnemyCard<FlyconidPet> { protected override string CompanionName => "Flyconid"; protected override string CardTitle => "Spore Smash"; protected override string CardArtFileName => "card_flyconid.png"; protected override decimal BaseDamage => 9m; }
public sealed class FlyconidPet : BossCompanionPet<Flyconid> { protected override float PetScale => 0.34f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class FogmogRelic : BossCompanionRelic<FogmogPet> { protected override string CompanionName => "Fogmog"; protected override string RelicIconFileName => "relic_fogmog.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class FogmogCard : NormalEnemyCard<FogmogPet> { protected override string CompanionName => "Fogmog"; protected override string CardTitle => "Fog Swipe"; protected override string CardArtFileName => "card_fogmog.png"; protected override decimal BaseDamage => 9m; }
public sealed class FogmogPet : BossCompanionPet<Fogmog> { protected override float PetScale => 0.34f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class MawlerRelic : BossCompanionRelic<MawlerPet> { protected override string CompanionName => "Mawler"; protected override string RelicIconFileName => "relic_mawler.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class MawlerCard : NormalEnemyCard<MawlerPet> { protected override string CompanionName => "Mawler"; protected override string CardTitle => "Rip and Tear"; protected override string CardArtFileName => "card_mawler.png"; protected override decimal BaseDamage => 5m; protected override int HitCount => 2; }
public sealed class MawlerPet : BossCompanionPet<Mawler> { protected override float PetScale => 0.34f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class FuzzyWurmCrawlerRelic : BossCompanionRelic<FuzzyWurmCrawlerPet> { protected override string CompanionName => "Fuzzy Wurm Crawler"; protected override string RelicIconFileName => "relic_fuzzy_wurm_crawler.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class FuzzyWurmCrawlerCard : NormalEnemyCard<FuzzyWurmCrawlerPet> { protected override string CompanionName => "Fuzzy Wurm Crawler"; protected override string CardTitle => "Acid Goop"; protected override string CardArtFileName => "card_fuzzy_wurm_crawler.png"; protected override decimal BaseDamage => 8m; }
public sealed class FuzzyWurmCrawlerPet : BossCompanionPet<FuzzyWurmCrawler> { protected override float PetScale => 0.32f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class InkletRelic : BossCompanionRelic<InkletPet> { protected override string CompanionName => "Inklet"; protected override string RelicIconFileName => "relic_inklet.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class InkletCard : NormalEnemyCard<InkletPet> { protected override string CompanionName => "Inklet"; protected override string CardTitle => "Ink Whirlwind"; protected override string CardArtFileName => "card_inklet.png"; protected override decimal BaseDamage => 3m; protected override int HitCount => 3; }
public sealed class InkletPet : BossCompanionPet<Inklet> { protected override float PetScale => 0.42f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class SnappingJaxfruitRelic : BossCompanionRelic<SnappingJaxfruitPet> { protected override string CompanionName => "Snapping Jaxfruit"; protected override string RelicIconFileName => "relic_snapping_jaxfruit.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class SnappingJaxfruitCard : NormalEnemyCard<SnappingJaxfruitPet> { protected override string CompanionName => "Snapping Jaxfruit"; protected override string CardTitle => "Energy Orb"; protected override string CardArtFileName => "card_snapping_jaxfruit.png"; protected override decimal BaseDamage => 10m; protected override string AnimationName => "Cast"; }
public sealed class SnappingJaxfruitPet : BossCompanionPet<SnappingJaxfruit> { protected override float PetScale => 0.34f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class SlitheringStranglerRelic : BossCompanionRelic<SlitheringStranglerPet> { protected override string CompanionName => "Slithering Strangler"; protected override string RelicIconFileName => "relic_slithering_strangler.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class SlitheringStranglerCard : NormalEnemyCard<SlitheringStranglerPet> { protected override string CompanionName => "Slithering Strangler"; protected override string CardTitle => "Tail Thwack"; protected override string CardArtFileName => "card_slithering_strangler.png"; protected override decimal BaseDamage => 10m; protected override string AnimationName => "AttackDefendTrigger"; }
public sealed class SlitheringStranglerPet : BossCompanionPet<SlitheringStrangler> { protected override float PetScale => 0.34f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class LeafSlimeSRelic : BossCompanionRelic<LeafSlimeSPet> { protected override string CompanionName => "Small Leaf Slime"; protected override string RelicIconFileName => "relic_leaf_slime_s.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class LeafSlimeSCard : NormalEnemyCard<LeafSlimeSPet> { protected override string CompanionName => "Small Leaf Slime"; protected override string CardTitle => "Leaf Tackle"; protected override string CardArtFileName => "card_leaf_slime_s.png"; protected override decimal BaseDamage => 7m; }
public sealed class LeafSlimeSPet : BossCompanionPet<LeafSlimeS> { protected override float PetScale => 0.46f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class LeafSlimeMRelic : BossCompanionRelic<LeafSlimeMPet> { protected override string CompanionName => "Medium Leaf Slime"; protected override string RelicIconFileName => "relic_leaf_slime_m.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class LeafSlimeMCard : NormalEnemyCard<LeafSlimeMPet> { protected override string CompanionName => "Medium Leaf Slime"; protected override string CardTitle => "Clump Shot"; protected override string CardArtFileName => "card_leaf_slime_m.png"; protected override decimal BaseDamage => 9m; }
public sealed class LeafSlimeMPet : BossCompanionPet<LeafSlimeM> { protected override float PetScale => 0.38f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class TwigSlimeSRelic : BossCompanionRelic<TwigSlimeSPet> { protected override string CompanionName => "Small Twig Slime"; protected override string RelicIconFileName => "relic_twig_slime_s.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class TwigSlimeSCard : NormalEnemyCard<TwigSlimeSPet> { protected override string CompanionName => "Small Twig Slime"; protected override string CardTitle => "Twig Tackle"; protected override string CardArtFileName => "card_twig_slime_s.png"; protected override decimal BaseDamage => 7m; }
public sealed class TwigSlimeSPet : BossCompanionPet<TwigSlimeS> { protected override float PetScale => 0.46f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class TwigSlimeMRelic : BossCompanionRelic<TwigSlimeMPet> { protected override string CompanionName => "Medium Twig Slime"; protected override string RelicIconFileName => "relic_twig_slime_m.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class TwigSlimeMCard : NormalEnemyCard<TwigSlimeMPet> { protected override string CompanionName => "Medium Twig Slime"; protected override string CardTitle => "Pokey Pounce"; protected override string CardArtFileName => "card_twig_slime_m.png"; protected override decimal BaseDamage => 9m; }
public sealed class TwigSlimeMPet : BossCompanionPet<TwigSlimeM> { protected override float PetScale => 0.38f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class VineShamblerRelic : BossCompanionRelic<VineShamblerPet> { protected override string CompanionName => "Vine Shambler"; protected override string RelicIconFileName => "relic_vine_shambler.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class VineShamblerCard : NormalEnemyCard<VineShamblerPet> { protected override string CompanionName => "Vine Shambler"; protected override string CardTitle => "Grasping Vines"; protected override string CardArtFileName => "card_vine_shambler.png"; protected override decimal BaseDamage => 10m; protected override string AnimationName => "Vines"; }
public sealed class VineShamblerPet : BossCompanionPet<VineShambler> { protected override float PetScale => 0.32f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class ChomperRelic : BossCompanionRelic<ChomperPet> { protected override string CompanionName => "Chomper"; protected override string RelicIconFileName => "relic_chomper.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class ChomperCard : NormalEnemyCard<ChomperPet> { protected override string CompanionName => "Chomper"; protected override string CardTitle => "Clamp Down"; protected override string CardArtFileName => "card_chomper.png"; protected override decimal BaseDamage => 11m; }
public sealed class ChomperPet : BossCompanionPet<Chomper> { protected override float PetScale => 0.38f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class CubexConstructRelic : BossCompanionRelic<CubexConstructPet> { protected override string CompanionName => "Cubex Construct"; protected override string RelicIconFileName => "relic_cubex_construct.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class CubexConstructCard : NormalEnemyCard<CubexConstructPet> { protected override string CompanionName => "Cubex Construct"; protected override string CardTitle => "Repeater Blast"; protected override string CardArtFileName => "card_cubex_construct.png"; protected override decimal BaseDamage => 3m; protected override int HitCount => 3; }
public sealed class CubexConstructPet : BossCompanionPet<CubexConstruct> { protected override float PetScale => 0.36f; }
#endregion

#region Underdocks Cultists
[Pool(typeof(NeowCompanionRelicPool))] public sealed class DampCultistRelic : BossCompanionRelic<DampCultistPet> { protected override string CompanionName => "Damp Cultist"; protected override string RelicIconFileName => "relic_damp_cultist.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class DampCultistCard : NormalEnemyCard<DampCultistPet> { protected override string CompanionName => "Damp Cultist"; protected override string CardTitle => "Damp Strike"; protected override string CardArtFileName => "card_damp_cultist.png"; protected override decimal BaseDamage => 9m; }
public sealed class DampCultistPet : BossCompanionPet<DampCultist> { protected override float PetScale => 0.34f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class CalcifiedCultistRelic : BossCompanionRelic<CalcifiedCultistPet> { protected override string CompanionName => "Calcified Cultist"; protected override string RelicIconFileName => "relic_calcified_cultist.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class CalcifiedCultistCard : NormalEnemyCard<CalcifiedCultistPet> { protected override string CompanionName => "Calcified Cultist"; protected override string CardTitle => "Calcified Strike"; protected override string CardArtFileName => "card_calcified_cultist.png"; protected override decimal BaseDamage => 10m; }
public sealed class CalcifiedCultistPet : BossCompanionPet<CalcifiedCultist> { protected override float PetScale => 0.34f; }
#endregion
