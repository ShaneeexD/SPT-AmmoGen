export interface AmmoPackDefinition {
  enabled: boolean
  name: string
  ammo: AmmoDefinition[]
}

export interface AmmoDefinition {
  id: string
  enabled: boolean
  baseTpl: string
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
  loot: LootEntry
}

export interface AmmoStats {
  damage: number
  penetration: number
  armorDamage: number
  initialSpeed: number
  ammoAccr: number
  ammoRec: number
  stackMaxSize: number
}

export interface AmmoEconomy {
  handbookPriceRoubles: number
  fleaPriceRoubles: number
  rarityPvE: string
}

export interface TraderEntry {
  enabled: boolean
  traderId: string
  loyaltyLevel: number
  priceRoubles: number
  stockCount: number
  buyRestrictionMax: number
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
  sellToTraders: boolean
  traderPriceRoubles: number
}

export interface LootEntry {
  enabled: boolean
  containerIds: string[]
  rarity: string
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

export { AMMO_TEMPLATES } from './generated_ammo_templates'

export const RARITY_OPTIONS = ['Common', 'Rare', 'SuperRare', 'NotExists']

export function createDefaultTraderEntry(): TraderEntry {
  return {
    enabled: true,
    traderId: '54cb50c76803fa8b248b4571',
    loyaltyLevel: 1,
    priceRoubles: 0,
    stockCount: 200,
    buyRestrictionMax: 200,
  }
}

export function createDefaultAmmo(): AmmoDefinition {
  return {
    id: generateMongoId(),
    enabled: true,
    baseTpl: '',
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
    },
    economy: {
      handbookPriceRoubles: 0,
      fleaPriceRoubles: 0,
      rarityPvE: 'Rare',
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
      sellToTraders: false,
      traderPriceRoubles: 0,
    },
    loot: {
      enabled: false,
      containerIds: [],
      rarity: 'Rare',
    },
  }
}

export function createDefaultPack(): AmmoPackDefinition {
  return {
    enabled: true,
    name: 'My Ammo Pack',
    ammo: [createDefaultAmmo()],
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
