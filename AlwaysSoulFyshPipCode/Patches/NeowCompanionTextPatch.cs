using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;

namespace NeowCompanions.NeowCompanionsCode.Patches;

public static class NeowCompanionText
{
    public const string Table = "neow_companions";

    public static string GetText(string key)
    {
        return key switch
        {
            "CHOOSE_COMPANION.title" => "Choose.... companion....",
            "CHOOSE_COMPANION.description" => "Choose.... companion....",

            "BYRDPIP.title" => "Byrdpip",
            "BYRDPIP.description" => "Gain Byrdpip as your companion.",

            "SOUL_FYSH.title" => "Soul Fysh Pip",
            "SOUL_FYSH.description" => "Gain Soul Fysh Pip as your companion.",

            "WRIGGLER.title" => "Wriggler",
            "WRIGGLER.description" => "Gain Wriggler as your companion.",

            "CEREMONIAL_BEAST.title" => "Ceremonial Beast",
            "CEREMONIAL_BEAST.description" => "Gain Ceremonial Beast as your companion.",

            "KIN_FOLLOWER.title" => "Kin Follower",
            "KIN_FOLLOWER.description" => "Gain Kin Follower as your companion.",

            _ => key
        };
    }
}

[HarmonyPatch(typeof(LocString), nameof(LocString.GetFormattedText))]
public static class NeowCompanionFormattedTextPatch
{
    public static bool Prefix(LocString __instance, ref string __result)
    {
        if (__instance.LocTable != NeowCompanionText.Table)
        {
            return true;
        }

        __result = NeowCompanionText.GetText(__instance.LocEntryKey);
        return false;
    }
}

[HarmonyPatch(typeof(LocString), nameof(LocString.GetRawText))]
public static class NeowCompanionRawTextPatch
{
    public static bool Prefix(LocString __instance, ref string __result)
    {
        if (__instance.LocTable != NeowCompanionText.Table)
        {
            return true;
        }

        __result = NeowCompanionText.GetText(__instance.LocEntryKey);
        return false;
    }
}

[HarmonyPatch(typeof(LocString), nameof(LocString.Exists), [])]
public static class NeowCompanionInstanceExistsPatch
{
    public static bool Prefix(LocString __instance, ref bool __result)
    {
        if (__instance.LocTable != NeowCompanionText.Table)
        {
            return true;
        }

        __result = true;
        return false;
    }
}

[HarmonyPatch(typeof(LocString), nameof(LocString.Exists), typeof(string), typeof(string))]
public static class NeowCompanionStaticExistsPatch
{
    public static bool Prefix(string table, string key, ref bool __result)
    {
        if (table != NeowCompanionText.Table)
        {
            return true;
        }

        __result = true;
        return false;
    }
}
