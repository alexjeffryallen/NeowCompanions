using System.Reflection;
using BaseLib.Config;
using Godot;
using HarmonyLib;
using NeowCompanions.NeowCompanionsCode.Config;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;

namespace NeowCompanions.NeowCompanionsCode;

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

        MethodInfo? target = AccessTools.Method(typeof(AncientEventModel), "GenerateInitialOptionsWrapper");
        MethodInfo? postfix = AccessTools.Method(
            typeof(Patches.NeowCompanionChoicePatch),
            nameof(Patches.NeowCompanionChoicePatch.Postfix));

        Logger.Info("[NeowCompanions] Ancient options wrapper target found: " + (target != null));
        GD.Print("[NeowCompanions] Ancient options wrapper target found: " + (target != null));

        Logger.Info("[NeowCompanions] Postfix found: " + (postfix != null));
        GD.Print("[NeowCompanions] Postfix found: " + (postfix != null));

        if (target != null && postfix != null)
        {
            HarmonyMethod companionPostfix = new(postfix)
            {
                priority = Priority.Last,
                after = ["AncientAffection.harmony"]
            };

            harmony.Patch(target, postfix: companionPostfix);

            Logger.Info("[NeowCompanions] Manual ancient options wrapper patch applied.");
            GD.Print("[NeowCompanions] Manual ancient options wrapper patch applied.");
        }
        else
        {
            Logger.Error("[NeowCompanions] Could not apply manual ancient options wrapper patch.");
            GD.PrintErr("[NeowCompanions] Could not apply manual ancient options wrapper patch.");
        }

        // This picks up NeowCompanionTextPatch, but not NeowCompanionChoicePatch
        // because NeowCompanionChoicePatch has no [HarmonyPatch] attribute.
        harmony.PatchAll(typeof(MainFile).Assembly);

        Logger.Info("[NeowCompanions] PatchAll complete.");
        GD.Print("[NeowCompanions] PatchAll complete.");

        ModConfigRegistry.Register(ModId, new NeowCompanionsConfig());
        Logger.Info("[NeowCompanions] Mod config registered.");
        GD.Print("[NeowCompanions] Mod config registered.");
    }
}
