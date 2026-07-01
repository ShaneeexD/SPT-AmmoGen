// Generated from SPT 4.0.13 database. Do not edit manually.
// Run: node scripts/generate-grenade-stats.cjs

export interface Vector3 {
  x: number
  y: number
  z: number
}

export interface GrenadeTemplateStats {
  name: string
  shortName: string
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
  throwType: string
  throwDamMax: number
  weight: number
  smokeColor: string
  bodyColor: string
}

export const GRENADE_STATS: Record<string, GrenadeTemplateStats> = {
  '5710c24ad2720bc3458b45a3': { name: "F-1 hand grenade", shortName: "F-1", minExplosionDistance: 3, maxExplosionDistance: 7, fragmentsCount: 90, fragmentType: "63b35f281745dd52341e5da7", explosionEffectType: "Grenade_new", armorDistanceDistanceDamage: { x: 1, y: 4, z: 25 }, contusion: { x: 1.5, y: 4, z: 15 }, blindness: { x: 0, y: 0, z: 0 }, contusionDistance: 12, explDelay: 3, minTimeToContactExplode: -1, playFuzeSound: true, strength: 80, throwType: "frag_grenade", throwDamMax: 0, weight: 0.6, smokeColor: "", bodyColor: "" },
  '67b49e7335dec48e3e05e057': { name: "F-1 hand grenade (Reduced delay)", shortName: "F-1 RD", minExplosionDistance: 3, maxExplosionDistance: 7, fragmentsCount: 90, fragmentType: "63b35f281745dd52341e5da7", explosionEffectType: "Grenade_new", armorDistanceDistanceDamage: { x: 1, y: 4, z: 25 }, contusion: { x: 1.5, y: 4, z: 15 }, blindness: { x: 0, y: 0, z: 0 }, contusionDistance: 12, explDelay: 3, minTimeToContactExplode: -1, playFuzeSound: true, strength: 80, throwType: "frag_grenade", throwDamMax: 0, weight: 0.6, smokeColor: "", bodyColor: "" },
  '617aa4dd8166f034d57de9c5': { name: "M18 smoke grenade (Green)", shortName: "M18", minExplosionDistance: 0, maxExplosionDistance: 0, fragmentsCount: 0, fragmentType: "5996f6d686f77467977ba6cc", explosionEffectType: "grenade_smoke_m18_green", armorDistanceDistanceDamage: { x: 0, y: 0, z: 0 }, contusion: { x: 0, y: 0, z: 0 }, blindness: { x: 0, y: 0, z: 0 }, contusionDistance: 0, explDelay: 1, minTimeToContactExplode: -1, playFuzeSound: true, strength: 0, throwType: "smoke_grenade", throwDamMax: 0, weight: 0.539, smokeColor: "", bodyColor: "" },
  '58d3db5386f77426186285a0': { name: "M67 hand grenade", shortName: "M67", minExplosionDistance: 4, maxExplosionDistance: 8, fragmentsCount: 75, fragmentType: "5996f6fc86f7745e585b4de3", explosionEffectType: "Grenade_new", armorDistanceDistanceDamage: { x: 1, y: 4, z: 29 }, contusion: { x: 2, y: 4, z: 14 }, blindness: { x: 0, y: 0, z: 0 }, contusionDistance: 11, explDelay: 5, minTimeToContactExplode: -1, playFuzeSound: false, strength: 110, throwType: "frag_grenade", throwDamMax: 0, weight: 0.396, smokeColor: "", bodyColor: "" },
  '619256e5f8af2c1a4e1f5d92': { name: "Model 7290 Flash Bang grenade", shortName: "M7290", minExplosionDistance: 0, maxExplosionDistance: 0, fragmentsCount: 0, fragmentType: "5996f6cb86f774678763a6ca", explosionEffectType: "Flashbang", armorDistanceDistanceDamage: { x: 0, y: 0, z: 0 }, contusion: { x: 2, y: 5, z: 25 }, blindness: { x: 10, y: 18, z: 35 }, contusionDistance: 10, explDelay: 1.5, minTimeToContactExplode: -1, playFuzeSound: true, strength: 0, throwType: "flash_grenade", throwDamMax: 0, weight: 0.42, smokeColor: "", bodyColor: "" },
  '5a2a57cfc4a2826c6e06d44a': { name: "RDG-2B smoke grenade", shortName: "RDG-2B", minExplosionDistance: 0, maxExplosionDistance: 0, fragmentsCount: 0, fragmentType: "5996f6d686f77467977ba6cc", explosionEffectType: "", armorDistanceDistanceDamage: { x: 0, y: 0, z: 0 }, contusion: { x: 0, y: 0, z: 0 }, blindness: { x: 0, y: 0, z: 0 }, contusionDistance: 0, explDelay: 3, minTimeToContactExplode: -1, playFuzeSound: true, strength: 0, throwType: "smoke_grenade", throwDamMax: 0, weight: 0.6, smokeColor: "", bodyColor: "" },
  '5448be9a4bdc2dfd2f8b456a': { name: "RGD-5 hand grenade", shortName: "RGD-5", minExplosionDistance: 3, maxExplosionDistance: 7, fragmentsCount: 70, fragmentType: "5996f6cb86f774678763a6ca", explosionEffectType: "Grenade_new", armorDistanceDistanceDamage: { x: 1, y: 4, z: 28 }, contusion: { x: 1.5, y: 4, z: 14 }, blindness: { x: 0, y: 0, z: 0 }, contusionDistance: 10, explDelay: 3, minTimeToContactExplode: -1, playFuzeSound: true, strength: 100, throwType: "frag_grenade", throwDamMax: 0, weight: 0.31, smokeColor: "", bodyColor: "" },
  '617fd91e5539a84ec44ce155': { name: "RGN hand grenade", shortName: "RGN", minExplosionDistance: 2, maxExplosionDistance: 5, fragmentsCount: 75, fragmentType: "5996f6cb86f774678763a6ca", explosionEffectType: "Grenade_new2", armorDistanceDistanceDamage: { x: 1, y: 4, z: 28 }, contusion: { x: 1.5, y: 4, z: 18 }, blindness: { x: 0, y: 0, z: 0 }, contusionDistance: 13, explDelay: 3, minTimeToContactExplode: 0.3, playFuzeSound: true, strength: 95, throwType: "frag_grenade", throwDamMax: 0, weight: 0.31, smokeColor: "", bodyColor: "" },
  '618a431df1eb8e24b8741deb': { name: "RGO hand grenade", shortName: "RGO", minExplosionDistance: 2, maxExplosionDistance: 7, fragmentsCount: 85, fragmentType: "63b35f281745dd52341e5da7", explosionEffectType: "Grenade_new2", armorDistanceDistanceDamage: { x: 1, y: 4, z: 28 }, contusion: { x: 1.5, y: 5, z: 15 }, blindness: { x: 0, y: 0, z: 0 }, contusionDistance: 12, explDelay: 3, minTimeToContactExplode: 0.3, playFuzeSound: true, strength: 55, throwType: "frag_grenade", throwDamMax: 0, weight: 0.53, smokeColor: "", bodyColor: "" },
  '66dae7cbeb28f0f96809f325': { name: "V40 Mini-Grenade", shortName: "V40", minExplosionDistance: 2, maxExplosionDistance: 5, fragmentsCount: 25, fragmentType: "67654a6759116d347b0bfb86", explosionEffectType: "Grenade_new", armorDistanceDistanceDamage: { x: 1, y: 3, z: 15 }, contusion: { x: 1, y: 5, z: 7 }, blindness: { x: 0, y: 0, z: 0 }, contusionDistance: 7, explDelay: 3, minTimeToContactExplode: -1, playFuzeSound: true, strength: 40, throwType: "frag_grenade", throwDamMax: 0, weight: 0.136, smokeColor: "", bodyColor: "" },
  '5e32f56fcb6d5863cc5e5ee4': { name: "VOG-17 Khattabka improvised hand grenade", shortName: "VOG-17", minExplosionDistance: 2, maxExplosionDistance: 6, fragmentsCount: 100, fragmentType: "5996f6cb86f774678763a6ca", explosionEffectType: "Grenade_new", armorDistanceDistanceDamage: { x: 1, y: 3, z: 21 }, contusion: { x: 1.5, y: 3, z: 8 }, blindness: { x: 0, y: 0, z: 0 }, contusionDistance: 9, explDelay: 3, minTimeToContactExplode: -1, playFuzeSound: true, strength: 120, throwType: "frag_grenade", throwDamMax: 0, weight: 0.28, smokeColor: "", bodyColor: "" },
  '5e340dcdcb6d5863cc5e5efb': { name: "VOG-25 Khattabka improvised hand grenade", shortName: "VOG-25", minExplosionDistance: 2, maxExplosionDistance: 7, fragmentsCount: 35, fragmentType: "63b35f281745dd52341e5da7", explosionEffectType: "Grenade_new", armorDistanceDistanceDamage: { x: 1, y: 3, z: 20 }, contusion: { x: 1.5, y: 3, z: 10 }, blindness: { x: 0, y: 0, z: 0 }, contusionDistance: 7, explDelay: 2, minTimeToContactExplode: -1, playFuzeSound: true, strength: 65, throwType: "frag_grenade", throwDamMax: 0, weight: 0.25, smokeColor: "", bodyColor: "" },
  '5a0c27731526d80618476ac4': { name: "Zarya stun grenade", shortName: "Zarya", minExplosionDistance: 0, maxExplosionDistance: 0, fragmentsCount: 0, fragmentType: "5996f6cb86f774678763a6ca", explosionEffectType: "Flashbang", armorDistanceDistanceDamage: { x: 0, y: 0, z: 0 }, contusion: { x: 1.5, y: 4, z: 20 }, blindness: { x: 10, y: 20, z: 40 }, contusionDistance: 10, explDelay: 2, minTimeToContactExplode: -1, playFuzeSound: true, strength: 0, throwType: "flash_grenade", throwDamMax: 0, weight: 0.175, smokeColor: "", bodyColor: "" },
}

export const GRENADE_FRAGMENT_TYPES: string[] = ["5996f6cb86f774678763a6ca","5996f6d686f77467977ba6cc","5996f6fc86f7745e585b4de3","63b35f281745dd52341e5da7","67654a6759116d347b0bfb86"]

export const GRENADE_EXPLOSION_EFFECT_TYPES: string[] = ["Flashbang","Grenade_new","Grenade_new2","grenade_smoke_m18_green"]

export const GRENADE_THROW_TYPES: string[] = ["flash_grenade","frag_grenade","smoke_grenade"]

export function getGrenadeStats(id: string): GrenadeTemplateStats | undefined {
  return GRENADE_STATS[id]
}
