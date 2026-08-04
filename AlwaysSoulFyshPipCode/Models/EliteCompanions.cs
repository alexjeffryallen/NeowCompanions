using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;

namespace NeowCompanions.NeowCompanionsCode.Models;

internal static class EliteCompanionAnimation
{
    public static Task Trigger<TPet>(
        MegaCrit.Sts2.Core.Entities.Players.Player owner,
        params string[] animationNames)
        where TPet : MonsterModel
    {
        Creature? pet = owner.PlayerCombatState?.GetPet<TPet>();
        return pet == null || pet.IsDead
            ? Task.CompletedTask
            : CompanionAnimation.TryTriggerAnimation(pet, animationNames);
    }
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class BygoneEffigyRelic : BossCompanionRelic<BygoneEffigyPet>
{
    protected override string CompanionName => "Bygone Effigy";
    protected override string RelicIconFileName => "relic_bygone_effigy.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class BygoneEffigyCard : BossCompanionCard<BygoneEffigyPet>
{
    protected override string CompanionName => "Bygone Effigy";
    protected override string CardTitle => "Stone Vigil";
    protected override string CardArtFileName => "card_bygone_effigy.png";
    public override TargetType TargetType =>
        IsUpgraded ? TargetType.AllEnemies : TargetType.AnyEnemy;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IfUpgradedVar("IfUpgraded", 0m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<SlowPower>()
    ];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Apply Slow to an enemy.{IfUpgraded:show: Targets ALL enemies.|}"),
        ("flavor", "The old stone remembers how to stand.")
    ];

    public BygoneEffigyCard() : base(2, CardType.Skill, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
            return;

        await EliteCompanionAnimation.Trigger<BygoneEffigyPet>(Owner, "Attack");

        IReadOnlyList<Creature> targets = IsUpgraded
            ? CombatState.HittableEnemies.Where(enemy => enemy.IsAlive).ToList()
            : [cardPlay.Target ?? throw new ArgumentNullException(nameof(cardPlay.Target))];

        await PowerCmd.Apply<SlowPower>(
            choiceContext,
            targets,
            1m,
            Owner.Creature,
            this);
    }
}

public sealed class BygoneEffigyPet : BossCompanionPet<BygoneEffigy>
{
    protected override float PetScale => 0.34f;
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class ByrdonisRelic : BossCompanionRelic<ByrdonisPet>
{
    protected override string CompanionName => "Byrdonis";
    protected override string RelicIconFileName => "relic_byrdonis.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class ByrdonisCard : BossCompanionCard<ByrdonisPet>
{
    protected override string CompanionName => "Byrdonis";
    protected override string CardTitle => "Royal Pecking";
    protected override string CardArtFileName => "card_byrdonis.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(2m, DamageProps.card),
        new RepeatVar(3)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Deal {Damage:diff} damage {Repeat:diff} times. If lethal, permanently add a hatchable Byrdonis Egg to your deck. Exhaust."),
        ("flavor", "A crown does not make the beak less sharp.")
    ];

    public ByrdonisCard() : base(0, CardType.Attack, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await EliteCompanionAnimation.Trigger<ByrdonisPet>(Owner, "Attack");
        for (int i = 0; i < DynamicVars.Repeat.IntValue && cardPlay.Target.IsAlive; i++)
        {
            await CreatureCmd.Damage(choiceContext, cardPlay.Target, DynamicVars.Damage, Owner.Creature, this, cardPlay);
        }

        if (!cardPlay.Target.IsAlive)
        {
            CardModel egg = Owner.RunState.CreateCard(ModelDb.Card<MegaCrit.Sts2.Core.Models.Cards.ByrdonisEgg>(), Owner);
            await CardPileCmd.Add(egg, PileType.Deck);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Repeat.UpgradeValueBy(1m);
}

public sealed class ByrdonisPet : BossCompanionPet<Byrdonis>
{
    protected override float PetScale => 0.36f;
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class PhrogParasiteRelic : BossCompanionRelic<PhrogParasitePet>
{
    protected override string CompanionName => "Phrog Parasite";
    protected override string RelicIconFileName => "relic_phrog_parasite.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class PhrogParasiteCard : BossCompanionCard<PhrogParasitePet>
{
    protected override string CompanionName => "Phrog Parasite";
    protected override string CardTitle => "Parasitic Lash";
    protected override string CardArtFileName => "card_phrog_parasite.png";

    public override TargetType TargetType =>
        IsUpgraded ? TargetType.AllEnemies : TargetType.AnyEnemy;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, DamageProps.card),
        new PowerVar<PoisonPower>(3m),
        new IfUpgradedVar("IfUpgraded", 0m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PoisonPower>()];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Deal {Damage:diff} damage. Apply {PoisonPower:diff} Poison. Add an Infection to your hand.{IfUpgraded:show: Targets ALL enemies.|}"),
        ("flavor", "The tongue arrives before the appetite.")
    ];

    public PhrogParasiteCard() : base(0, CardType.Attack, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
            return;

        await EliteCompanionAnimation.Trigger<PhrogParasitePet>(Owner, "Attack");

        IReadOnlyList<Creature> targets = IsUpgraded
            ? CombatState.HittableEnemies.Where(enemy => enemy.IsAlive).ToList()
            : [cardPlay.Target ?? throw new ArgumentNullException(nameof(cardPlay.Target))];

        foreach (Creature target in targets)
        {
            await CreatureCmd.Damage(choiceContext, target, DynamicVars.Damage, Owner.Creature, this, cardPlay);
        }

        await PowerCmd.Apply<PoisonPower>(
            choiceContext, targets, DynamicVars.Poison.BaseValue, Owner.Creature, this);

        CardModel infection = CombatState.CreateCard(ModelDb.Card<Infection>(), Owner);
        await CardPileCmd.AddGeneratedCardToCombat(infection, PileType.Hand, Owner);
    }
}

public sealed class PhrogParasitePet : BossCompanionPet<PhrogParasite>
{
    protected override float PetScale => 0.40f;
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class SkulkingColonyRelic : BossCompanionRelic<SkulkingColonyPet>
{
    protected override string CompanionName => "Skulking Colony";
    protected override string RelicIconFileName => "relic_skulking_colony.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class SkulkingColonyCard : BossCompanionCard<SkulkingColonyPet>
{
    protected override string CompanionName => "Skulking Colony";
    protected override string CardTitle => "Colony Momentum";
    protected override string CardArtFileName => "card_skulking_colony.png";
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<HardenedShellPower>(15m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<HardenedShellPower>()
    ];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "You can lose no more than {HardenedShellPower:diff} HP each turn. Ethereal."),
        ("flavor", "Every shell in the pile leans into the blow.")
    ];

    public SkulkingColonyCard() : base(3, CardType.Power, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await EliteCompanionAnimation.Trigger<SkulkingColonyPet>(
            Owner, "AttackDouble");
        await PowerCmd.Apply<HardenedShellPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["HardenedShellPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

public sealed class SkulkingColonyPet : BossCompanionPet<SkulkingColony>
{
    protected override float PetScale => 0.30f;
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class PhantasmalGardenerRelic : BossCompanionRelic<PhantasmalGardenerPet>
{
    protected override string CompanionName => "Phantasmal Gardener";
    protected override string RelicIconFileName => "relic_phantasmal_gardener.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class PhantasmalGardenerCard : BossCompanionCard<PhantasmalGardenerPet>
{
    protected override string CompanionName => "Phantasmal Gardener";
    protected override string CardTitle => "Ghostly Overgrowth";
    protected override string CardArtFileName => "card_phantasmal_gardener.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(1m, DamageProps.card),
        new RepeatVar(3),
        new DynamicVar("Replay", 1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.ReplayStatic)
    ];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Deal {Damage:diff} damage {Repeat:diff} times. Add {Replay:diff} Replay to this card for the rest of combat."),
        ("flavor", "What it tends grows larger than memory.")
    ];

    public PhantasmalGardenerCard() : base(1, CardType.Attack, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await EliteCompanionAnimation.Trigger<PhantasmalGardenerPet>(Owner, "Attack", "Cast");

        for (int i = 0; i < DynamicVars.Repeat.IntValue && cardPlay.Target.IsAlive; i++)
        {
            await CreatureCmd.Damage(
                choiceContext,
                cardPlay.Target,
                DynamicVars.Damage,
                Owner.Creature,
                this,
                cardPlay);
        }

        if (CurrentPlayIndex == 0)
        {
            BaseReplayCount += DynamicVars["Replay"].IntValue;
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(1m);
}

public sealed class PhantasmalGardenerPet : BossCompanionPet<PhantasmalGardener>
{
    protected override float PetScale => 0.43f;
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class TerrorEelRelic : BossCompanionRelic<TerrorEelPet>
{
    protected override string CompanionName => "Terror Eel";
    protected override string RelicIconFileName => "relic_terror_eel.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class TerrorEelCard : BossCompanionCard<TerrorEelPet>
{
    protected override string CompanionName => "Terror Eel";
    protected override string CardTitle => "Terror Thrash";
    protected override string CardArtFileName => "card_terror_eel.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<TerrorEelAmbushPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<TerrorEelAmbushPower>()
    ];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Mark an enemy. When its HP falls below half, Stun it for 1 turn."),
        ("flavor", "The water panics first.")
    ];

    public TerrorEelCard() : base(3, CardType.Skill, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await EliteCompanionAnimation.Trigger<TerrorEelPet>(Owner, "AttackTripleTrigger");
        await PowerCmd.Apply<TerrorEelAmbushPower>(
            choiceContext,
            cardPlay.Target,
            1m,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}

public sealed class TerrorEelAmbushPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization =>
    [
        ("title", "Terror Eel Ambush"),
        ("description", "When this creature's HP falls below half, it is Stunned for 1 turn and this is removed.")
    ];

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner || target.IsDead || target.CurrentHp * 2 >= target.MaxHp)
            return;

        Flash();
        await CreatureCmd.Stun(target);
        await PowerCmd.Remove(this);
    }
}

public sealed class TerrorEelPet : BossCompanionPet<TerrorEel>
{
    protected override float PetScale => 0.26f;
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class DecimillipedeRelic : CompanionRelicModel
{
    public override string IconFileName => "relic_decimillipede.png";

    public override List<(string, string)> Localization =>
    [
        ("title", "Decimillipede"),
        ("description", "At the start of each combat, summon all three Decimillipede segments."),
        ("flavor", "Every segment remembers the shape of the whole.")
    ];

    public override async Task BeforeCombatStart()
    {
        await PlayerCmd.AddPet<DecimillipedePet>(Owner);
        await PlayerCmd.AddPet<DecimillipedeMiddlePet>(Owner);
        await PlayerCmd.AddPet<DecimillipedeBackPet>(Owner);
    }
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class DecimillipedeCard : BossCompanionCard<DecimillipedePet>
{
    protected override string CompanionName => "Decimillipede";
    protected override string CardTitle => "Reattach";
    protected override string CardArtFileName => "card_decimillipede.png";
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Heal", 7m)
    ];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Heal {Heal:diff} HP."),
        ("flavor", "One segment remembers the shape of the whole.")
    ];

    public DecimillipedeCard() : base(1, CardType.Skill, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await TriggerAllSegments(Owner);
        await CreatureCmd.Heal(Owner.Creature, DynamicVars["Heal"].BaseValue, false);
    }

    internal static async Task TriggerAllSegments(MegaCrit.Sts2.Core.Entities.Players.Player owner)
    {
        if (owner.PlayerCombatState == null)
            return;

        TriggerSegment(owner.PlayerCombatState.GetPet<DecimillipedePet>());
        TriggerSegment(owner.PlayerCombatState.GetPet<DecimillipedeMiddlePet>());
        TriggerSegment(owner.PlayerCombatState.GetPet<DecimillipedeBackPet>());
        await Cmd.Wait(0.35f);
    }

    private static void TriggerSegment(Creature? creature)
    {
        if (creature is { IsDead: false, Monster: DecimillipedeSegment segment })
        {
            segment.SegmentAttack();
        }
    }

    protected override void OnUpgrade() => DynamicVars["Heal"].UpgradeValueBy(4m);
}

public sealed class DecimillipedePet : BossCompanionPet<DecimillipedeSegmentFront>
{
    protected override float PetScale => 0.48f;

    public override NCreatureVisuals? CreateCustomVisuals()
    {
        NCreatureVisuals visuals = ModelDb.Monster<DecimillipedeSegmentFront>().CreateVisuals();
        visuals.Scale = new Vector2(-PetScale, PetScale);
        return CompanionDrag.MakeLinkedDraggable(visuals, "Decimillipede", new Vector2(205f, 0f));
    }
}

public sealed class DecimillipedeMiddlePet : BossCompanionPet<DecimillipedeSegmentMiddle>
{
    protected override float PetScale => 0.48f;

    public override NCreatureVisuals? CreateCustomVisuals()
    {
        NCreatureVisuals visuals = ModelDb.Monster<DecimillipedeSegmentMiddle>().CreateVisuals();
        visuals.Scale = new Vector2(-PetScale, PetScale);
        return CompanionDrag.MakeLinkedDraggable(visuals, "Decimillipede", Vector2.Zero);
    }
}

public sealed class DecimillipedeBackPet : BossCompanionPet<DecimillipedeSegmentBack>
{
    protected override float PetScale => 0.48f;

    public override NCreatureVisuals? CreateCustomVisuals()
    {
        NCreatureVisuals visuals = ModelDb.Monster<DecimillipedeSegmentBack>().CreateVisuals();
        visuals.Scale = new Vector2(-PetScale, PetScale);
        return CompanionDrag.MakeLinkedDraggable(visuals, "Decimillipede", new Vector2(-205f, 0f));
    }
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class EntomancerRelic : BossCompanionRelic<EntomancerPet>
{
    protected override string CompanionName => "Entomancer";
    protected override string RelicIconFileName => "relic_entomancer.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class EntomancerCard : BossCompanionCard<EntomancerPet>
{
    protected override string CompanionName => "Entomancer";
    protected override string CardTitle => "Bee Barrage";
    protected override string CardArtFileName => "card_entomancer.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4m, DamageProps.card),
        new RepeatVar(2),
        new PowerVar<WeakPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WeakPower>()];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Deal {Damage:diff} damage {Repeat:diff} times. Apply {WeakPower:diff} Weak."),
        ("flavor", "The swarm has already received its orders.")
    ];

    public EntomancerCard() : base(1, CardType.Attack, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await EliteCompanionAnimation.Trigger<EntomancerPet>(Owner, "Attack");
        for (int i = 0; i < DynamicVars.Repeat.IntValue && cardPlay.Target.IsAlive; i++)
        {
            await CreatureCmd.Damage(choiceContext, cardPlay.Target, DynamicVars.Damage, Owner.Creature, this, cardPlay);
        }
        if (cardPlay.Target.IsAlive)
        {
            await PowerCmd.Apply<WeakPower>(
                choiceContext, cardPlay.Target, DynamicVars.Weak.BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Repeat.UpgradeValueBy(1m);
}

public sealed class EntomancerPet : BossCompanionPet<Entomancer>
{
    protected override float PetScale => 0.35f;
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class InfestedPrismRelic : BossCompanionRelic<InfestedPrismPet>
{
    protected override string CompanionName => "Infested Prism";
    protected override string RelicIconFileName => "relic_infested_prism.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class InfestedPrismCard : BossCompanionCard<InfestedPrismPet>
{
    protected override string CompanionName => "Infested Prism";
    protected override string CardTitle => "Vital Spark";
    protected override string CardArtFileName => "card_infested_prism.png";
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SelfAwareBlockVar(8m, ValueProp.Move),
        new PowerVar<VigorPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VigorPower>()];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Gain {Block:diff} Block and {VigorPower:diff} Vigor."),
        ("flavor", "The light bends, hardens, and points outward.")
    ];

    public InfestedPrismCard() : base(1, CardType.Skill, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await EliteCompanionAnimation.Trigger<InfestedPrismPet>(Owner, "AttackBlock");
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay, false);
        await PowerCmd.Apply<VigorPower>(
            choiceContext, Owner.Creature, DynamicVars["VigorPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
        DynamicVars["VigorPower"].UpgradeValueBy(1m);
    }
}

public sealed class InfestedPrismPet : BossCompanionPet<InfestedPrism>
{
    protected override float PetScale => 0.34f;
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class KnightGangRelic : BossCompanionRelic<KnightGangPet>
{
    protected override string CompanionName => "Knight Gang";
    protected override string RelicIconFileName => "relic_knight_gang.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class KnightGangCard : BossCompanionCard<KnightGangPet>
{
    protected override string CompanionName => "Knight Gang";
    protected override string CardTitle => "Spectral Assault";
    protected override string CardArtFileName => "card_knight_gang.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(14m, DamageProps.card),
        new PowerVar<WeakPower>(2m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WeakPower>()];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Deal {Damage:diff} damage. Apply {WeakPower:diff} Weak."),
        ("flavor", "The others are never far behind.")
    ];

    public KnightGangCard() : base(2, CardType.Attack, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await EliteCompanionAnimation.Trigger<KnightGangPet>(Owner, "AttackSword");
        await CreatureCmd.Damage(choiceContext, cardPlay.Target, DynamicVars.Damage, Owner.Creature, this, cardPlay);
        if (cardPlay.Target.IsAlive)
        {
            await PowerCmd.Apply<WeakPower>(
                choiceContext, cardPlay.Target, DynamicVars.Weak.BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(5m);
}

public sealed class KnightGangPet : BossCompanionPet<SpectralKnight>
{
    protected override float PetScale => 0.43f;
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class MechaKnightRelic : BossCompanionRelic<MechaKnightPet>
{
    protected override string CompanionName => "Mecha Knight";
    protected override string RelicIconFileName => "relic_mecha_knight.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class MechaKnightCard : BossCompanionCard<MechaKnightPet>
{
    protected override string CompanionName => "Mecha Knight";
    protected override string CardTitle => "Heavy Cleave";
    protected override string CardArtFileName => "card_mecha_knight.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(20m, DamageProps.card)
    ];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Deal {Damage:diff} damage."),
        ("flavor", "First the engine winds. Then the room moves.")
    ];

    public MechaKnightCard() : base(2, CardType.Attack, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await EliteCompanionAnimation.Trigger<MechaKnightPet>(Owner, "attack_cleave");
        await CreatureCmd.Damage(choiceContext, cardPlay.Target, DynamicVars.Damage, Owner.Creature, this, cardPlay);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(6m);
}

public sealed class MechaKnightPet : BossCompanionPet<MechaKnight>
{
    protected override float PetScale => 0.30f;
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class SoulNexusRelic : BossCompanionRelic<SoulNexusPet>
{
    protected override string CompanionName => "Soul Nexus";
    protected override string RelicIconFileName => "relic_soul_nexus.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class SoulNexusCard : BossCompanionCard<SoulNexusPet>
{
    protected override string CompanionName => "Soul Nexus";
    protected override string CardTitle => "Nexus Brand";
    protected override string CardArtFileName => "card_soul_nexus.png";
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<DoomPower>(6m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DoomPower>()];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Apply {DoomPower:diff} Doom."),
        ("flavor", "Every departing soul leaves a direction behind.")
    ];

    public SoulNexusCard() : base(1, CardType.Skill, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await EliteCompanionAnimation.Trigger<SoulNexusPet>(Owner, "Attack");
        await PowerCmd.Apply<DoomPower>(
            choiceContext, cardPlay.Target, DynamicVars.Doom.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Doom.UpgradeValueBy(3m);
}

public sealed class SoulNexusPet : BossCompanionPet<SoulNexus>
{
    protected override float PetScale => 0.30f;
}
