using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace NeowCompanions.NeowCompanionsCode.Models;

public static class GremlinMercSplitVisuals
{
    private const int EscapeSteps = 30;
    private const float EscapeDistance = -850f;

    private static NCreatureVisuals? fatGremlin;
    private static NCreatureVisuals? sneakyGremlin;

    public static void RegisterFatGremlin(NCreatureVisuals visuals)
    {
        fatGremlin = visuals;
        visuals.Visible = false;
    }

    public static void RegisterSneakyGremlin(NCreatureVisuals visuals)
    {
        sneakyGremlin = visuals;
        visuals.Visible = false;
    }

    public static void Show()
    {
        if (fatGremlin != null)
        {
            fatGremlin.Visible = true;
        }

        if (sneakyGremlin != null)
        {
            sneakyGremlin.Visible = true;
        }
    }

    public static async Task AnimateFatGremlinEscape()
    {
        if (fatGremlin == null)
        {
            return;
        }

        fatGremlin.Visible = true;
        Godot.Vector2 startPosition = fatGremlin.Position;
        for (int step = 1; step <= EscapeSteps; step++)
        {
            float progress = step / (float)EscapeSteps;
            fatGremlin.Position = startPosition + new Godot.Vector2(EscapeDistance * progress, 0f);
            await Task.Delay(16);
        }

        fatGremlin.Visible = false;
    }
}

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

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class KinFollowerRelic : CompanionRelicModel
{
    public override string IconFileName => "relic_kin_follower.png";

    public override List<(string, string)> Localization =>
    [
        ("title", "Kin Follower"),
        ("description", "At the start of each combat, summon Kin Follower."),
        ("flavor", "It follows quietly, waiting for a sign.")
    ];

    public override Task BeforeCombatStart()
    {
        return PlayerCmd.AddPet<KinFollowerPet>(Owner);
    }
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class EyeWithTeethRelic : CompanionRelicModel
{
    public override string IconFileName => "relic_eye_with_teeth.png";

    public override List<(string, string)> Localization =>
    [
        ("title", "Eye With Teeth"),
        ("description", "At the start of each combat, summon Eye With Teeth."),
        ("flavor", "It watches the path ahead. It also watches you.")
    ];

    public override Task BeforeCombatStart()
    {
        return PlayerCmd.AddPet<EyeWithTeethPet>(Owner);
    }
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class GremlinMercRelic : CompanionRelicModel
{
    public override string IconFileName => "relic_gremlin_merc.png";

    public override List<(string, string)> Localization =>
    [
        ("title", "Gremlin Merc"),
        ("description", "At the start of each combat, summon Gremlin Merc."),
        ("flavor", "Payment is implied. Loyalty is negotiable.")
    ];

    public override async Task BeforeCombatStart()
    {
        await PlayerCmd.AddPet<GremlinMercPet>(Owner);
        await PlayerCmd.AddPet<FatGremlinSplitPet>(Owner);
        await PlayerCmd.AddPet<SneakyGremlinSplitPet>(Owner);
    }
}

[Pool(typeof(NeowCompanionRelicPool))]
public sealed class ThievingHopperRelic : CompanionRelicModel
{
    public override string IconFileName => "relic_thieving_hopper.png";

    public override List<(string, string)> Localization =>
    [
        ("title", "Thieving Hopper"),
        ("description", "At the start of each combat, summon Thieving Hopper."),
        ("flavor", "A companion with suspiciously full pockets.")
    ];

    public override Task BeforeCombatStart()
    {
        return PlayerCmd.AddPet<ThievingHopperPet>(Owner);
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

public sealed class EyeWithTeethPet : CustomMonsterModel
{
    private const float PetScale = 0.60f;

    public override int MinInitialHp => 9999;

    public override int MaxInitialHp => 9999;

    public override bool IsHealthBarVisible => false;

    public override MegaCrit.Sts2.Core.Nodes.Combat.NCreatureVisuals? CreateCustomVisuals()
    {
        MegaCrit.Sts2.Core.Nodes.Combat.NCreatureVisuals visuals = ModelDb.Monster<EyeWithTeeth>().CreateVisuals();
        visuals.Scale = new Godot.Vector2(-PetScale, PetScale);
        return visuals;
    }

    public override MegaCrit.Sts2.Core.Animation.CreatureAnimator? SetupCustomAnimationStates(
        MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaSprite controller)
    {
        return ModelDb.Monster<EyeWithTeeth>().GenerateAnimator(controller);
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

public sealed class GremlinMercPet : CustomMonsterModel
{
    private const float PetScale = 0.70f;

    public override int MinInitialHp => 9999;

    public override int MaxInitialHp => 9999;

    public override bool IsHealthBarVisible => false;

    public override MegaCrit.Sts2.Core.Nodes.Combat.NCreatureVisuals? CreateCustomVisuals()
    {
        MegaCrit.Sts2.Core.Nodes.Combat.NCreatureVisuals visuals = ModelDb.Monster<GremlinMerc>().CreateVisuals();
        visuals.Scale = new Godot.Vector2(-PetScale, PetScale);
        return visuals;
    }

    public override MegaCrit.Sts2.Core.Animation.CreatureAnimator? SetupCustomAnimationStates(
        MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaSprite controller)
    {
        return ModelDb.Monster<GremlinMerc>().GenerateAnimator(controller);
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

public sealed class FatGremlinSplitPet : CustomMonsterModel
{
    private const float PetScale = 0.70f;

    public override int MinInitialHp => 9999;

    public override int MaxInitialHp => 9999;

    public override bool IsHealthBarVisible => false;

    public override MegaCrit.Sts2.Core.Nodes.Combat.NCreatureVisuals? CreateCustomVisuals()
    {
        MegaCrit.Sts2.Core.Nodes.Combat.NCreatureVisuals visuals = ModelDb.Monster<FatGremlin>().CreateVisuals();
        visuals.Scale = new Godot.Vector2(-PetScale, PetScale);
        GremlinMercSplitVisuals.RegisterFatGremlin(visuals);
        return visuals;
    }

    public override MegaCrit.Sts2.Core.Animation.CreatureAnimator? SetupCustomAnimationStates(
        MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaSprite controller)
    {
        return ModelDb.Monster<FatGremlin>().GenerateAnimator(controller);
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

public sealed class SneakyGremlinSplitPet : CustomMonsterModel
{
    private const float PetScale = 0.70f;

    public override int MinInitialHp => 9999;

    public override int MaxInitialHp => 9999;

    public override bool IsHealthBarVisible => false;

    public override MegaCrit.Sts2.Core.Nodes.Combat.NCreatureVisuals? CreateCustomVisuals()
    {
        MegaCrit.Sts2.Core.Nodes.Combat.NCreatureVisuals visuals = ModelDb.Monster<SneakyGremlin>().CreateVisuals();
        visuals.Scale = new Godot.Vector2(-PetScale, PetScale);
        GremlinMercSplitVisuals.RegisterSneakyGremlin(visuals);
        return visuals;
    }

    public override MegaCrit.Sts2.Core.Animation.CreatureAnimator? SetupCustomAnimationStates(
        MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaSprite controller)
    {
        return ModelDb.Monster<SneakyGremlin>().GenerateAnimator(controller);
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

public sealed class ThievingHopperPet : CustomMonsterModel
{
    private const float PetScale = 0.65f;

    public override int MinInitialHp => 9999;

    public override int MaxInitialHp => 9999;

    public override bool IsHealthBarVisible => false;

    public override MegaCrit.Sts2.Core.Nodes.Combat.NCreatureVisuals? CreateCustomVisuals()
    {
        MegaCrit.Sts2.Core.Nodes.Combat.NCreatureVisuals visuals = ModelDb.Monster<ThievingHopper>().CreateVisuals();
        visuals.Scale = new Godot.Vector2(-PetScale, PetScale);
        return visuals;
    }

    public override MegaCrit.Sts2.Core.Animation.CreatureAnimator? SetupCustomAnimationStates(
        MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaSprite controller)
    {
        return ModelDb.Monster<ThievingHopper>().GenerateAnimator(controller);
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

public sealed class KinFollowerPet : CustomMonsterModel
{
    private const float PetScale = 0.70f;

    public override int MinInitialHp => 9999;

    public override int MaxInitialHp => 9999;

    public override bool IsHealthBarVisible => false;

    public override MegaCrit.Sts2.Core.Nodes.Combat.NCreatureVisuals? CreateCustomVisuals()
    {
        MegaCrit.Sts2.Core.Nodes.Combat.NCreatureVisuals visuals = ModelDb.Monster<KinFollower>().CreateVisuals();
        visuals.Scale = new Godot.Vector2(-PetScale, PetScale);
        return visuals;
    }

    public override MegaCrit.Sts2.Core.Animation.CreatureAnimator? SetupCustomAnimationStates(
        MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaSprite controller)
    {
        return ModelDb.Monster<KinFollower>().GenerateAnimator(controller);
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
