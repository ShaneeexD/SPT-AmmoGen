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
  }
}

const entries = Object.entries(stats).sort((a, b) => a[1].name.localeCompare(b[1].name))

const lines = entries.map(([id, s]) => {
  return `  '${id}': { name: ${JSON.stringify(s.name)}, shortName: ${JSON.stringify(s.shortName)}, damage: ${s.damage}, penetration: ${s.penetration}, armorDamage: ${s.armorDamage}, initialSpeed: ${s.initialSpeed}, ammoAccr: ${s.ammoAccr}, ammoRec: ${s.ammoRec}, stackMaxSize: ${s.stackMaxSize}, lightBleedingDelta: ${s.lightBleedingDelta}, heavyBleedingDelta: ${s.heavyBleedingDelta} },`
})

const output = `// Generated from SPT 4.0.13 database. Do not edit manually.
// Run: node scripts/generate-ammo-stats.cjs

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
}

export const AMMO_STATS: Record<string, AmmoTemplateStats> = {
${lines.join('\n')}
}

export function getAmmoStats(id: string): AmmoTemplateStats | undefined {
  return AMMO_STATS[id]
}
`

fs.writeFileSync(outputPath, output, 'utf-8')
console.log(`Generated ${entries.length} ammo stat entries -> ${outputPath}`)
