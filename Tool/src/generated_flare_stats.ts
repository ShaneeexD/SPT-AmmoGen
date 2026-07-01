// Generated from SPT database. Do not edit manually.
// Run: node scripts/generate-flare-stats.cjs

export interface FlareTemplateStats {
  name: string
  shortName: string
  ammoBaseTpl: string
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

export const FLARE_STATS: Record<string, FlareTemplateStats> = {
  '62178be9d0050232da3485d9': { name: "ROP-30 reactive flare cartridge (White)", shortName: "Flare", ammoBaseTpl: "624c09da2cec124eb67c1046", damage: 40, initialSpeed: 80, stackMaxSize: 60, ammoLifeTimeSec: 5, tracer: true, tracerColor: "yellow", tracerDistance: 0.08, backgroundColor: "yellow", flareColor: "", weight: 0.063, misfireChance: 0.01, ricochetChance: 1, flareTypes: [], airDropTemplateId: "", casingSounds: "", ammoType: "bullet", weapClass: "specialWeapon", isSpecialSlotOnly: false },
  '66d98233302686954b0c6f81': { name: "RSP-30 reactive signal cartridge (Blue)", shortName: "Blue", ammoBaseTpl: "66d97834d2985e11480d5c1e", damage: 40, initialSpeed: 80, stackMaxSize: 60, ammoLifeTimeSec: 5, tracer: true, tracerColor: "yellow", tracerDistance: 0.08, backgroundColor: "yellow", flareColor: "", weight: 0.063, misfireChance: 0.01, ricochetChance: 1, flareTypes: ["CallArtilleryOnMyself"], airDropTemplateId: "", casingSounds: "", ammoType: "bullet", weapClass: "specialWeapon", isSpecialSlotOnly: false },
  '675ea3d6312c0a5c4e04e317': { name: "RSP-30 reactive signal cartridge (Firework)", shortName: "Firework", ammoBaseTpl: "675ea4891b2579e8fe0250aa", damage: 40, initialSpeed: 80, stackMaxSize: 60, ammoLifeTimeSec: 5, tracer: true, tracerColor: "yellow", tracerDistance: 0.08, backgroundColor: "yellow", flareColor: "", weight: 0.063, misfireChance: 0.01, ricochetChance: 1, flareTypes: ["Light"], airDropTemplateId: "", casingSounds: "", ammoType: "bullet", weapClass: "specialWeapon", isSpecialSlotOnly: false },
  '6217726288ed9f0845317459': { name: "RSP-30 reactive signal cartridge (Green)", shortName: "Green", ammoBaseTpl: "624c0570c9b794431568f5d5", damage: 40, initialSpeed: 80, stackMaxSize: 60, ammoLifeTimeSec: 5, tracer: true, tracerColor: "yellow", tracerDistance: 0.08, backgroundColor: "yellow", flareColor: "", weight: 0.063, misfireChance: 0.01, ricochetChance: 1, flareTypes: ["ExitActivate"], airDropTemplateId: "", casingSounds: "", ammoType: "bullet", weapClass: "specialWeapon", isSpecialSlotOnly: false },
  '62178c4d4ecf221597654e3d': { name: "RSP-30 reactive signal cartridge (Red)", shortName: "Red", ammoBaseTpl: "624c09cfbc2e27219346d955", damage: 40, initialSpeed: 80, stackMaxSize: 60, ammoLifeTimeSec: 5, tracer: true, tracerColor: "yellow", tracerDistance: 0.08, backgroundColor: "yellow", flareColor: "", weight: 0.063, misfireChance: 0.01, ricochetChance: 1, flareTypes: ["Airdrop"], airDropTemplateId: "", casingSounds: "", ammoType: "bullet", weapClass: "specialWeapon", isSpecialSlotOnly: false },
  '66d9f1abb16d9aacf5068468': { name: "RSP-30 reactive signal cartridge (Special Yellow)", shortName: "S-Yellow", ammoBaseTpl: "66d9f3047b82b9a9aa055d81", damage: 40, initialSpeed: 80, stackMaxSize: 60, ammoLifeTimeSec: 5, tracer: true, tracerColor: "yellow", tracerDistance: 0.08, backgroundColor: "yellow", flareColor: "", weight: 0.063, misfireChance: 0.01, ricochetChance: 1, flareTypes: ["Airdrop"], airDropTemplateId: "", casingSounds: "", ammoType: "bullet", weapClass: "specialWeapon", isSpecialSlotOnly: false },
  '624c0b3340357b5f566e8766': { name: "RSP-30 reactive signal cartridge (Yellow)", shortName: "Yellow", ammoBaseTpl: "624c09e49b98e019a3315b66", damage: 40, initialSpeed: 80, stackMaxSize: 60, ammoLifeTimeSec: 5, tracer: true, tracerColor: "yellow", tracerDistance: 0.08, backgroundColor: "yellow", flareColor: "", weight: 0.063, misfireChance: 0.01, ricochetChance: 1, flareTypes: ["Quest"], airDropTemplateId: "", casingSounds: "", ammoType: "bullet", weapClass: "specialWeapon", isSpecialSlotOnly: false }
}

export const CARTRIDGE_STATS: Record<string, FlareTemplateStats> = {
  '635267f063651329f75a4ee8': { name: "26x75mm flare cartridge (Acid Green)", shortName: "AG", ammoBaseTpl: "", damage: 37, initialSpeed: 80, stackMaxSize: 1, ammoLifeTimeSec: 5, tracer: true, tracerColor: "yellow", tracerDistance: 0.08, backgroundColor: "green", flareColor: "", weight: 0.043, misfireChance: 0.01, ricochetChance: 1, flareTypes: ["AIFollowEvent"], airDropTemplateId: "", casingSounds: "", ammoType: "bullet", weapClass: "", isSpecialSlotOnly: false },
  '62389bc9423ed1685422dc57': { name: "26x75mm flare cartridge (White)", shortName: "Flare", ammoBaseTpl: "", damage: 37, initialSpeed: 80, stackMaxSize: 1, ammoLifeTimeSec: 5, tracer: true, tracerColor: "yellow", tracerDistance: 0.08, backgroundColor: "grey", flareColor: "", weight: 0.055, misfireChance: 0.01, ricochetChance: 1, flareTypes: [], airDropTemplateId: "", casingSounds: "", ammoType: "bullet", weapClass: "", isSpecialSlotOnly: false },
  '62389be94d5d474bf712e709': { name: "26x75mm flare cartridge (Yellow)", shortName: "Yellow", ammoBaseTpl: "", damage: 37, initialSpeed: 80, stackMaxSize: 1, ammoLifeTimeSec: 5, tracer: true, tracerColor: "yellow", tracerDistance: 0.08, backgroundColor: "yellow", flareColor: "", weight: 0.05, misfireChance: 0.01, ricochetChance: 1, flareTypes: ["Quest"], airDropTemplateId: "", casingSounds: "", ammoType: "bullet", weapClass: "", isSpecialSlotOnly: false },
  '66d97834d2985e11480d5c1e': { name: "Signal flare (Blue)", shortName: "Signal flare (Blue)", ammoBaseTpl: "", damage: 40, initialSpeed: 80, stackMaxSize: 60, ammoLifeTimeSec: 5, tracer: true, tracerColor: "yellow", tracerDistance: 0.08, backgroundColor: "yellow", flareColor: "", weight: 0.063, misfireChance: 0.01, ricochetChance: 1, flareTypes: ["CallArtilleryOnMyself"], airDropTemplateId: "", casingSounds: "", ammoType: "bullet", weapClass: "", isSpecialSlotOnly: false },
  '675ea4891b2579e8fe0250aa': { name: "Signal flare (New Year)", shortName: "Signal flare (New Year)", ammoBaseTpl: "", damage: 40, initialSpeed: 80, stackMaxSize: 60, ammoLifeTimeSec: 5, tracer: true, tracerColor: "yellow", tracerDistance: 0.08, backgroundColor: "yellow", flareColor: "", weight: 0.063, misfireChance: 0.01, ricochetChance: 1, flareTypes: ["Light"], airDropTemplateId: "", casingSounds: "", ammoType: "bullet", weapClass: "", isSpecialSlotOnly: false },
  '624c09cfbc2e27219346d955': { name: "Signal flare (Red)", shortName: "Signal flare (Red)", ammoBaseTpl: "", damage: 40, initialSpeed: 80, stackMaxSize: 60, ammoLifeTimeSec: 5, tracer: true, tracerColor: "yellow", tracerDistance: 0.08, backgroundColor: "yellow", flareColor: "", weight: 0.063, misfireChance: 0.01, ricochetChance: 1, flareTypes: ["Airdrop"], airDropTemplateId: "", casingSounds: "", ammoType: "bullet", weapClass: "", isSpecialSlotOnly: false },
  '66d9f3047b82b9a9aa055d81': { name: "Signal flare (Special Yellow)", shortName: "Signal flare (Special Yellow)", ammoBaseTpl: "", damage: 40, initialSpeed: 80, stackMaxSize: 60, ammoLifeTimeSec: 5, tracer: true, tracerColor: "yellow", tracerDistance: 0.08, backgroundColor: "yellow", flareColor: "", weight: 0.063, misfireChance: 0.01, ricochetChance: 1, flareTypes: ["Airdrop"], airDropTemplateId: "", casingSounds: "", ammoType: "bullet", weapClass: "", isSpecialSlotOnly: false },
  '624c09da2cec124eb67c1046': { name: "Signal flare (White)", shortName: "Signal flare (White)", ammoBaseTpl: "", damage: 40, initialSpeed: 80, stackMaxSize: 60, ammoLifeTimeSec: 5, tracer: true, tracerColor: "yellow", tracerDistance: 0.08, backgroundColor: "yellow", flareColor: "", weight: 0.063, misfireChance: 0.01, ricochetChance: 1, flareTypes: [], airDropTemplateId: "", casingSounds: "", ammoType: "bullet", weapClass: "", isSpecialSlotOnly: false },
  '624c09e49b98e019a3315b66': { name: "Signal flare (Yellow)", shortName: "Signal flare (Yellow)", ammoBaseTpl: "", damage: 40, initialSpeed: 80, stackMaxSize: 60, ammoLifeTimeSec: 5, tracer: true, tracerColor: "yellow", tracerDistance: 0.08, backgroundColor: "yellow", flareColor: "", weight: 0.063, misfireChance: 0.01, ricochetChance: 1, flareTypes: ["Quest"], airDropTemplateId: "", casingSounds: "", ammoType: "bullet", weapClass: "", isSpecialSlotOnly: false }
}

export const FLARE_TYPES: string[] = ["ExitActivate","Airdrop","CallArtilleryOnMyself","Light","Quest"]

export interface AirdropTemplate {
  id: string
  name: string
}

export const AIRDROP_TEMPLATE_OPTIONS: AirdropTemplate[] = [
  {
    "id": "6223349b3136504a544d1608",
    "name": "Airdrop common supply crate"
  },
  {
    "id": "622334c873090231d904a9fc",
    "name": "Airdrop medical crate"
  },
  {
    "id": "61a89e5445a2672acf66c877",
    "name": "Airdrop supply crate"
  },
  {
    "id": "622334fa3136504a544d160c",
    "name": "Airdrop supply crate"
  },
  {
    "id": "61a89e812cc17d60cc5f9879",
    "name": "Airdrop supply crate 2"
  },
  {
    "id": "66da1b49099cf6adcc07a36b",
    "name": "Airdrop technical supply crate"
  },
  {
    "id": "66da1b546916142b3b022777",
    "name": "Airdrop technical supply crate"
  },
  {
    "id": "6223351bb5d97a7b2c635ca7",
    "name": "Airdrop weapon crate"
  },
  {
    "id": "62f10b79e7ee985f386b2f47",
    "name": "Airdrop weapon crate"
  },
  {
    "id": "633ffb5d419dbf4bea7004c6",
    "name": "Airdrop weapon crate"
  },
  {
    "id": "67614e3a6a90e4f10b0b140d",
    "name": "Festive airdrop supply crate"
  }
]

export function getFlareStats(id: string): FlareTemplateStats | undefined {
  return FLARE_STATS[id]
}

export function getCartridgeStats(id: string): FlareTemplateStats | undefined {
  return CARTRIDGE_STATS[id]
}
