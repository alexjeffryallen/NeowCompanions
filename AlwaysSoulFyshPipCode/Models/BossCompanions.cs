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
using MegaCrit.Sts2.Core.Nodes.Rooms;
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

        if (cardPlay.Target.CurrentHp >= 50m)
        {
            return;
        }

        Creature? insatiable = Owner.PlayerCombatState?.GetPet<TheInsatiablePet>();
        if (insatiable != null && !insatiable.IsDead)
        {
            await CompanionAnimation.TriggerInsatiableDevour(insatiable, cardPlay.Target);
            return;
        }

        await CreatureCmd.Kill(cardPlay.Target, true);
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

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class SeapunkRelic : BossCompanionRelic<SeapunkPet>
{
    protected override string CompanionName => "Seapunk";
    protected override string RelicIconFileName => "relic_seapunk.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class SeapunkCard : BossCompanionCard<SeapunkPet>
{
    private const int HitCount = 4;

    protected override string CompanionName => "Seapunk";
    protected override string CardTitle => "Spinning Kick";
    protected override string CardArtFileName => "card_seapunk.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(2m, DamageProps.card)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
    ];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Deal {Damage:diff} damage 4 times."),
        ("flavor", "Four kicks, one rhythm, no apologies.")
    ];

    public SeapunkCard()
        : base(1, CardType.Attack, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        Creature? seapunk = Owner.PlayerCombatState?.GetPet<SeapunkPet>();
        if (seapunk != null && !seapunk.IsDead)
        {
            MainFile.Logger.Info("Triggering Seapunk multi-attack animation from Spinning Kick.");
            SfxCmd.Play("event:/sfx/enemy/enemy_attacks/seapunk/seapunk_kick_multi");
            await CreatureCmd.TriggerAnim(seapunk, "MultiAttack", 0.15f);
        }

        for (int i = 0; i < HitCount && cardPlay.Target.IsAlive; i++)
        {
            await CreatureCmd.Damage(choiceContext, cardPlay.Target, DynamicVars.Damage, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
    }
}

public sealed class SeapunkPet : BossCompanionPet<Seapunk>
{
    protected override float PetScale => 0.60f;
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class ShrinkerBeetleRelic : BossCompanionRelic<ShrinkerBeetlePet>
{
    protected override string CompanionName => "Shrinker Beetle";
    protected override string RelicIconFileName => "relic_shrinker_beetle.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class ShrinkerBeetleCard : BossCompanionCard<ShrinkerBeetlePet>
{
    protected override string CompanionName => "Shrinker Beetle";
    protected override string CardTitle => "Beetle Juice";
    protected override string CardArtFileName => "card_shrinker_beetle.png";

    public override MegaCrit.Sts2.Core.Entities.Cards.TargetType TargetType =>
        IsUpgraded
            ? MegaCrit.Sts2.Core.Entities.Cards.TargetType.AllEnemies
            : MegaCrit.Sts2.Core.Entities.Cards.TargetType.AnyEnemy;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Shrink", 4m),
        new IfUpgradedVar("IfUpgraded", 0m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<ShrinkPower>()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Apply {Shrink:diff} Shrink.{IfUpgraded:show: Targets ALL enemies.|}"),
        ("flavor", "Everything looks smaller after a good sip.")
    ];

    public ShrinkerBeetleCard()
        : base(2, CardType.Skill, MegaCrit.Sts2.Core.Entities.Cards.TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
        {
            return;
        }

        await TriggerPetAnimation<ShrinkerBeetlePet>("Cast", 0.5f);

        IReadOnlyList<Creature> targets = IsUpgraded
            ? CombatState.HittableEnemies.Where(enemy => enemy.IsAlive).ToList()
            : [cardPlay.Target ?? throw new ArgumentNullException(nameof(cardPlay.Target))];

        foreach (Creature target in targets)
        {
            NCombatRoom.Instance?.PlaySplashVfx(target, new Color("65cf81"));
        }

        await PowerCmd.Apply<ShrinkPower>(
            choiceContext,
            targets,
            DynamicVars["Shrink"].BaseValue,
            Owner.Creature,
            this);
    }
}

public sealed class ShrinkerBeetlePet : BossCompanionPet<ShrinkerBeetle>
{
    protected override float PetScale => 0.55f;
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class OperosisRelic : BossCompanionRelic<OperosisPet>
{
    protected override string CompanionName => "Operosis";
    protected override string RelicIconFileName => "relic_operosis.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class OperosisCard : BossCompanionCard<OperosisPet>
{
    protected override string CompanionName => "Operosis";
    protected override string CardTitle => "Little Poke";
    protected override string CardArtFileName => "card_operosis.png";

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Minion];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(1m),
        new OstyDamageVar(5m, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.SummonDynamic, DynamicVars[SummonVar.defaultName])
    ];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Gain {Summon:diff} Summon. Osty attacks for {OstyDamage:diff} damage."),
        ("flavor", "Small bones. Big commitment.")
    ];

    public OperosisCard()
        : base(1, CardType.Attack, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await TriggerPetAnimation<OperosisPet>("Cast", 0.35f);
        await OstyCmd.Summon(choiceContext, Owner, DynamicVars[SummonVar.defaultName].BaseValue, this);

        Creature? osty = Owner.Osty;
        if (osty == null || !osty.IsAlive)
        {
            return;
        }

        OstyDamageVar ostyDamage = (OstyDamageVar)DynamicVars[OstyDamageVar.defaultName];
        SfxCmd.Play(Osty.ostyAttackSfx);
        await CreatureCmd.TriggerAnim(osty, Osty.pokeAnim, Osty.attackerAnimDelay);
        await CreatureCmd.Damage(choiceContext, cardPlay.Target, ostyDamage.BaseValue, ostyDamage.Props, osty, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[SummonVar.defaultName].UpgradeValueBy(1m);
        DynamicVars[OstyDamageVar.defaultName].UpgradeValueBy(3m);
    }
}

public sealed class OperosisPet : BossCompanionPet<Osty>
{
    private const float OperosisScale = 0.30f;
    private const float OperosisHueShift = -0.45f;

    protected override float PetScale => OperosisScale;

    public override NCreatureVisuals? CreateCustomVisuals()
    {
        NCreatureVisuals visuals = ModelDb.Monster<Osty>().CreateVisuals();
        visuals.SetScaleAndHue(OperosisScale, OperosisHueShift);
        visuals.CallDeferred(NCreatureVisuals.MethodName.SetScaleAndHue, OperosisScale, OperosisHueShift);
        return visuals;
    }
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

        List<Creature> targets = CombatState.HittableEnemies.Where(enemy => enemy.IsAlive).ToList();
        Creature? target = Owner.Player?.RunState.Rng.CombatTargets.NextItem(targets) ?? targets.FirstOrDefault();
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
    private const float InsatiableDevourScaleMultiplier = 7.0f;
    private const float InsatiableDevourGrowDuration = 0.76f;
    private const float InsatiableDevourEatDuration = 3.10f;
    private const float InsatiableDevourSwallowDelay = 1.35f;
    private const float InsatiableDevourPostEatHoldDuration = 0.36f;
    private const float InsatiableDevourRestoreDuration = 1.30f;
    private const int InsatiableDevourFrontZIndex = 1000;

    public static Task TryTriggerAnimation(Creature creature, params string[] animationNames)
    {
        return TryTriggerAnimation(creature, 0.5f, animationNames);
    }

    public static async Task TryTriggerAnimation(Creature creature, float waitTime, params string[] animationNames)
    {
        foreach (string animationName in animationNames)
        {
            try
            {
                await CreatureCmd.TriggerAnim(creature, animationName, waitTime);
                return;
            }
            catch
            {
                MainFile.Logger.Info($"Animation '{animationName}' did not play.");
            }
        }
    }

    public static async Task TriggerInsatiableDevour(Creature insatiable, Creature target)
    {
        MainFile.Logger.Info("Triggering The Insatiable EatPlayerTrigger animation from Insatiable Hunger.");
        SfxCmd.Play("event:/sfx/enemy/enemy_attacks/the_insatiable/the_insatiable_finisher");

        NCreature? insatiableNode = insatiable.GetCreatureNode();
        if (insatiableNode?.Visuals == null)
        {
            Task animationTask = CreatureCmd.TriggerAnim(insatiable, "EatPlayerTrigger", InsatiableDevourEatDuration);
            await Cmd.Wait(InsatiableDevourSwallowDelay, ignoreCombatEnd: true);
            await KillTargetAsSwallowed(target);
            await Cmd.Wait(InsatiableDevourEatDuration - InsatiableDevourSwallowDelay, ignoreCombatEnd: true);
            await ObserveAnimationTask(animationTask);
            return;
        }

        NCreatureVisuals visuals = insatiableNode.Visuals;
        Vector2 originalScale = visuals.Scale;
        Vector2 originalPosition = visuals.Position;
        int originalCreatureZIndex = insatiableNode.ZIndex;
        bool originalCreatureZAsRelative = insatiableNode.ZAsRelative;
        int originalVisualZIndex = visuals.ZIndex;
        bool originalVisualZAsRelative = visuals.ZAsRelative;
        Node? originalParent = insatiableNode.GetParent();
        int originalChildIndex = insatiableNode.GetIndex();
        Vector2 lungeOffset = Vector2.Zero;

        NCreature? targetNode = target.GetCreatureNode();
        if (targetNode != null)
        {
            Vector2 sourceCenter = visuals.VfxSpawnPosition.GlobalPosition;
            Vector2 targetCenter = targetNode.Hitbox.GlobalPosition + targetNode.Hitbox.Size * 0.5f;
            lungeOffset = new Vector2(Mathf.Clamp((targetCenter.X - sourceCenter.X) * 0.12f, -90f, 110f), 0f);
        }

        BringCreatureToFront(insatiableNode, visuals);
        try
        {
            Tween growTween = visuals.CreateTween().SetParallel();
            growTween.TweenProperty(visuals, "scale", originalScale * InsatiableDevourScaleMultiplier, InsatiableDevourGrowDuration)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Back);
            growTween.TweenProperty(visuals, "position", originalPosition + lungeOffset, InsatiableDevourGrowDuration)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Sine);

            await Cmd.Wait(InsatiableDevourGrowDuration, ignoreCombatEnd: true);

            Task animationTask = CreatureCmd.TriggerAnim(insatiable, "EatPlayerTrigger", InsatiableDevourEatDuration);
            await Cmd.Wait(InsatiableDevourSwallowDelay, ignoreCombatEnd: true);
            await KillTargetAsSwallowed(target);
            await Cmd.Wait(InsatiableDevourEatDuration - InsatiableDevourSwallowDelay, ignoreCombatEnd: true);
            await ObserveAnimationTask(animationTask);

            await Cmd.Wait(InsatiableDevourPostEatHoldDuration, ignoreCombatEnd: true);

            Tween restoreTween = visuals.CreateTween().SetParallel();
            restoreTween.TweenProperty(visuals, "scale", originalScale, InsatiableDevourRestoreDuration)
                .SetEase(Tween.EaseType.InOut)
                .SetTrans(Tween.TransitionType.Sine);
            restoreTween.TweenProperty(visuals, "position", originalPosition, InsatiableDevourRestoreDuration)
                .SetEase(Tween.EaseType.InOut)
                .SetTrans(Tween.TransitionType.Sine);

            await Cmd.Wait(InsatiableDevourRestoreDuration, ignoreCombatEnd: true);
        }
        finally
        {
            RestoreCreatureLayer(
                insatiableNode,
                visuals,
                originalParent,
                originalChildIndex,
                originalCreatureZIndex,
                originalCreatureZAsRelative,
                originalVisualZIndex,
                originalVisualZAsRelative);

            if (GodotObject.IsInstanceValid(visuals))
            {
                visuals.Scale = originalScale;
                visuals.Position = originalPosition;
            }
        }
    }

    private static void BringCreatureToFront(NCreature creatureNode, NCreatureVisuals visuals)
    {
        creatureNode.ZAsRelative = false;
        creatureNode.ZIndex = InsatiableDevourFrontZIndex;
        visuals.ZAsRelative = false;
        visuals.ZIndex = InsatiableDevourFrontZIndex + 1;

        Node? parent = creatureNode.GetParent();
        parent?.MoveChild(creatureNode, parent.GetChildCount() - 1);
    }

    private static void RestoreCreatureLayer(
        NCreature creatureNode,
        NCreatureVisuals visuals,
        Node? originalParent,
        int originalChildIndex,
        int originalCreatureZIndex,
        bool originalCreatureZAsRelative,
        int originalVisualZIndex,
        bool originalVisualZAsRelative)
    {
        if (!GodotObject.IsInstanceValid(creatureNode) || !GodotObject.IsInstanceValid(visuals))
        {
            return;
        }

        creatureNode.ZIndex = originalCreatureZIndex;
        creatureNode.ZAsRelative = originalCreatureZAsRelative;
        visuals.ZIndex = originalVisualZIndex;
        visuals.ZAsRelative = originalVisualZAsRelative;

        if (originalParent != null
            && GodotObject.IsInstanceValid(originalParent)
            && creatureNode.GetParent() == originalParent)
        {
            int restoredIndex = Math.Clamp(originalChildIndex, 0, Math.Max(0, originalParent.GetChildCount() - 1));
            originalParent.MoveChild(creatureNode, restoredIndex);
        }
    }

    private static async Task KillTargetAsSwallowed(Creature target)
    {
        if (!target.IsAlive)
        {
            return;
        }

        NCreature? targetNode = target.GetCreatureNode();
        if (targetNode != null)
        {
            targetNode.Visible = false;
        }

        await CreatureCmd.Kill(target, true);
    }

    private static async Task ObserveAnimationTask(Task animationTask)
    {
        try
        {
            await animationTask;
        }
        catch
        {
            MainFile.Logger.Info("The Insatiable EatPlayerTrigger animation task did not complete.");
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
