using System;
using NeowCompanions.NeowCompanionsCode.Assets;
using NeowCompanions.NeowCompanionsCode.Models;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Relics;

namespace NeowCompanions.NeowCompanionsCode.Patches;

[HarmonyPatch]
public static class ArtPatch
{
    [HarmonyPatch(typeof(RelicModel), nameof(RelicModel.Icon), MethodType.Getter)]
    [HarmonyPostfix]
    public static void RelicIconPostfix(RelicModel __instance, ref Texture2D __result)
    {
        if (GetCompanionIconFile(__instance) is { } iconFile && ModTextureLoader.Load(iconFile) is { } icon)
        {
            __result = icon;
        }
    }

    [HarmonyPatch(typeof(RelicModel), nameof(RelicModel.IconOutline), MethodType.Getter)]
    [HarmonyPostfix]
    public static void RelicIconOutlinePostfix(RelicModel __instance, ref Texture2D __result)
    {
        if (GetCompanionIconFile(__instance) is { } iconFile && ModTextureLoader.Load(iconFile) is { } icon)
        {
            __result = icon;
        }
    }

    [HarmonyPatch(typeof(RelicModel), nameof(RelicModel.BigIcon), MethodType.Getter)]
    [HarmonyPostfix]
    public static void RelicBigIconPostfix(RelicModel __instance, ref Texture2D __result)
    {
        if (GetCompanionIconFile(__instance) is { } iconFile && ModTextureLoader.Load(iconFile) is { } icon)
        {
            __result = icon;
        }
    }

    [HarmonyPatch(typeof(NRelic), "Reload")]
    [HarmonyPostfix]
    public static void NRelicReloadPostfix(NRelic __instance)
    {
        RelicModel relic;
        try
        {
            relic = __instance.Model;
        }
        catch (InvalidOperationException)
        {
            return;
        }

        if (GetCompanionIconFile(relic) is not { } iconFile || ModTextureLoader.Load(iconFile) is not { } icon)
        {
            return;
        }

        __instance.Icon.Texture = icon;
        __instance.Outline.Texture = icon;
        MainFile.Logger.Info("Applied companion icon to NRelic.");
    }

    private static string? GetCompanionIconFile(RelicModel relic)
    {
        return relic is CompanionRelicModel companionRelic ? companionRelic.IconFileName : null;
    }
}
