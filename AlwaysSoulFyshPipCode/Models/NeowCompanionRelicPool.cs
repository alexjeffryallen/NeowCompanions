using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;

namespace NeowCompanions.NeowCompanionsCode.Models;

public sealed class NeowCompanionRelicPool : CustomRelicPoolModel
{
    public override bool IsShared => true;

    public override bool SeenByDefault => true;

    protected override IEnumerable<RelicModel> GenerateAllRelics()
    {
        return
        [
            ModelDb.Relic<SoulFyshPipRelic>(),
            ModelDb.Relic<WrigglerRelic>(),
            ModelDb.Relic<CeremonialBeastRelic>(),
            ModelDb.Relic<KinFollowerRelic>()
        ];
    }
}
