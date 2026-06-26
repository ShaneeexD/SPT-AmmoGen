const fs = require('fs')
const path = require('path')

const itemsPath = 'C:\\SPT\\SPT\\SPT_Data\\database\\templates\\items.json'
const localePath = 'C:\\SPT\\SPT\\SPT_Data\\database\\locales\\global\\en.json'
const outputPath = path.join(__dirname, '..', 'src', 'generated_ammo_box_templates.ts')

function loadJson(filePath) {
  const raw = fs.readFileSync(filePath, 'utf-8')
  return JSON.parse(raw)
}

const items = loadJson(itemsPath)
const locale = loadJson(localePath)
const AmmoBoxParentId = '543be5cb4bdc2deb348b4568'

const boxes = []

for (const itemId of Object.keys(items)) {
  const item = items[itemId]
  if (item._parent !== AmmoBoxParentId) continue
  if (!item._props?.StackSlots || item._props.StackSlots.length === 0) continue

  const firstSlot = item._props.StackSlots[0]
  const roundTpl = firstSlot._props?.filters?.[0]?.Filter?.[0]
  if (!roundTpl) continue

  const name = locale[`${itemId} Name`] || item._props.Name || item._name || ''
  const shortName = locale[`${itemId} ShortName`] || item._props.ShortName || ''
  const count = firstSlot._max_count || 0

  boxes.push({
    id: itemId,
    name: String(name).trim(),
    shortName: String(shortName || name).trim(),
    roundTpl,
    count,
  })
}

boxes.sort((a, b) => a.name.localeCompare(b.name))

const lines = boxes.map((b) => {
  return `  { id: '${b.id}', name: ${JSON.stringify(b.name)}, shortName: ${JSON.stringify(b.shortName)}, roundTpl: '${b.roundTpl}', count: ${b.count} },`
})

const output = `// Generated from SPT 4.0.13 database. Do not edit manually.
// Run: node scripts/generate-ammo-box-templates.cjs

export interface AmmoBoxTemplate {
  id: string
  name: string
  shortName: string
  roundTpl: string
  count: number
}

export const AMMO_BOX_TEMPLATES: AmmoBoxTemplate[] = [
${lines.join('\n')}
]

export function getAmmoBoxTemplate(id: string): AmmoBoxTemplate | undefined {
  return AMMO_BOX_TEMPLATES.find((t) => t.id === id)
}
`

fs.writeFileSync(outputPath, output, 'utf-8')
console.log(`Generated ${boxes.length} ammo box templates -> ${outputPath}`)
