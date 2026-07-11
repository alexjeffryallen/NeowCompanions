using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using NeowCompanions.NeowCompanionsCode.Assets;
using NeowCompanions.NeowCompanionsCode.Models;

namespace NeowCompanions.NeowCompanionsCode.Patches;

[HarmonyPatch]
public static class PowerIconPatch
{
    [HarmonyPatch(typeof(PowerModel), nameof(PowerModel.Icon), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool PowerIconPrefix(PowerModel __instance, ref Texture2D __result)
    {
        if (GetPowerIconFile(__instance) is { } iconFile && ModTextureLoader.Load(iconFile) is { } icon)
        {
            __result = icon;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(PowerModel), nameof(PowerModel.BigIcon), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool PowerBigIconPrefix(PowerModel __instance, ref Texture2D __result)
    {
        if (GetPowerIconFile(__instance) is { } iconFile && ModTextureLoader.Load(iconFile) is { } icon)
        {
            __result = icon;
            return false;
        }

        return true;
    }

    private static string? GetPowerIconFile(PowerModel power)
    {
        return power switch
        {
            KnowledgeDemonDrawPower => "relic_knowledge_demon.png",
            RustcladBuffUpPower => "relic_rustclad.png",
            GlitchlingOrbitPower => "relic_glitchling.png",
            BonebinderDoombindPower => "relic_bonebinder.png",
            WaterfallGiantDelayedPower => "relic_waterfall_giant.png",
            WaterfallGiantRandomDelayedPower => "relic_waterfall_giant.png",
            LagavulinMatriarchDrainPower => "relic_lagavulin_matriarch.png",
            TestSubjectLastTurnDamagePower => "relic_test_subject.png",
            _ => null,
        };
    }
}
