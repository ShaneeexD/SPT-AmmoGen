export interface AmmoPackDefinition {
  enabled: boolean
  name: string
  ammo: AmmoDefinition[]
  grenades: GrenadeDefinition[]
  flares: FlareDefinition[]
  modFilterPatches: ModFilterPatch[]
}

export type FlareKind = 'handheld' | 'cartridge'

export const FLARE_KIND_OPTIONS: { value: FlareKind; label: string }[] = [
  { value: 'handheld', label: 'Handheld flare (RSP-30/ROP-30 style)' },
  { value: 'cartridge', label: 'Signal pistol cartridge (SP-81)' },
]

export interface FlareDefinition {
  id: string
  ammoId: string
  kind: FlareKind
  enabled: boolean
  baseTpl: string
  ammoBaseTpl: string
  compareToFlareId?: string
  name: string
  shortName: string
  description: string
  handbookParentId?: string
  stats: FlareStats
  economy: AmmoEconomy
  traders: TraderEntry[]
  crafting: CraftingEntry
  loot: LootEntry
}

export interface FlareStats {
  damage: number
  initialSpeed: number
  stackMaxSize: number
  ammoLifeTimeSec: number
  tracer: boolean
  tracerColor: string
  tracerDistance: number
  backgroundColor: string
  flareColor: string
  weight: number
  misfireChance: number
  ricochetChance: number
  flareTypes: string[]
  airDropTemplateId: string
  casingSounds: string
  ammoType: string
  weapClass: string
  isSpecialSlotOnly: boolean
}

export interface AmmoDefinition {
  id: string
  enabled: boolean
  baseTpl: string
  compareToAmmoId?: string
  name: string
  shortName: string
  description: string
  handbookParentId?: string
  stats: AmmoStats
  economy: AmmoEconomy
  traders: TraderEntry[]
  crafting: CraftingEntry
  filters: FilterEntry
  ammoBox: AmmoBoxEntry
  ammoLoot: LootEntry
  ammoBoxLoot: LootEntry
}

export interface GrenadeDefinition {
  id: string
  enabled: boolean
  baseTpl: string
  compareToGrenadeId?: string
  name: string
  shortName: string
  description: string
  handbookParentId?: string
  stats: GrenadeStats
  economy: AmmoEconomy
  traders: TraderEntry[]
  crafting: CraftingEntry
  loot: LootEntry
}

export interface Vector3 {
  x: number
  y: number
  z: number
}

export function createDefaultVector3(): Vector3 {
  return { x: 0, y: 0, z: 0 }
}

export interface AmmoStats {
  damage: number
  penetration: number
  armorDamage: number
  initialSpeed: number
  ammoAccr: number
  ammoRec: number
  stackMaxSize: number
  lightBleedingDelta: number
  heavyBleedingDelta: number
  durabilityBurnModificator: number
  ballisticCoeficient: number

  // Projectile / flight
  projectileCount: number
  ricochetChance: number
  fragmentationChance: number
  penetrationDamageMod: number
  penetrationChanceObstacle: number
  ammoLifeTimeSec: number
  bulletMassGram: number
  bulletDiameterMilimeters: number

  // Malfunctions / durability
  misfireChance: number
  malfMisfireChance: number
  malfFeedChance: number
  heatFactor: number
  staminaBurnPerDamage: number

  // Tracer
  tracer: boolean
  tracerColor: string
  tracerDistance: number

  // Audio / visual
  ammoSfx: string
  casingSounds: string
  backgroundColor: string

  // Explosive / grenade rounds
  fuzeArmTimeSec: number
  minExplosionDistance: number
  maxExplosionDistance: number
  fragmentsCount: number
  fragmentType: string
  explosionType: string
  explosionStrength: number
  showHitEffectOnExplode: boolean

  // Light-and-sound rounds
  isLightAndSoundShot: boolean
  lightAndSoundShotAngle: number
  lightAndSoundShotSelfContusionTime: number
  lightAndSoundShotSelfContusionStrength: number

  // Vector3 effect fields
  armorDistanceDistanceDamage: Vector3
  contusion: Vector3
  blindness: Vector3
}

export interface GrenadeStats {
  minExplosionDistance: number
  maxExplosionDistance: number
  fragmentsCount: number
  fragmentType: string
  explosionEffectType: string
  armorDistanceDistanceDamage: Vector3
  contusion: Vector3
  blindness: Vector3
  contusionDistance: number
  explDelay: number
  minTimeToContactExplode: number
  playFuzeSound: boolean
  strength: number
  minFragmentDamage: number
  canPlantOnGround: boolean
  throwType: string
  throwDamMax: number
  weight: number
  backgroundColor: string
  smokeColor: string
  bodyColor: string
  smokeRadius: number
  smokeDuration: number
  smokeFillSize: number
  smokeSizeOverTime: SmokeSizeKeyframe[]
  smokeStartSpeed: SmokeSpeedRange[]
  overrideSmokeRadius: boolean
  overrideSmokeDuration: boolean
  overrideSmokeFillSize: boolean
  overrideSmokeSizeOverTime: boolean
  overrideSmokeStartSpeed: boolean
}

export interface SmokeSizeKeyframe {
  time: number
  value: number
}

export interface SmokeSpeedRange {
  x: number
  y: number
}

export interface AmmoEconomy {
  handbookPriceRoubles: number
  fleaPriceRoubles: number
  rarityPvE: string
  fleaBanned: boolean
}

export interface TraderEntry {
  enabled: boolean
  traderId: string
  loyaltyLevel: number
  priceRoubles: number
  stockCount: number
  buyRestrictionMax: number
  unlimitedStock: boolean
  unlimitedBuyRestriction: boolean
}

export interface CraftingEntry {
  enabled: boolean
  workbenchLevel: number
  craftTimeSeconds: number
  outputCount: number
  requirements: CraftRequirement[]
}

export interface CraftRequirement {
  tpl: string
  count: number
}

export interface FilterEntry {
  patchMagazines: string[]
  patchWeapons: string[]
}

export interface ModFilterPatch {
  guid: string
  name: string
  ammoIds: string[]
  weaponIds: string[]
  magazineIds: string[]
}

export interface AmmoBoxEntry {
  id: string
  enabled: boolean
  baseTpl: string
  count: number
  name: string
  shortName: string
  description: string
  handbookPriceRoubles: number
  rarityPvE: string
  backgroundColor: string
  sellToTraders: boolean
  traderPriceRoubles: number
  traderId?: string
  loyaltyLevel?: number
  stockCount?: number
  buyRestrictionMax?: number
  unlimitedStock?: boolean
  unlimitedBuyRestriction?: boolean
}

export interface LootEntry {
  enabled: boolean
  containerIds: string[]
  rarity: string
}

export function createDefaultLootEntry(): LootEntry {
  return {
    enabled: false,
    containerIds: [],
    rarity: 'Rare',
  }
}

export interface ValidationError {
  field: string
  message: string
}

export const VANILLA_TRADERS = [
  { id: '54cb50c76803fa8b248b4571', name: 'Prapor' },
  { id: '54cb57776803fa99248b456e', name: 'Therapist' },
  { id: '58330581ace78e27b8b10cee', name: 'Skier' },
  { id: '5935c25fb3acc3127c3d8cd9', name: 'Peacekeeper' },
  { id: '579dc571d53a0658a154fbec', name: 'Fence' },
  { id: '5a7c2eca46aef81a7ca2145d', name: 'Mechanic' },
  { id: '5ac3b934156ae10c4430e83c', name: 'Ragman' },
  { id: '5c0647fdd443bc2504c2d371', name: 'Jaeger' },
  { id: '638f541a29ffd1183d187f57', name: 'Caretaker' },
  { id: '656f0f98d80a697f855d34b1', name: 'BTR' },
  { id: '6617beeaa9cfa777ca915b7c', name: 'Arena' },
  { id: '6864e812f9fe664cb8b8e152', name: 'Storyteller' },
]

export { AMMO_TEMPLATES, type AmmoTemplate } from './generated_ammo_templates'
export { GRENADE_TEMPLATES, type GrenadeTemplate } from './generated_grenade_templates'
export { GRENADE_STATS, type GrenadeTemplateStats } from './generated_grenade_stats'
export { HANDHELD_FLARE_TEMPLATES, CARTRIDGE_TEMPLATES, type FlareTemplate } from './generated_flare_templates'
export { FLARE_STATS, CARTRIDGE_STATS, type FlareTemplateStats } from './generated_flare_stats'

export const RARITY_OPTIONS = ['Common', 'Rare', 'SuperRare', 'NotExists']

export const BACKGROUND_COLORS = ['yellow', 'blue', 'green', 'red', 'violet', 'black', 'grey', 'white']

export function createDefaultTraderEntry(): TraderEntry {
  return {
    enabled: true,
    traderId: '54cb50c76803fa8b248b4571',
    loyaltyLevel: 1,
    priceRoubles: 0,
    stockCount: 200,
    buyRestrictionMax: 200,
    unlimitedStock: false,
    unlimitedBuyRestriction: false,
  }
}

export function createDefaultAmmo(): AmmoDefinition {
  return {
    id: generateMongoId(),
    enabled: true,
    baseTpl: '',
    compareToAmmoId: '',
    name: '',
    shortName: '',
    description: '',
    stats: {
      damage: 0,
      penetration: 0,
      armorDamage: 0,
      initialSpeed: 0,
      ammoAccr: 0,
      ammoRec: 0,
      stackMaxSize: 0,
      lightBleedingDelta: 0,
      heavyBleedingDelta: 0,
      durabilityBurnModificator: 1,
      ballisticCoeficient: 1,
      projectileCount: 0,
      ricochetChance: 0,
      fragmentationChance: 0,
      penetrationDamageMod: 0,
      penetrationChanceObstacle: 0,
      ammoLifeTimeSec: 0,
      bulletMassGram: 0,
      bulletDiameterMilimeters: 0,
      misfireChance: 0,
      malfMisfireChance: 0,
      malfFeedChance: 0,
      heatFactor: 1,
      staminaBurnPerDamage: 0,
      tracer: false,
      tracerColor: '',
      tracerDistance: 0,
      ammoSfx: '',
      casingSounds: '',
      backgroundColor: 'default',
      fuzeArmTimeSec: 0,
      minExplosionDistance: 0,
      maxExplosionDistance: 0,
      fragmentsCount: 0,
      fragmentType: '',
      explosionType: '',
      explosionStrength: 0,
      showHitEffectOnExplode: false,
      isLightAndSoundShot: false,
      lightAndSoundShotAngle: 0,
      lightAndSoundShotSelfContusionTime: 0,
      lightAndSoundShotSelfContusionStrength: 0,
      armorDistanceDistanceDamage: createDefaultVector3(),
      contusion: createDefaultVector3(),
      blindness: createDefaultVector3(),
    },
    economy: {
      handbookPriceRoubles: 0,
      fleaPriceRoubles: 0,
      rarityPvE: 'Rare',
      fleaBanned: false,
    },
    traders: [createDefaultTraderEntry()],
    crafting: {
      enabled: true,
      workbenchLevel: 2,
      craftTimeSeconds: 10800,
      outputCount: 100,
      requirements: [],
    },
    filters: {
      patchMagazines: [],
      patchWeapons: [],
    },
    ammoBox: {
      id: generateMongoId(),
      enabled: false,
      baseTpl: '',
      count: 0,
      name: '',
      shortName: '',
      description: '',
      handbookPriceRoubles: 0,
      rarityPvE: 'Rare',
      backgroundColor: 'default',
      sellToTraders: false,
      traderPriceRoubles: 0,
    },
    ammoLoot: createDefaultLootEntry(),
    ammoBoxLoot: createDefaultLootEntry(),
  }
}

export function createDefaultGrenade(): GrenadeDefinition {
  return {
    id: generateMongoId(),
    enabled: true,
    baseTpl: '',
    compareToGrenadeId: '',
    name: '',
    shortName: '',
    description: '',
    stats: {
      minExplosionDistance: 0,
      maxExplosionDistance: 0,
      fragmentsCount: 0,
      fragmentType: '',
      explosionEffectType: '',
      armorDistanceDistanceDamage: createDefaultVector3(),
      contusion: createDefaultVector3(),
      blindness: createDefaultVector3(),
      contusionDistance: 0,
      explDelay: 0,
      minTimeToContactExplode: -1,
      playFuzeSound: true,
      strength: 0,
      minFragmentDamage: 0,
      canPlantOnGround: false,
      throwType: '',
      throwDamMax: 0,
      weight: 0,
      backgroundColor: 'default',
      smokeColor: '',
      bodyColor: '',
      smokeRadius: 0,
      smokeDuration: 0,
      smokeFillSize: 0,
      smokeSizeOverTime: [],
      smokeStartSpeed: [],
      overrideSmokeRadius: false,
      overrideSmokeDuration: false,
      overrideSmokeFillSize: false,
      overrideSmokeSizeOverTime: false,
      overrideSmokeStartSpeed: false,
    },
    economy: {
      handbookPriceRoubles: 0,
      fleaPriceRoubles: 0,
      rarityPvE: 'Rare',
      fleaBanned: false,
    },
    traders: [createDefaultTraderEntry()],
    crafting: {
      enabled: true,
      workbenchLevel: 2,
      craftTimeSeconds: 10800,
      outputCount: 1,
      requirements: [],
    },
    loot: createDefaultLootEntry(),
  }
}

export function createDefaultFlare(): FlareDefinition {
  return {
    id: generateMongoId(),
    ammoId: generateMongoId(),
    kind: 'handheld',
    enabled: true,
    baseTpl: '',
    ammoBaseTpl: '',
    compareToFlareId: '',
    name: '',
    shortName: '',
    description: '',
    stats: {
      damage: 0,
      initialSpeed: 0,
      stackMaxSize: 0,
      ammoLifeTimeSec: 0,
      tracer: true,
      tracerColor: '',
      tracerDistance: 0,
      backgroundColor: 'default',
      flareColor: '',
      weight: 0,
      misfireChance: 0,
      ricochetChance: 0,
      flareTypes: [],
      airDropTemplateId: '',
      casingSounds: '',
      ammoType: '',
      weapClass: 'specialWeapon',
      isSpecialSlotOnly: false,
    },
    economy: {
      handbookPriceRoubles: 0,
      fleaPriceRoubles: 0,
      rarityPvE: 'Rare',
      fleaBanned: false,
    },
    traders: [createDefaultTraderEntry()],
    crafting: {
      enabled: true,
      workbenchLevel: 2,
      craftTimeSeconds: 10800,
      outputCount: 1,
      requirements: [],
    },
    loot: createDefaultLootEntry(),
  }
}

export function createDefaultPack(): AmmoPackDefinition {
  return {
    enabled: true,
    name: 'My Ammo Pack',
    ammo: [createDefaultAmmo()],
    grenades: [],
    flares: [],
    modFilterPatches: [],
  }
}

export function generateMongoId(): string {
  const hex = '0123456789abcdef'
  let id = ''
  for (let i = 0; i < 24; i++) {
    id += hex[Math.floor(Math.random() * 16)]
  }
  return id
}
