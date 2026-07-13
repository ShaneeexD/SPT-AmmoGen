const fs = require('fs')
const path = require('path')

const itemsPath = 'C:\\SPT\\SPT\\SPT_Data\\database\\templates\\items.json'
const outputPath = path.join(__dirname, '..', 'src', 'generated_ammo_properties.ts')

function loadJson(filePath) {
  const raw = fs.readFileSync(filePath, 'utf-8')
  return JSON.parse(raw)
}

const items = loadJson(itemsPath)
const AmmoParentId = '5485a8684bdc2da71d8b4567'

const properties = {}

for (const itemId of Object.keys(items)) {
  const item = items[itemId]
  if (item._parent !== AmmoParentId) continue
  if (!item._props || typeof item._props.Damage !== 'number') continue

  properties[itemId] = item._props
}

const entries = Object.entries(properties).sort((a, b) => {
  const nameA = a[1].Name || a[1].ShortName || a[0]
  const nameB = b[1].Name || b[1].ShortName || b[0]
  return String(nameA).localeCompare(String(nameB))
})

const sortedProperties = {}
for (const [id, props] of entries) {
  sortedProperties[id] = props
}

const output = `// Generated from SPT 4.0.13 database. Do not edit manually.
// Run: node scripts/generate-ammo-properties.cjs

export const AMMO_PROPERTIES: Record<string, Record<string, any>> = ${JSON.stringify(sortedProperties, null, 2)}

export function getAmmoProperties(id: string): Record<string, any> | undefined {
  return AMMO_PROPERTIES[id]
}
`

fs.writeFileSync(outputPath, output, 'utf-8')
console.log(`Generated ${entries.length} ammo property entries -> ${outputPath}`)
