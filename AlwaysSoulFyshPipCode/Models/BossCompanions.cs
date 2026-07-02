using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NeowCompanions.NeowCompanionsCode.Assets;
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

public abstract class BossCompanionRelic<TPet> : CompanionRelicModel
    where TPet : MonsterModel
{
    protected abstract string CompanionName { get; }

    protected abstract string RelicIconFileName { get; }

    public override string IconFileName => RelicIconFileName;

    public override List<(string, string)> Localization =>
    [
        ("title", CompanionName),
        ("description", $"At the start of each combat, summon {CompanionName}."),
        ("flavor", "Neow keeps stranger company than usual.")
    ];

    public override Task BeforeCombatStart()
    {
        return PlayerCmd.AddPet<TPet>(Owner);
    }
}

public abstract class BossCompanionPet<TMonster> : CustomMonsterModel
    where TMonster : MonsterModel
{
    protected virtual float PetScale => 0.30f;

    public override int MinInitialHp => 9999;

    public override int MaxInitialHp => 9999;

    public override bool IsHealthBarVisible => false;

    public override NCreatureVisuals? CreateCustomVisuals()
    {
        NCreatureVisuals visuals = ModelDb.Monster<TMonster>().CreateVisuals();
        visuals.Scale = new Vector2(-PetScale, PetScale);
        return visuals;
    }

    public override MegaCrit.Sts2.Core.Animation.CreatureAnimator? SetupCustomAnimationStates(
        MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaSprite controller)
    {
        return ModelDb.Monster<TMonster>().GenerateAnimator(controller);
    }

    protected override MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine.MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine.MonsterState> states = [];
        MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine.MoveState idle =
            new("NOTHING_MOVE", (IReadOnlyList<MegaCrit.Sts2.Core.Entities.Creatures.Creature> _) => Task.CompletedTask);

        idle.FollowUpState = idle;
        states.Add(idle);

        return new MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine.MonsterMoveStateMachine(states, idle);
    }
}

public abstract class BossCompanionCard<TPet> : CustomCardModel
    where TPet : MonsterModel
{
    protected abstract string CompanionName { get; }

    protected abstract string CardTitle { get; }

    protected abstract string CardArtFileName { get; }

    public override CardPoolModel Pool => ModelDb.Card<ByrdSwoop>().Pool;

    public override CardPoolModel VisualCardPool => Pool;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<VulnerablePower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<VulnerablePower>()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    public override Texture2D? CustomPortrait => ModTextureLoader.Load(CardArtFileName);

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Apply 1 Vulnerable to ALL enemies."),
        ("flavor", $"{CompanionName} answers with a fraction of its old force.")
    ];

    protected BossCompanionCard()
        : this(1, CardType.Skill, TargetType.AllEnemies)
    {
    }

    protected BossCompanionCard(int energyCost, CardType cardType, TargetType targetType)
        : base(energyCost, cardType, CardRarity.Event, targetType)
    {
    }

    protected async Task TriggerPetAnimation<TCompanionPet>(string animationName, float duration)
        where TCompanionPet : MonsterModel
    {
        var pet = Owner.PlayerCombatState?.GetPet<TCompanionPet>();
        if (pet != null && !pet.IsDead)
        {
            MainFile.Logger.Info($"Triggering {CompanionName} {animationName} animation from {CardTitle}.");
            await CreatureCmd.TriggerAnim(pet, animationName, duration);
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
        {
            return;
        }

        var pet = Owner.PlayerCombatState?.GetPet<TPet>();
        if (pet != null && !pet.IsDead)
        {
            MainFile.Logger.Info($"Triggering {CompanionName} attack animation from {CardTitle}.");
            await CreatureCmd.TriggerAnim(pet, "Attack", 0.5f);
        }

        await PowerCmd.Apply<VulnerablePower>(
            choiceContext,
            CombatState.HittableEnemies.Where(enemy => enemy.IsAlive),
            DynamicVars.Vulnerable.BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
    }
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class AeonglassRelic : BossCompanionRelic<AeonglassPet>
{
    protected override string CompanionName => "Aeonglass";
    protected override string RelicIconFileName => "relic_aeonglass.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class AeonglassCard : BossCompanionCard<AeonglassPet>
{
    protected override string CompanionName => "Aeonglass";
    protected override string CardTitle => "Aeon Fracture";
    protected override string CardArtFileName => "card_aeonglass.png";

    public override bool HasTurnEndInHandEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m, DamageProps.card),
        new IfUpgradedVar("IfUpgraded", 0m)
    ];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Deal {Damage:diff} damage. If left in your hand, take 2 damage."),
        ("flavor", "A mirrored second waits behind the first.")
    ];

    public AeonglassCard()
        : base(1, CardType.Attack, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await TriggerPetAnimation<AeonglassPet>("Attack", 0.5f);
        await CreatureCmd.Damage(choiceContext, cardPlay.Target, DynamicVars.Damage, Owner.Creature, this);
    }

    protected override Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    {
        return CreatureCmd.Damage(choiceContext, Owner.Creature, 2m, DamageProps.cardHpLoss, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}

public sealed class AeonglassPet : BossCompanionPet<Aeonglass>
{
    protected override float PetScale => 0.28f;
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class LagavulinMatriarchRelic : BossCompanionRelic<LagavulinMatriarchPet>
{
    protected override string CompanionName => "Lagavulin Matriarch";
    protected override string RelicIconFileName => "relic_lagavulin_matriarch.png";

    public override async Task BeforeCombatStart()
    {
        await base.BeforeCombatStart();

        Creature? matriarch = Owner.PlayerCombatState?.GetPet<LagavulinMatriarchPet>();
        if (matriarch != null && !matriarch.IsDead)
        {
            await CompanionAnimation.TryTriggerAnimation(matriarch, "Sleep", "Asleep", "IdleSleep");
        }
    }
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class LagavulinMatriarchCard : BossCompanionCard<LagavulinMatriarchPet>
{
    protected override string CompanionName => "Lagavulin Matriarch";
    protected override string CardTitle => "Matriarch's Wake";
    protected override string CardArtFileName => "card_lagavulin_matriarch.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(4m),
        new PowerVar<DexterityPower>(4m),
        new IfUpgradedVar("IfUpgraded", 0m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>()
    ];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Gain {IfUpgraded:show:6|4} Strength and {IfUpgraded:show:6|4} Dexterity. At the start of each turn, lose 1 Strength and 1 Dexterity."),
        ("flavor", "The shell opens just enough to teach violence posture.")
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Creature? matriarch = Owner.PlayerCombatState?.GetPet<LagavulinMatriarchPet>();
        if (matriarch != null && !matriarch.IsDead)
        {
            await CompanionAnimation.TryTriggerAnimation(matriarch, "WakeUp", "Wake", "Awake", "Attack");
        }

        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, DynamicVars.Strength.BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, DynamicVars.Dexterity.BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<LagavulinMatriarchDrainPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    public LagavulinMatriarchCard()
        : base(2, CardType.Power, TargetType.Self)
    {
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Strength.UpgradeValueBy(2m);
        DynamicVars.Dexterity.UpgradeValueBy(2m);
    }
}

public sealed class LagavulinMatriarchPet : BossCompanionPet<LagavulinMatriarch>
{
    protected override float PetScale => 0.25f;
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class TheKinRelic : BossCompanionRelic<TheKinPet>
{
    protected override string CompanionName => "The Kin";
    protected override string RelicIconFileName => "relic_the_kin.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class TheKinCard : BossCompanionCard<TheKinPet>
{
    protected override string CompanionName => "The Kin";
    protected override string CardTitle => "Kin Edict";
    protected override string CardArtFileName => "card_the_kin.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<VulnerablePower>(1m),
        new PowerVar<WeakPower>(1m),
        new IfUpgradedVar("IfUpgraded", 0m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<WeakPower>()
    ];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Apply 1 Vulnerable to ALL enemies.{IfUpgraded:show: Apply 1 Weak to ALL enemies.|}"),
        ("flavor", $"{CompanionName} answers with a fraction of its old force.")
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
        {
            return;
        }

        Creature? theKin = Owner.PlayerCombatState?.GetPet<TheKinPet>();
        if (theKin != null && !theKin.IsDead)
        {
            await CompanionAnimation.TryTriggerAnimation(theKin, "ThrowBomb", "Bomb", "Attack");
        }

        IEnumerable<Creature> enemies = CombatState.HittableEnemies.Where(enemy => enemy.IsAlive);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, enemies, 1m, Owner.Creature, this);
        if (IsUpgraded)
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, enemies, 1m, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Weak.UpgradeValueBy(0m);
    }
}

public sealed class TheKinPet : BossCompanionPet<KinPriest>
{
    protected override float PetScale => 0.45f;
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class WaterfallGiantRelic : BossCompanionRelic<WaterfallGiantPet>
{
    protected override string CompanionName => "Waterfall Giant";
    protected override string RelicIconFileName => "relic_waterfall_giant.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class WaterfallGiantCard : BossCompanionCard<WaterfallGiantPet>
{
    protected override string CompanionName => "Waterfall Giant";
    protected override string CardTitle => "Giant Undertow";
    protected override string CardArtFileName => "card_waterfall_giant.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IfUpgradedVar("IfUpgraded", 0m)
    ];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "In 2 turns, {IfUpgraded:show:deal 20 damage to ALL enemies|deal 20 damage to a random enemy}."),
        ("flavor", "The first sound is only water. The second is stone arriving.")
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await TriggerPetAnimation<WaterfallGiantPet>("Attack", 0.5f);
        if (IsUpgraded)
        {
            await PowerCmd.Apply<WaterfallGiantDelayedPower>(choiceContext, Owner.Creature, 2m, Owner.Creature, this);
        }
        else
        {
            await PowerCmd.Apply<WaterfallGiantRandomDelayedPower>(choiceContext, Owner.Creature, 2m, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
    }
}

public sealed class WaterfallGiantPet : BossCompanionPet<WaterfallGiant>
{
    protected override float PetScale => 0.20f;
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class VantomRelic : BossCompanionRelic<VantomPet>
{
    protected override string CompanionName => "Vantom";
    protected override string RelicIconFileName => "relic_vantom.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class VantomCard : BossCompanionCard<VantomPet>
{
    protected override string CompanionName => "Vantom";
    protected override string CardTitle => "Vantom Shade";
    protected override string CardArtFileName => "card_vantom.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<SlipperyPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<SlipperyPower>()];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Gain 1 Slippery."),
        ("flavor", "It leaves a shape where certainty used to be.")
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public VantomCard()
        : base(2, CardType.Skill, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await TriggerPetAnimation<VantomPet>("Attack", 0.5f);
        await PowerCmd.Apply<SlipperyPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}

public sealed class VantomPet : BossCompanionPet<Vantom>
{
    protected override float PetScale => 0.30f;
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class KnowledgeDemonRelic : BossCompanionRelic<KnowledgeDemonPet>
{
    protected override string CompanionName => "Knowledge Demon";
    protected override string RelicIconFileName => "relic_knowledge_demon.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class KnowledgeDemonCard : BossCompanionCard<KnowledgeDemonPet>
{
    protected override string CompanionName => "Knowledge Demon";
    protected override string CardTitle => "Forbidden Lesson";
    protected override string CardArtFileName => "card_knowledge_demon.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<KnowledgeDemonDrawPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<KnowledgeDemonDrawPower>()];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Take 6 damage. Draw 1 additional card at the start of each turn."),
        ("flavor", "The lesson is useful. The tuition is immediate.")
    ];

    public KnowledgeDemonCard()
        : base(1, CardType.Power, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Creature? knowledgeDemon = Owner.PlayerCombatState?.GetPet<KnowledgeDemonPet>();
        if (knowledgeDemon != null && !knowledgeDemon.IsDead)
        {
            await CompanionAnimation.TryTriggerAnimation(knowledgeDemon, "Buff", "Cast", "Attack");
        }

        await CreatureCmd.Damage(choiceContext, Owner.Creature, 6m, DamageProps.cardHpLoss, Owner.Creature, this);
        await PowerCmd.Apply<KnowledgeDemonDrawPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

public sealed class KnowledgeDemonPet : BossCompanionPet<KnowledgeDemon>
{
    protected override float PetScale => 0.27f;
}

public sealed class KnowledgeDemonDrawPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization =>
    [
        ("title", "Forbidden Lesson"),
        ("description", "Draw {Amount} additional card at the start of each turn.")
    ];

    public override decimal ModifyHandDraw(MegaCrit.Sts2.Core.Entities.Players.Player player, decimal count)
    {
        return player.Creature == Owner ? count + Amount : count;
    }
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class TheInsatiableRelic : BossCompanionRelic<TheInsatiablePet>
{
    protected override string CompanionName => "The Insatiable";
    protected override string RelicIconFileName => "relic_the_insatiable.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class TheInsatiableCard : BossCompanionCard<TheInsatiablePet>
{
    protected override string CompanionName => "The Insatiable";
    protected override string CardTitle => "Insatiable Hunger";
    protected override string CardArtFileName => "card_the_insatiable.png";

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Kill an enemy with less than 50 HP, ignoring Block."),
        ("flavor", "There is no wound. There is only absence.")
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public TheInsatiableCard()
        : base(3, CardType.Skill, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await TriggerPetAnimation<TheInsatiablePet>("Attack", 0.5f);
        if (cardPlay.Target.CurrentHp < 50m)
        {
            await CreatureCmd.SetCurrentHp(cardPlay.Target, 0m);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

public sealed class TheInsatiablePet : BossCompanionPet<TheInsatiable>
{
    protected override float PetScale => 0.22f;
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class QueenRelic : BossCompanionRelic<QueenPet>
{
    protected override string CompanionName => "Queen";
    protected override string RelicIconFileName => "relic_queen.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class QueenCard : BossCompanionCard<QueenPet>
{
    protected override string CompanionName => "Queen";
    protected override string CardTitle => "False Decree";
    protected override string CardArtFileName => "card_queen.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<WeakPower>(99m),
        new PowerVar<VulnerablePower>(99m),
        new IfUpgradedVar("IfUpgraded", 0m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>()
    ];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Apply 99 Weak and 99 Vulnerable."),
        ("flavor", "A crown is just a problem with witnesses.")
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    public QueenCard()
        : base(4, CardType.Skill, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await TriggerPetAnimation<QueenPet>("Attack", 0.5f);
        await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, DynamicVars.Weak.BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, DynamicVars.Vulnerable.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}

public sealed class QueenPet : BossCompanionPet<Queen>
{
    protected override float PetScale => 0.23f;
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class TestSubjectRelic : BossCompanionRelic<TestSubjectPet>
{
    protected override string CompanionName => "Test Subject";
    protected override string RelicIconFileName => "relic_test_subject.png";

    public override async Task BeforeCombatStart()
    {
        await base.BeforeCombatStart();
        await PowerCmd.Apply<TestSubjectLastTurnDamagePower>(
            new BlockingPlayerChoiceContext(),
            Owner.Creature,
            1m,
            Owner.Creature,
            null);
    }
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class TestSubjectCard : BossCompanionCard<TestSubjectPet>
{
    protected override string CompanionName => "Test Subject";
    protected override string CardTitle => "Subject Protocol";
    protected override string CardArtFileName => "card_test_subject.png";

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Heal HP equal to the damage you took last turn."),
        ("flavor", "The notes say recovery. The subject says otherwise.")
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public TestSubjectCard()
        : base(2, CardType.Skill, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await TriggerPetAnimation<TestSubjectPet>("Attack", 0.5f);
        TestSubjectLastTurnDamagePower? tracker = Owner.Creature.Powers.OfType<TestSubjectLastTurnDamagePower>().FirstOrDefault();
        if (tracker?.LastTurnDamage > 0m)
        {
            await CreatureCmd.Heal(Owner.Creature, tracker.LastTurnDamage, false);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

public sealed class TestSubjectPet : BossCompanionPet<TestSubject>
{
    protected override float PetScale => 0.25f;
}

public sealed class WaterfallGiantDelayedPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization =>
    [
        ("title", "Giant Undertow"),
        ("description", "In {Amount} turns, deal 20 damage to ALL enemies.")
    ];

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Combat.CombatSide side,
        IEnumerable<Creature> creatures)
    {
        if (!creatures.Contains(Owner) || CombatState == null)
        {
            return;
        }

        if (Amount > 1)
        {
            await PowerCmd.ModifyAmount(choiceContext, this, -1m, Owner, null);
            return;
        }

        IEnumerable<Creature> targets = CombatState.HittableEnemies.Where(enemy => enemy.IsAlive);
        await CompanionAnimation.TriggerWaterfallGiantExplosion(Owner);
        await CreatureCmd.Damage(choiceContext, targets, 20m, DamageProps.cardUnpowered, Owner);
        await PowerCmd.Remove(this);
    }
}

public sealed class WaterfallGiantRandomDelayedPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization =>
    [
        ("title", "Giant Undertow"),
        ("description", "In {Amount} turns, deal 20 damage to a random enemy.")
    ];

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Combat.CombatSide side,
        IEnumerable<Creature> creatures)
    {
        if (!creatures.Contains(Owner) || CombatState == null)
        {
            return;
        }

        if (Amount > 1)
        {
            await PowerCmd.ModifyAmount(choiceContext, this, -1m, Owner, null);
            return;
        }

        Creature? target = CombatState.HittableEnemies.Where(enemy => enemy.IsAlive).OrderBy(_ => Guid.NewGuid()).FirstOrDefault();
        if (target != null)
        {
            await CompanionAnimation.TriggerWaterfallGiantExplosion(Owner);
            await CreatureCmd.Damage(choiceContext, target, 20m, DamageProps.cardUnpowered, Owner);
        }

        await PowerCmd.Remove(this);
    }
}

public sealed class LagavulinMatriarchDrainPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization =>
    [
        ("title", "Matriarch's Toll"),
        ("description", "At the start of each turn, lose 1 Strength and 1 Dexterity.")
    ];

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Entities.Players.Player player)
    {
        if (player.Creature != Owner)
        {
            return;
        }

        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, -1m, Owner, null);
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner, -1m, Owner, null);
    }
}

public sealed class TestSubjectLastTurnDamagePower : CustomPowerModel
{
    private decimal currentTurnDamage;

    public decimal LastTurnDamage { get; private set; }

    protected override bool IsVisibleInternal => false;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization =>
    [
        ("title", "Subject Notes"),
        ("description", "Tracks damage taken last turn.")
    ];

    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature source,
        CardModel cardSource)
    {
        if (target == Owner)
        {
            currentTurnDamage += result.UnblockedDamage;
        }

        return Task.CompletedTask;
    }

    public override Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Combat.CombatSide side,
        IEnumerable<Creature> creatures)
    {
        if (!creatures.Contains(Owner) && currentTurnDamage > 0m)
        {
            LastTurnDamage = currentTurnDamage;
            currentTurnDamage = 0m;
        }

        return Task.CompletedTask;
    }
}

internal static class CompanionAnimation
{
    public static async Task TryTriggerAnimation(Creature creature, params string[] animationNames)
    {
        foreach (string animationName in animationNames)
        {
            try
            {
                await CreatureCmd.TriggerAnim(creature, animationName, 0.5f);
                return;
            }
            catch
            {
                MainFile.Logger.Info($"Animation '{animationName}' did not play.");
            }
        }
    }

    public static async Task TriggerWaterfallGiantExplosion(Creature owner)
    {
        Creature? waterfallGiant = owner.Player?.PlayerCombatState?.GetPet<WaterfallGiantPet>();
        if (waterfallGiant != null && !waterfallGiant.IsDead)
        {
            await TryTriggerAnimation(waterfallGiant, "Explode", "Explosion", "Attack");
        }
    }
}
