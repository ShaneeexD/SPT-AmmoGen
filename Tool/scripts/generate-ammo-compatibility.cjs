const fs = require('fs')
const path = require('path')

const itemsPath = 'C:\\SPT\\SPT\\SPT_Data\\database\\templates\\items.json'
const localePath = 'C:\\SPT\\SPT\\SPT_Data\\database\\locales\\global\\en.json'
const outputPath = path.join(__dirname, '..', 'src', 'generated_ammo_compatibility.ts')

function loadJson(filePath) {
  const raw = fs.readFileSync(filePath, 'utf-8')
  return JSON.parse(raw)
}

const items = loadJson(itemsPath)
const locale = loadJson(localePath)
const AmmoParentId = '5485a8684bdc2da71d8b4567'

const compatibility = {}

function getName(id) {
  return locale[`${id} Name`] || locale[`${id} ShortName`] || items[id]?._props?.Name || items[id]?._name || id
}

function slotAcceptsAmmo(slot, ammoId) {
  if (!slot?._props?.filters) return false
  for (const filter of slot._props.filters) {
    if (filter?.Filter?.includes(ammoId)) return true
  }
  return false
}

for (const itemId of Object.keys(items)) {
  const item = items[itemId]
  if (item._parent !== AmmoParentId) continue

  const magazines = []
  const weapons = []

  for (const otherId of Object.keys(items)) {
    const other = items[otherId]
    if (!other._props) continue

    const mags = other._props.Cartridges || []
    if (mags.some(slot => slotAcceptsAmmo(slot, itemId))) {
      magazines.push({ id: otherId, name: getName(otherId) })
      continue
    }

    const chambers = other._props.Chambers || []
    if (chambers.some(slot => slotAcceptsAmmo(slot, itemId))) {
      weapons.push({ id: otherId, name: getName(otherId) })
    }
  }

  compatibility[itemId] = {
    magazines: magazines.sort((a, b) => a.name.localeCompare(b.name)),
    weapons: weapons.sort((a, b) => a.name.localeCompare(b.name)),
  }
}

const entries = Object.entries(compatibility).sort((a, b) => a[0].localeCompare(b[0]))
const lines = entries.map(([id, data]) => {
  const mags = data.magazines.map(m => `{ id: '${m.id}', name: ${JSON.stringify(m.name)} }`).join(', ')
  const weps = data.weapons.map(w => `{ id: '${w.id}', name: ${JSON.stringify(w.name)} }`).join(', ')
  return `  '${id}': { magazines: [${mags}], weapons: [${weps}] },`
})

const output = `// Generated from SPT 4.0.13 database. Do not edit manually.
// Run: node scripts/generate-ammo-compatibility.cjs

export interface AmmoCompatibility {
  magazines: { id: string; name: string }[]
  weapons: { id: string; name: string }[]
}

export const AMMO_COMPATIBILITY: Record<string, AmmoCompatibility> = {
${lines.join('\n')}
}

export function getAmmoCompatibility(id: string): AmmoCompatibility | undefined {
  return AMMO_COMPATIBILITY[id]
}
`

fs.writeFileSync(outputPath, output, 'utf-8')
console.log(`Generated compatibility data for ${entries.length} ammo types -> ${outputPath}`)
