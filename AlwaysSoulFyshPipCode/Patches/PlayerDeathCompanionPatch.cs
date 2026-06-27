using AlwaysSoulFyshPip.AlwaysSoulFyshPipCode.Models;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace AlwaysSoulFyshPip.AlwaysSoulFyshPipCode.Patches;

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
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<WrigglerPet>(), "Wriggler");
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
}
