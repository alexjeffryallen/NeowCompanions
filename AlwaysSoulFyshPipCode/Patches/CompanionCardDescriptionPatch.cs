using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using NeowCompanions.NeowCompanionsCode.Models;

namespace NeowCompanions.NeowCompanionsCode.Patches;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.GetDescriptionForPile), [typeof(PileType), typeof(Creature)])]
public static class CompanionCardDescriptionPatch
{
    private static readonly Regex DealDamageRegex =
        new(@"Deal\s+(?:\[green\]|\[red\])?\d+(?:\[/green\]|\[/red\])?\s+damage", RegexOptions.Compiled);

    private static readonly Regex GainBlockRegex =
        new(@"Gain\s+(?:\[green\]|\[red\])?\d+(?:\[/green\]|\[/red\])?\s+Block", RegexOptions.Compiled);

    public static void Postfix(CardModel __instance, Creature? target, ref string __result)
    {
        if (__instance.GetType().Namespace != typeof(EyeWithTeethCard).Namespace)
        {
            return;
        }

        RefreshDamageText(__instance, target, ref __result);
        RefreshBlockText(__instance, ref __result);
    }

    private static void RefreshDamageText(CardModel card, Creature? target, ref string description)
    {
        if (!card.DynamicVars.TryGetValue(DamageVar.defaultName, out DynamicVar? variable)
            || variable is not DamageVar damage)
        {
            return;
        }

        Creature? previewTarget = GetPreviewTarget(card, target);
        damage.UpdateCardPreview(card, CardPreviewMode.Normal, previewTarget, card.CombatState != null);

        string damageText = damage.ToHighlightedString(false);
        description = DealDamageRegex.Replace(description, $"Deal {damageText} damage", 1);
    }

    private static void RefreshBlockText(CardModel card, ref string description)
    {
        if (card.CombatState == null
            || card.Owner?.Creature == null
            || !card.DynamicVars.TryGetValue(BlockVar.defaultName, out DynamicVar? variable)
            || variable is not BlockVar block)
        {
            return;
        }

        block.PreviewValue = Hook.ModifyBlock(
            card.CombatState,
            card.Owner.Creature,
            block.BaseValue,
            block.Props,
            card,
            null,
            out _);

        string blockText = block.ToHighlightedString(false);
        description = GainBlockRegex.Replace(description, $"Gain {blockText} Block", 1);
    }

    private static Creature? GetPreviewTarget(CardModel card, Creature? target)
    {
        if (target != null)
        {
            return target;
        }

        if (card.TargetType != TargetType.AllEnemies || card.CombatState == null)
        {
            return null;
        }

        List<Creature> livingEnemies = card.CombatState.HittableEnemies.Where(enemy => enemy.IsAlive).ToList();
        return livingEnemies.Count == 1 ? livingEnemies[0] : null;
    }
}
