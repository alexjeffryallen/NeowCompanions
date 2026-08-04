using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace NeowCompanions.NeowCompanionsCode.Patches;

public static class NeowCompanionText
{
    public const string Table = "neow_companions";
    public const string SettingsTable = "settings_ui";
    private static readonly Dictionary<string, string> SettingsOverrides = new(StringComparer.OrdinalIgnoreCase);

    public static void RegisterSettingsText(string key, string text)
    {
        SettingsOverrides[key] = text;
    }

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

            "SEAPUNK.title" => "Seapunk",
            "SEAPUNK.description" => "Gain Seapunk as your companion.",

            "SHRINKER_BEETLE.title" => "Shrinker Beetle",
            "SHRINKER_BEETLE.description" => "Gain Shrinker Beetle as your companion.",

            "OPEROSIS.title" => "Operosis",
            "OPEROSIS.description" => "Gain Operosis as your companion.",

            "ARCHITECT.title" => "The Architect",
            "ARCHITECT.description" => "Gain The Architect as your companion.",

            "RUSTCLAD.title" => "Rustclad",
            "RUSTCLAD.description" => "Gain Rustclad as your companion.",

            "SHADELEAF.title" => "Shadeleaf",
            "SHADELEAF.description" => "Gain Shadeleaf as your companion.",

            "GLITCHLING.title" => "Glitchling",
            "GLITCHLING.description" => "Gain Glitchling as your companion.",

            "BONEBINDER.title" => "Bonebinder",
            "BONEBINDER.description" => "Gain Bonebinder as your companion.",

            "GILDED_PAGE.title" => "Gilded Page",
            "GILDED_PAGE.description" => "Gain Gilded Page as your companion.",

            "EMBER_PIP.title" => "Ember Pip",
            "EMBER_PIP.description" => "Gain Ember Pip as your companion.",

            "FROST_PIP.title" => "Frost Pip",
            "FROST_PIP.description" => "Gain Frost Pip as your companion.",

            "STORM_PIP.title" => "Storm Pip",
            "STORM_PIP.description" => "Gain Storm Pip as your companion.",

            "THORN_PIP.title" => "Thorn Pip",
            "THORN_PIP.description" => "Gain Thorn Pip as your companion.",

            "KAISER_CRAB.title" => "Kaiser Crab",
            "KAISER_CRAB.description" => "Gain Kaiser Crab as your companion.",

            "BYGONE_EFFIGY.title" => "Bygone Effigy",
            "BYGONE_EFFIGY.description" => "Gain Bygone Effigy as your companion.",

            "BYRDONIS.title" => "Byrdonis",
            "BYRDONIS.description" => "Gain Byrdonis as your companion.",

            "PHROG_PARASITE.title" => "Phrog Parasite",
            "PHROG_PARASITE.description" => "Gain Phrog Parasite as your companion.",

            "SKULKING_COLONY.title" => "Skulking Colony",
            "SKULKING_COLONY.description" => "Gain Skulking Colony as your companion.",

            "PHANTASMAL_GARDENER.title" => "Phantasmal Gardener",
            "PHANTASMAL_GARDENER.description" => "Gain Phantasmal Gardener as your companion.",

            "TERROR_EEL.title" => "Terror Eel",
            "TERROR_EEL.description" => "Gain Terror Eel as your companion.",

            "DECIMILLIPEDE.title" => "Decimillipede",
            "DECIMILLIPEDE.description" => "Gain Decimillipede as your companion.",

            "ENTOMANCER.title" => "Entomancer",
            "ENTOMANCER.description" => "Gain Entomancer as your companion.",

            "INFESTED_PRISM.title" => "Infested Prism",
            "INFESTED_PRISM.description" => "Gain Infested Prism as your companion.",

            "KNIGHT_GANG.title" => "Knight Gang",
            "KNIGHT_GANG.description" => "Gain the Spectral Knight of the Knight Gang as your companion.",

            "MECHA_KNIGHT.title" => "Mecha Knight",
            "MECHA_KNIGHT.description" => "Gain Mecha Knight as your companion.",

            "SOUL_NEXUS.title" => "Soul Nexus",
            "SOUL_NEXUS.description" => "Gain Soul Nexus as your companion.",

            "ASSASSIN_RUBY_RAIDER.title" => "Assassin Ruby Raider",
            "ASSASSIN_RUBY_RAIDER.description" => "Gain Assassin Ruby Raider as your companion.",
            "AXE_RUBY_RAIDER.title" => "Axe Ruby Raider",
            "AXE_RUBY_RAIDER.description" => "Gain Axe Ruby Raider as your companion.",
            "BRUTE_RUBY_RAIDER.title" => "Brute Ruby Raider",
            "BRUTE_RUBY_RAIDER.description" => "Gain Brute Ruby Raider as your companion.",
            "CROSSBOW_RUBY_RAIDER.title" => "Crossbow Ruby Raider",
            "CROSSBOW_RUBY_RAIDER.description" => "Gain Crossbow Ruby Raider as your companion.",
            "FLYCONID.title" => "Flyconid",
            "FLYCONID.description" => "Gain Flyconid as your companion.",
            "FOGMOG.title" => "Fogmog",
            "FOGMOG.description" => "Gain Fogmog as your companion.",
            "MAWLER.title" => "Mawler",
            "MAWLER.description" => "Gain Mawler as your companion.",
            "FUZZY_WURM_CRAWLER.title" => "Fuzzy Wurm Crawler",
            "FUZZY_WURM_CRAWLER.description" => "Gain Fuzzy Wurm Crawler as your companion.",
            "INKLET.title" => "Inklet",
            "INKLET.description" => "Gain Inklet as your companion.",
            "SNAPPING_JAXFRUIT.title" => "Snapping Jaxfruit",
            "SNAPPING_JAXFRUIT.description" => "Gain Snapping Jaxfruit as your companion.",
            "SLITHERING_STRANGLER.title" => "Slithering Strangler",
            "SLITHERING_STRANGLER.description" => "Gain Slithering Strangler as your companion.",
            "LEAF_SLIME_S.title" => "Small Leaf Slime",
            "LEAF_SLIME_S.description" => "Gain Small Leaf Slime as your companion.",
            "LEAF_SLIME_M.title" => "Medium Leaf Slime",
            "LEAF_SLIME_M.description" => "Gain Medium Leaf Slime as your companion.",
            "TWIG_SLIME_S.title" => "Small Twig Slime",
            "TWIG_SLIME_S.description" => "Gain Small Twig Slime as your companion.",
            "TWIG_SLIME_M.title" => "Medium Twig Slime",
            "TWIG_SLIME_M.description" => "Gain Medium Twig Slime as your companion.",
            "VINE_SHAMBLER.title" => "Vine Shambler",
            "VINE_SHAMBLER.description" => "Gain Vine Shambler as your companion.",
            "CHOMPER.title" => "Chomper",
            "CHOMPER.description" => "Gain Chomper as your companion.",
            "CUBEX_CONSTRUCT.title" => "Cubex Construct",
            "CUBEX_CONSTRUCT.description" => "Gain Cubex Construct as your companion.",
            "DAMP_CULTIST.title" => "Damp Cultist",
            "DAMP_CULTIST.description" => "Gain Damp Cultist as your companion.",
            "CALCIFIED_CULTIST.title" => "Calcified Cultist",
            "CALCIFIED_CULTIST.description" => "Gain Calcified Cultist as your companion.",

            "CORPSE_SLUG.title" => "Corpse Slug",
            "CORPSE_SLUG.description" => "Gain Corpse Slug as your companion.",
            "TWO_TAILED_RAT.title" => "Two-Tailed Rat",
            "TWO_TAILED_RAT.description" => "Gain Two-Tailed Rat as your companion.",
            "SEWER_CLAM.title" => "Sewer Clam",
            "SEWER_CLAM.description" => "Gain Sewer Clam as your companion.",
            "HAUNTED_SHIP.title" => "Haunted Ship",
            "HAUNTED_SHIP.description" => "Gain Haunted Ship as your companion.",
            "SLUDGE_SPINNER.title" => "Sludge Spinner",
            "SLUDGE_SPINNER.description" => "Gain Sludge Spinner as your companion.",
            "PUNCH_CONSTRUCT.title" => "Punch Construct",
            "PUNCH_CONSTRUCT.description" => "Gain Punch Construct as your companion.",
            "FOSSIL_STALKER.title" => "Fossil Stalker",
            "FOSSIL_STALKER.description" => "Gain Fossil Stalker as your companion.",
            "LIVING_FOG.title" => "Living Fog",
            "LIVING_FOG.description" => "Gain Living Fog as your companion.",
            "PARAFRIGHT.title" => "Parafright",
            "PARAFRIGHT.description" => "Gain Parafright as your companion.",
            "TUNNELER.title" => "Tunneler",
            "TUNNELER.description" => "Gain Tunneler as your companion.",
            "SPINY_TOAD.title" => "Spiny Toad",
            "SPINY_TOAD.description" => "Gain Spiny Toad as your companion.",
            "STABBOT.title" => "Stabbot",
            "STABBOT.description" => "Gain Stabbot as your companion.",
            "HUNTER_KILLER.title" => "Hunter Killer",
            "HUNTER_KILLER.description" => "Gain Hunter Killer as your companion.",
            "TORCH_HEAD_AMALGAM.title" => "Torch Head Amalgam",
            "TORCH_HEAD_AMALGAM.description" => "Gain Torch Head Amalgam as your companion.",
            "BOWLBUG_EGG.title" => "Egg Bowlbug",
            "BOWLBUG_EGG.description" => "Gain Egg Bowlbug as your companion.",
            "BOWLBUG_NECTAR.title" => "Nectar Bowlbug",
            "BOWLBUG_NECTAR.description" => "Gain Nectar Bowlbug as your companion.",
            "BOWLBUG_ROCK.title" => "Rock Bowlbug",
            "BOWLBUG_ROCK.description" => "Gain Rock Bowlbug as your companion.",
            "BOWLBUG_SILK.title" => "Silk Bowlbug",
            "BOWLBUG_SILK.description" => "Gain Silk Bowlbug as your companion.",
            "LOUSE_PROGENITOR.title" => "Louse Progenitor",
            "LOUSE_PROGENITOR.description" => "Gain Louse Progenitor as your companion.",
            "SLUMBERING_BEETLE.title" => "Slumbering Beetle",
            "SLUMBERING_BEETLE.description" => "Gain Slumbering Beetle as your companion.",

            "SHADELEAF.dialogue.0" => "...",
            "SHADELEAF.dialogue.1" => "Too slow.",
            "SHADELEAF.dialogue.2" => "Watch closely.",

            _ => GetGeneratedCompanionText(key)
        };
    }

    private static string GetGeneratedCompanionText(string key)
    {
        int separator = key.LastIndexOf('.');
        if (separator <= 0)
            return key;

        string suffix = key[(separator + 1)..];
        if (suffix is not ("title" or "description"))
            return key;

        string name = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(
            key[..separator].Replace('_', ' ').ToLowerInvariant());
        name = name.Replace(" V1", " V1").Replace(" V2", " V2").Replace(" V3", " V3");
        return suffix == "title" ? name : $"Gain {name} as your companion.";
    }

    public static bool TryGetSettingsText(string key, out string text)
    {
        if (SettingsOverrides.TryGetValue(key, out text!))
            return true;

        string normalizedKey = key.ToUpperInvariant();

        if (normalizedKey.Contains("OFFER_ALL_COMPANIONS"))
        {
            text = normalizedKey.EndsWith(".HOVER.DESC")
                ? "Show every available companion in the current act's pool instead of choosing three at random."
                : "All companion options";
            return true;
        }

        if (normalizedKey.Contains("FULL_STARTING_COMPANION_POOL"))
        {
            text = normalizedKey.EndsWith(".HOVER.DESC")
                ? "Use every companion in the game as the eligible pool for the starting Neow companion choice."
                : "Full starting companion pool";
            return true;
        }

        if (normalizedKey.Contains("RANDOM_COMPANION_NO_CHOICES"))
        {
            text = normalizedKey.EndsWith(".HOVER.DESC")
                ? "When enabled, choosing a Neow option grants one random companion immediately instead of showing companion choices."
                : "Random companion, no choices";
            return true;
        }

        if (normalizedKey.Contains("GRANT_COMPANION_CARDS"))
        {
            text = normalizedKey.EndsWith(".HOVER.DESC")
                ? "When disabled, companion choices grant only the companion relic and pet, without adding the companion card to your deck."
                : "Grant companion cards";
            return true;
        }

        if (normalizedKey.Contains("GRANT_UPGRADED_COMPANION_CARDS"))
        {
            text = normalizedKey.EndsWith(".HOVER.DESC")
                ? "When enabled, companion cards are upgraded before being added to your deck."
                : "Grant upgraded companion cards";
            return true;
        }

        if (normalizedKey.Contains("OFFER_COMPANIONS_AT_EVERY_ANCIENT"))
        {
            text = normalizedKey.EndsWith(".HOVER.DESC")
                ? "When enabled, every Ancient encounter can grant a new companion, allowing several companions in one run."
                : "Companions at every Ancient";
            return true;
        }

        if (normalizedKey.Contains("CHOOSE_MULTIPLE_COMPANIONS_AT_ANCIENT"))
        {
            text = normalizedKey.EndsWith(".HOVER.DESC")
                ? "When enabled, companion choice screens stay open after each pick so you can take as many available companions as you want."
                : "Choose multiple companions";
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
