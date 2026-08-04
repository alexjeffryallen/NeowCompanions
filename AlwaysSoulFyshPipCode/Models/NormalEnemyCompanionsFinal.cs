using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Monsters;
using System.Threading.Tasks;

namespace NeowCompanions.NeowCompanionsCode.Models;

[Pool(typeof(NeowCompanionRelicPool))] public sealed class AxebotRelic : BossCompanionRelic<AxebotPet> { protected override string CompanionName => "Axebot"; protected override string RelicIconFileName => "relic_axebot.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class AxebotCard : NormalEnemyCard<AxebotPet> { protected override string CompanionName => "Axebot"; protected override string CardTitle => "Hammer Uppercut"; protected override string CardArtFileName => "card_axebot.png"; protected override decimal BaseDamage => 10m; }
public sealed class AxebotPet : BossCompanionPet<Axebot> { protected override float PetScale => 0.36f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class BattleFriendV1Relic : BossCompanionRelic<BattleFriendV1Pet> { protected override string CompanionName => "Battle Friend V1"; protected override string RelicIconFileName => "relic_battle_friend_v1.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class BattleFriendV1Card : NormalEnemyCard<BattleFriendV1Pet> { protected override string CompanionName => "Battle Friend V1"; protected override string CardTitle => "Friendly Bump"; protected override string CardArtFileName => "card_battle_friend_v1.png"; protected override decimal BaseDamage => 7m; protected override string AnimationName => "Idle"; }
public sealed class BattleFriendV1Pet : BossCompanionPet<BattleFriendV1> { protected override float PetScale => 0.36f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class BattleFriendV2Relic : BossCompanionRelic<BattleFriendV2Pet> { protected override string CompanionName => "Battle Friend V2"; protected override string RelicIconFileName => "relic_battle_friend_v2.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class BattleFriendV2Card : NormalEnemyCard<BattleFriendV2Pet> { protected override string CompanionName => "Battle Friend V2"; protected override string CardTitle => "Friendly Bash"; protected override string CardArtFileName => "card_battle_friend_v2.png"; protected override decimal BaseDamage => 9m; protected override string AnimationName => "Idle"; }
public sealed class BattleFriendV2Pet : BossCompanionPet<BattleFriendV2> { protected override float PetScale => 0.34f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class BattleFriendV3Relic : BossCompanionRelic<BattleFriendV3Pet> { protected override string CompanionName => "Battle Friend V3"; protected override string RelicIconFileName => "relic_battle_friend_v3.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class BattleFriendV3Card : NormalEnemyCard<BattleFriendV3Pet> { protected override string CompanionName => "Battle Friend V3"; protected override string CardTitle => "Friendly Crush"; protected override string CardArtFileName => "card_battle_friend_v3.png"; protected override decimal BaseDamage => 12m; protected override string AnimationName => "Idle"; }
public sealed class BattleFriendV3Pet : BossCompanionPet<BattleFriendV3> { protected override float PetScale => 0.32f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class CrusherRelic : BossCompanionRelic<CrusherPet> { protected override string CompanionName => "Crusher"; protected override string RelicIconFileName => "relic_crusher.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class CrusherCard : NormalEnemyCard<CrusherPet> { protected override string CompanionName => "Crusher"; protected override string CardTitle => "Enlarging Strike"; protected override string CardArtFileName => "card_crusher.png"; protected override decimal BaseDamage => 12m; protected override string AnimationName => "attack_heavy"; }
public sealed class CrusherPet : BossCompanionPet<Crusher> { protected override float PetScale => 0.30f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class DevotedSculptorRelic : BossCompanionRelic<DevotedSculptorPet> { protected override string CompanionName => "Devoted Sculptor"; protected override string RelicIconFileName => "relic_devoted_sculptor.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class DevotedSculptorCard : NormalEnemyCard<DevotedSculptorPet> { protected override string CompanionName => "Devoted Sculptor"; protected override string CardTitle => "Savage Chisel"; protected override string CardArtFileName => "card_devoted_sculptor.png"; protected override decimal BaseDamage => 11m; }
public sealed class DevotedSculptorPet : BossCompanionPet<DevotedSculptor> { protected override float PetScale => 0.32f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class ExoskeletonRelic : BossCompanionRelic<ExoskeletonPet> { protected override string CompanionName => "Exoskeleton"; protected override string RelicIconFileName => "relic_exoskeleton.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class ExoskeletonCard : NormalEnemyCard<ExoskeletonPet> { protected override string CompanionName => "Exoskeleton"; protected override string CardTitle => "Heavy Mandibles"; protected override string CardArtFileName => "card_exoskeleton.png"; protected override decimal BaseDamage => 11m; protected override string AnimationName => "HeavyAttack"; }
public sealed class ExoskeletonPet : BossCompanionPet<Exoskeleton> { protected override float PetScale => 0.38f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class FabricatorRelic : BossCompanionRelic<FabricatorPet> { protected override string CompanionName => "Fabricator"; protected override string RelicIconFileName => "relic_fabricator.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class FabricatorCard : NormalEnemyCard<FabricatorPet> { protected override string CompanionName => "Fabricator"; protected override string CardTitle => "Disintegrate"; protected override string CardArtFileName => "card_fabricator.png"; protected override decimal BaseDamage => 12m; }
public sealed class FabricatorPet : BossCompanionPet<Fabricator> { protected override float PetScale => 0.30f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class FlailKnightRelic : BossCompanionRelic<FlailKnightPet> { protected override string CompanionName => "Flail Knight"; protected override string RelicIconFileName => "relic_flail_knight.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class FlailKnightCard : NormalEnemyCard<FlailKnightPet> { protected override string CompanionName => "Flail Knight"; protected override string CardTitle => "Flail Assault"; protected override string CardArtFileName => "card_flail_knight.png"; protected override decimal BaseDamage => 11m; protected override string AnimationName => "FlailAttack"; }
public sealed class FlailKnightPet : BossCompanionPet<FlailKnight> { protected override float PetScale => 0.32f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class FrogKnightRelic : BossCompanionRelic<FrogKnightPet> { protected override string CompanionName => "Frog Knight"; protected override string RelicIconFileName => "relic_frog_knight.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class FrogKnightCard : NormalEnemyCard<FrogKnightPet> { protected override string CompanionName => "Frog Knight"; protected override string CardTitle => "Tongue Charge"; protected override string CardArtFileName => "card_frog_knight.png"; protected override decimal BaseDamage => 11m; protected override string AnimationName => "Lash"; }
public sealed class FrogKnightPet : BossCompanionPet<FrogKnight> { protected override float PetScale => 0.30f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class GasBombRelic : BossCompanionRelic<GasBombPet> { protected override string CompanionName => "Gas Bomb"; protected override string RelicIconFileName => "relic_gas_bomb.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class GasBombCard : NormalEnemyCard<GasBombPet> { protected override string CompanionName => "Gas Bomb"; protected override string CardTitle => "Controlled Explosion"; protected override string CardArtFileName => "card_gas_bomb.png"; protected override decimal BaseDamage => 12m; protected override string AnimationName => "ExplodeTrigger"; }
public sealed class GasBombPet : BossCompanionPet<GasBomb> { protected override float PetScale => 0.42f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class GlobeHeadRelic : BossCompanionRelic<GlobeHeadPet> { protected override string CompanionName => "Globe Head"; protected override string RelicIconFileName => "relic_globe_head.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class GlobeHeadCard : NormalEnemyCard<GlobeHeadPet> { protected override string CompanionName => "Globe Head"; protected override string CardTitle => "Thunder Strike"; protected override string CardArtFileName => "card_globe_head.png"; protected override decimal BaseDamage => 12m; }
public sealed class GlobeHeadPet : BossCompanionPet<GlobeHead> { protected override float PetScale => 0.30f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class GuardbotRelic : BossCompanionRelic<GuardbotPet> { protected override string CompanionName => "Guardbot"; protected override string RelicIconFileName => "relic_guardbot.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class GuardbotCard : NormalEnemyCard<GuardbotPet> { protected override string CompanionName => "Guardbot"; protected override string CardTitle => "Guard Bash"; protected override string CardArtFileName => "card_guardbot.png"; protected override decimal BaseDamage => 9m; protected override string AnimationName => "Cast"; }
public sealed class GuardbotPet : BossCompanionPet<Guardbot> { protected override float PetScale => 0.38f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class LivingShieldRelic : BossCompanionRelic<LivingShieldPet> { protected override string CompanionName => "Living Shield"; protected override string RelicIconFileName => "relic_living_shield.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class LivingShieldCard : NormalEnemyCard<LivingShieldPet> { protected override string CompanionName => "Living Shield"; protected override string CardTitle => "Shield Slam"; protected override string CardArtFileName => "card_living_shield.png"; protected override decimal BaseDamage => 10m; }
public sealed class LivingShieldPet : BossCompanionPet<LivingShield> { protected override float PetScale => 0.34f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class MagiKnightRelic : BossCompanionRelic<MagiKnightPet> { protected override string CompanionName => "Magi Knight"; protected override string RelicIconFileName => "relic_magi_knight.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class MagiKnightCard : NormalEnemyCard<MagiKnightPet> { protected override string CompanionName => "Magi Knight"; protected override string CardTitle => "Magic Bomb"; protected override string CardArtFileName => "card_magi_knight.png"; protected override decimal BaseDamage => 11m; protected override string AnimationName => "BombCast"; }
public sealed class MagiKnightPet : BossCompanionPet<MagiKnight> { protected override float PetScale => 0.32f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class MysteriousKnightRelic : BossCompanionRelic<MysteriousKnightPet> { protected override string CompanionName => "Mysterious Knight"; protected override string RelicIconFileName => "relic_mysterious_knight.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class MysteriousKnightCard : NormalEnemyCard<MysteriousKnightPet> { protected override string CompanionName => "Mysterious Knight"; protected override string CardTitle => "Mysterious Ram"; protected override string CardArtFileName => "card_mysterious_knight.png"; protected override decimal BaseDamage => 11m; protected override string AnimationName => "RamAttack"; }
public sealed class MysteriousKnightPet : BossCompanionPet<MysteriousKnight> { protected override float PetScale => 0.32f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class MyteRelic : BossCompanionRelic<MytePet> { protected override string CompanionName => "Myte"; protected override string RelicIconFileName => "relic_myte.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class MyteCard : NormalEnemyCard<MytePet> { protected override string CompanionName => "Myte"; protected override string CardTitle => "Toxic Bite"; protected override string CardArtFileName => "card_myte.png"; protected override decimal BaseDamage => 8m; }
public sealed class MytePet : BossCompanionPet<Myte> { protected override float PetScale => 0.42f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class NibbitRelic : BossCompanionRelic<NibbitPet> { protected override string CompanionName => "Nibbit"; protected override string RelicIconFileName => "relic_nibbit.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class NibbitCard : NormalEnemyCard<NibbitPet> { protected override string CompanionName => "Nibbit"; protected override string CardTitle => "Nibbit Slice"; protected override string CardArtFileName => "card_nibbit.png"; protected override decimal BaseDamage => 9m; }
public sealed class NibbitPet : BossCompanionPet<Nibbit> { protected override float PetScale => 0.40f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class NoisebotRelic : BossCompanionRelic<NoisebotPet> { protected override string CompanionName => "Noisebot"; protected override string RelicIconFileName => "relic_noisebot.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class NoisebotCard : NormalEnemyCard<NoisebotPet> { protected override string CompanionName => "Noisebot"; protected override string CardTitle => "Sonic Burst"; protected override string CardArtFileName => "card_noisebot.png"; protected override decimal BaseDamage => 9m; protected override string AnimationName => "Cast"; }
public sealed class NoisebotPet : BossCompanionPet<Noisebot> { protected override float PetScale => 0.38f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class OvicopterRelic : BossCompanionRelic<OvicopterPet> { protected override string CompanionName => "Ovicopter"; protected override string RelicIconFileName => "relic_ovicopter.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class OvicopterCard : NormalEnemyCard<OvicopterPet> { protected override string CompanionName => "Ovicopter"; protected override string CardTitle => "Tenderizer"; protected override string CardArtFileName => "card_ovicopter.png"; protected override decimal BaseDamage => 11m; }
public sealed class OvicopterPet : BossCompanionPet<Ovicopter> { protected override float PetScale => 0.30f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class OwlMagistrateRelic : BossCompanionRelic<OwlMagistratePet> { protected override string CompanionName => "Owl Magistrate"; protected override string RelicIconFileName => "relic_owl_magistrate.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class OwlMagistrateCard : NormalEnemyCard<OwlMagistratePet> { protected override string CompanionName => "Owl Magistrate"; protected override string CardTitle => "Final Verdict"; protected override string CardArtFileName => "card_owl_magistrate.png"; protected override decimal BaseDamage => 12m; }
public sealed class OwlMagistratePet : BossCompanionPet<OwlMagistrate> { protected override float PetScale => 0.30f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class PaelsLegionRelic : BossCompanionRelic<PaelsLegionPet> { protected override string CompanionName => "Pael's Legion"; protected override string RelicIconFileName => "relic_paels_legion.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class PaelsLegionCard : NormalEnemyCard<PaelsLegionPet> { protected override string CompanionName => "Pael's Legion"; protected override string CardTitle => "Legion Charge"; protected override string CardArtFileName => "card_paels_legion.png"; protected override decimal BaseDamage => 11m; protected override string AnimationName => "WakeUpTrigger"; }
public sealed class PaelsLegionPet : BossCompanionPet<PaelsLegion> { protected override float PetScale => 0.28f; protected override bool FlipHorizontally => false; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class RocketRelic : BossCompanionRelic<RocketPet> { protected override string CompanionName => "Rocket"; protected override string RelicIconFileName => "relic_rocket.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class RocketCard : NormalEnemyCard<RocketPet> { protected override string CompanionName => "Rocket"; protected override string CardTitle => "Precision Beam"; protected override string CardArtFileName => "card_rocket.png"; protected override decimal BaseDamage => 12m; protected override string AnimationName => "attack_med"; }
public sealed class RocketPet : BossCompanionPet<Rocket> { protected override float PetScale => 0.30f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class ScrollOfBitingRelic : BossCompanionRelic<ScrollOfBitingPet> { protected override string CompanionName => "Scroll of Biting"; protected override string RelicIconFileName => "relic_scroll_of_biting.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class ScrollOfBitingCard : NormalEnemyCard<ScrollOfBitingPet> { protected override string CompanionName => "Scroll of Biting"; protected override string CardTitle => "More Teeth"; protected override string CardArtFileName => "card_scroll_of_biting.png"; protected override decimal BaseDamage => 5m; protected override int HitCount => 2; protected override string AnimationName => "ATTACK_DOUBLE"; }
public sealed class ScrollOfBitingPet : BossCompanionPet<ScrollOfBiting> { protected override float PetScale => 0.38f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class SlimedBerserkerRelic : BossCompanionRelic<SlimedBerserkerPet> { protected override string CompanionName => "Slimed Berserker"; protected override string RelicIconFileName => "relic_slimed_berserker.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class SlimedBerserkerCard : NormalEnemyCard<SlimedBerserkerPet> { protected override string CompanionName => "Slimed Berserker"; protected override string CardTitle => "Furious Pummeling"; protected override string CardArtFileName => "card_slimed_berserker.png"; protected override decimal BaseDamage => 4m; protected override int HitCount => 3; }
public sealed class SlimedBerserkerPet : BossCompanionPet<SlimedBerserker> { protected override float PetScale => 0.28f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class TheForgottenRelic : BossCompanionRelic<TheForgottenPet> { protected override string CompanionName => "The Forgotten"; protected override string RelicIconFileName => "relic_the_forgotten.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class TheForgottenCard : NormalEnemyCard<TheForgottenPet> { protected override string CompanionName => "The Forgotten"; protected override string CardTitle => "Dread Miasma"; protected override string CardArtFileName => "card_the_forgotten.png"; protected override decimal BaseDamage => 11m; }
public sealed class TheForgottenPet : BossCompanionPet<TheForgotten> { protected override float PetScale => 0.32f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class TheLostRelic : BossCompanionRelic<TheLostPet> { protected override string CompanionName => "The Lost"; protected override string RelicIconFileName => "relic_the_lost.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class TheLostCard : NormalEnemyCard<TheLostPet> { protected override string CompanionName => "The Lost"; protected override string CardTitle => "Eye Lasers"; protected override string CardArtFileName => "card_the_lost.png"; protected override decimal BaseDamage => 11m; }
public sealed class TheLostPet : BossCompanionPet<TheLost> { protected override float PetScale => 0.32f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class TheObscuraRelic : BossCompanionRelic<TheObscuraPet> { protected override string CompanionName => "The Obscura"; protected override string RelicIconFileName => "relic_the_obscura.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class TheObscuraCard : NormalEnemyCard<TheObscuraPet> { protected override string CompanionName => "The Obscura"; protected override string CardTitle => "Hardening Strike"; protected override string CardArtFileName => "card_the_obscura.png"; protected override decimal BaseDamage => 11m; }
public sealed class TheObscuraPet : BossCompanionPet<TheObscura> { protected override float PetScale => 0.30f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class ToadpoleRelic : BossCompanionRelic<ToadpolePet> { protected override string CompanionName => "Toadpole"; protected override string RelicIconFileName => "relic_toadpole.png"; }
[Pool(typeof(NeowCompanionCardPool))]
public sealed class ToadpoleCard : NormalEnemyCard<ToadpolePet>
{
    protected override string CompanionName => "Toadpole";
    protected override string CardTitle => "Spike Whirl";
    protected override string CardArtFileName => "card_toadpole.png";
    protected override decimal BaseDamage => 3m;
    protected override int HitCount => 3;

    protected override Task TriggerAttackAnimation()
    {
        Creature? toadpole = Owner.PlayerCombatState?.GetPet<ToadpolePet>();
        var creatureNode = toadpole?.GetCreatureNode();
        if (creatureNode == null)
            return base.TriggerAttackAnimation();

        SfxCmd.Play("event:/sfx/enemy/enemy_attacks/toadpole/toadpole_attack_spin");
        creatureNode.SpineAnimation.SetAnimation("attack_triple", loop: false);
        creatureNode.SpineAnimation.AddAnimation("idle_loop", 0f, loop: true);
        return Cmd.Wait(0.2f);
    }
}
public sealed class ToadpolePet : BossCompanionPet<Toadpole> { protected override float PetScale => 0.40f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class ToughEggRelic : BossCompanionRelic<ToughEggPet> { protected override string CompanionName => "Tough Egg"; protected override string RelicIconFileName => "relic_tough_egg.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class ToughEggCard : NormalEnemyCard<ToughEggPet> { protected override string CompanionName => "Tough Egg"; protected override string CardTitle => "Hatchling Nibble"; protected override string CardArtFileName => "card_tough_egg.png"; protected override decimal BaseDamage => 9m; }
public sealed class ToughEggPet : BossCompanionPet<ToughEgg> { protected override float PetScale => 0.40f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class TrackerRubyRaiderRelic : BossCompanionRelic<TrackerRubyRaiderPet> { protected override string CompanionName => "Tracker Ruby Raider"; protected override string RelicIconFileName => "relic_tracker_ruby_raider.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class TrackerRubyRaiderCard : NormalEnemyCard<TrackerRubyRaiderPet> { protected override string CompanionName => "Tracker Ruby Raider"; protected override string CardTitle => "Release Hounds"; protected override string CardArtFileName => "card_tracker_ruby_raider.png"; protected override decimal BaseDamage => 10m; }
public sealed class TrackerRubyRaiderPet : BossCompanionPet<TrackerRubyRaider> { protected override float PetScale => 0.32f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class TurretOperatorRelic : BossCompanionRelic<TurretOperatorPet> { protected override string CompanionName => "Turret Operator"; protected override string RelicIconFileName => "relic_turret_operator.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class TurretOperatorCard : NormalEnemyCard<TurretOperatorPet> { protected override string CompanionName => "Turret Operator"; protected override string CardTitle => "Unload"; protected override string CardArtFileName => "card_turret_operator.png"; protected override decimal BaseDamage => 4m; protected override int HitCount => 3; }
public sealed class TurretOperatorPet : BossCompanionPet<TurretOperator> { protected override float PetScale => 0.34f; }
[Pool(typeof(NeowCompanionRelicPool))] public sealed class ZapbotRelic : BossCompanionRelic<ZapbotPet> { protected override string CompanionName => "Zapbot"; protected override string RelicIconFileName => "relic_zapbot.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class ZapbotCard : NormalEnemyCard<ZapbotPet> { protected override string CompanionName => "Zapbot"; protected override string CardTitle => "High Voltage"; protected override string CardArtFileName => "card_zapbot.png"; protected override decimal BaseDamage => 9m; }
public sealed class ZapbotPet : BossCompanionPet<Zapbot> { protected override float PetScale => 0.38f; }
