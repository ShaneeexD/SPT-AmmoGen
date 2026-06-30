const fs = require('fs')
const path = require('path')

const itemsPath = 'C:\\SPT\\SPT\\SPT_Data\\database\\templates\\items.json'
const localePath = 'C:\\SPT\\SPT\\SPT_Data\\database\\locales\\global\\en.json'
const outputPath = path.join(__dirname, '..', 'src', 'generated_ammo_templates.ts')

function loadJson(filePath) {
  const raw = fs.readFileSync(filePath, 'utf-8')
  return JSON.parse(raw)
}

const items = loadJson(itemsPath)
const locale = loadJson(localePath)
const AmmoParentId = '5485a8684bdc2da71d8b4567'

// Preserve any manually-added requiresMod metadata from the existing file
const existingPath = path.join(__dirname, '..', 'src', 'generated_ammo_templates.ts')
const existingRequiresMod = new Map()
if (fs.existsSync(existingPath)) {
  const existing = fs.readFileSync(existingPath, 'utf-8')
  const re = /\{ id: '([^']+)'[^}]*\}/g
  let m
  while ((m = re.exec(existing)) !== null) {
    const block = m[0]
    const id = m[1]
    const modMatch = block.match(/requiresMod:\s*['"]([^'"]+)['"]/)
    const urlMatch = block.match(/requiresModUrl:\s*['"]([^'"]+)['"]/)
    if (modMatch) {
      existingRequiresMod.set(id, {
        requiresMod: modMatch[1],
        requiresModUrl: urlMatch ? urlMatch[1] : undefined,
      })
    }
  }
}

const templates = []

for (const itemId of Object.keys(items)) {
  const item = items[itemId]
  if (item._parent !== AmmoParentId) continue
  if (!item._props || typeof item._props.Damage !== 'number') continue

  const name = locale[`${itemId} Name`] || item._props.Name || item._name || ''
  const cleanName = String(name).trim()
  if (!cleanName) continue

  const caliberMatch = cleanName.match(/^([0-9.]+[xX][0-9.]+[a-zA-Z]*|\.?[0-9]+\s*[a-zA-Z]+|12\/70|20\/70|23x75|\.338[^a-zA-Z]*)/i)
  const caliber = caliberMatch ? caliberMatch[1].replace(/\s+/g, ' ').trim() : 'Other'

  const entry = { id: itemId, name: cleanName, caliber }
  const extra = existingRequiresMod.get(itemId)
  if (extra) {
    entry.requiresMod = extra.requiresMod
    if (extra.requiresModUrl) entry.requiresModUrl = extra.requiresModUrl
  }

  templates.push(entry)
}

// Mod-added ammo templates that are not present in the base SPT database
const manualTemplates = [
  { id: '67c540c3d0538d12ec036c08', name: '.308 ME', caliber: '.308 ME', requiresMod: 'WTT - Content Backport', requiresModUrl: 'https://forge.sp-tarkov.com/mod/2512/wtt-content-backport' },
  { id: '67c540cfb032bbdb530201b8', name: '.308 ME LOKT', caliber: '.308 ME', requiresMod: 'WTT - Content Backport', requiresModUrl: 'https://forge.sp-tarkov.com/mod/2512/wtt-content-backport' },
]

for (const t of manualTemplates) {
  if (!templates.some((x) => x.id === t.id)) {
    templates.push(t)
  }
}

templates.sort((a, b) => a.name.localeCompare(b.name))

const lines = templates.map((t) => {
  const extra = []
  if (t.requiresMod) extra.push(`requiresMod: '${t.requiresMod.replace(/'/g, "\\'")}'`)
  if (t.requiresModUrl) extra.push(`requiresModUrl: '${t.requiresModUrl.replace(/'/g, "\\'")}'`)
  const extraStr = extra.length ? ', ' + extra.join(', ') : ''
  return `  { id: '${t.id}', name: ${JSON.stringify(t.name)}, caliber: '${t.caliber}'${extraStr} },`
})

const output = `// Generated from SPT 4.0.13 database. Do not edit manually.
// Run: node scripts/generate-ammo-templates.cjs

export interface AmmoTemplate {
  id: string
  name: string
  caliber: string
  requiresMod?: string
  requiresModUrl?: string
}

export const AMMO_TEMPLATES: AmmoTemplate[] = [
${lines.join('\n')}
]

export function getAmmoTemplate(id: string): AmmoTemplate | undefined {
  return AMMO_TEMPLATES.find((t) => t.id === id)
}
`

fs.writeFileSync(outputPath, output, 'utf-8')
console.log(`Generated ${templates.length} ammo templates -> ${outputPath}`)
