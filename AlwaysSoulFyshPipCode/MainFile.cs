using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.Events;

namespace AlwaysSoulFyshPip.AlwaysSoulFyshPipCode;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "NeowCompanions";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        Logger.Info("[NeowCompanions] Initializing.");
        GD.Print("[NeowCompanions] Initializing.");

        Harmony harmony = new("AlexAllen.NeowCompanions");

        MethodInfo? target = AccessTools.Method(typeof(Neow), "GenerateInitialOptions");
        MethodInfo? postfix = AccessTools.Method(
            typeof(Patches.NeowCompanionChoicePatch),
            nameof(Patches.NeowCompanionChoicePatch.Postfix));

        Logger.Info("[NeowCompanions] Neow target found: " + (target != null));
        GD.Print("[NeowCompanions] Neow target found: " + (target != null));

        Logger.Info("[NeowCompanions] Postfix found: " + (postfix != null));
        GD.Print("[NeowCompanions] Postfix found: " + (postfix != null));

        if (target != null && postfix != null)
        {
            harmony.Patch(target, postfix: new HarmonyMethod(postfix));

            Logger.Info("[NeowCompanions] Manual Neow patch applied.");
            GD.Print("[NeowCompanions] Manual Neow patch applied.");
        }
        else
        {
            Logger.Error("[NeowCompanions] Could not apply manual Neow patch.");
            GD.PrintErr("[NeowCompanions] Could not apply manual Neow patch.");
        }

        // This picks up NeowCompanionTextPatch, but not NeowCompanionChoicePatch
        // because NeowCompanionChoicePatch has no [HarmonyPatch] attribute.
        harmony.PatchAll(typeof(MainFile).Assembly);

        Logger.Info("[NeowCompanions] PatchAll complete.");
        GD.Print("[NeowCompanions] PatchAll complete.");
    }
}