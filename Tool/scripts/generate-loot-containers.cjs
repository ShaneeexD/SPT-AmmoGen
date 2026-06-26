const fs = require('fs')
const path = require('path')

const itemsPath = 'C:\\SPT\\SPT\\SPT_Data\\database\\templates\\items.json'
const localePath = 'C:\\SPT\\SPT\\SPT_Data\\database\\locales\\global\\en.json'
const outputPath = path.join(__dirname, '..', 'src', 'generated_loot_containers.ts')

function loadJson(filePath) {
  const raw = fs.readFileSync(filePath, 'utf-8')
  return JSON.parse(raw)
}

const items = loadJson(itemsPath)
const locale = loadJson(localePath)
const LootContainerParentId = '566965d44bdc2d814c8b4571'

const containers = []

for (const itemId of Object.keys(items)) {
  const item = items[itemId]
  if (item._parent !== LootContainerParentId) continue
  if (!item._props) continue

  const nameKey = `${itemId} Name`
  const shortNameKey = `${itemId} ShortName`

  const name = locale[nameKey] || item._props.Name || item._name || ''
  const shortName = locale[shortNameKey] || item._props.ShortName || name

  containers.push({
    id: itemId,
    name: String(name).trim(),
    shortName: String(shortName).trim(),
  })
}

containers.sort((a, b) => a.name.localeCompare(b.name))

const lines = containers.map((c) => {
  return `  { id: ${JSON.stringify(c.id)}, name: ${JSON.stringify(c.name)}, shortName: ${JSON.stringify(c.shortName)} },`
})

const output = `// Generated from SPT 4.0.13 database. Do not edit manually.
// Run: node scripts/generate-loot-containers.cjs

export interface LootContainer {
  id: string
  name: string
  shortName: string
}

export const LOOT_CONTAINERS: LootContainer[] = [
${lines.join('\n')}
]

export function getLootContainer(id: string): LootContainer | undefined {
  return LOOT_CONTAINERS.find((c) => c.id === id)
}
`

fs.writeFileSync(outputPath, output, 'utf-8')
console.log(`Generated ${containers.length} loot container entries -> ${outputPath}`)
