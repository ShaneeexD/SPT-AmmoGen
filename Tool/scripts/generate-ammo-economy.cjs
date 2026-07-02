const fs = require('fs')
const path = require('path')

const itemsPath = 'C:\\SPT\\SPT\\SPT_Data\\database\\templates\\items.json'
const pricesPath = 'C:\\SPT\\SPT\\SPT_Data\\database\\templates\\prices.json'
const handbookPath = 'C:\\SPT\\SPT\\SPT_Data\\database\\templates\\handbook.json'
const outputPath = path.join(__dirname, '..', 'src', 'generated_ammo_economy.ts')

function loadJson(filePath) {
  const raw = fs.readFileSync(filePath, 'utf-8')
  return JSON.parse(raw)
}

const items = loadJson(itemsPath)
const prices = loadJson(pricesPath)
const handbook = loadJson(handbookPath)
const AmmoParentId = '5485a8684bdc2da71d8b4567'

const handbookPriceById = {}
if (handbook.Items && Array.isArray(handbook.Items)) {
  for (const entry of handbook.Items) {
    if (entry.Id && typeof entry.Price === 'number') {
      handbookPriceById[entry.Id] = entry.Price
    }
  }
}

const economy = {}

for (const itemId of Object.keys(items)) {
  const item = items[itemId]
  if (item._parent !== AmmoParentId) continue
  if (!item._props || typeof item._props.Damage !== 'number') continue

  const handbookPrice = handbookPriceById[itemId] || 0
  const fleaPrice = prices[itemId] || 0
  const rarityPvE = item._props.RarityPvE || 'Rare'
  const fleaBanned = item._props.CanSellOnRagfair === false

  economy[itemId] = {
    handbookPriceRoubles: handbookPrice,
    fleaPriceRoubles: fleaPrice,
    rarityPvE: String(rarityPvE),
    fleaBanned,
  }
}

const entries = Object.entries(economy).sort((a, b) => a[0].localeCompare(b[0]))

const lines = entries.map(([id, e]) => {
  return `  '${id}': { handbookPriceRoubles: ${e.handbookPriceRoubles}, fleaPriceRoubles: ${e.fleaPriceRoubles}, rarityPvE: ${JSON.stringify(e.rarityPvE)}, fleaBanned: ${e.fleaBanned} },`
})

const output = `// Generated from SPT 4.0.13 database. Do not edit manually.
// Run: node scripts/generate-ammo-economy.cjs

export interface AmmoTemplateEconomy {
  handbookPriceRoubles: number
  fleaPriceRoubles: number
  rarityPvE: string
  fleaBanned: boolean
}

export const AMMO_ECONOMY: Record<string, AmmoTemplateEconomy> = {
${lines.join('\n')}
}

export function getAmmoEconomy(id: string): AmmoTemplateEconomy | undefined {
  return AMMO_ECONOMY[id]
}
`

fs.writeFileSync(outputPath, output, 'utf-8')
console.log(`Generated ${entries.length} ammo economy entries -> ${outputPath}`)
