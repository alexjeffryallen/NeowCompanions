using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AlwaysSoulFyshPip.AlwaysSoulFyshPipCode;
using AlwaysSoulFyshPip.AlwaysSoulFyshPipCode.Models;
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

namespace AlwaysSoulFyshPip.AlwaysSoulFyshPipCode.Patches;

public static class NeowCompanionChoicePatch
{
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
            _ => "CHOOSE_COMPANION.title"
        };

        public string DescriptionKey => Kind switch
        {
            CompanionKind.Byrdpip => "BYRDPIP.description",
            CompanionKind.SoulFysh => "SOUL_FYSH.description",
            CompanionKind.Wriggler => "WRIGGLER.description",
            _ => "CHOOSE_COMPANION.description"
        };
    }

    private static readonly List<CompanionOption> CompanionPool =
    [
        new CompanionOption(CompanionKind.Byrdpip, "Byrdpip", typeof(Byrdpip), typeof(ByrdSwoop)),
        new CompanionOption(CompanionKind.SoulFysh, "Soul Fysh Pip", typeof(SoulFyshPipRelic), typeof(FyshSwoop)),
        new CompanionOption(CompanionKind.Wriggler, "Wriggler", typeof(WrigglerRelic), typeof(WrigglerCard))
    ];

    public static void Postfix(Neow __instance, ref IReadOnlyList<EventOption> __result)
    {
        GD.Print("[NeowCompanions] GenerateInitialOptions patch HIT. Option count: " + (__result?.Count ?? -1));

        if (__result == null || __result.Count == 0)
        {
            return;
        }

        List<EventOption> wrappedOptions = new();

        foreach (EventOption originalOption in __result)
        {
            EventOption capturedOriginalOption = originalOption;

            EventOption wrapped = new EventOption(
                __instance,
                async () =>
                {
                    GD.Print("[NeowCompanions] Original Neow relic option selected, delaying original completion.");
                    await Task.CompletedTask;

                    ShowCompanionChoices(__instance, capturedOriginalOption);
                },
                originalOption.Title,
                originalOption.Description,
                originalOption.TextKey,
                originalOption.HoverTips);

            if (originalOption.Relic != null)
            {
                wrapped.WithRelic(originalOption.Relic);
            }

            wrappedOptions.Add(wrapped);
        }

        __result = wrappedOptions;
    }

    private static void ShowCompanionChoices(Neow neow, EventOption originalNeowOption)
    {
        GD.Print("[NeowCompanions] Showing companion choices.");

        List<CompanionOption> offeredCompanions = CompanionPool
            .OrderBy(_ => Guid.NewGuid())
            .Take(Math.Min(3, CompanionPool.Count))
            .ToList();

        List<EventOption> companionOptions = new();

        foreach (CompanionOption companion in offeredCompanions)
        {
            GD.Print("[NeowCompanions] Offering companion: " + companion.DebugName);

            EventOption companionOption = new EventOption(
                neow,
                async () =>
                {
                    await ChooseCompanion(neow, companion, originalNeowOption);
                },
                CompanionLoc(companion.TitleKey),
                CompanionLoc(companion.DescriptionKey),
                "COMPANION." + companion.Kind,
                Array.Empty<IHoverTip>());

            RelicModel displayRelic = GetCompanionRelic(companion).ToMutable();
            if (neow.Owner != null)
            {
                displayRelic.Owner = neow.Owner;
            }

            companionOptions.Add(companionOption.WithRelic(displayRelic));
        }

        InvokeSetEventState(
            neow,
            CompanionLoc("CHOOSE_COMPANION.description"),
            companionOptions);
    }

    private static async Task ChooseCompanion(Neow neow, CompanionOption companion, EventOption originalNeowOption)
    {
        GD.Print("[NeowCompanions] Chose companion: " + companion.DebugName);

        if (neow.Owner == null)
        {
            GD.PrintErr("[NeowCompanions] ERROR: Neow Owner was null when choosing companion.");
            await originalNeowOption.Chosen();
            return;
        }

        CompanionState.SelectedCompanion = companion.Kind;

        await RelicCmd.Obtain(GetCompanionRelic(companion).ToMutable(), neow.Owner);
        await AddCompanionCard(companion, neow.Owner);

        GD.Print("[NeowCompanions] Finishing original Neow option now.");
        await originalNeowOption.Chosen();
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
        MethodInfo method = AccessTools.Method(typeof(ModelDb), nameof(ModelDb.Card), Type.EmptyTypes)
            ?? throw new MissingMethodException("Could not find ModelDb.Card<T>()");

        object? canonicalCard = method.MakeGenericMethod(companion.CardType).Invoke(null, []);
        if (canonicalCard is not CardModel cardModel)
        {
            throw new InvalidOperationException($"Companion card type '{companion.CardType.FullName}' did not produce a CardModel.");
        }

        CardModel deckCard = owner.RunState.CreateCard(cardModel, owner);
        await CardPileCmd.Add(deckCard, PileType.Deck);
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
