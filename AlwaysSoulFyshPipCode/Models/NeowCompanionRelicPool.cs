using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Models;

namespace AlwaysSoulFyshPip.AlwaysSoulFyshPipCode.Models;

public sealed class NeowCompanionRelicPool : CustomRelicPoolModel
{
    public override string Title => "NeowCompanions";

    public override bool IsShared => false;

    public override Color RelicBackgroundColor => new("1E2A33");

    public override bool SeenByDefault => true;

    protected override IEnumerable<RelicModel> GenerateAllRelics()
    {
        return
        [
            ModelDb.Relic<SoulFyshPipRelic>(),
            ModelDb.Relic<WrigglerRelic>(),
            ModelDb.Relic<CeremonialBeastRelic>()
        ];
    }
}
