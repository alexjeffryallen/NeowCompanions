using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace NeowCompanions.NeowCompanionsCode.Models;

[Pool(typeof(NeowCompanionRelicPool))] public sealed class CorpseSlugRelic : BossCompanionRelic<CorpseSlugPet> { protected override string CompanionName => "Corpse Slug"; protected override string RelicIconFileName => "relic_corpse_slug.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class CorpseSlugCard : NormalEnemyCard<CorpseSlugPet> { protected override string CompanionName => "Corpse Slug"; protected override string CardTitle => "Glomp"; protected override string CardArtFileName => "card_corpse_slug.png"; protected override decimal BaseDamage => 5m; protected override int HitCount => 2; protected override string AnimationName => "DoubleAttackTrigger"; }
public sealed class CorpseSlugPet : BossCompanionPet<CorpseSlug> { protected override float PetScale => 0.36f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class TwoTailedRatRelic : BossCompanionRelic<TwoTailedRatPet> { protected override string CompanionName => "Two-Tailed Rat"; protected override string RelicIconFileName => "relic_two_tailed_rat.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class TwoTailedRatCard : NormalEnemyCard<TwoTailedRatPet> { protected override string CompanionName => "Two-Tailed Rat"; protected override string CardTitle => "Disease Bite"; protected override string CardArtFileName => "card_two_tailed_rat.png"; protected override decimal BaseDamage => 9m; }
public sealed class TwoTailedRatPet : BossCompanionPet<TwoTailedRat> { protected override float PetScale => 0.38f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class SewerClamRelic : BossCompanionRelic<SewerClamPet> { protected override string CompanionName => "Sewer Clam"; protected override string RelicIconFileName => "relic_sewer_clam.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class SewerClamCard : NormalEnemyCard<SewerClamPet> { protected override string CompanionName => "Sewer Clam"; protected override string CardTitle => "Pressure Jet"; protected override string CardArtFileName => "card_sewer_clam.png"; protected override decimal BaseDamage => 10m; }
public sealed class SewerClamPet : BossCompanionPet<SewerClam> { protected override float PetScale => 0.38f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class HauntedShipRelic : BossCompanionRelic<HauntedShipPet> { protected override string CompanionName => "Haunted Ship"; protected override string RelicIconFileName => "relic_haunted_ship.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class HauntedShipCard : NormalEnemyCard<HauntedShipPet> { protected override string CompanionName => "Haunted Ship"; protected override string CardTitle => "Spectral Broadside"; protected override string CardArtFileName => "card_haunted_ship.png"; protected override decimal BaseDamage => 3m; protected override int HitCount => 3; protected override string AnimationName => "AttackTriple"; }
public sealed class HauntedShipPet : BossCompanionPet<HauntedShip> { protected override float PetScale => 0.28f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class SludgeSpinnerRelic : BossCompanionRelic<SludgeSpinnerPet> { protected override string CompanionName => "Sludge Spinner"; protected override string RelicIconFileName => "relic_sludge_spinner.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class SludgeSpinnerCard : NormalEnemyCard<SludgeSpinnerPet> { protected override string CompanionName => "Sludge Spinner"; protected override string CardTitle => "Sludge Slam"; protected override string CardArtFileName => "card_sludge_spinner.png"; protected override decimal BaseDamage => 10m; protected override string AnimationName => "slam"; }
public sealed class SludgeSpinnerPet : BossCompanionPet<SludgeSpinner> { protected override float PetScale => 0.38f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class PunchConstructRelic : BossCompanionRelic<PunchConstructPet> { protected override string CompanionName => "Punch Construct"; protected override string RelicIconFileName => "relic_punch_construct.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class PunchConstructCard : NormalEnemyCard<PunchConstructPet> { protected override string CompanionName => "Punch Construct"; protected override string CardTitle => "Double Punch"; protected override string CardArtFileName => "card_punch_construct.png"; protected override decimal BaseDamage => 5m; protected override int HitCount => 2; protected override string AnimationName => "DoubleAttack"; }
public sealed class PunchConstructPet : BossCompanionPet<PunchConstruct> { protected override float PetScale => 0.34f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class FossilStalkerRelic : BossCompanionRelic<FossilStalkerPet> { protected override string CompanionName => "Fossil Stalker"; protected override string RelicIconFileName => "relic_fossil_stalker.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class FossilStalkerCard : NormalEnemyCard<FossilStalkerPet> { protected override string CompanionName => "Fossil Stalker"; protected override string CardTitle => "Fossil Lash"; protected override string CardArtFileName => "card_fossil_stalker.png"; protected override decimal BaseDamage => 5m; protected override int HitCount => 2; protected override string AnimationName => "AttackDouble"; }
public sealed class FossilStalkerPet : BossCompanionPet<FossilStalker> { protected override float PetScale => 0.34f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class LivingFogRelic : BossCompanionRelic<LivingFogPet> { protected override string CompanionName => "Living Fog"; protected override string RelicIconFileName => "relic_living_fog.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class LivingFogCard : NormalEnemyCard<LivingFogPet> { protected override string CompanionName => "Living Fog"; protected override string CardTitle => "Gas Blast"; protected override string CardArtFileName => "card_living_fog.png"; protected override decimal BaseDamage => 10m; }
public sealed class LivingFogPet : BossCompanionPet<LivingFog> { protected override float PetScale => 0.32f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class ParafrightRelic : BossCompanionRelic<ParafrightPet> { protected override string CompanionName => "Parafright"; protected override string RelicIconFileName => "relic_parafright.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class ParafrightCard : NormalEnemyCard<ParafrightPet> { protected override string CompanionName => "Parafright"; protected override string CardTitle => "Hologram Slam"; protected override string CardArtFileName => "card_parafright.png"; protected override decimal BaseDamage => 10m; }
public sealed class ParafrightPet : BossCompanionPet<Parafright> { protected override float PetScale => 0.36f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class TunnelerRelic : BossCompanionRelic<TunnelerPet> { protected override string CompanionName => "Tunneler"; protected override string RelicIconFileName => "relic_tunneler.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class TunnelerCard : NormalEnemyCard<TunnelerPet> { protected override string CompanionName => "Tunneler"; protected override string CardTitle => "Burrow Attack"; protected override string CardArtFileName => "card_tunneler.png"; protected override decimal BaseDamage => 11m; protected override string AnimationName => "BurrowAttack"; }
public sealed class TunnelerPet : BossCompanionPet<Tunneler> { protected override float PetScale => 0.34f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class SpinyToadRelic : BossCompanionRelic<SpinyToadPet> { protected override string CompanionName => "Spiny Toad"; protected override string RelicIconFileName => "relic_spiny_toad.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class SpinyToadCard : NormalEnemyCard<SpinyToadPet> { protected override string CompanionName => "Spiny Toad"; protected override string CardTitle => "Tongue Lash"; protected override string CardArtFileName => "card_spiny_toad.png"; protected override decimal BaseDamage => 10m; protected override string AnimationName => "lick"; }
public sealed class SpinyToadPet : BossCompanionPet<SpinyToad> { protected override float PetScale => 0.34f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class StabbotRelic : BossCompanionRelic<StabbotPet> { protected override string CompanionName => "Stabbot"; protected override string RelicIconFileName => "relic_stabbot.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class StabbotCard : NormalEnemyCard<StabbotPet> { protected override string CompanionName => "Stabbot"; protected override string CardTitle => "Mechanical Stab"; protected override string CardArtFileName => "card_stabbot.png"; protected override decimal BaseDamage => 9m; }
public sealed class StabbotPet : BossCompanionPet<Stabbot> { protected override float PetScale => 0.38f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class HunterKillerRelic : BossCompanionRelic<HunterKillerPet> { protected override string CompanionName => "Hunter Killer"; protected override string RelicIconFileName => "relic_hunter_killer.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class HunterKillerCard : NormalEnemyCard<HunterKillerPet> { protected override string CompanionName => "Hunter Killer"; protected override string CardTitle => "Puncture"; protected override string CardArtFileName => "card_hunter_killer.png"; protected override decimal BaseDamage => 3m; protected override int HitCount => 3; protected override string AnimationName => "TripleAttack"; }
public sealed class HunterKillerPet : BossCompanionPet<HunterKiller> { protected override float PetScale => 0.32f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class TorchHeadAmalgamRelic : BossCompanionRelic<TorchHeadAmalgamPet> { protected override string CompanionName => "Torch Head Amalgam"; protected override string RelicIconFileName => "relic_torch_head_amalgam.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class TorchHeadAmalgamCard : NormalEnemyCard<TorchHeadAmalgamPet> { protected override string CompanionName => "Torch Head Amalgam"; protected override string CardTitle => "Focused Beam"; protected override string CardArtFileName => "card_torch_head_amalgam.png"; protected override decimal BaseDamage => 11m; }
public sealed class TorchHeadAmalgamPet : BossCompanionPet<TorchHeadAmalgam> { protected override float PetScale => 0.32f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class BowlbugEggRelic : BossCompanionRelic<BowlbugEggPet> { protected override string CompanionName => "Egg Bowlbug"; protected override string RelicIconFileName => "relic_bowlbug_egg.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class BowlbugEggCard : NormalEnemyCard<BowlbugEggPet> { protected override string CompanionName => "Egg Bowlbug"; protected override string CardTitle => "Cocoon Bite"; protected override string CardArtFileName => "card_bowlbug_egg.png"; protected override decimal BaseDamage => 8m; }
public sealed class BowlbugEggPet : BossCompanionPet<BowlbugEgg> { protected override float PetScale => 0.40f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class BowlbugNectarRelic : BossCompanionRelic<BowlbugNectarPet> { protected override string CompanionName => "Nectar Bowlbug"; protected override string RelicIconFileName => "relic_bowlbug_nectar.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class BowlbugNectarCard : NormalEnemyCard<BowlbugNectarPet> { protected override string CompanionName => "Nectar Bowlbug"; protected override string CardTitle => "Nectar Thrash"; protected override string CardArtFileName => "card_bowlbug_nectar.png"; protected override decimal BaseDamage => 9m; }
public sealed class BowlbugNectarPet : BossCompanionPet<BowlbugNectar> { protected override float PetScale => 0.40f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class BowlbugRockRelic : BossCompanionRelic<BowlbugRockPet> { protected override string CompanionName => "Rock Bowlbug"; protected override string RelicIconFileName => "relic_bowlbug_rock.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class BowlbugRockCard : NormalEnemyCard<BowlbugRockPet> { protected override string CompanionName => "Rock Bowlbug"; protected override string CardTitle => "Rock Headbutt"; protected override string CardArtFileName => "card_bowlbug_rock.png"; protected override decimal BaseDamage => 10m; protected override string AnimationName => "headbutt"; }
public sealed class BowlbugRockPet : BossCompanionPet<BowlbugRock> { protected override float PetScale => 0.40f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class BowlbugSilkRelic : BossCompanionRelic<BowlbugSilkPet> { protected override string CompanionName => "Silk Bowlbug"; protected override string RelicIconFileName => "relic_bowlbug_silk.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class BowlbugSilkCard : NormalEnemyCard<BowlbugSilkPet> { protected override string CompanionName => "Silk Bowlbug"; protected override string CardTitle => "Toxic Spit"; protected override string CardArtFileName => "card_bowlbug_silk.png"; protected override decimal BaseDamage => 9m; }
public sealed class BowlbugSilkPet : BossCompanionPet<BowlbugSilk> { protected override float PetScale => 0.40f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class LouseProgenitorRelic : BossCompanionRelic<LouseProgenitorPet> { protected override string CompanionName => "Louse Progenitor"; protected override string RelicIconFileName => "relic_louse_progenitor.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class LouseProgenitorCard : NormalEnemyCard<LouseProgenitorPet> { protected override string CompanionName => "Louse Progenitor"; protected override string CardTitle => "Web Cannon"; protected override string CardArtFileName => "card_louse_progenitor.png"; protected override decimal BaseDamage => 10m; protected override string AnimationName => "Web"; }
public sealed class LouseProgenitorPet : BossCompanionPet<LouseProgenitor> { protected override float PetScale => 0.32f; }

[Pool(typeof(NeowCompanionRelicPool))] public sealed class SlumberingBeetleRelic : BossCompanionRelic<SlumberingBeetlePet> { protected override string CompanionName => "Slumbering Beetle"; protected override string RelicIconFileName => "relic_slumbering_beetle.png"; }
[Pool(typeof(NeowCompanionCardPool))] public sealed class SlumberingBeetleCard : NormalEnemyCard<SlumberingBeetlePet> { protected override string CompanionName => "Slumbering Beetle"; protected override string CardTitle => "Roll Out"; protected override string CardArtFileName => "card_slumbering_beetle.png"; protected override decimal BaseDamage => 11m; protected override string AnimationName => "Rollout"; }
public sealed class SlumberingBeetlePet : BossCompanionPet<SlumberingBeetle> { protected override float PetScale => 0.34f; }
