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
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace NeowCompanions.NeowCompanionsCode.Models;

[Pool(typeof(NeowCompanionCardPool))]
public sealed class FyshSwoop : CustomCardModel
{
    public override CardPoolModel Pool => ModelDb.Card<ByrdSwoop>().Pool;

    public override CardPoolModel VisualCardPool => Pool;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<VulnerablePower>(1m),
        new PowerVar<WeakPower>(1m),
        new IfUpgradedVar("IfUpgraded", 0m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override Texture2D? CustomPortrait => ModTextureLoader.Load("card_soul_fysh_pip.png");

    public override List<(string, string)> Localization =>
    [
        ("title", "Fysh Swoop"),
        ("description", "Apply 1 Vulnerable and 1 Weak.{IfUpgraded:show: Apply -1 Strength.|}"),
        ("flavor", "A tiny Soul Fysh dives in with improbable menace.")
    ];

    public FyshSwoop()
        : base(0, CardType.Skill, CardRarity.Event, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        var soulFysh = Owner.PlayerCombatState?.GetPet<SoulFyshPipPet>();
        if (soulFysh != null && !soulFysh.IsDead)
        {
            SfxCmd.Play("event:/sfx/enemy/enemy_attacks/soul_fysh/soul_fysh_wave");
            await CreatureCmd.TriggerAnim(soulFysh, "AttackDebuffTrigger", 0.65f);
        }

        await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, DynamicVars.Vulnerable.BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, DynamicVars.Weak.BaseValue, Owner.Creature, this);

        if (IsUpgraded)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, cardPlay.Target, -1m, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class WrigglerCard : CustomCardModel
{
    public override CardPoolModel Pool => ModelDb.Card<ByrdSwoop>().Pool;

    public override CardPoolModel VisualCardPool => Pool;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<PoisonPower>(3m),
        new IfUpgradedVar("IfUpgraded", 0m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<PoisonPower>()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override Texture2D? CustomPortrait => ModTextureLoader.Load("card_wriggler.png");

    public override List<(string, string)> Localization =>
    [
        ("title", "Wriggler"),
        ("description", "Apply 3 Poison to ALL enemies.{IfUpgraded:show:| Exhaust.}"),
        ("flavor", "It writhes with a terrible little purpose.")
    ];

    public WrigglerCard()
        : base(0, CardType.Skill, CardRarity.Event, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
        {
            return;
        }

        var wriggler = Owner.PlayerCombatState?.GetPet<WrigglerPet>();
        if (wriggler != null && !wriggler.IsDead)
        {
            SfxCmd.Play("event:/sfx/enemy/enemy_attacks/wriggler/wriggler_attack");
            await CreatureCmd.TriggerAnim(wriggler, "Attack", 0.15f);
        }

        await PowerCmd.Apply<PoisonPower>(
            choiceContext,
            CombatState.HittableEnemies.Where(enemy => enemy.IsAlive),
            DynamicVars.Poison.BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class CeremonialBeastCard : CustomCardModel
{
    public override CardPoolModel Pool => ModelDb.Card<ByrdSwoop>().Pool;

    public override CardPoolModel VisualCardPool => Pool;

    public override Texture2D? CustomPortrait => ModTextureLoader.Load("card_ceremonial_beast.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override List<(string, string)> Localization =>
    [
        ("title", "Ceremonial Toll"),
        ("description", "Deal 20 damage to ALL enemies. Exhaust."),
        ("flavor", "The bell rings once. The room answers.")
    ];

    public CeremonialBeastCard()
        : base(3, CardType.Attack, CardRarity.Event, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
        {
            return;
        }

        var beast = Owner.PlayerCombatState?.GetPet<CeremonialBeastPet>();
        if (beast != null && !beast.IsDead)
        {
            SfxCmd.Play("event:/sfx/enemy/enemy_attacks/ceremonial_beast/ceremonial_beast_shrill");
            await CreatureCmd.TriggerAnim(beast, "Charge", 0.6f);
        }

        await CreatureCmd.Damage(
            choiceContext,
            CombatState.HittableEnemies.Where(enemy => enemy.IsAlive),
            20m,
            DamageProps.card,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class KinFollowerCard : CustomCardModel
{
    public override CardPoolModel Pool => ModelDb.Card<ByrdSwoop>().Pool;

    public override CardPoolModel VisualCardPool => Pool;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<WeakPower>(1m),
        new PowerVar<VulnerablePower>(1m),
        new IfUpgradedVar("IfUpgraded", 0m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    public override Texture2D? CustomPortrait => ModTextureLoader.Load("card_kin_follower.png");

    public override List<(string, string)> Localization =>
    [
        ("title", "Kin Mark"),
        ("description", "Apply 1 Weak to ALL enemies.{IfUpgraded:show: Apply 1 Vulnerable to ALL enemies.|}"),
        ("flavor", "A quiet gesture, and the room turns against you.")
    ];

    public KinFollowerCard()
        : base(1, CardType.Skill, CardRarity.Event, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
        {
            return;
        }

        var kinFollower = Owner.PlayerCombatState?.GetPet<KinFollowerPet>();
        if (kinFollower != null && !kinFollower.IsDead)
        {
            MainFile.Logger.Info("Triggering Kin Follower attack animation from Kin Mark.");
            await CreatureCmd.TriggerAnim(kinFollower, "Attack", 0.6f);
        }

        await PowerCmd.Apply<WeakPower>(
            choiceContext,
            CombatState.HittableEnemies.Where(enemy => enemy.IsAlive),
            DynamicVars.Weak.BaseValue,
            Owner.Creature,
            this);

        if (IsUpgraded)
        {
            await PowerCmd.Apply<VulnerablePower>(
                choiceContext,
                CombatState.HittableEnemies.Where(enemy => enemy.IsAlive),
                DynamicVars.Vulnerable.BaseValue,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Vulnerable.UpgradeValueBy(0m);
    }
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class EyeWithTeethCard : CustomCardModel
{
    public override CardPoolModel Pool => ModelDb.Card<ByrdSwoop>().Pool;

    public override CardPoolModel VisualCardPool => Pool;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IfUpgradedVar("IfUpgraded", 0m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override Texture2D? CustomPortrait => ModTextureLoader.Load("card_eye_with_teeth.png");

    public override List<(string, string)> Localization =>
    [
        ("title", "Toothy Stare"),
        ("description", "Gain 6 Block.{IfUpgraded:show:| Exhaust.}"),
        ("flavor", "It sees the threat, then smiles.")
    ];

    public EyeWithTeethCard()
        : base(0, CardType.Skill, CardRarity.Event, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var eyeWithTeeth = Owner.PlayerCombatState?.GetPet<EyeWithTeethPet>();
        if (eyeWithTeeth != null && !eyeWithTeeth.IsDead)
        {
            MainFile.Logger.Info("Triggering Eye With Teeth attack animation from Toothy Stare.");
            await CreatureCmd.TriggerAnim(eyeWithTeeth, "Attack", 0.5f);
        }

        await CreatureCmd.GainBlock(Owner.Creature, 6m, BlockProps.card, cardPlay, false);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class GremlinMercCard : CustomCardModel
{
    public override CardPoolModel Pool => ModelDb.Card<ByrdSwoop>().Pool;

    public override CardPoolModel VisualCardPool => Pool;

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override Texture2D? CustomPortrait => ModTextureLoader.Load("card_gremlin_merc.png");

    public override List<(string, string)> Localization =>
    [
        ("title", "Mercenary Feint"),
        ("description", "Deal 10 damage. If this kills an enemy, gain 10 Gold. Exhaust."),
        ("flavor", "Cheap work, but enthusiastic.")
    ];

    public GremlinMercCard()
        : base(1, CardType.Attack, CardRarity.Event, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        var gremlinMerc = Owner.PlayerCombatState?.GetPet<GremlinMercPet>();
        if (gremlinMerc != null && !gremlinMerc.IsDead)
        {
            MainFile.Logger.Info("Triggering Gremlin Merc attack animation from Mercenary Feint.");
            await CreatureCmd.TriggerAnim(gremlinMerc, "Attack", 0.5f);
        }

        await CreatureCmd.Damage(choiceContext, cardPlay.Target, 10m, DamageProps.card, Owner.Creature, this);
        if (!cardPlay.Target.IsAlive)
        {
            await PlayerCmd.GainGold(10m, Owner, false);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class ThievingHopperCard : CustomCardModel
{
    public override CardPoolModel Pool => ModelDb.Card<ByrdSwoop>().Pool;

    public override CardPoolModel VisualCardPool => Pool;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, DamageProps.card),
        new IfUpgradedVar("IfUpgraded", 0m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override Texture2D? CustomPortrait => ModTextureLoader.Load("card_thieving_hopper.png");

    public override List<(string, string)> Localization =>
    [
        ("title", "Hopper Jab"),
        ("description", "Deal {IfUpgraded:show:10|6} damage. If this kills an enemy, add a random card to your deck permanently. Exhaust."),
        ("flavor", "It darts in before anyone can check their pockets.")
    ];

    public ThievingHopperCard()
        : base(1, CardType.Attack, CardRarity.Event, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        var thievingHopper = Owner.PlayerCombatState?.GetPet<ThievingHopperPet>();
        if (thievingHopper != null && !thievingHopper.IsDead)
        {
            MainFile.Logger.Info("Triggering Thieving Hopper attack animation from Hopper Jab.");
            await CreatureCmd.TriggerAnim(thievingHopper, "Attack", 0.5f);
        }

        await CreatureCmd.Damage(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue, DamageProps.card, Owner.Creature, this);
        if (!cardPlay.Target.IsAlive)
        {
            CardModel randomCard = ModelDb.AllCards
                .Where(card => card.CanBeGeneratedInCombat && card.ShouldShowInCardLibrary)
                .OrderBy(_ => Guid.NewGuid())
                .First()
                .ToMutable();

            CardModel deckCard = Owner.RunState.CreateCard(randomCard, Owner);
            await CardPileCmd.Add(deckCard, PileType.Deck);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
