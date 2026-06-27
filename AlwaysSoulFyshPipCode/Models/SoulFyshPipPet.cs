using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace AlwaysSoulFyshPip.AlwaysSoulFyshPipCode.Models;

public sealed class SoulFyshPipPet : CustomMonsterModel
{
    private const float PetScale = 0.35f;

    public override int MinInitialHp => 9999;

    public override int MaxInitialHp => 9999;

    public override bool IsHealthBarVisible => false;

    public override NCreatureVisuals? CreateCustomVisuals()
    {
        NCreatureVisuals visuals = ModelDb.Monster<SoulFysh>().CreateVisuals();
        visuals.Scale = new Vector2(-PetScale, PetScale);
        return visuals;
    }

    public override CreatureAnimator? SetupCustomAnimationStates(MegaSprite controller)
    {
        return ModelDb.Monster<SoulFysh>().GenerateAnimator(controller);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> states = [];
        MoveState idle = new("NOTHING_MOVE", (IReadOnlyList<Creature> _) => Task.CompletedTask);

        idle.FollowUpState = idle;
        states.Add(idle);

        return new MonsterMoveStateMachine(states, idle);
    }
}
