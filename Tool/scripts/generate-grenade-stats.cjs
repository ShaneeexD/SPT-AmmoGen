const fs = require('fs')
const path = require('path')

const itemsPath = 'C:\\SPT\\SPT\\SPT_Data\\database\\templates\\items.json'
const localePath = 'C:\\SPT\\SPT\\SPT_Data\\database\\locales\\global\\en.json'
const templatesOutputPath = path.join(__dirname, '..', 'src', 'generated_grenade_templates.ts')
const statsOutputPath = path.join(__dirname, '..', 'src', 'generated_grenade_stats.ts')

function loadJson(filePath) {
  const raw = fs.readFileSync(filePath, 'utf-8')
  return JSON.parse(raw)
}

const items = loadJson(itemsPath)
const locale = loadJson(localePath)
const GrenadeParentId = '543be6564bdc2df4348b4568'

const stats = {}
const templates = []

for (const itemId of Object.keys(items)) {
  const item = items[itemId]
  if (item._parent !== GrenadeParentId) continue

  const nameKey = `${itemId} Name`
  const shortNameKey = `${itemId} ShortName`

  const name = locale[nameKey] || item._props?.Name || item._name || ''
  const shortName = locale[shortNameKey] || item._props?.ShortName || ''

  const props = item._props || {}

  templates.push({ id: itemId, name: String(name).trim(), shortName: String(shortName || name).trim() })

  stats[itemId] = {
    name: String(name).trim(),
    shortName: String(shortName || name).trim(),
    minExplosionDistance: props.MinExplosionDistance || 0,
    maxExplosionDistance: props.MaxExplosionDistance || 0,
    fragmentsCount: props.FragmentsCount || 0,
    fragmentType: props.FragmentType || '',
    explosionEffectType: props.ExplosionEffectType || '',
    armorDistanceDistanceDamage: props.ArmorDistanceDistanceDamage || { x: 0, y: 0, z: 0 },
    contusion: props.Contusion || { x: 0, y: 0, z: 0 },
    blindness: props.Blindness || { x: 0, y: 0, z: 0 },
    contusionDistance: props.ContusionDistance || 0,
    explDelay: props.ExplDelay || 0,
    minTimeToContactExplode: props.MinTimeToContactExplode ?? -1,
    playFuzeSound: props.PlayFuzeSound ?? true,
    strength: props.Strength || 0,
    minFragmentDamage: props.MinFragmentDamage || 0,
    canPlantOnGround: props.CanPlantOnGround || false,
    throwType: props.ThrowType || '',
    throwDamMax: props.throwDamMax || 0,
    weight: props.Weight || 0,
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
  }
}

const sortedTemplates = templates.sort((a, b) => a.name.localeCompare(b.name))
const sortedStats = Object.entries(stats).sort((a, b) => a[1].name.localeCompare(b[1].name))

const fragmentTypes = [...new Set(Object.values(stats).map(s => s.fragmentType).filter(Boolean))].sort()
const explosionEffectTypes = [...new Set(Object.values(stats).map(s => s.explosionEffectType).filter(Boolean))].sort()
const throwTypes = [...new Set(Object.values(stats).map(s => s.throwType).filter(Boolean))].sort()

const v3 = (v) => `{ x: ${v.x}, y: ${v.y}, z: ${v.z} }`
const statLines = sortedStats.map(([id, s]) => {
  const statLine = [
    `name: ${JSON.stringify(s.name)}`,
    `shortName: ${JSON.stringify(s.shortName)}`,
    `minExplosionDistance: ${s.minExplosionDistance}`,
    `maxExplosionDistance: ${s.maxExplosionDistance}`,
    `fragmentsCount: ${s.fragmentsCount}`,
    `fragmentType: ${JSON.stringify(s.fragmentType)}`,
    `explosionEffectType: ${JSON.stringify(s.explosionEffectType)}`,
    `armorDistanceDistanceDamage: ${v3(s.armorDistanceDistanceDamage)}`,
    `contusion: ${v3(s.contusion)}`,
    `blindness: ${v3(s.blindness)}`,
    `contusionDistance: ${s.contusionDistance}`,
    `explDelay: ${s.explDelay}`,
    `minTimeToContactExplode: ${s.minTimeToContactExplode}`,
    `playFuzeSound: ${s.playFuzeSound}`,
    `strength: ${s.strength}`,
    `minFragmentDamage: ${s.minFragmentDamage}`,
    `canPlantOnGround: ${s.canPlantOnGround}`,
    `throwType: ${JSON.stringify(s.throwType)}`,
    `throwDamMax: ${s.throwDamMax}`,
    `weight: ${s.weight}`,
    `smokeColor: ${JSON.stringify(s.smokeColor)}`,
    `bodyColor: ${JSON.stringify(s.bodyColor)}`,
    `smokeRadius: ${s.smokeRadius}`,
    `smokeDuration: ${s.smokeDuration}`,
    `smokeFillSize: ${s.smokeFillSize}`,
    `smokeSizeOverTime: ${JSON.stringify(s.smokeSizeOverTime)}`,
    `smokeStartSpeed: ${JSON.stringify(s.smokeStartSpeed)}`,
    `overrideSmokeRadius: ${s.overrideSmokeRadius}`,
    `overrideSmokeDuration: ${s.overrideSmokeDuration}`,
    `overrideSmokeFillSize: ${s.overrideSmokeFillSize}`,
    `overrideSmokeSizeOverTime: ${s.overrideSmokeSizeOverTime}`,
    `overrideSmokeStartSpeed: ${s.overrideSmokeStartSpeed}`,
  ].join(', ')
  return `  '${id}': { ${statLine} },`
})

const templateLines = sortedTemplates.map(t =>
  `  { id: '${t.id}', name: ${JSON.stringify(t.name)}, shortName: ${JSON.stringify(t.shortName)} },`
)

const templatesOutput = `// Generated from SPT 4.0.13 database. Do not edit manually.
// Run: node scripts/generate-grenade-stats.cjs

export interface GrenadeTemplate {
  id: string
  name: string
  shortName: string
}

export const GRENADE_TEMPLATES: GrenadeTemplate[] = [
${templateLines.join('\n')}
]

export function getGrenadeTemplate(id: string): GrenadeTemplate | undefined {
  return GRENADE_TEMPLATES.find(t => t.id === id)
}
`

const statsOutput = `// Generated from SPT 4.0.13 database. Do not edit manually.
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
  minFragmentDamage: number
  canPlantOnGround: boolean
  throwType: string
  throwDamMax: number
  weight: number
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

export const GRENADE_STATS: Record<string, GrenadeTemplateStats> = {
${statLines.join('\n')}
}

export const GRENADE_FRAGMENT_TYPES: string[] = ${JSON.stringify(fragmentTypes)}

export const GRENADE_EXPLOSION_EFFECT_TYPES: string[] = ${JSON.stringify(explosionEffectTypes)}

export const GRENADE_THROW_TYPES: string[] = ${JSON.stringify(throwTypes)}

export function getGrenadeStats(id: string): GrenadeTemplateStats | undefined {
  return GRENADE_STATS[id]
}
`

fs.writeFileSync(templatesOutputPath, templatesOutput, 'utf-8')
fs.writeFileSync(statsOutputPath, statsOutput, 'utf-8')
console.log(`Generated ${templates.length} grenade templates -> ${templatesOutputPath}`)
console.log(`Generated ${sortedStats.length} grenade stat entries -> ${statsOutputPath}`)
