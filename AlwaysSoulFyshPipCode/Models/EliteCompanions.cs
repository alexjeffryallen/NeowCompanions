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
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SelfAwareBlockVar(9m, ValueProp.Move)
    ];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Gain {Block:diff} Block."),
        ("flavor", "The old stone remembers how to stand.")
    ];

    public BygoneEffigyCard() : base(1, CardType.Skill, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await EliteCompanionAnimation.Trigger<BygoneEffigyPet>(Owner, "Attack");
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay, false);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
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
        new DamageVar(3m, DamageProps.card),
        new RepeatVar(3)
    ];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Deal {Damage:diff} damage {Repeat:diff} times."),
        ("flavor", "A crown does not make the beak less sharp.")
    ];

    public ByrdonisCard() : base(1, CardType.Attack, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await EliteCompanionAnimation.Trigger<ByrdonisPet>(Owner, "Attack");
        for (int i = 0; i < DynamicVars.Repeat.IntValue && cardPlay.Target.IsAlive; i++)
        {
            await CreatureCmd.Damage(choiceContext, cardPlay.Target, DynamicVars.Damage, Owner.Creature, this, cardPlay);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(1m);
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

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, DamageProps.card),
        new PowerVar<PoisonPower>(3m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PoisonPower>()];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Deal {Damage:diff} damage. Apply {PoisonPower:diff} Poison."),
        ("flavor", "The tongue arrives before the appetite.")
    ];

    public PhrogParasiteCard() : base(1, CardType.Attack, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await EliteCompanionAnimation.Trigger<PhrogParasitePet>(Owner, "Attack");
        await CreatureCmd.Damage(choiceContext, cardPlay.Target, DynamicVars.Damage, Owner.Creature, this, cardPlay);
        await PowerCmd.Apply<PoisonPower>(
            choiceContext, cardPlay.Target, DynamicVars.Poison.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars.Poison.UpgradeValueBy(1m);
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
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, DamageProps.card),
        new SelfAwareBlockVar(7m, ValueProp.Move)
    ];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Deal {Damage:diff} damage. Gain {Block:diff} Block."),
        ("flavor", "Every shell in the pile leans into the blow.")
    ];

    public SkulkingColonyCard() : base(1, CardType.Attack, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await EliteCompanionAnimation.Trigger<SkulkingColonyPet>(
            Owner, "AttackDouble");
        await CreatureCmd.Damage(choiceContext, cardPlay.Target, DynamicVars.Damage, Owner.Creature, this, cardPlay);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay, false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars.Block.UpgradeValueBy(2m);
    }
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
    public override bool GainsBlock => true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SelfAwareBlockVar(10m, ValueProp.Move),
        new PowerVar<StrengthPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Gain {Block:diff} Block and {StrengthPower:diff} Strength."),
        ("flavor", "What it tends grows larger than memory.")
    ];

    public PhantasmalGardenerCard() : base(1, CardType.Skill, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await EliteCompanionAnimation.Trigger<PhantasmalGardenerPet>(Owner, "Cast");
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay, false);
        await PowerCmd.Apply<StrengthPower>(
            choiceContext, Owner.Creature, DynamicVars.Strength.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(4m);
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
        new DamageVar(4m, DamageProps.card),
        new RepeatVar(3),
        new PowerVar<VulnerablePower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Deal {Damage:diff} damage {Repeat:diff} times. Apply {VulnerablePower:diff} Vulnerable."),
        ("flavor", "The water panics first.")
    ];

    public TerrorEelCard() : base(2, CardType.Attack, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await EliteCompanionAnimation.Trigger<TerrorEelPet>(Owner, "AttackTripleTrigger");
        for (int i = 0; i < DynamicVars.Repeat.IntValue && cardPlay.Target.IsAlive; i++)
        {
            await CreatureCmd.Damage(choiceContext, cardPlay.Target, DynamicVars.Damage, Owner.Creature, this, cardPlay);
        }
        if (cardPlay.Target.IsAlive)
        {
            await PowerCmd.Apply<VulnerablePower>(
                choiceContext, cardPlay.Target, DynamicVars.Vulnerable.BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(1m);
}

public sealed class TerrorEelPet : BossCompanionPet<TerrorEel>
{
    protected override float PetScale => 0.26f;
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class DecimillipedeRelic : BossCompanionRelic<DecimillipedePet>
{
    protected override string CompanionName => "Decimillipede";
    protected override string RelicIconFileName => "relic_decimillipede.png";
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
        await EliteCompanionAnimation.Trigger<DecimillipedePet>(Owner, "regenerate");
        await CreatureCmd.Heal(Owner.Creature, DynamicVars["Heal"].BaseValue, false);
    }

    protected override void OnUpgrade() => DynamicVars["Heal"].UpgradeValueBy(4m);
}

public sealed class DecimillipedePet : BossCompanionPet<DecimillipedeSegmentFront>
{
    protected override float PetScale => 0.48f;
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
