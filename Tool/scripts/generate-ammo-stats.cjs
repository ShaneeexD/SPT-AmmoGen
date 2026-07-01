const fs = require('fs')
const path = require('path')

const itemsPath = 'C:\\SPT\\SPT\\SPT_Data\\database\\templates\\items.json'
const localePath = 'C:\\SPT\\SPT\\SPT_Data\\database\\locales\\global\\en.json'
const outputPath = path.join(__dirname, '..', 'src', 'generated_ammo_stats.ts')

function loadJson(filePath) {
  const raw = fs.readFileSync(filePath, 'utf-8')
  return JSON.parse(raw)
}

const items = loadJson(itemsPath)
const locale = loadJson(localePath)
const AmmoParentId = '5485a8684bdc2da71d8b4567'

const stats = {}
const tracerColors = new Set()
const ammoSfxOptions = new Set()
const casingSoundsOptions = new Set()

for (const itemId of Object.keys(items)) {
  const item = items[itemId]
  if (item._parent !== AmmoParentId) continue
  if (!item._props || typeof item._props.Damage !== 'number') continue

  const nameKey = `${itemId} Name`
  const shortNameKey = `${itemId} ShortName`

  const name = locale[nameKey] || item._props.Name || item._name || ''
  const shortName = locale[shortNameKey] || item._props.ShortName || ''

  stats[itemId] = {
    name: String(name).trim(),
    shortName: String(shortName || name).trim(),
    damage: item._props.Damage || 0,
    penetration: item._props.PenetrationPower || 0,
    armorDamage: item._props.ArmorDamage || 0,
    initialSpeed: item._props.InitialSpeed || 0,
    ammoAccr: item._props.AmmoAccr || 0,
    ammoRec: item._props.AmmoRec || 0,
    stackMaxSize: item._props.StackMaxSize || 0,
    lightBleedingDelta: item._props.LightBleedingDelta || 0,
    heavyBleedingDelta: item._props.HeavyBleedingDelta || 0,
    durabilityBurnModificator: item._props.DurabilityBurnModificator ?? 1,
    ballisticCoeficient: item._props.BallisticCoeficient ?? 1,
    projectileCount: item._props.ProjectileCount || 0,
    ricochetChance: item._props.RicochetChance || 0,
    fragmentationChance: item._props.FragmentationChance || 0,
    penetrationDamageMod: item._props.PenetrationDamageMod ?? 0,
    penetrationChanceObstacle: item._props.PenetrationChanceObstacle || 0,
    ammoLifeTimeSec: item._props.AmmoLifeTimeSec ?? 2,
    bulletMassGram: item._props.BulletMassGram || 0,
    bulletDiameterMilimeters: item._props.BulletDiameterMilimeters || 0,
    misfireChance: item._props.MisfireChance || 0,
    malfMisfireChance: item._props.MalfMisfireChance || 0,
    malfFeedChance: item._props.MalfFeedChance || 0,
    heatFactor: item._props.HeatFactor ?? 1,
    staminaBurnPerDamage: item._props.StaminaBurnPerDamage || 0,
    tracer: item._props.Tracer || false,
    tracerColor: item._props.TracerColor || '',
    tracerDistance: item._props.TracerDistance || 0,
    ammoSfx: item._props.ammoSfx || '',
    casingSounds: item._props.casingSounds || '',
    fuzeArmTimeSec: item._props.FuzeArmTimeSec || 0,
    minExplosionDistance: item._props.MinExplosionDistance || 0,
    maxExplosionDistance: item._props.MaxExplosionDistance || 0,
    fragmentsCount: item._props.FragmentsCount || 0,
    fragmentType: item._props.FragmentType || '',
    explosionType: item._props.ExplosionType || '',
    explosionStrength: item._props.ExplosionStrength || 0,
    showHitEffectOnExplode: item._props.ShowHitEffectOnExplode || false,
    isLightAndSoundShot: item._props.IsLightAndSoundShot || false,
    lightAndSoundShotAngle: item._props.LightAndSoundShotAngle || 0,
    lightAndSoundShotSelfContusionTime: item._props.LightAndSoundShotSelfContusionTime || 0,
    lightAndSoundShotSelfContusionStrength: item._props.LightAndSoundShotSelfContusionStrength || 0,
    armorDistanceDistanceDamage: item._props.ArmorDistanceDistanceDamage || { x: 0, y: 0, z: 0 },
    contusion: item._props.Contusion || { x: 0, y: 0, z: 0 },
    blindness: item._props.Blindness || { x: 0, y: 0, z: 0 },
  }

  if (item._props.TracerColor) tracerColors.add(item._props.TracerColor)
  if (item._props.ammoSfx) ammoSfxOptions.add(item._props.ammoSfx)
  if (item._props.casingSounds) casingSoundsOptions.add(item._props.casingSounds)
}

const sortedSet = (s) => Array.from(s).sort((a, b) => a.localeCompare(b))
const tracerColorOptions = sortedSet(tracerColors)
const ammoSfxValues = sortedSet(ammoSfxOptions)
const casingSoundsValues = sortedSet(casingSoundsOptions)

const entries = Object.entries(stats).sort((a, b) => a[1].name.localeCompare(b[1].name))

const lines = entries.map(([id, s]) => {
  const v3 = (v) => `{ x: ${v.x}, y: ${v.y}, z: ${v.z} }`
  const statLine = Object.entries(s)
    .filter(([k]) => k !== 'name' && k !== 'shortName')
    .map(([k, v]) => {
      if (typeof v === 'string') return `${k}: ${JSON.stringify(v)}`
      if (typeof v === 'boolean') return `${k}: ${v}`
      if (v && typeof v === 'object' && 'x' in v) return `${k}: ${v3(v)}`
      return `${k}: ${v}`
    })
    .join(', ')
  return `  '${id}': { name: ${JSON.stringify(s.name)}, shortName: ${JSON.stringify(s.shortName)}, ${statLine} },`
})

const output = `// Generated from SPT 4.0.13 database. Do not edit manually.
// Run: node scripts/generate-ammo-stats.cjs

export interface Vector3 {
  x: number
  y: number
  z: number
}

export interface AmmoTemplateStats {
  name: string
  shortName: string
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
  projectileCount: number
  ricochetChance: number
  fragmentationChance: number
  penetrationDamageMod: number
  penetrationChanceObstacle: number
  ammoLifeTimeSec: number
  bulletMassGram: number
  bulletDiameterMilimeters: number
  misfireChance: number
  malfMisfireChance: number
  malfFeedChance: number
  heatFactor: number
  staminaBurnPerDamage: number
  tracer: boolean
  tracerColor: string
  tracerDistance: number
  ammoSfx: string
  casingSounds: string
  fuzeArmTimeSec: number
  minExplosionDistance: number
  maxExplosionDistance: number
  fragmentsCount: number
  fragmentType: string
  explosionType: string
  explosionStrength: number
  showHitEffectOnExplode: boolean
  isLightAndSoundShot: boolean
  lightAndSoundShotAngle: number
  lightAndSoundShotSelfContusionTime: number
  lightAndSoundShotSelfContusionStrength: number
  armorDistanceDistanceDamage: Vector3
  contusion: Vector3
  blindness: Vector3
}

export const AMMO_STATS: Record<string, AmmoTemplateStats> = {
${lines.join('\n')}
}

export function getAmmoStats(id: string): AmmoTemplateStats | undefined {
  return AMMO_STATS[id]
}

export const TRACER_COLOR_OPTIONS: string[] = ${JSON.stringify(tracerColorOptions)}
export const AMMO_SFX_OPTIONS: string[] = ${JSON.stringify(ammoSfxValues)}
export const CASING_SOUNDS_OPTIONS: string[] = ${JSON.stringify(casingSoundsValues)}
`

fs.writeFileSync(outputPath, output, 'utf-8')
console.log(`Generated ${entries.length} ammo stat entries -> ${outputPath}`)
