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

// Manual overrides for weapons the automatic detection misses (e.g. revolvers with internal cylinders).
const weaponOverrides = {
  // 12.7x55mm ammo -> RSh-12 revolver
  '5cadf6ddae9215051e1c23b2': ['633ec7c2a6918cb895019c6c'],
  '5cadf6e5ae921500113bb973': ['633ec7c2a6918cb895019c6c'],
  '5cadf6eeae921500134b2799': ['633ec7c2a6918cb895019c6c'],
  // 12/70 ammo -> MTs-255-12 12ga revolver shotgun
  '5d6e6772a4b936088465b17c': ['60db29ce99594040e04c4a27'],
  '5d6e67fba4b9361bc73bc779': ['60db29ce99594040e04c4a27'],
  '560d5e524bdc2d25448b4571': ['60db29ce99594040e04c4a27'],
  '5d6e6806a4b936088465b17e': ['60db29ce99594040e04c4a27'],
  '5d6e68a8a4b9360b6c0d54e2': ['60db29ce99594040e04c4a27'],
  '5d6e68b3a4b9361bca7e50b5': ['60db29ce99594040e04c4a27'],
  '5d6e68dea4b9361bcc29e659': ['60db29ce99594040e04c4a27'],
  '5d6e6911a4b9361bd5780d52': ['60db29ce99594040e04c4a27'],
  '5d6e68e6a4b9361c140bcfe0': ['60db29ce99594040e04c4a27'],
  '5d6e6869a4b9361c140bcfde': ['60db29ce99594040e04c4a27'],
  '58820d1224597753c90aeb13': ['60db29ce99594040e04c4a27'],
  '5d6e68c4a4b9361b93413f79': ['60db29ce99594040e04c4a27'],
  '64b8ee384b75259c590fa89b': ['60db29ce99594040e04c4a27'],
  '5c0d591486f7744c505b416f': ['60db29ce99594040e04c4a27'],
  '5d6e68d1a4b93622fe60e845': ['60db29ce99594040e04c4a27'],
  '5d6e6891a4b9361bd473feea': ['60db29ce99594040e04c4a27'],
  '5d6e689ca4b9361bc8618956': ['60db29ce99594040e04c4a27'],
  // .357 Magnum ammo -> Chiappa Rhino 50DS revolver
  '62330b3ed4dc74626d570b95': ['61a4c8884f95bc3b2c5dc96f'],
  '62330bfadc5883093563729b': ['61a4c8884f95bc3b2c5dc96f'],
  '62330c18744e5e31df12f516': ['61a4c8884f95bc3b2c5dc96f'],
  '62330c40bdd19b369e1e53d1': ['61a4c8884f95bc3b2c5dc96f'],
  // 9x19mm ammo -> Chiappa Rhino 200DS revolver
  '5c925fa22e221601da359b7b': ['624c2e8614da335f1e034d8c'],
  '64b7bbb74b75259c590fa897': ['624c2e8614da335f1e034d8c'],
  '5c3df7d588a4501f290594e5': ['624c2e8614da335f1e034d8c'],
  '5a3c16fe86f77452b62de32a': ['624c2e8614da335f1e034d8c'],
  '5efb0da7a29a85116f6ea05f': ['624c2e8614da335f1e034d8c'],
  '58864a4f2459770fcc257101': ['624c2e8614da335f1e034d8c'],
  '56d59d3ad2720bdb418b4577': ['624c2e8614da335f1e034d8c'],
  '5efb0e16aeb21837e749c7ff': ['624c2e8614da335f1e034d8c'],
  '5c0d56a986f774449d5de529': ['624c2e8614da335f1e034d8c'],
}

// Manual overrides for magazines the automatic detection misses (e.g. revolver cylinders with camora slots).
const magazineOverrides = {
  // 12/70 ammo -> MTs-255-12 cylinder magazine
  '5d6e6772a4b936088465b17c': ['60dc519adf4c47305f6d410d'],
  '5d6e67fba4b9361bc73bc779': ['60dc519adf4c47305f6d410d'],
  '560d5e524bdc2d25448b4571': ['60dc519adf4c47305f6d410d'],
  '5d6e6806a4b936088465b17e': ['60dc519adf4c47305f6d410d'],
  '5d6e68a8a4b9360b6c0d54e2': ['60dc519adf4c47305f6d410d'],
  '5d6e68b3a4b9361bca7e50b5': ['60dc519adf4c47305f6d410d'],
  '5d6e68dea4b9361bcc29e659': ['60dc519adf4c47305f6d410d'],
  '5d6e6911a4b9361bd5780d52': ['60dc519adf4c47305f6d410d'],
  '5d6e68e6a4b9361c140bcfe0': ['60dc519adf4c47305f6d410d'],
  '5d6e6869a4b9361c140bcfde': ['60dc519adf4c47305f6d410d'],
  '58820d1224597753c90aeb13': ['60dc519adf4c47305f6d410d'],
  '5d6e68c4a4b9361b93413f79': ['60dc519adf4c47305f6d410d'],
  '64b8ee384b75259c590fa89b': ['60dc519adf4c47305f6d410d'],
  '5c0d591486f7744c505b416f': ['60dc519adf4c47305f6d410d'],
  '5d6e68d1a4b93622fe60e845': ['60dc519adf4c47305f6d410d'],
  '5d6e6891a4b9361bd473feea': ['60dc519adf4c47305f6d410d'],
  '5d6e689ca4b9361bc8618956': ['60dc519adf4c47305f6d410d'],
  // .357 Magnum ammo -> Chiappa Rhino cylinder + speedloader
  '62330b3ed4dc74626d570b95': ['619f54a1d25cbd424731fb99', '61a4cda622af7f4f6a3ce617'],
  '62330bfadc5883093563729b': ['619f54a1d25cbd424731fb99', '61a4cda622af7f4f6a3ce617'],
  '62330c18744e5e31df12f516': ['619f54a1d25cbd424731fb99', '61a4cda622af7f4f6a3ce617'],
  '62330c40bdd19b369e1e53d1': ['619f54a1d25cbd424731fb99', '61a4cda622af7f4f6a3ce617'],
  // 9x19mm ammo -> Chiappa Rhino 200DS cylinder magazine
  '5c925fa22e221601da359b7b': ['624c3074dbbd335e8e6becf3'],
  '64b7bbb74b75259c590fa897': ['624c3074dbbd335e8e6becf3'],
  '5c3df7d588a4501f290594e5': ['624c3074dbbd335e8e6becf3'],
  '5a3c16fe86f77452b62de32a': ['624c3074dbbd335e8e6becf3'],
  '5efb0da7a29a85116f6ea05f': ['624c3074dbbd335e8e6becf3'],
  '58864a4f2459770fcc257101': ['624c3074dbbd335e8e6becf3'],
  '56d59d3ad2720bdb418b4577': ['624c3074dbbd335e8e6becf3'],
  '5efb0e16aeb21837e749c7ff': ['624c3074dbbd335e8e6becf3'],
  '5c0d56a986f774449d5de529': ['624c3074dbbd335e8e6becf3'],
}

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

for (const [ammoId, weaponIds] of Object.entries(weaponOverrides)) {
  if (!compatibility[ammoId]) continue
  for (const weaponId of weaponIds) {
    if (!weaponId || compatibility[ammoId].weapons.some(w => w.id === weaponId)) continue
    compatibility[ammoId].weapons.push({ id: weaponId, name: getName(weaponId) })
  }
  compatibility[ammoId].weapons.sort((a, b) => a.name.localeCompare(b.name))
}

for (const [ammoId, magazineIds] of Object.entries(magazineOverrides)) {
  if (!compatibility[ammoId]) continue
  for (const magazineId of magazineIds) {
    if (!magazineId || compatibility[ammoId].magazines.some(m => m.id === magazineId)) continue
    compatibility[ammoId].magazines.push({ id: magazineId, name: getName(magazineId) })
  }
  compatibility[ammoId].magazines.sort((a, b) => a.name.localeCompare(b.name))
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
