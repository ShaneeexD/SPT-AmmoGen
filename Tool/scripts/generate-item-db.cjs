const fs = require('fs')
const path = require('path')

const itemsPath = 'C:\\SPT\\SPT\\SPT_Data\\database\\templates\\items.json'
const localePath = 'C:\\SPT\\SPT\\SPT_Data\\database\\locales\\global\\en.json'
const outputPath = path.join(__dirname, '..', 'src', 'generated_items.ts')

function loadJson(filePath) {
  const raw = fs.readFileSync(filePath, 'utf-8')
  return JSON.parse(raw)
}

const items = loadJson(itemsPath)
const locale = loadJson(localePath)

const entries = []

for (const itemId of Object.keys(items)) {
  const item = items[itemId]
  const nameKey = `${itemId} Name`
  const shortNameKey = `${itemId} ShortName`

  const name = locale[nameKey] || item?._props?.Name || item?._name || ''
  const shortName = locale[shortNameKey] || item?._props?.ShortName || ''

  if (!name || name.trim() === '' || name === itemId) continue

  entries.push({
    id: itemId,
    name: String(name).trim(),
    shortName: String(shortName || name).trim(),
  })
}

entries.sort((a, b) => a.name.localeCompare(b.name))

const lines = entries.map((e) => `  { id: '${e.id}', name: ${JSON.stringify(e.name)}, shortName: ${JSON.stringify(e.shortName)} },`)

const output = `// Generated from SPT 4.0.13 database. Do not edit manually.
// Run: node scripts/generate-item-db.cjs

export interface ItemEntry {
  id: string
  name: string
  shortName: string
}

export const ITEMS: ItemEntry[] = [
${lines.join('\n')}
]

export function getItemName(id: string): string | undefined {
  return ITEMS.find((i) => i.id === id)?.name
}
`

fs.writeFileSync(outputPath, output, 'utf-8')
console.log(`Generated ${entries.length} item entries -> ${outputPath}`)
