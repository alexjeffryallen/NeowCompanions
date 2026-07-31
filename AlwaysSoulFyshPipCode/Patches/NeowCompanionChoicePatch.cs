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
            CompanionKind.EmberPip => "EMBER_PIP.title",
            CompanionKind.FrostPip => "FROST_PIP.title",
            CompanionKind.StormPip => "STORM_PIP.title",
            CompanionKind.ThornPip => "THORN_PIP.title",
            CompanionKind.KaiserCrab => "KAISER_CRAB.title",
            CompanionKind.BygoneEffigy => "BYGONE_EFFIGY.title",
            CompanionKind.Byrdonis => "BYRDONIS.title",
            CompanionKind.PhrogParasite => "PHROG_PARASITE.title",
            CompanionKind.SkulkingColony => "SKULKING_COLONY.title",
            CompanionKind.PhantasmalGardener => "PHANTASMAL_GARDENER.title",
            CompanionKind.TerrorEel => "TERROR_EEL.title",
            CompanionKind.Decimillipede => "DECIMILLIPEDE.title",
            CompanionKind.Entomancer => "ENTOMANCER.title",
            CompanionKind.InfestedPrism => "INFESTED_PRISM.title",
            CompanionKind.KnightGang => "KNIGHT_GANG.title",
            CompanionKind.MechaKnight => "MECHA_KNIGHT.title",
            CompanionKind.SoulNexus => "SOUL_NEXUS.title",
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
            CompanionKind.EmberPip => "EMBER_PIP.description",
            CompanionKind.FrostPip => "FROST_PIP.description",
            CompanionKind.StormPip => "STORM_PIP.description",
            CompanionKind.ThornPip => "THORN_PIP.description",
            CompanionKind.KaiserCrab => "KAISER_CRAB.description",
            CompanionKind.BygoneEffigy => "BYGONE_EFFIGY.description",
            CompanionKind.Byrdonis => "BYRDONIS.description",
            CompanionKind.PhrogParasite => "PHROG_PARASITE.description",
            CompanionKind.SkulkingColony => "SKULKING_COLONY.description",
            CompanionKind.PhantasmalGardener => "PHANTASMAL_GARDENER.description",
            CompanionKind.TerrorEel => "TERROR_EEL.description",
            CompanionKind.Decimillipede => "DECIMILLIPEDE.description",
            CompanionKind.Entomancer => "ENTOMANCER.description",
            CompanionKind.InfestedPrism => "INFESTED_PRISM.description",
            CompanionKind.KnightGang => "KNIGHT_GANG.description",
            CompanionKind.MechaKnight => "MECHA_KNIGHT.description",
            CompanionKind.SoulNexus => "SOUL_NEXUS.description",
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
        new CompanionOption(CompanionKind.GildedPage, "Gilded Page", typeof(GildedPageRelic), typeof(CommandingFlourishCard)),
        new CompanionOption(CompanionKind.EmberPip, "Ember Pip", typeof(EmberPipRelic), typeof(EmberPipCard)),
        new CompanionOption(CompanionKind.FrostPip, "Frost Pip", typeof(FrostPipRelic), typeof(FrostPipCard)),
        new CompanionOption(CompanionKind.StormPip, "Storm Pip", typeof(StormPipRelic), typeof(StormPipCard)),
        new CompanionOption(CompanionKind.ThornPip, "Thorn Pip", typeof(ThornPipRelic), typeof(ThornPipCard)),
        new CompanionOption(CompanionKind.KaiserCrab, "Kaiser Crab", typeof(KaiserCrabRelic), typeof(KaiserCrabCard)),
        new CompanionOption(CompanionKind.BygoneEffigy, "Bygone Effigy", typeof(BygoneEffigyRelic), typeof(BygoneEffigyCard)),
        new CompanionOption(CompanionKind.Byrdonis, "Byrdonis", typeof(ByrdonisRelic), typeof(ByrdonisCard)),
        new CompanionOption(CompanionKind.PhrogParasite, "Phrog Parasite", typeof(PhrogParasiteRelic), typeof(PhrogParasiteCard)),
        new CompanionOption(CompanionKind.SkulkingColony, "Skulking Colony", typeof(SkulkingColonyRelic), typeof(SkulkingColonyCard)),
        new CompanionOption(CompanionKind.PhantasmalGardener, "Phantasmal Gardener", typeof(PhantasmalGardenerRelic), typeof(PhantasmalGardenerCard)),
        new CompanionOption(CompanionKind.TerrorEel, "Terror Eel", typeof(TerrorEelRelic), typeof(TerrorEelCard)),
        new CompanionOption(CompanionKind.Decimillipede, "Decimillipede", typeof(DecimillipedeRelic), typeof(DecimillipedeCard)),
        new CompanionOption(CompanionKind.Entomancer, "Entomancer", typeof(EntomancerRelic), typeof(EntomancerCard)),
        new CompanionOption(CompanionKind.InfestedPrism, "Infested Prism", typeof(InfestedPrismRelic), typeof(InfestedPrismCard)),
        new CompanionOption(CompanionKind.KnightGang, "Knight Gang", typeof(KnightGangRelic), typeof(KnightGangCard)),
        new CompanionOption(CompanionKind.MechaKnight, "Mecha Knight", typeof(MechaKnightRelic), typeof(MechaKnightCard)),
        new CompanionOption(CompanionKind.SoulNexus, "Soul Nexus", typeof(SoulNexusRelic), typeof(SoulNexusCard))
    ];

    // Each base-game Ancient gets a compact pool with a mixture of high-impact,
    // steady, and situational companion cards. Elite companions are distributed
    // across these pools, and Aeonglass intentionally appears twice so every
    // companion remains represented while pool sizes stay close.
    private static readonly Dictionary<Type, HashSet<CompanionKind>> CompanionPoolsByAncient = new()
    {
        [typeof(Neow)] =
        [
            CompanionKind.Architect,
            CompanionKind.Byrdpip,
            CompanionKind.Aeonglass,
            CompanionKind.EmberPip,
            CompanionKind.BygoneEffigy,
            CompanionKind.Byrdonis
        ],
        [typeof(Darv)] =
        [
            CompanionKind.KaiserCrab,
            CompanionKind.GremlinMerc,
            CompanionKind.ThievingHopper,
            CompanionKind.KinFollower,
            CompanionKind.PhrogParasite
        ],
        [typeof(Nonupeipe)] =
        [
            CompanionKind.FrostPip,
            CompanionKind.WaterfallGiant,
            CompanionKind.SoulFysh,
            CompanionKind.Seapunk,
            CompanionKind.SkulkingColony,
            CompanionKind.PhantasmalGardener
        ],
        [typeof(Orobas)] =
        [
            CompanionKind.LagavulinMatriarch,
            CompanionKind.Vantom,
            CompanionKind.Wriggler,
            CompanionKind.Operosis,
            CompanionKind.TerrorEel
        ],
        [typeof(Pael)] =
        [
            CompanionKind.Queen,
            CompanionKind.CeremonialBeast,
            CompanionKind.TestSubject,
            CompanionKind.Shadeleaf,
            CompanionKind.Decimillipede,
            CompanionKind.Entomancer
        ],
        [typeof(Tanx)] =
        [
            CompanionKind.ThornPip,
            CompanionKind.TheInsatiable,
            CompanionKind.EyeWithTeeth,
            CompanionKind.ShrinkerBeetle,
            CompanionKind.InfestedPrism
        ],
        [typeof(Tezcatara)] =
        [
            CompanionKind.KnowledgeDemon,
            CompanionKind.TheKin,
            CompanionKind.Rustclad,
            CompanionKind.GildedPage,
            CompanionKind.KnightGang,
            CompanionKind.MechaKnight
        ],
        [typeof(Vakuu)] =
        [
            CompanionKind.Glitchling,
            CompanionKind.StormPip,
            CompanionKind.Bonebinder,
            CompanionKind.Aeonglass,
            CompanionKind.SoulNexus
        ]
    };

    public static void Postfix(AncientEventModel __instance, ref IReadOnlyList<EventOption> __result)
    {
        if (__result == null
            || __result.Count == 0
            || (__instance is not Neow && !ModSettings.OfferCompanionsAtEveryAncient))
        {
            return;
        }

        GD.Print("[NeowCompanions] GenerateInitialOptionsWrapper patch HIT. Option count: " + __result.Count);

        int wrappedCount = 0;
        foreach (EventOption option in __result)
        {
            if (TryWrapInitialAncientOption(__instance, option))
            {
                wrappedCount++;
            }
        }

        GD.Print("[NeowCompanions] Wrapped Ancient option actions: " + wrappedCount);
    }

    private static bool TryWrapInitialAncientOption(AncientEventModel ancient, EventOption option)
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
            GD.Print("[NeowCompanions] Ancient option selected, delaying original completion for companion flow: " + option.TextKey);
            if (GetAvailableCompanions(ancient).Count == 0)
            {
                GD.Print("[NeowCompanions] No new companions available; finishing original Ancient option.");
                await originalOnChosen();
                return;
            }

            if (ModSettings.RandomCompanionNoChoices)
            {
                await ChooseRandomCompanion(ancient, originalOnChosen);
                return;
            }

            ShowCompanionChoices(ancient, option, originalOnChosen);
        };

        try
        {
            EventOptionOnChosenField.SetValue(option, wrappedOnChosen);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error("[NeowCompanions] Could not wrap Ancient option action: " + ex);
            GD.PrintErr("[NeowCompanions] Could not wrap Ancient option action: " + ex);
            return false;
        }

        WrappedInitialOptions.Add(option);
        return true;
    }

    private static void ShowCompanionChoices(AncientEventModel ancient, EventOption selectedAncientOption, Func<Task> finishOriginalOption, int page = 0)
    {
        GD.Print("[NeowCompanions] Showing companion choices.");

        List<CompanionOption> availableCompanions = GetAvailableCompanions(ancient);
        List<CompanionOption> offeredCompanions = ModSettings.OfferAllCompanions
            ? availableCompanions
            : GetSeededCompanionChoices(ancient, Math.Min(3, availableCompanions.Count), availableCompanions);

        List<CompanionOption> visibleCompanions = offeredCompanions;

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

            if (ancient.Owner != null)
            {
                displayRelic.Owner = ancient.Owner;
            }

            CardModel previewCard = GetCompanionCard(companion);
            IHoverTip[] hoverTips = [HoverTipFactory.FromCard(previewCard, upgrade: false)];

            EventOption companionOption = new EventOption(
                ancient,
                async () =>
                {
                    await ChooseCompanion(ancient, companion, finishOriginalOption);
                    if (!ModSettings.ChooseMultipleCompanionsAtAncient)
                    {
                        return;
                    }

                    if (GetAvailableCompanions(ancient).Count == 0)
                    {
                        ActiveCompanionOptionTexts = null;
                        ActiveCompanionIconFiles = null;
                        await finishOriginalOption();
                        return;
                    }

                    ShowCompanionChoices(ancient, selectedAncientOption, finishOriginalOption, page);
                },
                selectedAncientOption.Title,
                selectedAncientOption.Description,
                "COMPANION." + companion.Kind,
                hoverTips);

            companionOptions.Add(companionOption.WithRelic(displayRelic));
            optionTexts.Add(NeowCompanionText.GetText(companion.TitleKey) + "\n" + NeowCompanionText.GetText(companion.DescriptionKey));
            iconFiles.Add(iconFile ?? string.Empty);
        }

        if (ModSettings.ChooseMultipleCompanionsAtAncient)
        {
            AddFinishChoosingOption(
                ancient,
                selectedAncientOption,
                companionOptions,
                optionTexts,
                iconFiles,
                finishOriginalOption);
        }

        ActiveCompanionOptionTexts = optionTexts;
        ActiveCompanionIconFiles = iconFiles;

        InvokeSetEventState(
            ancient,
            selectedAncientOption.Description,
            companionOptions);
    }

    private static async Task ChooseRandomCompanion(AncientEventModel ancient, Func<Task> finishOriginalOption)
    {
        List<CompanionOption> availableCompanions = GetAvailableCompanions(ancient);
        CompanionOption companion = ancient.Rng.NextItem(availableCompanions) ?? availableCompanions[0];
        GD.Print("[NeowCompanions] Random companion selected: " + companion.DebugName);
        await ChooseCompanion(ancient, companion, finishOriginalOption);
        if (ModSettings.ChooseMultipleCompanionsAtAncient)
        {
            await finishOriginalOption();
        }
    }

    private static List<CompanionOption> GetSeededCompanionChoices(AncientEventModel ancient, int count, List<CompanionOption> companions)
    {
        companions = companions.ToList();
        ancient.Rng.Shuffle(companions);
        return companions.Take(count).ToList();
    }

    private static List<CompanionOption> GetAvailableCompanions(AncientEventModel ancient)
    {
        IEnumerable<CompanionOption> ancientPool = GetCompanionPoolForAncient(ancient);
        if (ancient.Owner == null)
        {
            return ancientPool.ToList();
        }

        return ancientPool
            .Where(companion => !ancient.Owner.Relics.Any(relic => companion.RelicType.IsInstanceOfType(relic)))
            .ToList();
    }

    private static IEnumerable<CompanionOption> GetCompanionPoolForAncient(AncientEventModel ancient)
    {
        if (ModSettings.OfferAllCompanions)
        {
            return CompanionPool;
        }

        if (!CompanionPoolsByAncient.TryGetValue(ancient.GetType(), out HashSet<CompanionKind>? ancientPool))
        {
            MainFile.Logger.Info($"[NeowCompanions] No dedicated pool for {ancient.GetType().Name}; using the full companion pool.");
            return CompanionPool;
        }

        MainFile.Logger.Info($"[NeowCompanions] Using the dedicated {ancient.GetType().Name} companion pool.");
        return CompanionPool.Where(companion => ancientPool.Contains(companion.Kind));
    }

    private static async Task ChooseCompanion(AncientEventModel ancient, CompanionOption companion, Func<Task> finishOriginalOption)
    {
        GD.Print("[NeowCompanions] Chose companion: " + companion.DebugName);

        if (ancient.Owner == null)
        {
            GD.PrintErr("[NeowCompanions] ERROR: Ancient Owner was null when choosing companion.");
            await finishOriginalOption();
            return;
        }

        CompanionState.SelectedCompanion = companion.Kind;
        ActiveCompanionOptionTexts = null;
        ActiveCompanionIconFiles = null;

        await RelicCmd.Obtain(GetCompanionRelic(companion).ToMutable(), ancient.Owner);
        if (ModSettings.GrantCompanionCards)
        {
            await AddCompanionCard(companion, ancient.Owner);
        }

        if (ModSettings.ChooseMultipleCompanionsAtAncient)
        {
            GD.Print("[NeowCompanions] Returning to companion choices after multi-pick.");
            return;
        }

        GD.Print("[NeowCompanions] Finishing original Ancient option now.");
        await finishOriginalOption();
    }

    private static void AddFinishChoosingOption(
        AncientEventModel ancient,
        EventOption originalAncientOption,
        List<EventOption> companionOptions,
        List<string> optionTexts,
        List<string> iconFiles,
        Func<Task> finishOriginalOption)
    {
        EventOption finishOption = new EventOption(
            ancient,
            async () =>
            {
                ActiveCompanionOptionTexts = null;
                ActiveCompanionIconFiles = null;
                GD.Print("[NeowCompanions] Finished choosing multiple companions.");
                await finishOriginalOption();
            },
            originalAncientOption.Title,
            originalAncientOption.Description,
            "COMPANION_NAV.DONE_CHOOSING",
            []);

        companionOptions.Add(finishOption);
        optionTexts.Add("Done choosing\nContinue with the Ancient option.");
        iconFiles.Add(string.Empty);
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

    private static void InvokeSetEventState(AncientEventModel ancient, LocString description, IReadOnlyList<EventOption> options)
    {
        MethodInfo? method = AccessTools.Method(
            typeof(AncientEventModel),
            "SetEventState",
            [typeof(LocString), typeof(IReadOnlyList<EventOption>)]);

        if (method == null)
        {
            throw new MissingMethodException("Could not find AncientEventModel.SetEventState");
        }

        method.Invoke(ancient, [description, options]);
    }
}

[HarmonyPatch(typeof(NAncientEventLayout), "AnimateButtonsIn")]
public static class NeowCompanionScrollableOptionsPatch
{
    public static void Postfix(NAncientEventLayout __instance)
    {
        if (NeowCompanionChoicePatch.ActiveCompanionOptionCount <= 6)
        {
            return;
        }

        if (AccessTools.Field(typeof(NEventLayout), "_optionsContainer")?.GetValue(__instance)
            is not VBoxContainer optionsContainer)
        {
            return;
        }

        if (optionsContainer.GetParent() is ScrollContainer existingScroll)
        {
            existingScroll.ScrollVertical = 0;
            return;
        }

        if (optionsContainer.GetParent() is not Container parent)
        {
            return;
        }

        int childIndex = optionsContainer.GetIndex();
        float viewportHeight = __instance.GetViewportRect().Size.Y;
        float listHeight = Mathf.Clamp(viewportHeight * 0.62f, 360f, 560f);

        ScrollContainer scroll = new()
        {
            Name = "NeowCompanionScrollList",
            CustomMinimumSize = new Vector2(0f, listHeight),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ShrinkBegin,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            VerticalScrollMode = ScrollContainer.ScrollMode.Auto,
            FollowFocus = true
        };

        parent.AddChild(scroll);
        parent.MoveChild(scroll, childIndex);
        optionsContainer.Reparent(scroll);
        optionsContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        optionsContainer.SizeFlagsVertical = Control.SizeFlags.ShrinkBegin;
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
