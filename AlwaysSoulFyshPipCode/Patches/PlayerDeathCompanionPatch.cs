using System.Threading.Tasks;
using NeowCompanions.NeowCompanionsCode.Models;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace NeowCompanions.NeowCompanionsCode.Patches;

[HarmonyPatch(typeof(Creature), nameof(Creature.InvokeDiedEvent))]
public static class PlayerDeathCompanionPatch
{
    public static void Postfix(Creature __instance)
    {
        if (!__instance.IsPlayer || __instance.Player?.PlayerCombatState == null)
        {
            return;
        }

        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<SoulFyshPipPet>(), "Soul Fysh Pip");
        TriggerWrigglerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<WrigglerPet>());
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<CeremonialBeastPet>(), "Ceremonial Beast");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<KinFollowerPet>(), "Kin Follower");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<EyeWithTeethPet>(), "Eye With Teeth");
        TriggerGremlinMercDeathAnimation(__instance.Player);
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<ThievingHopperPet>(), "Thieving Hopper");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<AeonglassPet>(), "Aeonglass");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<LagavulinMatriarchPet>(), "Lagavulin Matriarch");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<TheKinPet>(), "The Kin");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<WaterfallGiantPet>(), "Waterfall Giant");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<VantomPet>(), "Vantom");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<KnowledgeDemonPet>(), "Knowledge Demon");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<TheInsatiablePet>(), "The Insatiable");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<QueenPet>(), "Queen");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<TestSubjectPet>(), "Test Subject");
    }

    private static void TriggerDeathAnimation(Creature? pet, string companionName)
    {
        if (pet == null || pet.IsDead)
        {
            return;
        }

        MainFile.Logger.Info($"Triggering {companionName} death animation.");
        _ = CreatureCmd.TriggerAnim(pet, "Dead", 1f);
    }

    private static void TriggerWrigglerDeathAnimation(Creature? wriggler)
    {
        if (wriggler == null || wriggler.IsDead)
        {
            return;
        }

        MainFile.Logger.Info("Triggering Wriggler death animation.");
        _ = TriggerFirstAvailableWrigglerDeathAnimation(wriggler);
    }

    private static async Task TriggerFirstAvailableWrigglerDeathAnimation(Creature wriggler)
    {
        foreach (string animationName in new[] { "Dead", "Death", "Die" })
        {
            try
            {
                await CreatureCmd.TriggerAnim(wriggler, animationName, 0.35f);
                return;
            }
            catch
            {
                MainFile.Logger.Info($"Wriggler did not have death animation '{animationName}'.");
            }
        }
    }

    private static void TriggerGremlinMercDeathAnimation(Player owner)
    {
        Creature? gremlinMerc = owner.PlayerCombatState?.GetPet<GremlinMercPet>();
        if (gremlinMerc == null || gremlinMerc.IsDead)
        {
            return;
        }

        MainFile.Logger.Info("Triggering Gremlin Merc split death animation.");
        _ = TriggerGremlinMercSplitSequence(owner, gremlinMerc);
    }

    private static async Task TriggerGremlinMercSplitSequence(Player owner, Creature gremlinMerc)
    {
        await CreatureCmd.TriggerAnim(gremlinMerc, "Dead", 0.55f);
        GremlinMercSplitVisuals.Show();

        Creature? fatGremlin = owner.PlayerCombatState?.GetPet<FatGremlinSplitPet>();
        Creature? sneakyGremlin = owner.PlayerCombatState?.GetPet<SneakyGremlinSplitPet>();

        if (fatGremlin != null && !fatGremlin.IsDead)
        {
            MainFile.Logger.Info("Triggering Fat Gremlin split escape animation.");
            _ = CreatureCmd.TriggerAnim(fatGremlin, "Run", 0.5f);
            await GremlinMercSplitVisuals.AnimateFatGremlinEscape();
            await CreatureCmd.Escape(fatGremlin, false);
        }

        if (sneakyGremlin != null && !sneakyGremlin.IsDead)
        {
            MainFile.Logger.Info("Triggering Sneaky Gremlin split death animation.");
            _ = CreatureCmd.TriggerAnim(sneakyGremlin, "Dead", 0.8f);
        }
    }
}
