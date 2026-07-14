using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeowCompanions.NeowCompanionsCode.Assets;
using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
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
        return visuals == null ? null : CompanionDrag.MakeDraggable(visuals);
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
            await CompanionAnimation.TriggerLagavulinMatriarchWake(matriarch);
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
        CompanionSelectivePalette.ApplyShader(visuals, CompanionSelectivePalette.OperosisShader);
        visuals.AddChild(new OperosisVfxColorApplier());
        if (visuals == null)
        {
            return null;
        }

        return CompanionDrag.MakeDraggable(visuals);
    }
}

internal sealed partial class OperosisVfxColorApplier : Node
{
    private int framesWaited;
    private bool logged;

    public override void _Process(double delta)
    {
        framesWaited++;
        if (GetParent() is not NCreatureVisuals visuals)
        {
            if (framesWaited > 300)
            {
                QueueFree();
            }
            return;
        }

        int recolored = RecolorVfxMaterials(visuals);
        if (!logged && framesWaited is 2 or 30)
        {
            logged = true;
            MainFile.Logger.Info($"[NeowCompanions] Operosis VFX color pass recolored {recolored} material(s).");
            if (recolored == 0)
            {
                MainFile.Logger.Info(DumpOperosisVisualTree(visuals, 0, 5));
            }
        }

        if (framesWaited > 300)
        {
            QueueFree();
        }
    }

    private static int RecolorVfxMaterials(Node node)
    {
        int count = 0;
        RecolorVfxMaterials(node, ref count);
        return count;
    }

    private static void RecolorVfxMaterials(Node node, ref int count)
    {
        if (node is CanvasItem canvasItem && canvasItem.Material is ShaderMaterial shaderMaterial)
        {
            bool hasOuterColor = HasShaderUniform(shaderMaterial, "OuterColor");
            bool hasInnerColor = HasShaderUniform(shaderMaterial, "InnerColor");
            if (hasOuterColor || hasInnerColor)
            {
                ShaderMaterial localMaterial = shaderMaterial.ResourceLocalToScene
                    ? shaderMaterial
                    : (ShaderMaterial)shaderMaterial.Duplicate();
                localMaterial.ResourceLocalToScene = true;
                if (hasOuterColor)
                {
                    localMaterial.SetShaderParameter("OuterColor", new Color(1.0f, 0.34f, 0.04f, 1.0f));
                }
                if (hasInnerColor)
                {
                    localMaterial.SetShaderParameter("InnerColor", new Color(0.18f, 0.015f, 0.0f, 1.0f));
                }

                canvasItem.Material = localMaterial;
                canvasItem.UseParentMaterial = false;
                count++;
            }
        }

        foreach (Node child in node.GetChildren())
        {
            RecolorVfxMaterials(child, ref count);
        }
    }

    private static bool HasShaderUniform(ShaderMaterial material, string uniformName)
    {
        Shader? shader = material.Shader;
        if (shader == null)
        {
            return false;
        }

        foreach (Godot.Collections.Dictionary uniform in shader.GetShaderUniformList())
        {
            if (uniform.TryGetValue("name", out Variant nameVariant)
                && nameVariant.AsString() == uniformName)
            {
                return true;
            }
        }

        return false;
    }

    private static string DumpOperosisVisualTree(Node node, int depth, int maxDepth)
    {
        StringBuilder builder = new();
        DumpOperosisVisualTree(node, depth, maxDepth, builder);
        return builder.ToString();
    }

    private static void DumpOperosisVisualTree(Node node, int depth, int maxDepth, StringBuilder builder)
    {
        if (depth > maxDepth)
        {
            return;
        }

        bool interesting = depth <= 1
            || node is Sprite2D
            || node.GetClass().Contains("Spine", StringComparison.OrdinalIgnoreCase)
            || node.Name.ToString().Contains("fire", StringComparison.OrdinalIgnoreCase)
            || node.Name.ToString().Contains("flame", StringComparison.OrdinalIgnoreCase)
            || node.Name.ToString().Contains("vfx", StringComparison.OrdinalIgnoreCase);

        if (interesting)
        {
            builder.Append(' ', depth * 2);
            builder.Append("[NeowCompanions] Operosis node ");
            builder.Append(node.GetPath());
            builder.Append(" :: ");
            builder.Append(node.GetType().Name);
            builder.Append(" class=");
            builder.Append(node.GetClass());
            if (node is CanvasItem canvasItem)
            {
                builder.Append(" material=");
                builder.Append(canvasItem.Material?.GetType().Name ?? "<null>");
                builder.Append(" materialPath=");
                builder.Append(canvasItem.Material?.ResourcePath ?? "<null>");
            }
            builder.AppendLine();
        }

        foreach (Node child in node.GetChildren())
        {
            DumpOperosisVisualTree(child, depth + 1, maxDepth, builder);
        }
    }
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class ArchitectRelic : BossCompanionRelic<ArchitectPet>
{
    protected override string CompanionName => "The Architect";
    protected override string RelicIconFileName => "relic_architect.png";

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        await base.BeforeCardPlayed(cardPlay);

        if (cardPlay.Card.Owner != Owner)
        {
            return;
        }

        if (!ArchitectCard.TryConsumeGeneratedCompanionCard(cardPlay.Card))
        {
            return;
        }

        Creature? architect = Owner.PlayerCombatState?.GetPet<ArchitectPet>();
        if (architect == null || architect.IsDead)
        {
            return;
        }

        MainFile.Logger.Info("Triggering Architect attack animation from generated companion card.");
        await CreatureCmd.TriggerAnim(architect, "Attack", 0.5f);
    }
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class ArchitectCard : BossCompanionCard<ArchitectPet>
{
    private static readonly HashSet<CardModel> GeneratedCompanionCards = new(ReferenceEqualityComparer.Instance);

    protected override string CompanionName => "The Architect";
    protected override string CardTitle => "Grand Design";
    protected override string CardArtFileName => "card_architect.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IfUpgradedVar("IfUpgraded", 0m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
    ];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Generate a random companion card.{IfUpgraded:show: It is Upgraded.|}"),
        ("flavor", "A plan inside a plan, folded into a sharper plan.")
    ];

    public ArchitectCard()
        : base(0, CardType.Skill, TargetType.Self)
    {
    }

    internal static bool TryConsumeGeneratedCompanionCard(CardModel card)
    {
        return GeneratedCompanionCards.Remove(card);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
        {
            return;
        }

        List<CardModel> options = GetGeneratedCompanionPool().ToList();
        CardModel? canonicalCard = Owner.RunState.Rng.CombatCardGeneration.NextItem(options);
        if (canonicalCard == null)
        {
            return;
        }

        CardModel generatedCard = CombatState.CreateCard(canonicalCard, Owner);
        if (IsUpgraded)
        {
            CardCmd.Upgrade(generatedCard);
        }

        GeneratedCompanionCards.Add(generatedCard);
        MainFile.Logger.Info($"Grand Design generated {generatedCard.GetType().Name}.");
        await CardPileCmd.AddGeneratedCardToCombat(generatedCard, PileType.Hand, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["IfUpgraded"].UpgradeValueBy(0m);
    }

    private static IEnumerable<CardModel> GetGeneratedCompanionPool()
    {
        return
        [
            ModelDb.Card<ByrdSwoop>(),
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
            ModelDb.Card<BuffUpCard>(),
            ModelDb.Card<NeedleTossCard>(),
            ModelDb.Card<OverclockCard>(),
            ModelDb.Card<GraveCallCard>(),
            ModelDb.Card<CommandingFlourishCard>()
        ];
    }
}

public sealed class ArchitectPet : BossCompanionPet<Architect>
{
    protected override float PetScale => 0.30f;
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class RustcladRelic : BossCompanionRelic<RustcladPet>
{
    protected override string CompanionName => "Rustclad";
    protected override string RelicIconFileName => "relic_rustclad.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class BuffUpCard : BossCompanionCard<RustcladPet>
{
    private static readonly Random DialogueRng = new();

    private static readonly LocString[] ArchitectLines =
    [
        new("ancients", "THE_ARCHITECT.talk.IRONCLAD.0-1r.char"),
        new("ancients", "THE_ARCHITECT.talk.IRONCLAD.1-1r.char"),
        new("ancients", "THE_ARCHITECT.talk.IRONCLAD.2-1r.char")
    ];

    protected override string CompanionName => "Rustclad";
    protected override string CardTitle => "Buff Up";
    protected override string CardArtFileName => "card_rustclad.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HpLossVar(3m),
        new PowerVar<RustcladBuffUpPower>(5m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<RustcladBuffUpPower>()
    ];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Lose {HpLoss} HP. Gain {RustcladBuffUpPower:diff} temporary Strength."),
        ("flavor", "Old rage in smaller armor.")
    ];

    public BuffUpCard()
        : base(0, CardType.Skill, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Creature? rustclad = Owner.PlayerCombatState?.GetPet<RustcladPet>();
        if (rustclad != null && !rustclad.IsDead)
        {
            TalkCmd.Play(GetRandomArchitectLine(), rustclad, VfxColor.Red, VfxDuration.Long);
            await CreatureCmd.TriggerAnim(rustclad, "PowerUp", 0.25f);
        }

        await CreatureCmd.Damage(
            choiceContext,
            Owner.Creature,
            DynamicVars.HpLoss.BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
            Owner.Creature,
            this);

        await PowerCmd.Apply<RustcladBuffUpPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["RustcladBuffUpPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["RustcladBuffUpPower"].UpgradeValueBy(2m);
    }

    private static LocString GetRandomArchitectLine()
    {
        return ArchitectLines[DialogueRng.Next(ArchitectLines.Length)];
    }
}

public sealed class RustcladBuffUpPower : TemporaryStrengthPower, ICustomModel
{
    public override AbstractModel OriginModel => ModelDb.Card<BuffUpCard>();
}

public sealed class RustcladPet : CustomMonsterModel
{
    private const float PetScale = 0.70f;
    private const float RustcladHueShift = 0.55f;

    public override int MinInitialHp => 9999;

    public override int MaxInitialHp => 9999;

    public override bool IsHealthBarVisible => false;

    public override NCreatureVisuals? CreateCustomVisuals()
    {
        NCreatureVisuals visuals = ModelDb.Character<Ironclad>().CreateVisuals();
        visuals.SetScaleAndHue(PetScale, RustcladHueShift);
        visuals.CallDeferred(NCreatureVisuals.MethodName.SetScaleAndHue, PetScale, RustcladHueShift);
        return CompanionDrag.MakeDraggable(visuals);
    }

    public override MegaCrit.Sts2.Core.Animation.CreatureAnimator? SetupCustomAnimationStates(
        MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaSprite controller)
    {
        return ModelDb.Character<Ironclad>().GenerateAnimator(controller);
    }

    protected override MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine.MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine.MonsterState> states = [];
        MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine.MoveState idle =
            new("NOTHING_MOVE", (IReadOnlyList<Creature> _) => Task.CompletedTask);

        idle.FollowUpState = idle;
        states.Add(idle);

        return new MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine.MonsterMoveStateMachine(states, idle);
    }

}

public abstract class CharacterCompanionPet<TCharacter> : CustomMonsterModel
    where TCharacter : CharacterModel
{
    protected abstract float PetScale { get; }
    protected abstract float HueShift { get; }
    protected virtual Color Tint => Colors.White;

    public override int MinInitialHp => 9999;
    public override int MaxInitialHp => 9999;
    public override bool IsHealthBarVisible => false;

    public override NCreatureVisuals? CreateCustomVisuals()
    {
        NCreatureVisuals visuals = ModelDb.Character<TCharacter>().CreateVisuals();
        visuals.SetScaleAndHue(PetScale, HueShift);
        visuals.Modulate = Tint;
        visuals.CallDeferred(NCreatureVisuals.MethodName.SetScaleAndHue, PetScale, HueShift);

        return CompanionDrag.MakeDraggable(visuals);
    }

    public override MegaCrit.Sts2.Core.Animation.CreatureAnimator? SetupCustomAnimationStates(
        MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaSprite controller)
    {
        return ModelDb.Character<TCharacter>().GenerateAnimator(controller);
    }

    protected override MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine.MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine.MonsterState> states = [];
        MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine.MoveState idle =
            new("NOTHING_MOVE", (IReadOnlyList<Creature> _) => Task.CompletedTask);

        idle.FollowUpState = idle;
        states.Add(idle);

        return new MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine.MonsterMoveStateMachine(states, idle);
    }
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class ShadeleafRelic : BossCompanionRelic<ShadeleafPet>
{
    protected override string CompanionName => "Shadeleaf";
    protected override string RelicIconFileName => "relic_shadeleaf.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class NeedleTossCard : BossCompanionCard<ShadeleafPet>
{
    private static readonly LocString[] Lines =
    [
        new(NeowCompanions.NeowCompanionsCode.Patches.NeowCompanionText.Table, "SHADELEAF.dialogue.0"),
        new(NeowCompanions.NeowCompanionsCode.Patches.NeowCompanionText.Table, "SHADELEAF.dialogue.1"),
        new(NeowCompanions.NeowCompanionsCode.Patches.NeowCompanionText.Table, "SHADELEAF.dialogue.2")
    ];

    protected override string CompanionName => "Shadeleaf";
    protected override string CardTitle => "Poisoned Shiv";
    protected override string CardArtFileName => "card_shadeleaf.png";

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Shiv];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4m, DamageProps.card),
        new PowerVar<PoisonPower>(2m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<PoisonPower>()
    ];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Deal {Damage:diff} damage. Apply {PoisonPower:diff} Poison."),
        ("flavor", "A whisper, a glint, then venom.")
    ];

    public NeedleTossCard()
        : base(0, CardType.Attack, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await CharacterCompanionDialogue.SayRandom<ShadeleafPet>(Owner, Lines, VfxColor.Swamp);
        await TriggerPetAnimation<ShadeleafPet>("Shiv", 0.15f);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitVfxNode(target => NShivThrowVfx.Create(Owner.Creature, target, Colors.Green))
            .Execute(choiceContext);
        await PowerCmd.Apply<PoisonPower>(choiceContext, cardPlay.Target, DynamicVars["PoisonPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["PoisonPower"].UpgradeValueBy(1m);
    }
}

public sealed class ShadeleafPet : CharacterCompanionPet<Silent>
{
    protected override float PetScale => 0.70f;
    protected override float HueShift => 0.72f;
    protected override Color Tint => new(0.95f, 0.84f, 1.0f, 1.0f);
}

internal static partial class CompanionSelectivePalette
{
    public const string EmberPipShader = """
shader_type canvas_item;
void fragment() {
    vec4 c = texture(TEXTURE, UV) * COLOR;
    float lum = dot(c.rgb, vec3(0.299, 0.587, 0.114));
    float maxc = max(c.r, max(c.g, c.b));
    float minc = min(c.r, min(c.g, c.b));
    float sat = maxc - minc;

    float warm_feathers = smoothstep(0.06, 0.24, c.r - c.b)
        * smoothstep(0.00, 0.18, c.g - c.b)
        * smoothstep(0.10, 0.36, sat)
        * smoothstep(0.32, 0.78, lum);
    float green_body = smoothstep(0.02, 0.18, c.g - c.r)
        * smoothstep(0.00, 0.16, c.g - c.b)
        * smoothstep(0.08, 0.32, sat);
    float pale_highlights = smoothstep(0.56, 0.88, lum)
        * smoothstep(0.02, 0.24, sat);

    vec3 ember_red_shadow = vec3(0.42, 0.00, 0.01);
    vec3 ember_red_mid = vec3(0.86, 0.03, 0.02);
    vec3 ember_red_hot = vec3(1.00, 0.30, 0.08);
    vec3 ember_red = mix(ember_red_shadow, ember_red_mid, smoothstep(0.24, 0.62, lum));
    ember_red = mix(ember_red, ember_red_hot, smoothstep(0.62, 0.94, lum));

    vec3 black_low = vec3(0.025, 0.018, 0.018);
    vec3 black_mid = vec3(0.13, 0.075, 0.070);
    vec3 black_high = vec3(0.30, 0.08, 0.07);
    vec3 ember_black = mix(black_low, black_mid, smoothstep(0.10, 0.50, lum));
    ember_black = mix(ember_black, black_high, smoothstep(0.54, 0.92, lum));

    vec3 rgb = c.rgb;
    rgb = mix(rgb, ember_black, green_body * 0.98);
    rgb = mix(rgb, ember_red, warm_feathers * 0.98);
    rgb = mix(rgb, ember_red_hot, pale_highlights * green_body * 0.48);
    COLOR = vec4(rgb, c.a);
}
""";

    public const string ShadeleafShader = """
shader_type canvas_item;
void fragment() {
    vec4 c = texture(TEXTURE, UV) * COLOR;
    float lum = dot(c.rgb, vec3(0.299, 0.587, 0.114));
    float maxc = max(c.r, max(c.g, c.b));
    float minc = min(c.r, min(c.g, c.b));
    float sat = maxc - minc;

    float green_cloth = smoothstep(-0.02, 0.07, min(c.g, c.b) - c.r)
        * (1.0 - smoothstep(0.50, 0.74, lum));
    float cyan_blade = smoothstep(0.04, 0.20, min(c.g, c.b) - c.r)
        * smoothstep(0.42, 0.78, lum)
        * smoothstep(0.06, 0.24, sat);

    vec3 purple_shadow = vec3(0.15, 0.08, 0.25);
    vec3 purple_mid = vec3(0.43, 0.20, 0.62);
    vec3 purple_high = vec3(0.72, 0.48, 0.95);
    vec3 purple = mix(purple_shadow, purple_mid, smoothstep(0.15, 0.62, lum));
    purple = mix(purple, purple_high, smoothstep(0.58, 0.92, lum));

    vec3 gold_shadow = vec3(0.55, 0.32, 0.06);
    vec3 gold_high = vec3(1.00, 0.78, 0.20);
    vec3 gold = mix(gold_shadow, gold_high, smoothstep(0.25, 0.86, lum));

    vec3 rgb = mix(c.rgb, purple, green_cloth * 0.96);
    rgb = mix(rgb, gold, cyan_blade * 0.94);
    COLOR = vec4(rgb, c.a);
}
""";

    public const string RustcladShader = """
shader_type canvas_item;
void fragment() {
    vec4 tex = texture(TEXTURE, UV);
    float lum = dot(tex.rgb, vec3(0.299, 0.587, 0.114));
    float maxc = max(tex.r, max(tex.g, tex.b));
    float minc = min(tex.r, min(tex.g, tex.b));
    float sat = maxc - minc;

    float yellow_armor = smoothstep(0.18, 0.40, lum)
        * smoothstep(0.10, 0.28, tex.r)
        * smoothstep(0.08, 0.24, tex.g)
        * (1.0 - smoothstep(0.36, 0.66, tex.b))
        * smoothstep(0.28, 0.68, tex.g / max(tex.r, 0.001))
        * smoothstep(0.00, 0.12, tex.r - tex.b);
    float dark_underlayer = (1.0 - smoothstep(0.22, 0.42, lum))
        * smoothstep(0.02, 0.20, tex.r - tex.g)
        * (1.0 - smoothstep(0.44, 0.74, tex.g / max(tex.r, 0.001)))
        * (1.0 - yellow_armor);
    float pale_sword = smoothstep(0.55, 0.86, lum)
        * (1.0 - smoothstep(0.00, 0.16, sat));

    vec3 black_armor = mix(vec3(0.18, 0.19, 0.20), vec3(0.68, 0.69, 0.72), smoothstep(0.14, 0.76, lum));
    vec3 crimson = mix(vec3(0.16, 0.00, 0.01), vec3(0.50, 0.03, 0.035), smoothstep(0.06, 0.48, lum));
    vec3 cold_sword = mix(vec3(0.22, 0.42, 0.58), vec3(0.86, 0.97, 1.00), smoothstep(0.28, 0.92, lum));

    vec3 rgb = mix(tex.rgb, black_armor, yellow_armor);
    rgb = mix(rgb, crimson, dark_underlayer);
    rgb = mix(rgb, cold_sword, pale_sword * 0.78);
    COLOR = vec4(rgb * COLOR.rgb, tex.a * COLOR.a);
}
""";

    public const string OperosisShader = """
shader_type canvas_item;
void fragment() {
    vec4 c = texture(TEXTURE, UV) * COLOR;
    float lum = dot(c.rgb, vec3(0.299, 0.587, 0.114));
    float maxc = max(c.r, max(c.g, c.b));
    float minc = min(c.r, min(c.g, c.b));
    float sat = maxc - minc;

    float cyan_shell = smoothstep(0.04, 0.20, min(c.g, c.b) - c.r)
        * smoothstep(0.12, 0.42, lum)
        * smoothstep(0.05, 0.24, sat);
    float blue_glow = smoothstep(0.05, 0.22, c.b - c.r)
        * smoothstep(-0.05, 0.14, c.b - c.g)
        * smoothstep(0.16, 0.58, lum)
        * smoothstep(0.06, 0.24, sat);

    vec3 orange_shadow = vec3(0.42, 0.05, 0.02);
    vec3 orange_mid = vec3(0.90, 0.24, 0.04);
    vec3 orange_high = vec3(1.00, 0.60, 0.14);
    vec3 orange = mix(orange_shadow, orange_mid, smoothstep(0.10, 0.58, lum));
    orange = mix(orange, orange_high, smoothstep(0.56, 0.92, lum));

    vec3 hot_shadow = vec3(0.50, 0.00, 0.02);
    vec3 hot_mid = vec3(1.00, 0.12, 0.03);
    vec3 hot_high = vec3(1.00, 0.78, 0.20);
    vec3 hot = mix(hot_shadow, hot_mid, smoothstep(0.12, 0.62, lum));
    hot = mix(hot, hot_high, smoothstep(0.58, 0.95, lum));

    vec3 rgb = mix(c.rgb, orange, cyan_shell * 0.96);
    rgb = mix(rgb, hot, blue_glow * 0.88);
    COLOR = vec4(rgb, c.a);
}
""";

    public const string BonebinderShader = """
shader_type canvas_item;
void fragment() {
    vec4 c = texture(TEXTURE, UV) * COLOR;
    float lum = dot(c.rgb, vec3(0.299, 0.587, 0.114));
    float maxc = max(c.r, max(c.g, c.b));
    float minc = min(c.r, min(c.g, c.b));
    float sat = maxc - minc;

    float blue_flame = smoothstep(0.10, 0.28, c.b - c.r)
        * smoothstep(0.02, 0.18, c.g - c.r)
        * smoothstep(0.24, 0.62, lum)
        * smoothstep(0.10, 0.32, sat);
    float robe = smoothstep(0.04, 0.22, c.r - c.g)
        * smoothstep(-0.04, 0.12, c.b - c.g)
        * (1.0 - smoothstep(0.70, 0.90, lum))
        * smoothstep(0.10, 0.34, sat);

    vec3 pink_shadow = vec3(0.54, 0.08, 0.42);
    vec3 pink_hot = vec3(1.00, 0.36, 0.82);
    vec3 pink_core = vec3(1.00, 0.72, 0.94);
    vec3 pink = mix(pink_shadow, pink_hot, smoothstep(0.18, 0.68, lum));
    pink = mix(pink, pink_core, smoothstep(0.62, 0.96, lum));

    vec3 green_shadow = vec3(0.11, 0.24, 0.08);
    vec3 green_mid = vec3(0.33, 0.66, 0.18);
    vec3 green_high = vec3(0.76, 0.95, 0.44);
    vec3 green = mix(green_shadow, green_mid, smoothstep(0.08, 0.58, lum));
    green = mix(green, green_high, smoothstep(0.56, 0.88, lum));

    vec3 rgb = mix(c.rgb, green, robe * 0.92);
    rgb = mix(rgb, pink, blue_flame * 0.98);
    COLOR = vec4(rgb, c.a);
}
""";

    public static void ApplyShader(Node node, string shaderCode)
    {
        ShaderMaterial material = CreateSpineShaderMaterial(shaderCode);
        if (node is NCreatureVisuals visuals && visuals.SpineBody != null)
        {
            visuals.SpineBody.SetNormalMaterial(material);
            MainFile.Logger.Info($"[NeowCompanions] Applied custom Spine material to {visuals.Name}; current={visuals.SpineBody.GetNormalMaterial()?.GetType().Name ?? "null"}.");
            visuals.AddChild(new DelayedSpineMaterialApplier(shaderCode));
            return;
        }
        if (node is NCreatureVisuals pendingVisuals)
        {
            MainFile.Logger.Info($"[NeowCompanions] Queued custom Spine material for {pendingVisuals.Name}; SpineBody not ready.");
            pendingVisuals.AddChild(new DelayedSpineMaterialApplier(shaderCode));
            return;
        }

        ApplyMaterialRecursive(node, material);
    }

    private static ShaderMaterial CreateSpineShaderMaterial(string shaderCode)
    {
        try
        {
            ShaderMaterial material = (ShaderMaterial)PreloadManager.Cache.GetMaterial("res://materials/vfx/hsv.tres").Duplicate();
            material.Shader = new Shader { Code = shaderCode };
            return material;
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error("[NeowCompanions] Could not duplicate built-in HSV material for custom palette: " + ex);
            return new ShaderMaterial { Shader = new Shader { Code = shaderCode } };
        }
    }

    private static void ApplyMaterialRecursive(Node node, ShaderMaterial material)
    {
        if (node is CanvasItem canvasItem)
        {
            canvasItem.Material = material;
        }

        foreach (Node child in node.GetChildren())
        {
            ApplyMaterialRecursive(child, material);
        }
    }

    private sealed partial class DelayedSpineMaterialApplier : Node
    {
        private readonly string shaderCode;
        private ShaderMaterial? material;
        private int framesWaited;

        public DelayedSpineMaterialApplier(string shaderCode)
        {
            this.shaderCode = shaderCode;
        }

        public override void _Process(double delta)
        {
            framesWaited++;
            if (framesWaited < 2 || GetParent() is not NCreatureVisuals visuals)
            {
                return;
            }

            if (visuals.SpineBody == null)
            {
                if (framesWaited > 300)
                {
                    QueueFree();
                }
                return;
            }

            material ??= CreateSpineShaderMaterial(shaderCode);
            visuals.SpineBody.SetNormalMaterial(material);
            if (framesWaited is 2 or 30 or 120 or 300)
            {
                Material? current = visuals.SpineBody.GetNormalMaterial();
                MainFile.Logger.Info($"[NeowCompanions] Reapplied custom Spine material to {visuals.Name}; frame={framesWaited}; same={ReferenceEquals(current, material)}; current={current?.GetType().Name ?? "null"}.");
            }
            if (framesWaited > 300)
            {
                QueueFree();
            }
        }
    }
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class GlitchlingRelic : BossCompanionRelic<GlitchlingPet>
{
    protected override string CompanionName => "Glitchling";
    protected override string RelicIconFileName => "relic_glitchling.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class OverclockCard : BossCompanionCard<GlitchlingPet>
{
    private static readonly LocString[] Lines =
    [
        new("ancients", "THE_ARCHITECT.talk.DEFECT.0-1r.char"),
        new("ancients", "THE_ARCHITECT.talk.DEFECT.1-1r.char"),
        new("ancients", "THE_ARCHITECT.talk.DEFECT.2-1r.char")
    ];

    protected override string CompanionName => "Glitchling";
    protected override string CardTitle => "Orbital Glitch";
    protected override string CardArtFileName => "card_glitchling.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Orb", 1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<GlitchlingOrbitPower>()
    ];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Channel {Orb:diff} random Orb. Every other turn, Evoke your rightmost Orb at the end of your turn."),
        ("flavor", "It hums one impossible note too high.")
    ];

    public OverclockCard()
        : base(1, CardType.Power, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CharacterCompanionDialogue.SayRandom<GlitchlingPet>(Owner, Lines, VfxColor.Blue);
        await TriggerPetAnimation<GlitchlingPet>("PowerUp", 0.5f);
        await GlitchlingOrbitPower.ChannelRandomOrbs(choiceContext, Owner, DynamicVars["Orb"].IntValue);
        await PowerCmd.Apply<GlitchlingOrbitPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Orb"].UpgradeValueBy(1m);
    }
}

public sealed class GlitchlingOrbitPower : CustomPowerModel
{
    private int turnsElapsed;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization =>
    [
        ("title", "Orbital Glitch"),
        ("description", "Every other turn, Evoke your rightmost Orb at the end of your turn.")
    ];

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Combat.CombatSide side,
        IEnumerable<Creature> creatures)
    {
        if (!creatures.Contains(Owner) || Owner.Player == null)
        {
            return;
        }

        turnsElapsed++;
        if (turnsElapsed % 2 != 0)
        {
            return;
        }

        Flash();
        await OrbCmd.EvokeLast(choiceContext, Owner.Player);
    }

    public static async Task ChannelRandomOrbs(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Entities.Players.Player owner,
        int count)
    {
        List<OrbModel> options =
        [
            ModelDb.Orb<LightningOrb>(),
            ModelDb.Orb<FrostOrb>(),
            ModelDb.Orb<DarkOrb>(),
            ModelDb.Orb<PlasmaOrb>()
        ];

        for (int i = 0; i < count; i++)
        {
            OrbModel? orb = owner.RunState.Rng.CombatCardGeneration.NextItem(options);
            if (orb == null)
            {
                continue;
            }

            await OrbCmd.Channel(choiceContext, orb.ToMutable(), owner);
        }
    }
}

public sealed class GlitchlingPet : CharacterCompanionPet<Defect>
{
    protected override float PetScale => 0.34f;
    protected override float HueShift => -0.18f;
    protected override Color Tint => new(1.12f, 0.88f, 0.56f, 1.0f);
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class BonebinderRelic : BossCompanionRelic<BonebinderPet>
{
    protected override string CompanionName => "Bonebinder";
    protected override string RelicIconFileName => "relic_bonebinder.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class GraveCallCard : BossCompanionCard<BonebinderPet>
{
    private static readonly LocString[] Lines =
    [
        new("ancients", "THE_ARCHITECT.talk.NECROBINDER.0-0.char"),
        new("ancients", "THE_ARCHITECT.talk.NECROBINDER.1-0r.char"),
        new("ancients", "THE_ARCHITECT.talk.NECROBINDER.2-0r.char"),
        new("ancients", "THE_ARCHITECT.talk.NECROBINDER.3-0r.char"),
        new("ancients", "THE_ARCHITECT.talk.NECROBINDER.3-2r.char")
    ];

    protected override string CompanionName => "Bonebinder";
    protected override string CardTitle => "Doombind";
    protected override string CardArtFileName => "card_bonebinder.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<BonebinderDoombindPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<DoomPower>(),
        HoverTipFactory.FromPower<BonebinderDoombindPower>()
    ];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Your next {BonebinderDoombindPower:diff} Attack applies Doom equal to unblocked damage dealt."),
        ("flavor", "A tiny hand knocks from the other side.")
    ];

    public GraveCallCard()
        : base(1, CardType.Skill, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CharacterCompanionDialogue.SayRandom<BonebinderPet>(Owner, Lines, VfxColor.Purple);
        await TriggerPetAnimation<BonebinderPet>("summonTrigger", 0.4f);
        await PowerCmd.Apply<BonebinderDoombindPower>(choiceContext, Owner.Creature, DynamicVars["BonebinderDoombindPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BonebinderDoombindPower"].UpgradeValueBy(1m);
    }
}

public sealed class BonebinderDoombindPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization =>
    [
        ("title", "Doombind"),
        ("description", "Your next {Amount} Attack applies Doom equal to unblocked damage dealt.")
    ];

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? source,
        CardModel? cardSource)
    {
        if (source != Owner || cardSource == null || cardSource.Type != CardType.Attack || target.Side == Owner.Side)
        {
            return;
        }

        Flash();
        if (result.UnblockedDamage > 0)
        {
            await PowerCmd.Apply<DoomPower>(choiceContext, target, result.UnblockedDamage, Owner, null);
        }

        if (Amount <= 1)
        {
            await PowerCmd.Remove(this);
        }
        else
        {
            await PowerCmd.ModifyAmount(choiceContext, this, -1m, Owner, null);
        }
    }
}

public sealed class BonebinderPet : CharacterCompanionPet<Necrobinder>
{
    protected override float PetScale => 0.50f;
    protected override float HueShift => 0.0f;

    public override NCreatureVisuals? CreateCustomVisuals()
    {
        NCreatureVisuals? visuals = base.CreateCustomVisuals();
        if (visuals != null)
        {
            CompanionSelectivePalette.ApplyShader(visuals, CompanionSelectivePalette.BonebinderShader);
            visuals.AddChild(new BonebinderFlamePigmentApplier());
        }

        if (visuals == null)
        {
            return null;
        }

        return CompanionDrag.MakeDraggable(visuals);
    }
}

internal sealed partial class BonebinderFlamePigmentApplier : Node
{
    private int framesWaited;
    private bool loggedDiagnostics;
    private bool loggedFlameMaterial;

    public override void _Process(double delta)
    {
        framesWaited++;

        if (GetParent() is not NCreatureVisuals visuals || visuals.SpineBody == null)
        {
            if (framesWaited > 300)
            {
                QueueFree();
            }
            return;
        }

        MegaSkeleton? skeleton = visuals.SpineBody.GetSkeleton();
        if (skeleton == null)
        {
            if (framesWaited > 300)
            {
                QueueFree();
            }
            return;
        }

        int flameSprites = TintFlameSprites(visuals);
        int tintedSlots = 0;

        if (!loggedDiagnostics && framesWaited is 2 or 30)
        {
            loggedDiagnostics = true;
            MainFile.Logger.Info($"[NeowCompanions] Bonebinder pigment pass: flameSprites={flameSprites}; tintedSlots={tintedSlots}; skeletonMethods={InterestingMethods(skeleton.BoundObject)}.");
            MainFile.Logger.Info(DumpInterestingTree(visuals, 0, 5));
        }

        if (framesWaited > 300)
        {
            QueueFree();
        }
    }

    private static T? FindNodeRecursive<T>(Node node, string name)
        where T : Node
    {
        if (node.Name == name && node is T typedNode)
        {
            return typedNode;
        }

        foreach (Node child in node.GetChildren())
        {
            T? found = FindNodeRecursive<T>(child, name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private int TintFlameSprites(NCreatureVisuals visuals)
    {
        Node2D? headNode = FindNodeRecursive<Node2D>(visuals, "HeadBoneNode");
        if (headNode == null)
        {
            return 0;
        }

        return TintFlameSprites(headNode);
    }

    private int TintFlameSprites(Node node)
    {
        int count = 0;
        TintFlameSprites(node, ref count);
        return count;
    }

    private void TintFlameSprites(Node node, ref int count)
    {
        string name = node.Name.ToString();
        bool likelyFlame = name.Contains("SteppedFire", StringComparison.OrdinalIgnoreCase)
            || name.Contains("fire", StringComparison.OrdinalIgnoreCase)
            || name.Contains("flame", StringComparison.OrdinalIgnoreCase);

        if (node is Sprite2D sprite && likelyFlame)
        {
            if (!loggedFlameMaterial)
            {
                loggedFlameMaterial = true;
                MainFile.Logger.Info(DescribeFlameSprite(sprite));
            }

            ApplyOriginalFlameMaterialColors(sprite);
            sprite.Modulate = Colors.White;
            sprite.SelfModulate = Colors.White;
            sprite.UseParentMaterial = false;
            count++;
        }

        foreach (Node child in node.GetChildren())
        {
            TintFlameSprites(child, ref count);
        }
    }

    private static void ApplyOriginalFlameMaterialColors(Sprite2D sprite)
    {
        if (sprite.Material is not ShaderMaterial shaderMaterial)
        {
            return;
        }

        if (shaderMaterial.ResourceLocalToScene)
        {
            shaderMaterial.SetShaderParameter("OuterColor", new Color(1.0f, 0.12f, 0.62f, 1.0f));
            shaderMaterial.SetShaderParameter("InnerColor", new Color(0.16f, 0.0f, 0.10f, 1.0f));
            return;
        }

        ShaderMaterial localMaterial = (ShaderMaterial)shaderMaterial.Duplicate();
        localMaterial.ResourceLocalToScene = true;
        localMaterial.SetShaderParameter("OuterColor", new Color(1.0f, 0.12f, 0.62f, 1.0f));
        localMaterial.SetShaderParameter("InnerColor", new Color(0.16f, 0.0f, 0.10f, 1.0f));
        sprite.Material = localMaterial;
    }

    private static string DescribeFlameSprite(Sprite2D sprite)
    {
        StringBuilder builder = new();
        builder.Append("[NeowCompanions] Bonebinder flame sprite ");
        builder.Append(sprite.GetPath());
        builder.Append("; texture=");
        builder.Append(sprite.Texture?.ResourcePath ?? "<null>");
        builder.Append("; material=");
        builder.Append(sprite.Material?.GetType().Name ?? "<null>");
        builder.Append("; materialPath=");
        builder.Append(sprite.Material?.ResourcePath ?? "<null>");

        if (sprite.Material is ShaderMaterial shaderMaterial)
        {
            Shader? shader = shaderMaterial.Shader;
            builder.Append("; shaderPath=");
            builder.Append(shader?.ResourcePath ?? "<null>");

            try
            {
                if (shader != null)
                {
                    builder.Append("; uniforms=");
                    List<string> uniforms = [];
                    foreach (Godot.Collections.Dictionary uniform in shader.GetShaderUniformList())
                    {
                        string uniformName = uniform.TryGetValue("name", out Variant uniformNameVariant)
                            ? uniformNameVariant.AsString()
                            : "<unnamed>";
                        Variant value = shaderMaterial.GetShaderParameter(uniformName);
                        uniforms.Add($"{uniformName}={value}");
                    }
                    builder.Append(string.Join(", ", uniforms));
                }
            }
            catch (Exception ex)
            {
                builder.Append("; uniformError=");
                builder.Append(ex.GetType().Name);
                builder.Append(": ");
                builder.Append(ex.Message);
            }
        }

        return builder.ToString();
    }

    private static int TryTintSpineSlots(MegaSkeleton skeleton)
    {
        GodotObject skeletonObject = skeleton.BoundObject;
        if (!skeletonObject.HasMethod("get_slots"))
        {
            return 0;
        }

        int tinted = 0;
        Variant slotsVariant = skeletonObject.Call("get_slots");
        foreach (Variant slotVariant in slotsVariant.AsGodotArray())
        {
            GodotObject? slot = slotVariant.AsGodotObject();
            if (slot == null)
            {
                continue;
            }

            string slotName = GetSpineObjectName(slot);
            bool likelyFlame = slotName.Contains("head", StringComparison.OrdinalIgnoreCase)
                || slotName.Contains("hair", StringComparison.OrdinalIgnoreCase)
                || slotName.Contains("fire", StringComparison.OrdinalIgnoreCase)
                || slotName.Contains("flame", StringComparison.OrdinalIgnoreCase)
                || slotName.Contains("blue", StringComparison.OrdinalIgnoreCase);

            bool blueColor = false;
            if (slot.HasMethod("get_color"))
            {
                Color color = slot.Call("get_color").As<Color>();
                blueColor = color.B > color.R + 0.15f && color.B > color.G + 0.05f;
            }

            if ((likelyFlame || blueColor) && slot.HasMethod("set_color"))
            {
                slot.Call("set_color", new Color(1.0f, 0.10f, 0.62f, 1.0f));
                tinted++;
            }
        }

        return tinted;
    }

    private static string GetSpineObjectName(GodotObject spineObject)
    {
        if (spineObject.HasMethod("get_name"))
        {
            return spineObject.Call("get_name").AsString();
        }

        if (spineObject.HasMethod("get_data"))
        {
            GodotObject? data = spineObject.Call("get_data").AsGodotObject();
            if (data != null && data.HasMethod("get_name"))
            {
                return data.Call("get_name").AsString();
            }
        }

        return string.Empty;
    }

    private static string InterestingMethods(GodotObject obj)
    {
        List<string> names = [];
        foreach (Godot.Collections.Dictionary method in obj.GetMethodList())
        {
            string name = method["name"].AsString();
            if (name.Contains("slot", StringComparison.OrdinalIgnoreCase)
                || name.Contains("material", StringComparison.OrdinalIgnoreCase)
                || name.Contains("color", StringComparison.OrdinalIgnoreCase)
                || name.Contains("attachment", StringComparison.OrdinalIgnoreCase)
                || name.Contains("skin", StringComparison.OrdinalIgnoreCase))
            {
                names.Add(name);
            }
        }

        return names.Count == 0 ? "<none>" : string.Join(", ", names.Distinct().OrderBy(name => name));
    }

    private static string DumpInterestingTree(Node node, int depth, int maxDepth)
    {
        StringBuilder builder = new();
        DumpInterestingTree(node, depth, maxDepth, builder);
        return builder.ToString();
    }

    private static void DumpInterestingTree(Node node, int depth, int maxDepth, StringBuilder builder)
    {
        if (depth > maxDepth)
        {
            return;
        }

        string name = node.Name.ToString();
        bool interesting = depth <= 1
            || name.Contains("head", StringComparison.OrdinalIgnoreCase)
            || name.Contains("fire", StringComparison.OrdinalIgnoreCase)
            || name.Contains("flame", StringComparison.OrdinalIgnoreCase)
            || name.Contains("vfx", StringComparison.OrdinalIgnoreCase)
            || name.Contains("bone", StringComparison.OrdinalIgnoreCase)
            || name.Contains("spine", StringComparison.OrdinalIgnoreCase)
            || node.GetClass() == "SpineSlotNode";

        if (interesting)
        {
            builder.Append(' ', depth * 2);
            builder.Append("[NeowCompanions] Bonebinder node ");
            builder.Append(node.GetPath());
            builder.Append(" :: ");
            builder.Append(node.GetType().Name);
            builder.Append(" class=");
            builder.Append(node.GetClass());
            builder.AppendLine();
        }

        foreach (Node child in node.GetChildren())
        {
            DumpInterestingTree(child, depth + 1, maxDepth, builder);
        }
    }
}

internal sealed partial class BonebinderPinkFlameOverlay : Node2D
{
    public const string NodeName = "NeowCompanionsPinkFlame";

    public Node2D? Target { get; set; }

    private float time;

    public override void _Process(double delta)
    {
        time += (float)delta;
        if (Target == null || !IsInstanceValid(Target))
        {
            QueueFree();
            return;
        }

        GlobalPosition = Target.GlobalPosition + new Vector2(27.0f, 2.0f);
        GlobalRotation = 0.0f;
        GlobalScale = new Vector2(1.25f, 1.25f);
        QueueRedraw();
    }

    public override void _Draw()
    {
        float sway = MathF.Sin(time * 7.5f) * 2.0f;
        float pulse = 1.0f + MathF.Sin(time * 11.0f) * 0.06f;

        DrawColoredPolygon(
            [
                new Vector2(-12.0f * pulse, 7.0f),
                new Vector2(-6.0f + sway, -13.0f),
                new Vector2(0.0f - sway, -34.0f),
                new Vector2(6.0f + sway, -12.0f),
                new Vector2(12.0f * pulse, 7.0f)
            ],
            new Color(0.82f, 0.0f, 0.45f, 1.0f));

        DrawColoredPolygon(
            [
                new Vector2(-7.5f * pulse, 4.0f),
                new Vector2(-1.5f - sway, -12.0f),
                new Vector2(3.5f + sway, -28.0f),
                new Vector2(7.5f * pulse, 4.0f)
            ],
            new Color(1.0f, 0.12f, 0.74f, 0.96f));

        DrawColoredPolygon(
            [
                new Vector2(-3.0f, 0.0f),
                new Vector2(1.5f + sway, -18.0f),
                new Vector2(4.0f, 0.0f)
            ],
            new Color(1.0f, 0.66f, 0.95f, 0.88f));

        DrawCircle(new Vector2(0.0f, 1.0f), 10.0f * pulse, new Color(1.0f, 0.03f, 0.56f, 0.9f));
        DrawCircle(new Vector2(0.5f + sway * 0.2f, -8.5f), 6.5f * pulse, new Color(1.0f, 0.45f, 0.9f, 0.72f));
    }
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class GildedPageRelic : BossCompanionRelic<GildedPagePet>
{
    protected override string CompanionName => "Gilded Page";
    protected override string RelicIconFileName => "relic_gilded_page.png";

    public override List<(string, string)> Localization =>
    [
        ("title", CompanionName),
        ("description", $"At the start of each combat, summon {CompanionName} and gain 1 Star."),
        ("flavor", "Neow keeps stranger company than usual.")
    ];

    public override async Task BeforeCombatStart()
    {
        await base.BeforeCombatStart();
        await PlayerCmd.GainStars(1, Owner);
    }
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class CommandingFlourishCard : BossCompanionCard<GildedPagePet>
{
    private static readonly LocString[] Lines =
    [
        new("ancients", "THE_ARCHITECT.talk.REGENT.0-0.char"),
        new("ancients", "THE_ARCHITECT.talk.REGENT.0-2.char"),
        new("ancients", "THE_ARCHITECT.talk.REGENT.1-0r.char"),
        new("ancients", "THE_ARCHITECT.talk.REGENT.1-2r.char"),
        new("ancients", "THE_ARCHITECT.talk.REGENT.2-0r.char"),
        new("ancients", "THE_ARCHITECT.talk.REGENT.2-2r.char")
    ];

    protected override string CompanionName => "Gilded Page";
    protected override string CardTitle => "Royal Draft";
    protected override string CardArtFileName => "card_gilded_page.png";

    public override bool HasStarCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IfUpgradedVar("IfUpgraded", 0m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Generate a random non-colorless card from another character X times.{IfUpgraded:show: They are Upgraded.|}"),
        ("flavor", "A very small decree, delivered with enormous confidence.")
    ];

    public CommandingFlourishCard()
        : base(0, CardType.Skill, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CharacterCompanionDialogue.SayRandom<GildedPagePet>(Owner, Lines, VfxColor.Orange);
        await TriggerPetAnimation<GildedPagePet>("sovereignBladeTrigger", 0.25f);
        int count = ResolveStarXValue();
        if (count <= 0 || CombatState == null)
        {
            return;
        }

        List<CardModel> options = GetDraftOptions().ToList();
        if (options.Count == 0)
        {
            MainFile.Logger.Info("Royal Draft could not find any valid character cards to generate.");
            return;
        }

        List<CardModel> generatedCards = [];
        for (int i = 0; i < count; i++)
        {
            CardModel? randomCard = Owner.RunState.Rng.CombatCardGeneration.NextItem(options);
            if (randomCard == null)
            {
                continue;
            }

            CardModel generatedCard = CombatState.CreateCard(randomCard, Owner);
            if (IsUpgraded)
            {
                CardCmd.Upgrade(generatedCard);
            }

            generatedCards.Add(generatedCard);
        }

        await CardPileCmd.AddGeneratedCardsToCombat(generatedCards, PileType.Hand, Owner);
    }

    private IEnumerable<CardModel> GetDraftOptions()
    {
        CardMultiplayerConstraint runConstraint = Owner.RunState.CardMultiplayerConstraint;

        return ModelDb.AllCards
            .Where(card => card.CanBeGeneratedInCombat && card.ShouldShowInCardLibrary)
            .Where(card => !card.Pool.IsColorless)
            .Where(card => IsAllowedForCurrentRun(card, runConstraint));
    }

    private static bool IsAllowedForCurrentRun(CardModel card, CardMultiplayerConstraint runConstraint)
    {
        return runConstraint switch
        {
            CardMultiplayerConstraint.MultiplayerOnly => card.MultiplayerConstraint != CardMultiplayerConstraint.SingleplayerOnly,
            CardMultiplayerConstraint.SingleplayerOnly => card.MultiplayerConstraint != CardMultiplayerConstraint.MultiplayerOnly,
            _ => card.MultiplayerConstraint != CardMultiplayerConstraint.MultiplayerOnly
        };
    }

    protected override void OnUpgrade()
    {
        DynamicVars["IfUpgraded"].UpgradeValueBy(0m);
    }
}

public sealed class GildedPagePet : CharacterCompanionPet<Regent>
{
    protected override float PetScale => 0.68f;
    protected override float HueShift => 0.36f;
}

public abstract class ElementalByrdpipPet : CustomMonsterModel
{
    protected abstract float HueShift { get; }
    protected abstract Color Tint { get; }
    protected abstract string SkinName { get; }
    protected virtual string? PaletteShader => null;

    public override int MinInitialHp => 9999;
    public override int MaxInitialHp => 9999;
    public override bool IsHealthBarVisible => false;

    public override NCreatureVisuals? CreateCustomVisuals()
    {
        NCreatureVisuals visuals = ModelDb.Monster<MegaCrit.Sts2.Core.Models.Monsters.Byrdpip>().CreateVisuals();
        visuals.SetScaleAndHue(0.72f, HueShift);
        visuals.Scale = new Vector2(-0.72f, 0.72f);
        visuals.Modulate = Tint;
        visuals.CallDeferred(NCreatureVisuals.MethodName.SetScaleAndHue, 0.72f, HueShift);
        if (PaletteShader is { } paletteShader)
        {
            CompanionSelectivePalette.ApplyShader(visuals, paletteShader);
        }

        return CompanionDrag.MakeDraggable(visuals);
    }

    public override void SetupSkins(MegaSprite spine, MegaSkeleton skeleton)
    {
        MegaSkeletonDataResource data = skeleton.GetData();
        skeleton.SetSkin(data.FindSkin(SkinName));
        skeleton.SetSlotsToSetupPose();
    }

    public override MegaCrit.Sts2.Core.Animation.CreatureAnimator? SetupCustomAnimationStates(MegaSprite controller)
    {
        return ModelDb.Monster<MegaCrit.Sts2.Core.Models.Monsters.Byrdpip>().GenerateAnimator(controller);
    }

    protected override MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine.MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine.MonsterState> states = [];
        MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine.MoveState idle =
            new("NOTHING_MOVE", (IReadOnlyList<Creature> _) => Task.CompletedTask);

        idle.FollowUpState = idle;
        states.Add(idle);

        return new MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine.MonsterMoveStateMachine(states, idle);
    }
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class EmberPipRelic : BossCompanionRelic<EmberPipPet>
{
    protected override string CompanionName => "Ember Pip";
    protected override string RelicIconFileName => "relic_ember_pip.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class EmberPipCard : BossCompanionCard<EmberPipPet>
{
    protected override string CompanionName => "Ember Pip";
    protected override string CardTitle => "Ember Swoop";
    protected override string CardArtFileName => "card_ember_pip.png";

    public override Texture2D? CustomPortrait => ModelDb.Card<ByrdSwoop>().Portrait;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SingleEnemyAwareDamageVar(8m, DamageProps.card),
        new PowerVar<VulnerablePower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Deal {Damage:diff} damage. Apply {VulnerablePower:diff} Vulnerable."),
        ("flavor", "A little spark with total confidence.")
    ];

    public EmberPipCard()
        : base(1, CardType.Attack, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await TriggerPetAnimation<EmberPipPet>("Attack", 0.35f);
        await CreatureCmd.Damage(choiceContext, cardPlay.Target, DynamicVars.Damage, Owner.Creature, this);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, DynamicVars.Vulnerable.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}

public sealed class EmberPipPet : ElementalByrdpipPet
{
    protected override float HueShift => 0.0f;
    protected override Color Tint => Colors.White;
    protected override string SkinName => "version1";
    protected override string? PaletteShader => CompanionSelectivePalette.EmberPipShader;
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class FrostPipRelic : BossCompanionRelic<FrostPipPet>
{
    protected override string CompanionName => "Frost Pip";
    protected override string RelicIconFileName => "relic_frost_pip.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class FrostPipCard : BossCompanionCard<FrostPipPet>
{
    protected override string CompanionName => "Frost Pip";
    protected override string CardTitle => "Frost Flutter";
    protected override string CardArtFileName => "card_frost_pip.png";

    public override Texture2D? CustomPortrait => ModelDb.Card<ByrdSwoop>().Portrait;

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SelfAwareBlockVar(7m, ValueProp.Move),
        new PowerVar<WeakPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WeakPower>()];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Gain {Block:diff} Block. Apply {WeakPower:diff} Weak to ALL enemies."),
        ("flavor", "The smallest chill can still find the spine.")
    ];

    public FrostPipCard()
        : base(1, CardType.Skill, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
        {
            return;
        }

        await TriggerPetAnimation<FrostPipPet>("Attack", 0.35f);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay, false);
        await PowerCmd.Apply<WeakPower>(
            choiceContext,
            CombatState.HittableEnemies.Where(enemy => enemy.IsAlive),
            DynamicVars.Weak.BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}

public sealed class FrostPipPet : ElementalByrdpipPet
{
    protected override float HueShift => 0.52f;
    protected override Color Tint => new(0.62f, 1.05f, 1.22f, 1.0f);
    protected override string SkinName => "version3";
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class StormPipRelic : BossCompanionRelic<StormPipPet>
{
    protected override string CompanionName => "Storm Pip";
    protected override string RelicIconFileName => "relic_storm_pip.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class StormPipCard : BossCompanionCard<StormPipPet>
{
    protected override string CompanionName => "Storm Pip";
    protected override string CardTitle => "Static Dive";
    protected override string CardArtFileName => "card_storm_pip.png";

    public override Texture2D? CustomPortrait => ModelDb.Card<ByrdSwoop>().Portrait;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4m, DamageProps.card),
        new RepeatVar(2)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Deal {Damage:diff} damage to ALL enemies {Repeat:diff} times."),
        ("flavor", "Tiny wings. Bad weather.")
    ];

    public StormPipCard()
        : base(1, CardType.Attack, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
        {
            return;
        }

        await TriggerPetAnimation<StormPipPet>("Attack", 0.25f);
        List<Creature> enemies = CombatState.HittableEnemies.Where(enemy => enemy.IsAlive).ToList();
        for (int i = 0; i < DynamicVars.Repeat.IntValue; i++)
        {
            await CreatureCmd.Damage(choiceContext, enemies, DynamicVars.Damage, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1m);
    }
}

public sealed class StormPipPet : ElementalByrdpipPet
{
    protected override float HueShift => 0.66f;
    protected override Color Tint => new(0.88f, 0.76f, 1.28f, 1.0f);
    protected override string SkinName => "version4";
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class ThornPipRelic : BossCompanionRelic<ThornPipPet>
{
    protected override string CompanionName => "Thorn Pip";
    protected override string RelicIconFileName => "relic_thorn_pip.png";
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class ThornPipCard : BossCompanionCard<ThornPipPet>
{
    protected override string CompanionName => "Thorn Pip";
    protected override string CardTitle => "Briar Chirp";
    protected override string CardArtFileName => "card_thorn_pip.png";

    public override Texture2D? CustomPortrait => ModelDb.Card<ByrdSwoop>().Portrait;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<PoisonPower>(4m),
        new DynamicVar("Thorns", 2m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<PoisonPower>(),
        HoverTipFactory.FromPower<ThornsPower>()
    ];

    public override List<(string, string)> Localization =>
    [
        ("title", CardTitle),
        ("description", "Apply {PoisonPower:diff} Poison to ALL enemies. Gain {Thorns:diff} Thorns."),
        ("flavor", "Adorable, in the way a bramble is adorable.")
    ];

    public ThornPipCard()
        : base(1, CardType.Skill, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
        {
            return;
        }

        await TriggerPetAnimation<ThornPipPet>("Attack", 0.35f);
        await PowerCmd.Apply<PoisonPower>(
            choiceContext,
            CombatState.HittableEnemies.Where(enemy => enemy.IsAlive),
            DynamicVars.Poison.BaseValue,
            Owner.Creature,
            this);
        await PowerCmd.Apply<ThornsPower>(choiceContext, Owner.Creature, DynamicVars["Thorns"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Poison.UpgradeValueBy(2m);
        DynamicVars["Thorns"].UpgradeValueBy(1m);
    }
}

public sealed class ThornPipPet : ElementalByrdpipPet
{
    protected override float HueShift => 0.30f;
    protected override Color Tint => new(0.62f, 1.12f, 0.58f, 1.0f);
    protected override string SkinName => "version2";
}

internal static class CharacterCompanionDialogue
{
    private static readonly Random Rng = new();

    public static Task SayRandom<TPet>(MegaCrit.Sts2.Core.Entities.Players.Player owner, IReadOnlyList<LocString> lines, VfxColor color)
        where TPet : MonsterModel
    {
        Creature? pet = owner.PlayerCombatState?.GetPet<TPet>();
        if (pet == null || pet.IsDead || lines.Count == 0)
        {
            return Task.CompletedTask;
        }

        TalkCmd.Play(lines[Rng.Next(lines.Count)], pet, color, VfxDuration.Long);
        return Task.CompletedTask;
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
    private static readonly HashSet<Creature> SuppressedAttackAnimationPets = [];

    private sealed record AttackAnimationCandidate(Creature Pet, string[] AnimationNames);

    private const float InsatiableDevourScaleMultiplier = 7.0f;
    private const float InsatiableDevourGrowDuration = 0.76f;
    private const float InsatiableDevourEatDuration = 3.10f;
    private const float InsatiableDevourSwallowDelay = 1.35f;
    private const float InsatiableDevourPostEatHoldDuration = 0.36f;
    private const float InsatiableDevourRestoreDuration = 1.30f;
    private const int InsatiableDevourFrontZIndex = 1000;

    public static async Task TriggerRandomAttackForActiveCompanion(MegaCrit.Sts2.Core.Entities.Players.Player owner)
    {
        if (owner.PlayerCombatState == null)
        {
            return;
        }

        List<AttackAnimationCandidate> candidates =
        [
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<SoulFyshPipPet>(), "Attack", "AttackDebuffTrigger"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<WrigglerPet>(), "Attack"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<CeremonialBeastPet>(), "Attack"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<KinFollowerPet>(), "SlashTrigger", "BoomerangTrigger", "Attack"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<EyeWithTeethPet>(), "Attack"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<GremlinMercPet>(), "Attack"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<ThievingHopperPet>(), "Attack"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<AeonglassPet>(), "Attack"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<LagavulinMatriarchPet>(), "AttackHeavy", "AttackDouble"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<TheKinPet>(), "ThrowBomb", "Bomb", "Attack"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<WaterfallGiantPet>(), "Attack", "AttackKick", "AttackStomp"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<VantomPet>(), "Attack", "Dismember", "Extend1", "Extend2", "Extend3"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<KnowledgeDemonPet>(), "Slap", "Attack"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<TheInsatiablePet>(), "LungingBite", "Thrash", "Attack"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<QueenPet>(), "ArmsAttack", "Attack"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<TestSubjectPet>(), "Slash", "Bite", "Attack"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<SeapunkPet>(), "Kick", "MultiAttack", "Attack"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<ShrinkerBeetlePet>(), "Attack"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<OperosisPet>(), "Attack", "Cast"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<ArchitectPet>(), "Attack"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<RustcladPet>(), "heavyAttack", "Attack"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<ShadeleafPet>(), "Shiv", "Attack"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<GlitchlingPet>(), "Attack"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<BonebinderPet>(), "Attack", "Cast"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<GildedPagePet>(), "sovereignBladeTrigger", "Attack"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<EmberPipPet>(), "Attack"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<FrostPipPet>(), "Attack"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<StormPipPet>(), "Attack"),
            CreateAttackCandidate(owner.PlayerCombatState.GetPet<ThornPipPet>(), "Attack")
        ];

        candidates = candidates
            .Where(candidate => candidate.Pet != null
                && !candidate.Pet.IsDead
                && !SuppressedAttackAnimationPets.Contains(candidate.Pet))
            .ToList();

        if (candidates.Count == 0)
        {
            return;
        }

        AttackAnimationCandidate candidate = owner.RunState.Rng.CombatTargets.NextItem(candidates) ?? candidates[0];
        await TryTriggerAnimation(candidate.Pet, 0.2f, candidate.AnimationNames);
    }

    private static AttackAnimationCandidate CreateAttackCandidate(Creature? pet, params string[] animationNames)
    {
        return new AttackAnimationCandidate(pet!, animationNames);
    }

    private static async Task TryTriggerPetAttack(Creature? pet, params string[] animationNames)
    {
        if (pet == null || pet.IsDead || SuppressedAttackAnimationPets.Contains(pet))
        {
            return;
        }

        await TryTriggerAnimation(pet, 0.2f, animationNames);
    }

    public static bool IsAttackAnimationSuppressed(Creature pet)
    {
        return SuppressedAttackAnimationPets.Contains(pet);
    }

    public static void SuppressAttackAnimations(Creature pet)
    {
        SuppressedAttackAnimationPets.Add(pet);
    }

    public static async Task TriggerLagavulinMatriarchWake(Creature matriarch)
    {
        SfxCmd.Play(LagavulinMatriarch.awakenSfx);
        NCreature? matriarchNode = matriarch.GetCreatureNode();
        matriarchNode?.SpineAnimation.SetAnimation("_tracks/eyes_open", loop: false, 1);
        matriarchNode?.SpineAnimation.AddAnimation("_tracks/eyes_open_loop", 0f, loop: true, 1);

        await TryTriggerAnimation(matriarch, 0.6f, LagavulinMatriarch.wakeTrigger);
        await TryTriggerAnimation(matriarch, 0.25f, "AttackHeavy", "AttackDouble");
    }

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

