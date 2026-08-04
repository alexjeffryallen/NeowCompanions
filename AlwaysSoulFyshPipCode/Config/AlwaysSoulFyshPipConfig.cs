using BaseLib.Config;
using BaseLib.Config.UI;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using NeowCompanions.NeowCompanionsCode.Patches;
using System.Collections.Generic;

namespace NeowCompanions.NeowCompanionsCode.Config;

[ConfigHoverTipsByDefault]
public sealed class NeowCompanionsConfig : SimpleModConfig
{
    public override void SetupConfigUI(Control optionContainer)
    {
        GenerateOptionsForAllProperties(optionContainer);

        foreach ((string actName, IReadOnlyList<NeowCompanionChoicePatch.CompanionPoolConfigEntry> companions)
                 in NeowCompanionChoicePatch.GetCompanionPoolsForConfig())
        {
            NConfigCollapsibleSection section = CreateCollapsibleSection(
                $"{actName} companion pool ({companions.Count})",
                collapsedByDefault: true);
            optionContainer.AddChild(section);

            foreach (NeowCompanionChoicePatch.CompanionPoolConfigEntry companion in companions)
            {
                string keyStem = $"NEOWCOMPANIONS-POOL-{actName}-{companion.CardType.Name}";
                string titleKey = keyStem + ".hover.title";
                string descriptionKey = keyStem + ".hover.desc";
                CardModel card = ModelDb.GetById<CardModel>(ModelDb.GetId(companion.CardType));

                NeowCompanionText.RegisterSettingsText(titleKey, card.Title);
                NeowCompanionText.RegisterSettingsText(
                    descriptionKey,
                    $"Base card (Cost {FormatCost(card.EnergyCost)}):\n{card.GetDescriptionForPile(PileType.None)}\n\n" +
                    $"Upgraded card:\n{card.GetDescriptionForUpgradePreview()}");

                Control spacer = new() { CustomMinimumSize = new Vector2(1f, 1f) };
                NConfigOptionRow row = new(
                    ModPrefix,
                    keyStem,
                    ModConfig.CreateRawLabelControl(companion.Name, 28),
                    spacer);
                row.AddCustomHoverTip(titleKey, descriptionKey);
                section.ContentContainer.AddChild(row);
            }
        }

        AddRestoreDefaultsButton(optionContainer);
        SetupFocusNeighbors(optionContainer);
    }

    private static string FormatCost(CardEnergyCost cost)
    {
        return cost.CostsX ? "X" : cost.Canonical.ToString();
    }

    [ConfigHideInUI]
    public static bool StartWithFyshSwoop
    {
        get => ModSettings.StartWithFyshSwoop;
        set => ModSettings.StartWithFyshSwoop = value;
    }

    public static bool OfferAllCompanions
    {
        get => ModSettings.OfferAllCompanions;
        set => ModSettings.OfferAllCompanions = value;
    }

    public static bool FullStartingCompanionPool
    {
        get => ModSettings.FullStartingCompanionPool;
        set => ModSettings.FullStartingCompanionPool = value;
    }

    public static bool RandomCompanionNoChoices
    {
        get => ModSettings.RandomCompanionNoChoices;
        set => ModSettings.RandomCompanionNoChoices = value;
    }

    public static bool GrantCompanionCards
    {
        get => ModSettings.GrantCompanionCards;
        set => ModSettings.GrantCompanionCards = value;
    }

    public static bool GrantUpgradedCompanionCards
    {
        get => ModSettings.GrantUpgradedCompanionCards;
        set => ModSettings.GrantUpgradedCompanionCards = value;
    }

    public static bool OfferCompanionsAtEveryAncient
    {
        get => ModSettings.OfferCompanionsAtEveryAncient;
        set => ModSettings.OfferCompanionsAtEveryAncient = value;
    }

    public static bool ChooseMultipleCompanionsAtAncient
    {
        get => ModSettings.ChooseMultipleCompanionsAtAncient;
        set => ModSettings.ChooseMultipleCompanionsAtAncient = value;
    }

}
