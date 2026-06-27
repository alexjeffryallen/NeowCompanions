using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace AlwaysSoulFyshPip.AlwaysSoulFyshPipCode.Models;

public abstract class CompanionRelicModel : RelicModel, ICustomModel, ILocalizationProvider
{
    public override bool AddsPet => true;

    public override bool SpawnsPets => true;

    public override bool HasUponPickupEffect => true;

    public override RelicRarity Rarity => RelicRarity.Starter;

    public abstract string IconFileName { get; }

    public abstract List<(string, string)> Localization { get; }
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class SoulFyshPipRelic : CompanionRelicModel
{
    public override string IconFileName => "relic_soul_fysh_pip.png";

    public override List<(string, string)> Localization =>
    [
        ("title", "Soul Fysh Pip"),
        ("description", "At the start of each combat, summon Soul Fysh Pip."),
        ("flavor", "A tiny echo of the deep answers Neow's call.")
    ];

    public override Task BeforeCombatStart()
    {
        return PlayerCmd.AddPet<SoulFyshPipPet>(Owner);
    }
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class WrigglerRelic : CompanionRelicModel
{
    public override string IconFileName => "relic_wriggler.png";

    public override List<(string, string)> Localization =>
    [
        ("title", "Wriggler"),
        ("description", "At the start of each combat, summon Wriggler."),
        ("flavor", "Small, determined, and already chewing on destiny.")
    ];

    public override Task BeforeCombatStart()
    {
        return PlayerCmd.AddPet<WrigglerPet>(Owner);
    }
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class CeremonialBeastRelic : CompanionRelicModel
{
    public override string IconFileName => "relic_ceremonial_beast.png";

    public override List<(string, string)> Localization =>
    [
        ("title", "Ceremonial Beast"),
        ("description", "At the start of each combat, summon Ceremonial Beast."),
        ("flavor", "A solemn bell tolls from somewhere behind the veil.")
    ];

    public override Task BeforeCombatStart()
    {
        return PlayerCmd.AddPet<CeremonialBeastPet>(Owner);
    }
}

public sealed class WrigglerPet : CustomMonsterModel
{
    private const float PetScale = 0.90f;

    public override int MinInitialHp => 9999;

    public override int MaxInitialHp => 9999;

    public override bool IsHealthBarVisible => false;

    public override MegaCrit.Sts2.Core.Nodes.Combat.NCreatureVisuals? CreateCustomVisuals()
    {
        MegaCrit.Sts2.Core.Nodes.Combat.NCreatureVisuals visuals = ModelDb.Monster<Wriggler>().CreateVisuals();
        visuals.Scale = new Godot.Vector2(-PetScale, PetScale);
        return visuals;
    }

    public override MegaCrit.Sts2.Core.Animation.CreatureAnimator? SetupCustomAnimationStates(
        MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaSprite controller)
    {
        return ModelDb.Monster<Wriggler>().GenerateAnimator(controller);
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

public sealed class CeremonialBeastPet : CustomMonsterModel
{
    private const float PetScale = 0.35f;

    public override int MinInitialHp => 9999;

    public override int MaxInitialHp => 9999;

    public override bool IsHealthBarVisible => false;

    public override bool ShouldFadeAfterDeath => false;

    public override MegaCrit.Sts2.Core.Nodes.Combat.NCreatureVisuals? CreateCustomVisuals()
    {
        MegaCrit.Sts2.Core.Nodes.Combat.NCreatureVisuals visuals = ModelDb.Monster<CeremonialBeast>().CreateVisuals();
        visuals.Scale = new Godot.Vector2(-PetScale, PetScale);
        return visuals;
    }

    public override MegaCrit.Sts2.Core.Animation.CreatureAnimator? SetupCustomAnimationStates(
        MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaSprite controller)
    {
        return ModelDb.Monster<CeremonialBeast>().GenerateAnimator(controller);
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
