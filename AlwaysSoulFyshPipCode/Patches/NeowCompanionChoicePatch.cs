using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NeowCompanions.NeowCompanionsCode;
using NeowCompanions.NeowCompanionsCode.Assets;
using NeowCompanions.NeowCompanionsCode.Config;
using NeowCompanions.NeowCompanionsCode.Models;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Events;

namespace NeowCompanions.NeowCompanionsCode.Patches;

public static class NeowCompanionChoicePatch
{
    private const int CompanionPageSize = 6;

    private static readonly FieldInfo? EventOptionOnChosenField =
        AccessTools.Field(typeof(EventOption), "<OnChosen>k__BackingField");

    private static readonly HashSet<EventOption> WrappedInitialOptions =
        new(ReferenceEqualityComparer.Instance);

    internal static IReadOnlyList<string>? ActiveCompanionOptionTexts { get; private set; }

    internal static IReadOnlyList<string>? ActiveCompanionIconFiles { get; private set; }

    internal static int ActiveCompanionOptionCount => ActiveCompanionOptionTexts?.Count ?? 0;

    private sealed class CompanionOption
    {
        public CompanionKind Kind { get; }
        public string DebugName { get; }
        public Type RelicType { get; }
        public Type CardType { get; }

        public CompanionOption(CompanionKind kind, string debugName, Type relicType, Type cardType)
        {
            Kind = kind;
            DebugName = debugName;
            RelicType = relicType;
            CardType = cardType;
        }

        public string TitleKey => Kind switch
        {
            CompanionKind.Byrdpip => "BYRDPIP.title",
            CompanionKind.SoulFysh => "SOUL_FYSH.title",
            CompanionKind.Wriggler => "WRIGGLER.title",
            CompanionKind.CeremonialBeast => "CEREMONIAL_BEAST.title",
            CompanionKind.KinFollower => "KIN_FOLLOWER.title",
            CompanionKind.EyeWithTeeth => "EYE_WITH_TEETH.title",
            CompanionKind.GremlinMerc => "GREMLIN_MERC.title",
            CompanionKind.ThievingHopper => "THIEVING_HOPPER.title",
            CompanionKind.Aeonglass => "AEONGLASS.title",
            CompanionKind.LagavulinMatriarch => "LAGAVULIN_MATRIARCH.title",
            CompanionKind.TheKin => "THE_KIN.title",
            CompanionKind.WaterfallGiant => "WATERFALL_GIANT.title",
            CompanionKind.Vantom => "VANTOM.title",
            CompanionKind.KnowledgeDemon => "KNOWLEDGE_DEMON.title",
            CompanionKind.TheInsatiable => "THE_INSATIABLE.title",
            CompanionKind.Queen => "QUEEN.title",
            CompanionKind.TestSubject => "TEST_SUBJECT.title",
            CompanionKind.Seapunk => "SEAPUNK.title",
            CompanionKind.ShrinkerBeetle => "SHRINKER_BEETLE.title",
            CompanionKind.Operosis => "OPEROSIS.title",
            CompanionKind.Architect => "ARCHITECT.title",
            CompanionKind.Rustclad => "RUSTCLAD.title",
            CompanionKind.Shadeleaf => "SHADELEAF.title",
            CompanionKind.Glitchling => "GLITCHLING.title",
            CompanionKind.Bonebinder => "BONEBINDER.title",
            CompanionKind.GildedPage => "GILDED_PAGE.title",
            _ => "CHOOSE_COMPANION.title"
        };

        public string DescriptionKey => Kind switch
        {
            CompanionKind.Byrdpip => "BYRDPIP.description",
            CompanionKind.SoulFysh => "SOUL_FYSH.description",
            CompanionKind.Wriggler => "WRIGGLER.description",
            CompanionKind.CeremonialBeast => "CEREMONIAL_BEAST.description",
            CompanionKind.KinFollower => "KIN_FOLLOWER.description",
            CompanionKind.EyeWithTeeth => "EYE_WITH_TEETH.description",
            CompanionKind.GremlinMerc => "GREMLIN_MERC.description",
            CompanionKind.ThievingHopper => "THIEVING_HOPPER.description",
            CompanionKind.Aeonglass => "AEONGLASS.description",
            CompanionKind.LagavulinMatriarch => "LAGAVULIN_MATRIARCH.description",
            CompanionKind.TheKin => "THE_KIN.description",
            CompanionKind.WaterfallGiant => "WATERFALL_GIANT.description",
            CompanionKind.Vantom => "VANTOM.description",
            CompanionKind.KnowledgeDemon => "KNOWLEDGE_DEMON.description",
            CompanionKind.TheInsatiable => "THE_INSATIABLE.description",
            CompanionKind.Queen => "QUEEN.description",
            CompanionKind.TestSubject => "TEST_SUBJECT.description",
            CompanionKind.Seapunk => "SEAPUNK.description",
            CompanionKind.ShrinkerBeetle => "SHRINKER_BEETLE.description",
            CompanionKind.Operosis => "OPEROSIS.description",
            CompanionKind.Architect => "ARCHITECT.description",
            CompanionKind.Rustclad => "RUSTCLAD.description",
            CompanionKind.Shadeleaf => "SHADELEAF.description",
            CompanionKind.Glitchling => "GLITCHLING.description",
            CompanionKind.Bonebinder => "BONEBINDER.description",
            CompanionKind.GildedPage => "GILDED_PAGE.description",
            _ => "CHOOSE_COMPANION.description"
        };
    }

    private static readonly List<CompanionOption> CompanionPool =
    [
        new CompanionOption(CompanionKind.Byrdpip, "Byrdpip", typeof(Byrdpip), typeof(ByrdSwoop)),
        new CompanionOption(CompanionKind.SoulFysh, "Soul Fysh Pip", typeof(SoulFyshPipRelic), typeof(FyshSwoop)),
        new CompanionOption(CompanionKind.Wriggler, "Wriggler", typeof(WrigglerRelic), typeof(WrigglerCard)),
        new CompanionOption(CompanionKind.CeremonialBeast, "Ceremonial Beast", typeof(CeremonialBeastRelic), typeof(CeremonialBeastCard)),
        new CompanionOption(CompanionKind.KinFollower, "Kin Follower", typeof(KinFollowerRelic), typeof(KinFollowerCard)),
        new CompanionOption(CompanionKind.EyeWithTeeth, "Eye With Teeth", typeof(EyeWithTeethRelic), typeof(EyeWithTeethCard)),
        new CompanionOption(CompanionKind.GremlinMerc, "Gremlin Merc", typeof(GremlinMercRelic), typeof(GremlinMercCard)),
        new CompanionOption(CompanionKind.ThievingHopper, "Thieving Hopper", typeof(ThievingHopperRelic), typeof(ThievingHopperCard)),
        new CompanionOption(CompanionKind.Aeonglass, "Aeonglass", typeof(AeonglassRelic), typeof(AeonglassCard)),
        new CompanionOption(CompanionKind.LagavulinMatriarch, "Lagavulin Matriarch", typeof(LagavulinMatriarchRelic), typeof(LagavulinMatriarchCard)),
        new CompanionOption(CompanionKind.TheKin, "The Kin", typeof(TheKinRelic), typeof(TheKinCard)),
        new CompanionOption(CompanionKind.WaterfallGiant, "Waterfall Giant", typeof(WaterfallGiantRelic), typeof(WaterfallGiantCard)),
        new CompanionOption(CompanionKind.Vantom, "Vantom", typeof(VantomRelic), typeof(VantomCard)),
        new CompanionOption(CompanionKind.KnowledgeDemon, "Knowledge Demon", typeof(KnowledgeDemonRelic), typeof(KnowledgeDemonCard)),
        new CompanionOption(CompanionKind.TheInsatiable, "The Insatiable", typeof(TheInsatiableRelic), typeof(TheInsatiableCard)),
        new CompanionOption(CompanionKind.Queen, "Queen", typeof(QueenRelic), typeof(QueenCard)),
        new CompanionOption(CompanionKind.TestSubject, "Test Subject", typeof(TestSubjectRelic), typeof(TestSubjectCard)),
        new CompanionOption(CompanionKind.Seapunk, "Seapunk", typeof(SeapunkRelic), typeof(SeapunkCard)),
        new CompanionOption(CompanionKind.ShrinkerBeetle, "Shrinker Beetle", typeof(ShrinkerBeetleRelic), typeof(ShrinkerBeetleCard)),
        new CompanionOption(CompanionKind.Operosis, "Operosis", typeof(OperosisRelic), typeof(OperosisCard)),
        new CompanionOption(CompanionKind.Architect, "The Architect", typeof(ArchitectRelic), typeof(ArchitectCard)),
        new CompanionOption(CompanionKind.Rustclad, "Rustclad", typeof(RustcladRelic), typeof(BuffUpCard)),
        new CompanionOption(CompanionKind.Shadeleaf, "Shadeleaf", typeof(ShadeleafRelic), typeof(NeedleTossCard)),
        new CompanionOption(CompanionKind.Glitchling, "Glitchling", typeof(GlitchlingRelic), typeof(OverclockCard)),
        new CompanionOption(CompanionKind.Bonebinder, "Bonebinder", typeof(BonebinderRelic), typeof(GraveCallCard)),
        new CompanionOption(CompanionKind.GildedPage, "Gilded Page", typeof(GildedPageRelic), typeof(CommandingFlourishCard))
    ];

    public static void Postfix(AncientEventModel __instance, ref IReadOnlyList<EventOption> __result)
    {
        if (__instance is not Neow neow || __result == null || __result.Count == 0)
        {
            return;
        }

        GD.Print("[NeowCompanions] GenerateInitialOptionsWrapper patch HIT. Option count: " + __result.Count);

        int wrappedCount = 0;
        foreach (EventOption option in __result)
        {
            if (TryWrapInitialNeowOption(neow, option))
            {
                wrappedCount++;
            }
        }

        GD.Print("[NeowCompanions] Wrapped Neow option actions: " + wrappedCount);
    }

    private static bool TryWrapInitialNeowOption(Neow neow, EventOption option)
    {
        if (EventOptionOnChosenField == null
            || option.IsLocked
            || option.IsProceed
            || WrappedInitialOptions.Contains(option))
        {
            return false;
        }

        if (EventOptionOnChosenField.GetValue(option) is not Func<Task> originalOnChosen)
        {
            return false;
        }

        Func<Task> wrappedOnChosen = async () =>
        {
            GD.Print("[NeowCompanions] Neow option selected, delaying original completion for companion flow: " + option.TextKey);
            if (ModSettings.RandomCompanionNoChoices)
            {
                await ChooseRandomCompanion(neow, originalOnChosen);
                return;
            }

            ShowCompanionChoices(neow, option, originalOnChosen);
        };

        try
        {
            EventOptionOnChosenField.SetValue(option, wrappedOnChosen);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error("[NeowCompanions] Could not wrap Neow option action: " + ex);
            GD.PrintErr("[NeowCompanions] Could not wrap Neow option action: " + ex);
            return false;
        }

        WrappedInitialOptions.Add(option);
        return true;
    }

    private static void ShowCompanionChoices(Neow neow, EventOption selectedNeowOption, Func<Task> finishOriginalOption, int page = 0)
    {
        GD.Print("[NeowCompanions] Showing companion choices.");

        List<CompanionOption> offeredCompanions = ModSettings.OfferAllCompanions
            ? CompanionPool.ToList()
            : GetSeededCompanionChoices(neow, Math.Min(3, CompanionPool.Count));

        int maxPage = Math.Max(0, (int)Math.Ceiling(offeredCompanions.Count / (double)CompanionPageSize) - 1);
        page = Math.Clamp(page, 0, maxPage);

        List<CompanionOption> visibleCompanions = ModSettings.OfferAllCompanions
            ? offeredCompanions.Skip(page * CompanionPageSize).Take(CompanionPageSize).ToList()
            : offeredCompanions;

        List<EventOption> companionOptions = new();
        List<string> optionTexts = new();
        List<string> iconFiles = new();

        foreach (CompanionOption companion in visibleCompanions)
        {
            GD.Print("[NeowCompanions] Offering companion: " + companion.DebugName);

            RelicModel displayRelic = GetCompanionRelic(companion).ToMutable();
            string? iconFile = displayRelic is CompanionRelicModel companionRelic
                ? companionRelic.IconFileName
                : null;

            if (neow.Owner != null)
            {
                displayRelic.Owner = neow.Owner;
            }

            CardModel previewCard = GetCompanionCard(companion);
            IHoverTip[] hoverTips = [HoverTipFactory.FromCard(previewCard, upgrade: false)];

            EventOption companionOption = new EventOption(
                neow,
                async () =>
                {
                    await ChooseCompanion(neow, companion, finishOriginalOption);
                },
                selectedNeowOption.Title,
                selectedNeowOption.Description,
                "COMPANION." + companion.Kind,
                hoverTips);

            companionOptions.Add(companionOption.WithRelic(displayRelic));
            optionTexts.Add(NeowCompanionText.GetText(companion.TitleKey) + "\n" + NeowCompanionText.GetText(companion.DescriptionKey));
            iconFiles.Add(iconFile ?? string.Empty);
        }

        if (ModSettings.OfferAllCompanions && maxPage > 0)
        {
            if (page > 0)
            {
                AddNavigationOption(
                    neow,
                    selectedNeowOption,
                    companionOptions,
                    optionTexts,
                    iconFiles,
                    () => ShowCompanionChoices(neow, selectedNeowOption, finishOriginalOption, page - 1),
                    "Previous companions",
                    $"Page {page} of {maxPage + 1}");
            }

            if (page < maxPage)
            {
                AddNavigationOption(
                    neow,
                    selectedNeowOption,
                    companionOptions,
                    optionTexts,
                    iconFiles,
                    () => ShowCompanionChoices(neow, selectedNeowOption, finishOriginalOption, page + 1),
                    "More companions",
                    $"Page {page + 2} of {maxPage + 1}");
            }
        }

        ActiveCompanionOptionTexts = optionTexts;
        ActiveCompanionIconFiles = iconFiles;

        InvokeSetEventState(
            neow,
            selectedNeowOption.Description,
            companionOptions);
    }

    private static Task ChooseRandomCompanion(Neow neow, Func<Task> finishOriginalOption)
    {
        CompanionOption companion = neow.Rng.NextItem(CompanionPool) ?? CompanionPool[0];
        GD.Print("[NeowCompanions] Random companion selected: " + companion.DebugName);
        return ChooseCompanion(neow, companion, finishOriginalOption);
    }

    private static List<CompanionOption> GetSeededCompanionChoices(Neow neow, int count)
    {
        List<CompanionOption> companions = CompanionPool.ToList();
        neow.Rng.Shuffle(companions);
        return companions.Take(count).ToList();
    }

    private static void AddNavigationOption(
        Neow neow,
        EventOption originalNeowOption,
        List<EventOption> companionOptions,
        List<string> optionTexts,
        List<string> iconFiles,
        Action selected,
        string title,
        string description)
    {
        EventOption navOption = new EventOption(
            neow,
            () =>
            {
                selected();
                return Task.CompletedTask;
            },
            originalNeowOption.Title,
            originalNeowOption.Description,
            "COMPANION_NAV." + title.Replace(" ", "_").ToUpperInvariant(),
            []);

        companionOptions.Add(navOption);
        optionTexts.Add(title + "\n" + description);
        iconFiles.Add(string.Empty);
    }

    private static async Task ChooseCompanion(Neow neow, CompanionOption companion, Func<Task> finishOriginalOption)
    {
        GD.Print("[NeowCompanions] Chose companion: " + companion.DebugName);

        if (neow.Owner == null)
        {
            GD.PrintErr("[NeowCompanions] ERROR: Neow Owner was null when choosing companion.");
            await finishOriginalOption();
            return;
        }

        CompanionState.SelectedCompanion = companion.Kind;
        ActiveCompanionOptionTexts = null;
        ActiveCompanionIconFiles = null;

        await RelicCmd.Obtain(GetCompanionRelic(companion).ToMutable(), neow.Owner);
        if (ModSettings.GrantCompanionCards)
        {
            await AddCompanionCard(companion, neow.Owner);
        }

        GD.Print("[NeowCompanions] Finishing original Neow option now.");
        await finishOriginalOption();
    }

    private static LocString CompanionLoc(string key)
    {
        return new LocString("neow_companions", key);
    }

    private static RelicModel GetCompanionRelic(CompanionOption companion)
    {
        MethodInfo method = AccessTools.Method(typeof(ModelDb), nameof(ModelDb.Relic), Type.EmptyTypes)
            ?? throw new MissingMethodException("Could not find ModelDb.Relic<T>()");

        object? relic = method.MakeGenericMethod(companion.RelicType).Invoke(null, []);

        return relic as RelicModel
            ?? throw new InvalidOperationException($"Companion relic type '{companion.RelicType.FullName}' did not produce a RelicModel.");
    }

    private static async Task AddCompanionCard(CompanionOption companion, MegaCrit.Sts2.Core.Entities.Players.Player owner)
    {
        CardModel deckCard = owner.RunState.CreateCard(GetCompanionCard(companion), owner);
        await CardPileCmd.Add(deckCard, PileType.Deck);
    }

    private static CardModel GetCompanionCard(CompanionOption companion)
    {
        MethodInfo method = AccessTools.Method(typeof(ModelDb), nameof(ModelDb.Card), Type.EmptyTypes)
            ?? throw new MissingMethodException("Could not find ModelDb.Card<T>()");

        object? canonicalCard = method.MakeGenericMethod(companion.CardType).Invoke(null, []);

        return canonicalCard as CardModel
            ?? throw new InvalidOperationException($"Companion card type '{companion.CardType.FullName}' did not produce a CardModel.");
    }

    private static void InvokeSetEventState(Neow neow, LocString description, IReadOnlyList<EventOption> options)
    {
        MethodInfo? method = AccessTools.Method(
            typeof(AncientEventModel),
            "SetEventState",
            [typeof(LocString), typeof(IReadOnlyList<EventOption>)]);

        if (method == null)
        {
            throw new MissingMethodException("Could not find AncientEventModel.SetEventState");
        }

        method.Invoke(neow, [description, options]);
    }
}

[HarmonyPatch(typeof(NAncientEventLayout), "AnimateButtonsIn")]
public static class NeowCompanionScrollableOptionsPatch
{
    public static void Postfix(NAncientEventLayout __instance)
    {
        return;
    }
}

[HarmonyPatch(typeof(NEventOptionButton), "_Ready")]
public static class NeowCompanionOptionButtonTextPatch
{
    public static void Postfix(NEventOptionButton __instance)
    {
        IReadOnlyList<string>? optionTexts = NeowCompanionChoicePatch.ActiveCompanionOptionTexts;
        IReadOnlyList<string>? iconFiles = NeowCompanionChoicePatch.ActiveCompanionIconFiles;
        object? indexValue = AccessTools.Property(typeof(NEventOptionButton), "Index")?.GetValue(__instance);
        if (optionTexts == null || indexValue is not int index || index < 0 || index >= optionTexts.Count)
        {
            return;
        }

        if (AccessTools.Field(typeof(NEventOptionButton), "_label")?.GetValue(__instance) is GodotObject label)
        {
            label.Set("text", optionTexts[index]);
        }

        if (iconFiles == null || index >= iconFiles.Count || string.IsNullOrEmpty(iconFiles[index]))
        {
            return;
        }

        Texture2D? icon = ModTextureLoader.Load(iconFiles[index]);
        if (icon == null)
        {
            MainFile.Logger.Error($"[NeowCompanions] Could not load companion option icon '{iconFiles[index]}'.");
            return;
        }

        if (AccessTools.Field(typeof(NEventOptionButton), "_image")?.GetValue(__instance) is TextureRect image)
        {
            image.Texture = icon;
            image.Visible = true;
        }

        if (AccessTools.Field(typeof(NEventOptionButton), "_outline")?.GetValue(__instance) is TextureRect outline)
        {
            outline.Texture = icon;
            outline.Visible = true;
        }
    }
}
