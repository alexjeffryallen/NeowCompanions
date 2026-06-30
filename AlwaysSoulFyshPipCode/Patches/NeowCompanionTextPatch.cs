using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;

namespace NeowCompanions.NeowCompanionsCode.Patches;

public static class NeowCompanionText
{
    public const string Table = "neow_companions";
    public const string SettingsTable = "settings_ui";

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

            "EYE_WITH_TEETH.title" => "Eye With Teeth",
            "EYE_WITH_TEETH.description" => "Gain Eye With Teeth as your companion.",

            "GREMLIN_MERC.title" => "Gremlin Merc",
            "GREMLIN_MERC.description" => "Gain Gremlin Merc as your companion.",

            "THIEVING_HOPPER.title" => "Thieving Hopper",
            "THIEVING_HOPPER.description" => "Gain Thieving Hopper as your companion.",

            "AEONGLASS.title" => "Aeonglass",
            "AEONGLASS.description" => "Gain Aeonglass as your companion.",

            "LAGAVULIN_MATRIARCH.title" => "Lagavulin Matriarch",
            "LAGAVULIN_MATRIARCH.description" => "Gain Lagavulin Matriarch as your companion.",

            "THE_KIN.title" => "The Kin",
            "THE_KIN.description" => "Gain The Kin as your companion.",

            "WATERFALL_GIANT.title" => "Waterfall Giant",
            "WATERFALL_GIANT.description" => "Gain Waterfall Giant as your companion.",

            "VANTOM.title" => "Vantom",
            "VANTOM.description" => "Gain Vantom as your companion.",

            "KNOWLEDGE_DEMON.title" => "Knowledge Demon",
            "KNOWLEDGE_DEMON.description" => "Gain Knowledge Demon as your companion.",

            "THE_INSATIABLE.title" => "The Insatiable",
            "THE_INSATIABLE.description" => "Gain The Insatiable as your companion.",

            "QUEEN.title" => "Queen",
            "QUEEN.description" => "Gain Queen as your companion.",

            "TEST_SUBJECT.title" => "Test Subject",
            "TEST_SUBJECT.description" => "Gain Test Subject as your companion.",

            _ => key
        };
    }

    public static bool TryGetSettingsText(string key, out string text)
    {
        string normalizedKey = key.ToUpperInvariant();

        if (normalizedKey.Contains("OFFER_ALL_COMPANIONS"))
        {
            text = normalizedKey.EndsWith(".HOVER.DESC")
                ? "Show every companion in the pool instead of choosing three at random."
                : "Offer all companions";
            return true;
        }

        if (normalizedKey.Contains("GRANT_COMPANION_CARDS"))
        {
            text = normalizedKey.EndsWith(".HOVER.DESC")
                ? "When disabled, companion choices grant only the companion relic and pet, without adding the companion card to your deck."
                : "Grant companion cards";
            return true;
        }

        text = key;
        return false;
    }
}

[HarmonyPatch(typeof(LocString), nameof(LocString.GetFormattedText))]
public static class NeowCompanionFormattedTextPatch
{
    public static bool Prefix(LocString __instance, ref string __result)
    {
        if (__instance.LocTable == NeowCompanionText.SettingsTable
            && NeowCompanionText.TryGetSettingsText(__instance.LocEntryKey, out string settingsText))
        {
            __result = settingsText;
            return false;
        }

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
        if (__instance.LocTable == NeowCompanionText.SettingsTable
            && NeowCompanionText.TryGetSettingsText(__instance.LocEntryKey, out string settingsText))
        {
            __result = settingsText;
            return false;
        }

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
        if (__instance.LocTable == NeowCompanionText.SettingsTable
            && NeowCompanionText.TryGetSettingsText(__instance.LocEntryKey, out _))
        {
            __result = true;
            return false;
        }

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
        if (table == NeowCompanionText.SettingsTable
            && NeowCompanionText.TryGetSettingsText(key, out _))
        {
            __result = true;
            return false;
        }

        if (table != NeowCompanionText.Table)
        {
            return true;
        }

        __result = true;
        return false;
    }
}
