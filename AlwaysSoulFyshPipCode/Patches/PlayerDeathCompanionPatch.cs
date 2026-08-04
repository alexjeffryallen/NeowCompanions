using System.Threading.Tasks;
using NeowCompanions.NeowCompanionsCode.Models;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace NeowCompanions.NeowCompanionsCode.Patches;

[HarmonyPatch(typeof(Creature), nameof(Creature.InvokeDiedEvent))]
public static class PlayerDeathCompanionPatch
{
    public static void Postfix(Creature __instance)
    {
        if (!__instance.IsPlayer || __instance.Player?.PlayerCombatState == null)
        {
            return;
        }

        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<SoulFyshPipPet>(), "Soul Fysh Pip");
        TriggerWrigglerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<WrigglerPet>());
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<CeremonialBeastPet>(), "Ceremonial Beast");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<KinFollowerPet>(), "Kin Follower");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<EyeWithTeethPet>(), "Eye With Teeth");
        TriggerGremlinMercDeathAnimation(__instance.Player);
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<ThievingHopperPet>(), "Thieving Hopper");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<AeonglassPet>(), "Aeonglass");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<LagavulinMatriarchPet>(), "Lagavulin Matriarch");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<TheKinPet>(), "The Kin");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<WaterfallGiantPet>(), "Waterfall Giant");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<VantomPet>(), "Vantom");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<KnowledgeDemonPet>(), "Knowledge Demon");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<TheInsatiablePet>(), "The Insatiable");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<QueenPet>(), "Queen");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<TestSubjectPet>(), "Test Subject");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<SeapunkPet>(), "Seapunk");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<ShrinkerBeetlePet>(), "Shrinker Beetle");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<OperosisPet>(), "Operosis");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<ArchitectPet>(), "The Architect");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<RustcladPet>(), "Rustclad");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<ShadeleafPet>(), "Shadeleaf");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<GlitchlingPet>(), "Glitchling");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<BonebinderPet>(), "Bonebinder");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<GildedPagePet>(), "Gilded Page");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<EmberPipPet>(), "Ember Pip");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<FrostPipPet>(), "Frost Pip");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<StormPipPet>(), "Storm Pip");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<ThornPipPet>(), "Thorn Pip");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<BygoneEffigyPet>(), "Bygone Effigy");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<ByrdonisPet>(), "Byrdonis");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<PhrogParasitePet>(), "Phrog Parasite");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<SkulkingColonyPet>(), "Skulking Colony");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<PhantasmalGardenerPet>(), "Phantasmal Gardener");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<TerrorEelPet>(), "Terror Eel");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<DecimillipedePet>(), "Decimillipede");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<DecimillipedeMiddlePet>(), "Decimillipede middle");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<DecimillipedeBackPet>(), "Decimillipede back");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<EntomancerPet>(), "Entomancer");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<InfestedPrismPet>(), "Infested Prism");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<KnightGangPet>(), "Knight Gang");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<MechaKnightPet>(), "Mecha Knight");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<SoulNexusPet>(), "Soul Nexus");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<AssassinRubyRaiderPet>(), "Assassin Ruby Raider");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<AxeRubyRaiderPet>(), "Axe Ruby Raider");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<BruteRubyRaiderPet>(), "Brute Ruby Raider");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<CrossbowRubyRaiderPet>(), "Crossbow Ruby Raider");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<FlyconidPet>(), "Flyconid");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<FogmogPet>(), "Fogmog");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<MawlerPet>(), "Mawler");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<FuzzyWurmCrawlerPet>(), "Fuzzy Wurm Crawler");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<InkletPet>(), "Inklet");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<SnappingJaxfruitPet>(), "Snapping Jaxfruit");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<SlitheringStranglerPet>(), "Slithering Strangler");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<LeafSlimeSPet>(), "Small Leaf Slime");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<LeafSlimeMPet>(), "Medium Leaf Slime");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<TwigSlimeSPet>(), "Small Twig Slime");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<TwigSlimeMPet>(), "Medium Twig Slime");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<VineShamblerPet>(), "Vine Shambler");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<ChomperPet>(), "Chomper");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<CubexConstructPet>(), "Cubex Construct");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<DampCultistPet>(), "Damp Cultist");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<CalcifiedCultistPet>(), "Calcified Cultist");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<CorpseSlugPet>(), "Corpse Slug");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<TwoTailedRatPet>(), "Two-Tailed Rat");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<SewerClamPet>(), "Sewer Clam");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<HauntedShipPet>(), "Haunted Ship");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<SludgeSpinnerPet>(), "Sludge Spinner");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<PunchConstructPet>(), "Punch Construct");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<FossilStalkerPet>(), "Fossil Stalker");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<LivingFogPet>(), "Living Fog");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<ParafrightPet>(), "Parafright");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<TunnelerPet>(), "Tunneler");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<SpinyToadPet>(), "Spiny Toad");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<StabbotPet>(), "Stabbot");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<HunterKillerPet>(), "Hunter Killer");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<TorchHeadAmalgamPet>(), "Torch Head Amalgam");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<BowlbugEggPet>(), "Egg Bowlbug");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<BowlbugNectarPet>(), "Nectar Bowlbug");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<BowlbugRockPet>(), "Rock Bowlbug");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<BowlbugSilkPet>(), "Silk Bowlbug");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<LouseProgenitorPet>(), "Louse Progenitor");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<SlumberingBeetlePet>(), "Slumbering Beetle");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<AxebotPet>(), "Axebot");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<BattleFriendV1Pet>(), "Battle Friend V1"); TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<BattleFriendV2Pet>(), "Battle Friend V2"); TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<BattleFriendV3Pet>(), "Battle Friend V3");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<CrusherPet>(), "Crusher");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<DevotedSculptorPet>(), "Devoted Sculptor"); TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<ExoskeletonPet>(), "Exoskeleton");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<FabricatorPet>(), "Fabricator"); TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<FlailKnightPet>(), "Flail Knight");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<FrogKnightPet>(), "Frog Knight"); TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<GasBombPet>(), "Gas Bomb");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<GlobeHeadPet>(), "Globe Head"); TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<GuardbotPet>(), "Guardbot");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<LivingShieldPet>(), "Living Shield"); TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<MagiKnightPet>(), "Magi Knight");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<MysteriousKnightPet>(), "Mysterious Knight"); TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<MytePet>(), "Myte");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<NibbitPet>(), "Nibbit"); TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<NoisebotPet>(), "Noisebot");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<OvicopterPet>(), "Ovicopter"); TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<OwlMagistratePet>(), "Owl Magistrate");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<PaelsLegionPet>(), "Pael's Legion"); TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<RocketPet>(), "Rocket");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<ScrollOfBitingPet>(), "Scroll of Biting"); TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<SlimedBerserkerPet>(), "Slimed Berserker");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<TheForgottenPet>(), "The Forgotten"); TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<TheLostPet>(), "The Lost");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<TheObscuraPet>(), "The Obscura"); TriggerToadpoleDeathAnimation(__instance.Player.PlayerCombatState.GetPet<ToadpolePet>());
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<ToughEggPet>(), "Tough Egg"); TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<TrackerRubyRaiderPet>(), "Tracker Ruby Raider");
        TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<TurretOperatorPet>(), "Turret Operator"); TriggerDeathAnimation(__instance.Player.PlayerCombatState.GetPet<ZapbotPet>(), "Zapbot");
    }

    private static void TriggerDeathAnimation(Creature? pet, string companionName)
    {
        if (pet == null || pet.IsDead)
        {
            return;
        }

        MainFile.Logger.Info($"Triggering {companionName} death animation.");
        _ = CreatureCmd.TriggerAnim(pet, "Dead", 1f);
    }

    private static void TriggerToadpoleDeathAnimation(Creature? toadpole)
    {
        if (toadpole == null || toadpole.IsDead)
            return;

        MainFile.Logger.Info("Triggering Toadpole native death animation.");
        var creatureNode = toadpole.GetCreatureNode();
        if (creatureNode != null)
            creatureNode.SpineAnimation.SetAnimation("die", loop: false);
        else
            _ = CreatureCmd.TriggerAnim(toadpole, "Dead", 1f);
    }

    private static void TriggerWrigglerDeathAnimation(Creature? wriggler)
    {
        if (wriggler == null || wriggler.IsDead)
        {
            return;
        }

        MainFile.Logger.Info("Triggering Wriggler death animation.");
        _ = TriggerFirstAvailableWrigglerDeathAnimation(wriggler);
    }

    private static async Task TriggerFirstAvailableWrigglerDeathAnimation(Creature wriggler)
    {
        foreach (string animationName in new[] { "Dead", "Death", "Die" })
        {
            try
            {
                await CreatureCmd.TriggerAnim(wriggler, animationName, 0.35f);
                return;
            }
            catch
            {
                MainFile.Logger.Info($"Wriggler did not have death animation '{animationName}'.");
            }
        }
    }

    private static void TriggerGremlinMercDeathAnimation(Player owner)
    {
        Creature? gremlinMerc = owner.PlayerCombatState?.GetPet<GremlinMercPet>();
        if (gremlinMerc == null || gremlinMerc.IsDead)
        {
            return;
        }

        MainFile.Logger.Info("Triggering Gremlin Merc split death animation.");
        _ = TriggerGremlinMercSplitSequence(owner, gremlinMerc);
    }

    private static async Task TriggerGremlinMercSplitSequence(Player owner, Creature gremlinMerc)
    {
        await CreatureCmd.TriggerAnim(gremlinMerc, "Dead", 0.55f);
        GremlinMercSplitVisuals.Show();

        Creature? fatGremlin = owner.PlayerCombatState?.GetPet<FatGremlinSplitPet>();
        Creature? sneakyGremlin = owner.PlayerCombatState?.GetPet<SneakyGremlinSplitPet>();

        if (fatGremlin != null && !fatGremlin.IsDead)
        {
            MainFile.Logger.Info("Triggering Fat Gremlin split escape animation.");
            _ = CreatureCmd.TriggerAnim(fatGremlin, "Run", 0.5f);
            await GremlinMercSplitVisuals.AnimateFatGremlinEscape();
            await CreatureCmd.Escape(fatGremlin, false);
        }

        if (sneakyGremlin != null && !sneakyGremlin.IsDead)
        {
            MainFile.Logger.Info("Triggering Sneaky Gremlin split death animation.");
            _ = CreatureCmd.TriggerAnim(sneakyGremlin, "Dead", 0.8f);
        }
    }
}
