using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlwaysSoulFyshPip.AlwaysSoulFyshPipCode.Assets;
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

namespace AlwaysSoulFyshPip.AlwaysSoulFyshPipCode.Models;

[Pool(typeof(NeowCompanionCardPool))]
public sealed class FyshSwoop : CustomCardModel
{
    public override CardPoolModel Pool => ModelDb.Card<ByrdSwoop>().Pool;

    public override CardPoolModel VisualCardPool => Pool;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<VulnerablePower>(1m),
        new PowerVar<WeakPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override Texture2D? CustomPortrait => ModTextureLoader.Load("card_fysh_swoop.png");

    public override List<(string, string)> Localization =>
    [
        ("title", "Fysh Swoop"),
        ("description", "Apply 1 Vulnerable and 1 Weak.{IfUpgraded:show: Apply -1 Strength.}"),
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
    }
}

[Pool(typeof(NeowCompanionCardPool))]
public sealed class WrigglerCard : CustomCardModel
{
    public override CardPoolModel Pool => ModelDb.Card<ByrdSwoop>().Pool;

    public override CardPoolModel VisualCardPool => Pool;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<PoisonPower>(3m)
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
        ("description", "Apply 3 Poison to ALL enemies."),
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

    public override List<(string, string)> Localization =>
    [
        ("title", "Ceremonial Toll"),
        ("description", "Gain 1 Max Energy. End your turn."),
        ("flavor", "The bell rings once. The climb answers.")
    ];

    public CeremonialBeastCard()
        : base(2, CardType.Power, CardRarity.Event, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var beast = Owner.PlayerCombatState?.GetPet<CeremonialBeastPet>();
        if (beast != null && !beast.IsDead)
        {
            SfxCmd.Play("event:/sfx/enemy/enemy_attacks/ceremonial_beast/ceremonial_beast_shrill");
            await CreatureCmd.TriggerAnim(beast, "Cast", 0.6f);
        }

        Owner.MaxEnergy += 1;
        PlayerCmd.EndTurn(Owner, canBackOut: false);
        await Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
