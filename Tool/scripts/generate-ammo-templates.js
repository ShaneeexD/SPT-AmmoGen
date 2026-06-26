const fs = require('fs')
const path = require('path')

const files = [
  'D:/SPT MODS/TraderGen/Tool/src/ammoBoxes_part1.ts',
  'D:/SPT MODS/TraderGen/Tool/src/ammoBoxes_part2.ts',
  'D:/SPT MODS/TraderGen/Tool/src/ammoBoxes_part3.ts',
]

const seen = new Map()

for (const file of files) {
  const content = fs.readFileSync(file, 'utf8')
  const lines = content.split('\n')
  for (let i = 0; i < lines.length; i++) {
    const line = lines[i].trim()
    if (line.startsWith('//')) {
      const comment = line.slice(2).trim()
      const nextLine = lines[i + 1]
      if (!nextLine) continue
      const m = nextLine.match(/roundTpl:\s*['"]([^'"]+)['"]/)
      if (m) {
        const id = m[1]
        const name = comment.replace(/\s+ammo pack\s*\(\d+\s*pcs\)/i, '').trim()
        if (!seen.has(id)) {
          seen.set(id, name)
        }
      }
    }
  }
}

const entries = Array.from(seen.entries()).map(([id, name]) => {
  const caliberMatch = name.match(/^([0-9.]+[xX][0-9.]+[a-zA-Z]*|\.?[0-9]+\s*[a-zA-Z]+|12\/70|20\/70|23x75|\.338[^a-zA-Z]*)/i)
  const caliber = caliberMatch ? caliberMatch[1].replace(/\s+/g, ' ').trim() : 'Other'
  return `  { id: '${id}', name: '${name.replace(/'/g, "\\'")}', caliber: '${caliber}' },`
})

const output = 'export const AMMO_TEMPLATES = [\n' + entries.join('\n') + '\n]\n'
fs.writeFileSync('D:/SPT MODS/AmmoGen/Tool/src/generated_ammo_templates.ts', output)
console.log('Generated', entries.length, 'ammo templates')
