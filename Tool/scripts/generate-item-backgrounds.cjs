const fs = require('fs')
const path = require('path')

const itemsPath = 'C:\\SPT\\SPT\\SPT_Data\\database\\templates\\items.json'
const outputPath = path.join(__dirname, '..', 'src', 'generated_item_backgrounds.ts')

function loadJson(filePath) {
  const raw = fs.readFileSync(filePath, 'utf-8')
  return JSON.parse(raw)
}

const items = loadJson(itemsPath)
const backgrounds = {}

for (const itemId of Object.keys(items)) {
  const item = items[itemId]
  const color = item._props?.BackgroundColor
  if (color && typeof color === 'string') {
    backgrounds[itemId] = color
  }
}

const entries = Object.entries(backgrounds).sort((a, b) => a[0].localeCompare(b[0]))
const lines = entries.map(([id, color]) => {
  return `  '${id}': ${JSON.stringify(color)},`
})

const output = `// Generated from SPT 4.0.13 database. Do not edit manually.
// Run: node scripts/generate-item-backgrounds.cjs

export const ITEM_BACKGROUNDS: Record<string, string> = {
${lines.join('\n')}
}

export function getItemBackgroundColor(id: string): string | undefined {
  return ITEM_BACKGROUNDS[id]
}
`

fs.writeFileSync(outputPath, output, 'utf-8')
console.log(`Generated background colors for ${entries.length} items -> ${outputPath}`)
