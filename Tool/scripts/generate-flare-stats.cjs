const fs = require('fs')
const path = require('path')

const itemsPath = process.argv[2] || 'C:/SPT/SPT/SPT_Data/database/templates/items.json'
const items = JSON.parse(fs.readFileSync(itemsPath, 'utf-8'))

const localesPath = process.argv[3] || 'C:/SPT/SPT/SPT_Data/database/locales/global/en.json'
const locales = fs.existsSync(localesPath) ? JSON.parse(fs.readFileSync(localesPath, 'utf-8')) : {}

function getLocaleName(id) {
  return locales[`${id} Name`] || null
}

function getLocaleShortName(id) {
  return locales[`${id} ShortName`] || null
}

const HANDHELD_FLARE_IDS = [
  '6217726288ed9f0845317459',
  '62178be9d0050232da3485d9',
  '62178c4d4ecf221597654e3d',
  '66d98233302686954b0c6f81',
  '66d9f1abb16d9aacf5068468',
  '675ea3d6312c0a5c4e04e317',
  '624c0b3340357b5f566e8766',
]

const CARTRIDGE_IDS = [
  '62389bc9423ed1685422dc57',
  '62389be94d5d474bf712e709',
  '624c09cfbc2e27219346d955',
  '624c09da2cec124eb67c1046',
  '624c09e49b98e019a3315b66',
  '635267f063651329f75a4ee8',
  '66d97834d2985e11480d5c1e',
  '66d9f3047b82b9a9aa055d81',
  '675ea4891b2579e8fe0250aa',
]

const FLARE_TYPES = ['ExitActivate', 'Airdrop', 'CallArtilleryOnMyself', 'Light', 'Quest']

const AIRDROP_TEMPLATE_OPTIONS = [
  { id: '6223349b3136504a544d1608', name: 'Airdrop common supply crate' },
  { id: '622334c873090231d904a9fc', name: 'Airdrop medical crate' },
  { id: '61a89e5445a2672acf66c877', name: 'Airdrop supply crate' },
  { id: '622334fa3136504a544d160c', name: 'Airdrop supply crate' },
  { id: '61a89e812cc17d60cc5f9879', name: 'Airdrop supply crate 2' },
  { id: '66da1b49099cf6adcc07a36b', name: 'Airdrop technical supply crate' },
  { id: '66da1b546916142b3b022777', name: 'Airdrop technical supply crate' },
  { id: '6223351bb5d97a7b2c635ca7', name: 'Airdrop weapon crate' },
  { id: '62f10b79e7ee985f386b2f47', name: 'Airdrop weapon crate' },
  { id: '633ffb5d419dbf4bea7004c6', name: 'Airdrop weapon crate' },
  { id: '67614e3a6a90e4f10b0b140d', name: 'Festive airdrop supply crate' },
]

function getAmmoItemStats(id) {
  const item = items[id]
  if (!item) return null
  const props = item._props || {}
  return {
    name: getLocaleName(id) || item._name,
    shortName: getLocaleShortName(id) || item._name,
    ammoBaseTpl: '',
    damage: props.Damage || 0,
    initialSpeed: props.InitialSpeed || 0,
    stackMaxSize: props.StackMaxSize || 0,
    ammoLifeTimeSec: props.AmmoLifeTimeSec || 0,
    tracer: props.Tracer ?? true,
    tracerColor: props.TracerColor || '',
    tracerDistance: props.TracerDistance || 0,
    backgroundColor: props.BackgroundColor || '',
    backgroundAlpha: 1,
    flareColor: '',
    weight: props.Weight || 0,
    misfireChance: props.MisfireChance || 0,
    ricochetChance: props.RicochetChance || 0,
    flareTypes: props.FlareTypes || [],
    airDropTemplateId: props.AirDropTemplateId || '',
    casingSounds: props.CasingSounds || '',
    ammoType: props.ammoType || '',
    weapClass: props.weapClass || '',
    isSpecialSlotOnly: props.IsSpecialSlotOnly ?? false,
  }
}

function getHandheldStats(id) {
  const item = items[id]
  if (!item) return null
  const props = item._props || {}
  const defAmmo = props.defAmmo
  const ammoProps = defAmmo && items[defAmmo] ? items[defAmmo]._props || {} : {}
  return {
    name: getLocaleName(id) || item._name,
    shortName: getLocaleShortName(id) || item._name,
    ammoBaseTpl: defAmmo || '',
    damage: ammoProps.Damage || 0,
    initialSpeed: ammoProps.InitialSpeed || 0,
    stackMaxSize: ammoProps.StackMaxSize || 0,
    ammoLifeTimeSec: ammoProps.AmmoLifeTimeSec || 0,
    tracer: ammoProps.Tracer ?? true,
    tracerColor: ammoProps.TracerColor || '',
    tracerDistance: ammoProps.TracerDistance || 0,
    backgroundColor: ammoProps.BackgroundColor || '',
    backgroundAlpha: 1,
    flareColor: '',
    weight: ammoProps.Weight || 0,
    misfireChance: ammoProps.MisfireChance || 0,
    ricochetChance: ammoProps.RicochetChance || 0,
    flareTypes: ammoProps.FlareTypes || [],
    airDropTemplateId: ammoProps.AirDropTemplateId || '',
    casingSounds: ammoProps.CasingSounds || '',
    ammoType: ammoProps.ammoType || '',
    weapClass: props.weapClass || '',
    isSpecialSlotOnly: props.IsSpecialSlotOnly ?? false,
  }
}

const handheldStats = {}
for (const id of HANDHELD_FLARE_IDS) {
  const s = getHandheldStats(id)
  if (s) handheldStats[id] = s
}

const cartridgeStats = {}
for (const id of CARTRIDGE_IDS) {
  const s = getAmmoItemStats(id)
  if (s) cartridgeStats[id] = s
}

const sortedHandheld = Object.entries(handheldStats).sort((a, b) => a[1].name.localeCompare(b[1].name))
const sortedCartridges = Object.entries(cartridgeStats).sort((a, b) => a[1].name.localeCompare(b[1].name))

const handheldTemplateLines = sortedHandheld.map(([id, s]) => {
  return `  { id: '${id}', name: ${JSON.stringify(s.name)}, shortName: ${JSON.stringify(s.shortName)} }`
})

const cartridgeTemplateLines = sortedCartridges.map(([id, s]) => {
  return `  { id: '${id}', name: ${JSON.stringify(s.name)}, shortName: ${JSON.stringify(s.shortName)} }`
})

const templateOutput = `// Generated from SPT database. Do not edit manually.
// Run: node scripts/generate-flare-stats.cjs

export interface FlareTemplate {
  id: string
  name: string
  shortName: string
}

export const HANDHELD_FLARE_TEMPLATES: FlareTemplate[] = [
${handheldTemplateLines.join(',\n')}
]

export const CARTRIDGE_TEMPLATES: FlareTemplate[] = [
${cartridgeTemplateLines.join(',\n')}
]

export function getHandheldFlareTemplate(id: string): FlareTemplate | undefined {
  return HANDHELD_FLARE_TEMPLATES.find(t => t.id === id)
}

export function getCartridgeTemplate(id: string): FlareTemplate | undefined {
  return CARTRIDGE_TEMPLATES.find(t => t.id === id)
}
`

const templatePath = path.join(__dirname, '..', 'src', 'generated_flare_templates.ts')
fs.writeFileSync(templatePath, templateOutput)
console.log(`Wrote ${sortedHandheld.length} handheld + ${sortedCartridges.length} cartridge templates to ${templatePath}`)

function buildStatsLines(sortedStats) {
  return sortedStats.map(([id, s]) => {
    const fields = [
      `name: ${JSON.stringify(s.name)}`,
      `shortName: ${JSON.stringify(s.shortName)}`,
      `ammoBaseTpl: ${JSON.stringify(s.ammoBaseTpl)}`,
      `damage: ${s.damage}`,
      `initialSpeed: ${s.initialSpeed}`,
      `stackMaxSize: ${s.stackMaxSize}`,
      `ammoLifeTimeSec: ${s.ammoLifeTimeSec}`,
      `tracer: ${s.tracer}`,
      `tracerColor: ${JSON.stringify(s.tracerColor)}`,
      `tracerDistance: ${s.tracerDistance}`,
      `backgroundColor: ${JSON.stringify(s.backgroundColor)}`,
      `backgroundAlpha: ${s.backgroundAlpha}`,
      `flareColor: ${JSON.stringify(s.flareColor)}`,
      `weight: ${s.weight}`,
      `misfireChance: ${s.misfireChance}`,
      `ricochetChance: ${s.ricochetChance}`,
      `flareTypes: ${JSON.stringify(s.flareTypes)}`,
      `airDropTemplateId: ${JSON.stringify(s.airDropTemplateId)}`,
      `casingSounds: ${JSON.stringify(s.casingSounds)}`,
      `ammoType: ${JSON.stringify(s.ammoType)}`,
      `weapClass: ${JSON.stringify(s.weapClass)}`,
      `isSpecialSlotOnly: ${s.isSpecialSlotOnly}`,
    ]
    return `  '${id}': { ${fields.join(', ')} }`
  })
}

const output = `// Generated from SPT database. Do not edit manually.
// Run: node scripts/generate-flare-stats.cjs

export interface FlareTemplateStats {
  name: string
  shortName: string
  ammoBaseTpl: string
  damage: number
  initialSpeed: number
  stackMaxSize: number
  ammoLifeTimeSec: number
  tracer: boolean
  tracerColor: string
  tracerDistance: number
  backgroundColor: string
  backgroundAlpha: number
  flareColor: string
  weight: number
  misfireChance: number
  ricochetChance: number
  flareTypes: string[]
  airDropTemplateId: string
  casingSounds: string
  ammoType: string
  weapClass: string
  isSpecialSlotOnly: boolean
}

export const FLARE_STATS: Record<string, FlareTemplateStats> = {
${buildStatsLines(sortedHandheld).join(',\n')}
}

export const CARTRIDGE_STATS: Record<string, FlareTemplateStats> = {
${buildStatsLines(sortedCartridges).join(',\n')}
}

export const FLARE_TYPES: string[] = ${JSON.stringify(FLARE_TYPES)}

export interface AirdropTemplate {
  id: string
  name: string
}

export const AIRDROP_TEMPLATE_OPTIONS: AirdropTemplate[] = ${JSON.stringify(AIRDROP_TEMPLATE_OPTIONS, null, 2)}

export function getFlareStats(id: string): FlareTemplateStats | undefined {
  return FLARE_STATS[id]
}

export function getCartridgeStats(id: string): FlareTemplateStats | undefined {
  return CARTRIDGE_STATS[id]
}
`

const outPath = path.join(__dirname, '..', 'src', 'generated_flare_stats.ts')
fs.writeFileSync(outPath, output)
console.log(`Wrote ${sortedHandheld.length} handheld + ${sortedCartridges.length} cartridge stats to ${outPath}`)
