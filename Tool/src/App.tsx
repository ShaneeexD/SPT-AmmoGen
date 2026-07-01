import { useState, useRef, useEffect, useMemo, type Dispatch, type SetStateAction } from 'react'
import {
  Crosshair,
  Plus,
  Trash2,
  Download,
  AlertCircle,
  CheckCircle,
  RefreshCw,
  FileJson,
  Shield,
  Star,
  Package,
  Target,
  Filter,
  Wrench,
  Upload,
  Menu,
  X,
  Store,
  ExternalLink,
  HelpCircle,
  Box,
  MapPin,
  ChevronDown,
  Bomb,
} from 'lucide-react'
import {
  AmmoDefinition,
  AmmoPackDefinition,
  AmmoStats,
  AmmoEconomy,
  Vector3,
  AmmoBoxEntry,
  LootEntry,
  CraftingEntry,
  AMMO_TEMPLATES,
  createDefaultAmmo,
  createDefaultLootEntry,
  createDefaultPack,
  createDefaultTraderEntry,
  generateMongoId,
  RARITY_OPTIONS,
  TraderEntry,
  VANILLA_TRADERS,
  ValidationError,
  GrenadeDefinition,
  GrenadeStats,
  GRENADE_TEMPLATES,
  createDefaultGrenade,
} from './types'
import { ITEMS, getItemName } from './generated_items'
import { getAmmoStats, AmmoTemplateStats, TRACER_COLOR_OPTIONS, AMMO_SFX_OPTIONS, CASING_SOUNDS_OPTIONS } from './generated_ammo_stats'
import { getGrenadeStats, type GrenadeTemplateStats, GRENADE_FRAGMENT_TYPES, GRENADE_EXPLOSION_EFFECT_TYPES, GRENADE_THROW_TYPES } from './generated_grenade_stats'
import { getAmmoCompatibility } from './generated_ammo_compatibility'
import { AMMO_BOX_TEMPLATES, getAmmoBoxTemplate } from './generated_ammo_box_templates'
import { LOOT_CONTAINERS, getLootContainer } from './generated_loot_containers'
import JSZip from 'jszip'
import { saveAs } from 'file-saver'

function SearchableSelect({
  value,
  onChange,
  placeholder = 'Search item...',
}: {
  value: string
  onChange: (id: string) => void
  placeholder?: string
}) {
  const [open, setOpen] = useState(false)
  const [query, setQuery] = useState('')
  const containerRef = useRef<HTMLDivElement>(null)
  const selected = ITEMS.find((i) => i.id === value)

  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClick)
    return () => document.removeEventListener('mousedown', handleClick)
  }, [])

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase()
    if (!q) return ITEMS.slice(0, 50)
    return ITEMS.filter(
      (i) =>
        i.name.toLowerCase().includes(q) ||
        i.shortName.toLowerCase().includes(q) ||
        i.id.toLowerCase().includes(q)
    ).slice(0, 100)
  }, [query])

  return (
    <div ref={containerRef} className="relative flex-1">
      <input
        className="input-field w-full"
        placeholder={placeholder}
        value={open ? query : selected?.name || value}
        onFocus={() => {
          setQuery(selected?.name || '')
          setOpen(true)
        }}
        onChange={(e) => {
          setQuery(e.target.value)
          setOpen(true)
        }}
      />
      {open && (
        <div className="absolute z-10 mt-1 w-full max-h-60 overflow-y-auto bg-tarkov-surface border border-tarkov-border rounded shadow-lg">
          {filtered.length === 0 ? (
            <div className="px-3 py-2 text-sm text-tarkov-text-dim">No items found</div>
          ) : (
            filtered.map((item) => (
              <button
                key={item.id}
                className="w-full text-left px-3 py-2 text-sm hover:bg-tarkov-border/50 text-tarkov-text"
                onClick={() => {
                  onChange(item.id)
                  setQuery(item.name)
                  setOpen(false)
                }}
              >
                <div className="truncate">{item.name}</div>
                <div className="text-xs text-tarkov-text-dim font-mono truncate">{item.id}</div>
              </button>
            ))
          )}
        </div>
      )}
    </div>
  )
}

function Sidebar({ open, onClose }: { open: boolean; onClose: () => void }) {
  const ref = useRef<HTMLDivElement>(null)

  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        onClose()
      }
    }
    if (open) {
      document.addEventListener('mousedown', handleClick)
      return () => document.removeEventListener('mousedown', handleClick)
    }
  }, [open, onClose])

  const links = [
    { name: 'AmmoGen Tool', url: 'https://ammogen-tool.netlify.app', icon: <Target size={18} />, active: true },
    { name: 'TraderGen Tool', url: 'https://tradergen-tool.netlify.app', icon: <Store size={18} />, active: false },
  ]

  return (
    <>
      {/* Backdrop */}
      {open && (
        <div className="fixed inset-0 bg-black/50 z-40 transition-opacity" onClick={onClose} />
      )}

      {/* Drawer */}
      <div
        ref={ref}
        className={`fixed top-0 left-0 h-full w-64 bg-tarkov-surface border-r border-tarkov-border z-50 transform transition-transform duration-200 ${
          open ? 'translate-x-0' : '-translate-x-full'
        }`}
      >
        <div className="flex items-center justify-between px-4 py-4 border-b border-tarkov-border">
          <div className="flex items-center gap-2 text-tarkov-accent">
            <Target size={22} />
            <span className="font-bold">Serenity Mods</span>
          </div>
          <button
            onClick={onClose}
            className="p-1 rounded hover:bg-tarkov-border/50 text-tarkov-text-dim hover:text-tarkov-text transition-colors"
            aria-label="Close menu"
          >
            <X size={20} />
          </button>
        </div>

        <nav className="p-2 space-y-1">
          {links.map((link) => (
            <a
              key={link.name}
              href={link.url}
              target="_blank"
              rel="noopener noreferrer"
              className={`flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm transition-colors ${
                link.active
                  ? 'bg-tarkov-accent/20 text-tarkov-accent border border-tarkov-accent/50'
                  : 'text-tarkov-text hover:bg-tarkov-border/50 hover:text-tarkov-text'
              }`}
            >
              {link.icon}
              <span className="flex-1">{link.name}</span>
              <ExternalLink size={14} className="text-tarkov-text-dim" />
            </a>
          ))}
        </nav>
      </div>
    </>
  )
}

function validatePack(pack: AmmoPackDefinition): ValidationError[] {
  const errors: ValidationError[] = []
  const hex24 = /^[0-9a-fA-F]{24}$/

  if (!pack.name.trim()) errors.push({ field: 'name', message: 'Pack name is required' })

  pack.ammo.forEach((ammo, i) => {
    const prefix = `ammo[${i}]`
    if (!hex24.test(ammo.id)) errors.push({ field: `${prefix}.id`, message: 'ID must be 24 hex chars' })
    if (!hex24.test(ammo.baseTpl)) errors.push({ field: `${prefix}.baseTpl`, message: 'Base template must be 24 hex chars' })
    if (!ammo.name.trim()) errors.push({ field: `${prefix}.name`, message: 'Name is required' })
    if (!ammo.shortName.trim()) errors.push({ field: `${prefix}.shortName`, message: 'Short name is required' })
    if (!ammo.description.trim()) errors.push({ field: `${prefix}.description`, message: 'Description is required' })

    ammo.traders.forEach((trader, j) => {
      if (!trader.enabled) return
      const tPrefix = `${prefix}.traders[${j}]`
      if (!hex24.test(trader.traderId)) errors.push({ field: `${tPrefix}.traderId`, message: 'Trader ID must be 24 hex chars' })
      if (trader.loyaltyLevel < 1) errors.push({ field: `${tPrefix}.loyaltyLevel`, message: 'Loyalty level must be >= 1' })
      if (trader.priceRoubles < 0) errors.push({ field: `${tPrefix}.priceRoubles`, message: 'Price cannot be negative' })
    })

    if (ammo.crafting.enabled) {
      if (ammo.crafting.workbenchLevel < 1) errors.push({ field: `${prefix}.crafting.workbenchLevel`, message: 'Workbench level must be >= 1' })
      if (ammo.crafting.craftTimeSeconds < 1) errors.push({ field: `${prefix}.crafting.craftTimeSeconds`, message: 'Craft time must be >= 1' })
      if (ammo.crafting.outputCount < 1) errors.push({ field: `${prefix}.crafting.outputCount`, message: 'Output count must be >= 1' })
      ammo.crafting.requirements.forEach((req, j) => {
        if (!hex24.test(req.tpl)) errors.push({ field: `${prefix}.crafting.requirements[${j}]`, message: 'Item tpl must be 24 hex chars' })
        if (req.count < 1) errors.push({ field: `${prefix}.crafting.requirements[${j}]`, message: 'Count must be >= 1' })
      })
    }

    ammo.filters.patchMagazines.forEach((id, j) => {
      if (!hex24.test(id)) errors.push({ field: `${prefix}.filters.patchMagazines[${j}]`, message: 'Magazine ID must be 24 hex chars' })
    })
    ammo.filters.patchWeapons.forEach((id, j) => {
      if (!hex24.test(id)) errors.push({ field: `${prefix}.filters.patchWeapons[${j}]`, message: 'Weapon ID must be 24 hex chars' })
    })

    if (ammo.ammoBox.enabled) {
      if (!hex24.test(ammo.ammoBox.id)) errors.push({ field: `${prefix}.ammoBox.id`, message: 'Ammo box ID must be 24 hex chars' })
      if (!hex24.test(ammo.ammoBox.baseTpl)) errors.push({ field: `${prefix}.ammoBox.baseTpl`, message: 'Base ammo box template must be 24 hex chars' })
      if (ammo.ammoBox.count < 1) errors.push({ field: `${prefix}.ammoBox.count`, message: 'Ammo box count must be >= 1' })
      if (!ammo.ammoBox.name.trim()) errors.push({ field: `${prefix}.ammoBox.name`, message: 'Ammo box name is required' })
      if (!ammo.ammoBox.shortName.trim()) errors.push({ field: `${prefix}.ammoBox.shortName`, message: 'Ammo box short name is required' })
    }

    if (ammo.ammoLoot.enabled) {
      ammo.ammoLoot.containerIds.forEach((id, j) => {
        if (!hex24.test(id)) errors.push({ field: `${prefix}.ammoLoot.containerIds[${j}]`, message: 'Container ID must be 24 hex chars' })
      })
    }

    if (ammo.ammoBoxLoot.enabled) {
      ammo.ammoBoxLoot.containerIds.forEach((id, j) => {
        if (!hex24.test(id)) errors.push({ field: `${prefix}.ammoBoxLoot.containerIds[${j}]`, message: 'Container ID must be 24 hex chars' })
      })
      if (!ammo.ammoBox.enabled) {
        errors.push({ field: `${prefix}.ammoBoxLoot.enabled`, message: 'Ammo box loot requires ammoBox.enabled to be true' })
      }
    }

    if (ammo.stats.durabilityBurnModificator < 0) {
      errors.push({ field: `${prefix}.stats.durabilityBurnModificator`, message: 'Durability burn cannot be negative' })
    }

    if (ammo.stats.ballisticCoeficient < 0) {
      errors.push({ field: `${prefix}.stats.ballisticCoeficient`, message: 'Ballistic coefficient cannot be negative' })
    }

    const nonNegativeStats = [
      'ricochetChance', 'fragmentationChance', 'penetrationChanceObstacle', 'misfireChance',
      'malfMisfireChance', 'malfFeedChance', 'heatFactor', 'staminaBurnPerDamage',
      'bulletMassGram', 'bulletDiameterMilimeters', 'tracerDistance', 'fuzeArmTimeSec',
      'minExplosionDistance', 'maxExplosionDistance', 'explosionStrength', 'lightAndSoundShotAngle',
      'lightAndSoundShotSelfContusionTime', 'lightAndSoundShotSelfContusionStrength',
    ] as const
    nonNegativeStats.forEach(stat => {
      if (ammo.stats[stat] < 0) {
        errors.push({ field: `${prefix}.stats.${stat}`, message: `${stat} cannot be negative` })
      }
    })

    if (ammo.stats.projectileCount < 0) {
      errors.push({ field: `${prefix}.stats.projectileCount`, message: 'Projectile count cannot be negative' })
    }
    if (ammo.stats.fragmentsCount < 0) {
      errors.push({ field: `${prefix}.stats.fragmentsCount`, message: 'Fragments count cannot be negative' })
    }
    if (ammo.stats.ammoLifeTimeSec < 0) {
      errors.push({ field: `${prefix}.stats.ammoLifeTimeSec`, message: 'Ammo life time cannot be negative' })
    }

    ;['armorDistanceDistanceDamage', 'contusion', 'blindness'].forEach(key => {
      const v = ammo.stats[key as keyof AmmoStats] as Vector3
      if (v.x < 0 || v.y < 0 || v.z < 0) {
        errors.push({ field: `${prefix}.stats.${key}`, message: `${key} components cannot be negative` })
      }
    })
  })

  pack.grenades.forEach((grenade, i) => {
    const prefix = `grenade[${i}]`
    if (!hex24.test(grenade.id)) errors.push({ field: `${prefix}.id`, message: 'ID must be 24 hex chars' })
    if (!hex24.test(grenade.baseTpl)) errors.push({ field: `${prefix}.baseTpl`, message: 'Base template must be 24 hex chars' })
    if (!grenade.name.trim()) errors.push({ field: `${prefix}.name`, message: 'Name is required' })
    if (!grenade.shortName.trim()) errors.push({ field: `${prefix}.shortName`, message: 'Short name is required' })
    if (!grenade.description.trim()) errors.push({ field: `${prefix}.description`, message: 'Description is required' })

    grenade.traders.forEach((trader, j) => {
      if (!trader.enabled) return
      const tPrefix = `${prefix}.traders[${j}]`
      if (!hex24.test(trader.traderId)) errors.push({ field: `${tPrefix}.traderId`, message: 'Trader ID must be 24 hex chars' })
      if (trader.loyaltyLevel < 1) errors.push({ field: `${tPrefix}.loyaltyLevel`, message: 'Loyalty level must be >= 1' })
      if (trader.priceRoubles < 0) errors.push({ field: `${tPrefix}.priceRoubles`, message: 'Price cannot be negative' })
    })

    if (grenade.crafting.enabled) {
      if (grenade.crafting.workbenchLevel < 1) errors.push({ field: `${prefix}.crafting.workbenchLevel`, message: 'Workbench level must be >= 1' })
      if (grenade.crafting.craftTimeSeconds < 1) errors.push({ field: `${prefix}.crafting.craftTimeSeconds`, message: 'Craft time must be >= 1' })
      if (grenade.crafting.outputCount < 1) errors.push({ field: `${prefix}.crafting.outputCount`, message: 'Output count must be >= 1' })
      grenade.crafting.requirements.forEach((req, j) => {
        if (!hex24.test(req.tpl)) errors.push({ field: `${prefix}.crafting.requirements[${j}]`, message: 'Item tpl must be 24 hex chars' })
        if (req.count < 1) errors.push({ field: `${prefix}.crafting.requirements[${j}]`, message: 'Count must be >= 1' })
      })
    }

    if (grenade.loot.enabled) {
      grenade.loot.containerIds.forEach((id, j) => {
        if (!hex24.test(id)) errors.push({ field: `${prefix}.loot.containerIds[${j}]`, message: 'Container ID must be 24 hex chars' })
      })
    }

    const nonNegativeGrenadeStats = [
      'minExplosionDistance', 'maxExplosionDistance', 'contusionDistance', 'explDelay',
      'strength', 'throwDamMax', 'weight',
    ] as const
    nonNegativeGrenadeStats.forEach(stat => {
      if (grenade.stats[stat] < 0) {
        errors.push({ field: `${prefix}.stats.${stat}`, message: `${stat} cannot be negative` })
      }
    })
    if (grenade.stats.fragmentsCount < 0) {
      errors.push({ field: `${prefix}.stats.fragmentsCount`, message: 'Fragments count cannot be negative' })
    }

    ;['armorDistanceDistanceDamage', 'contusion', 'blindness'].forEach(key => {
      const v = grenade.stats[key as keyof GrenadeStats] as Vector3
      if (v.x < 0 || v.y < 0 || v.z < 0) {
        errors.push({ field: `${prefix}.stats.${key}`, message: `${key} components cannot be negative` })
      }
    })
  })

  return errors
}

function buildExportJson(pack: AmmoPackDefinition): object {
  return {
    enabled: pack.enabled,
    name: pack.name,
    ammo: pack.ammo.map((ammo) => ({
      id: ammo.id,
      enabled: ammo.enabled,
      baseTpl: ammo.baseTpl,
      name: ammo.name,
      shortName: ammo.shortName,
      description: ammo.description,
      ...(ammo.handbookParentId ? { handbookParentId: ammo.handbookParentId } : {}),
      stats: ammo.stats,
      economy: ammo.economy,
      traders: ammo.traders,
      crafting: ammo.crafting,
      filters: ammo.filters,
      ammoBox: ammo.ammoBox,
      ammoLoot: ammo.ammoLoot,
      ammoBoxLoot: ammo.ammoBoxLoot,
    })),
    grenades: pack.grenades.map((grenade) => ({
      id: grenade.id,
      enabled: grenade.enabled,
      baseTpl: grenade.baseTpl,
      name: grenade.name,
      shortName: grenade.shortName,
      description: grenade.description,
      ...(grenade.handbookParentId ? { handbookParentId: grenade.handbookParentId } : {}),
      stats: grenade.stats,
      economy: grenade.economy,
      traders: grenade.traders,
      crafting: grenade.crafting,
      loot: grenade.loot,
    })),
  }
}

type Tab = 'identity' | 'stats' | 'economy' | 'trader' | 'crafting' | 'filters' | 'ammobox' | 'loot' | 'preview'

export default function App() {
  const [pack, setPack] = useState<AmmoPackDefinition>(createDefaultPack())
  const [activeIndex, setActiveIndex] = useState(0)
  const [mode, setMode] = useState<'ammo' | 'grenade'>('ammo')
  const [errors, setErrors] = useState<ValidationError[]>([])
  const [activeTab, setActiveTab] = useState<Tab>('identity')
  const [showExportSuccess, setShowExportSuccess] = useState(false)
  const [sidebarOpen, setSidebarOpen] = useState(false)

  const updateAmmo = (index: number, updates: Partial<AmmoDefinition>) => {
    const next = { ...pack, ammo: [...pack.ammo] }
    next.ammo[index] = { ...next.ammo[index], ...updates }
    setPack(next)
    setErrors([])
  }

  const addAmmo = () => {
    setPack({ ...pack, ammo: [...pack.ammo, createDefaultAmmo()] })
    setActiveIndex(pack.ammo.length)
    setErrors([])
  }

  const removeAmmo = (index: number) => {
    const next = { ...pack, ammo: pack.ammo.filter((_, i) => i !== index) }
    setPack(next)
    if (activeIndex >= next.ammo.length) setActiveIndex(Math.max(0, next.ammo.length - 1))
    setErrors([])
  }

  const updateGrenade = (index: number, updates: Partial<GrenadeDefinition>) => {
    const next = { ...pack, grenades: [...pack.grenades] }
    next.grenades[index] = { ...next.grenades[index], ...updates }
    setPack(next)
    setErrors([])
  }

  const addGrenade = () => {
    setPack({ ...pack, grenades: [...pack.grenades, createDefaultGrenade()] })
    setActiveIndex(pack.grenades.length)
    setErrors([])
  }

  const removeGrenade = (index: number) => {
    const next = { ...pack, grenades: pack.grenades.filter((_, i) => i !== index) }
    setPack(next)
    if (activeIndex >= next.grenades.length) setActiveIndex(Math.max(0, next.grenades.length - 1))
    setErrors([])
  }

  const downloadJson = () => {
    const validationErrors = validatePack(pack)
    setErrors(validationErrors)
    if (validationErrors.length > 0) {
      setActiveTab('identity')
      return
    }

    const json = buildExportJson(pack)
    const blob = new Blob([JSON.stringify(json, null, 2)], { type: 'application/json' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `${pack.name.toLowerCase().replace(/\s+/g, '-')}.json`
    a.click()
    URL.revokeObjectURL(url)
    setShowExportSuccess(true)
    setTimeout(() => setShowExportSuccess(false), 3000)
  }

  const exportModZip = async () => {
    const validationErrors = validatePack(pack)
    setErrors(validationErrors)
    if (validationErrors.length > 0) {
      setActiveTab('identity')
      return
    }

    const zip = new JSZip()
    const json = buildExportJson(pack)
    const packName = pack.name.toLowerCase().replace(/\s+/g, '-')
    zip.file(`SPT/user/mods/AmmoGen/ammo/${packName}.json`, JSON.stringify(json, null, 2))

    const zipBlob = await zip.generateAsync({ type: 'blob' })
    saveAs(zipBlob, `${packName}.zip`)
    setShowExportSuccess(true)
    setTimeout(() => setShowExportSuccess(false), 3000)
  }

  const parsePackJson = (raw: string): AmmoPackDefinition => {
    const cleaned = raw
      .replace(/\/\/.*$/gm, '')
      .replace(/\/\*[\s\S]*?\*\//g, '')
      .replace(/,\s*([\]}])/g, '$1')
    const parsed = JSON.parse(cleaned)
    const ammo = (parsed.ammo ?? []).map((a: AmmoDefinition & { trader?: Partial<TraderEntry>; loot?: LootEntry & { lootItem?: 'ammo' | 'box' | 'both' } }) => {
      const defaults = createDefaultAmmo()
      // Strip the legacy loot field so it doesn't leak into the new AmmoDefinition shape
      const { loot: _, ...aWithoutLoot } = a
      const normalized: AmmoDefinition = { ...defaults, ...aWithoutLoot }
      // Backward compatibility: old single "trader" field -> new "traders" array
      if (a.trader && !Array.isArray(a.traders)) {
        normalized.traders = [a.trader as TraderEntry]
      }
      // Backward compatibility: missing ammoBox / loot fields
      if (!a.ammoBox) normalized.ammoBox = defaults.ammoBox
      if (!a.ammoLoot) normalized.ammoLoot = defaults.ammoLoot
      if (!a.ammoBoxLoot) normalized.ammoBoxLoot = defaults.ammoBoxLoot
      // Backward compatibility: legacy single "loot" field -> split into ammoLoot and ammoBoxLoot
      if (a.loot && !a.ammoLoot && !a.ammoBoxLoot) {
        const legacyLoot = a.loot
        normalized.ammoLoot = { ...createDefaultLootEntry(), enabled: legacyLoot.enabled, containerIds: legacyLoot.containerIds, rarity: legacyLoot.rarity }
        normalized.ammoBoxLoot = { ...createDefaultLootEntry(), enabled: legacyLoot.enabled, containerIds: legacyLoot.containerIds, rarity: legacyLoot.rarity }
        if (legacyLoot.lootItem === 'ammo' || legacyLoot.lootItem === undefined) {
          normalized.ammoBoxLoot.enabled = false
        } else if (legacyLoot.lootItem === 'box') {
          normalized.ammoLoot.enabled = false
        }
      }
      // Backward compatibility: missing stats fields (light/heavy bleed delta)
      normalized.stats = { ...defaults.stats, ...a.stats }
      // Backward compatibility: missing ammoBox sellToTraders / traderPriceRoubles / trader settings
      if (normalized.ammoBox) {
        normalized.ammoBox = { ...createDefaultAmmo().ammoBox, ...a.ammoBox }
      }
      // Backward compatibility: old "0 means infinite" semantics -> new unlimited flags
      normalized.traders = normalized.traders.map((t) => {
        if (t.unlimitedStock === undefined && t.stockCount === 0) {
          t.unlimitedStock = true
          t.stockCount = 200
        }
        if (t.unlimitedBuyRestriction === undefined && t.buyRestrictionMax === 0) {
          t.unlimitedBuyRestriction = true
          t.buyRestrictionMax = 200
        }
        return t
      })
      if (normalized.ammoBox && normalized.ammoBox.stockCount === 0 && normalized.ammoBox.unlimitedStock === undefined) {
        normalized.ammoBox.unlimitedStock = true
        normalized.ammoBox.stockCount = 200
      }
      if (normalized.ammoBox && normalized.ammoBox.buyRestrictionMax === 0 && normalized.ammoBox.unlimitedBuyRestriction === undefined) {
        normalized.ammoBox.unlimitedBuyRestriction = true
        normalized.ammoBox.buyRestrictionMax = 200
      }
      return normalized
    })
    const grenades = (parsed.grenades ?? []).map((g: GrenadeDefinition) => {
      const defaults = createDefaultGrenade()
      const normalized: GrenadeDefinition = { ...defaults, ...g }
      normalized.stats = { ...defaults.stats, ...g.stats }
      normalized.traders = normalized.traders.map((t) => {
        if (t.unlimitedStock === undefined && t.stockCount === 0) {
          t.unlimitedStock = true
          t.stockCount = 200
        }
        if (t.unlimitedBuyRestriction === undefined && t.buyRestrictionMax === 0) {
          t.unlimitedBuyRestriction = true
          t.buyRestrictionMax = 200
        }
        return t
      })
      return normalized
    })
    return {
      ...createDefaultPack(),
      ...parsed,
      ammo,
      grenades,
    }
  }

  const importPack = () => {
    const input = document.createElement('input')
    input.type = 'file'
    input.accept = '.json,.zip'
    input.onchange = async (e) => {
      const file = (e.target as HTMLInputElement).files?.[0]
      if (!file) return

      try {
        let raw = ''

        if (file.name.toLowerCase().endsWith('.zip')) {
          const zip = await JSZip.loadAsync(file)
          const jsonFiles = Object.values(zip.files).filter(
            f => f.name.toLowerCase().endsWith('.json') && !f.dir
          )
          const packFile = jsonFiles.find(f => /AmmoGen[\\/]ammo[\\/].+\.json$/i.test(f.name)) || jsonFiles[0]
          if (!packFile) {
            alert('No JSON file found in the selected ZIP.')
            return
          }
          raw = await packFile.async('text')
        } else {
          raw = await file.text()
        }

        const merged = parsePackJson(raw)
        setPack(merged)
        setActiveIndex(0)
        setErrors([])
        setActiveTab('identity')
      } catch (err) {
        alert('Failed to import pack. Check the file format.')
        console.error(err)
      }
    }
    input.click()
  }

  const activeAmmo = pack.ammo[activeIndex]
  const activeGrenade = pack.grenades[activeIndex]

  const tabs: { id: Tab; label: string; icon: React.ReactNode }[] = [
    { id: 'identity', label: 'Identity', icon: <Shield size={16} /> },
    { id: 'stats', label: 'Stats', icon: mode === 'ammo' ? <Crosshair size={16} /> : <Bomb size={16} /> },
    { id: 'economy', label: 'Economy', icon: <Star size={16} /> },
    { id: 'trader', label: 'Trader', icon: <Package size={16} /> },
    { id: 'crafting', label: 'Crafting', icon: <Wrench size={16} /> },
    ...(mode === 'ammo' ? [
      { id: 'filters' as Tab, label: 'Filters', icon: <Filter size={16} /> },
      { id: 'ammobox' as Tab, label: 'Ammo Box', icon: <Box size={16} /> },
    ] : []),
    { id: 'loot', label: 'Loot', icon: <MapPin size={16} /> },
    { id: 'preview', label: 'JSON Preview', icon: <FileJson size={16} /> },
  ]

  return (
    <div className="min-h-screen flex flex-col bg-tarkov-bg text-tarkov-text">
      {/* Sidebar */}
      <Sidebar open={sidebarOpen} onClose={() => setSidebarOpen(false)} />

      {/* Header */}
      <header className="bg-tarkov-surface border-b border-tarkov-border px-6 py-4 flex items-center justify-between">
        <div className="flex items-center gap-3">
          <button
            onClick={() => setSidebarOpen(true)}
            className="p-2 -ml-2 rounded-lg hover:bg-tarkov-border/50 text-tarkov-text-dim hover:text-tarkov-text transition-colors"
            aria-label="Open menu"
          >
            <Menu size={24} />
          </button>
          <Target className="text-tarkov-accent" size={28} />
          <div>
            <h1 className="text-xl font-bold text-tarkov-accent">AmmoGen Tool</h1>
            <p className="text-xs text-tarkov-text-dim">SPTarkov 4.0.13 Ammo & Grenade Pack Editor</p>
          </div>
          <div className="hidden sm:flex items-center bg-tarkov-bg rounded-lg border border-tarkov-border p-1 ml-2">
            <button
              onClick={() => { setMode('ammo'); setActiveIndex(0); setActiveTab('identity') }}
              className={`flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium rounded transition-colors ${
                mode === 'ammo' ? 'bg-tarkov-accent text-white' : 'text-tarkov-text-dim hover:text-tarkov-text'
              }`}
            >
              <Crosshair size={14} /> Ammo
            </button>
            <button
              onClick={() => { setMode('grenade'); setActiveIndex(0); setActiveTab('identity') }}
              className={`flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium rounded transition-colors ${
                mode === 'grenade' ? 'bg-tarkov-accent text-white' : 'text-tarkov-text-dim hover:text-tarkov-text'
              }`}
            >
              <Bomb size={14} /> Grenades
            </button>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={() => { setPack(createDefaultPack()); setActiveIndex(0); setErrors([]); setActiveTab('identity') }}
            className="btn-secondary text-sm flex items-center gap-1.5"
          >
            <Plus size={14} /> New Pack
          </button>
          <button onClick={importPack} className="btn-secondary text-sm flex items-center gap-1.5">
            <Upload size={14} /> Import
          </button>
          <button onClick={downloadJson} className="btn-secondary text-sm flex items-center gap-1.5">
            <Download size={14} /> Export JSON
          </button>
          <button onClick={exportModZip} className="btn-primary text-sm flex items-center gap-1.5">
            <Download size={14} /> Export
          </button>
        </div>
      </header>

      {/* Success toast */}
      {showExportSuccess && (
        <div className="fixed top-4 right-4 z-50 bg-tarkov-success/20 border border-tarkov-success/50 text-tarkov-success px-4 py-3 rounded-lg flex items-center gap-2 shadow-lg">
          <CheckCircle size={18} /> Ammo pack exported!
        </div>
      )}

      {/* Errors banner */}
      {errors.length > 0 && (
        <div className="bg-tarkov-error/10 border-b border-tarkov-error/30 px-6 py-3">
          <div className="flex items-center gap-2 text-tarkov-error font-medium mb-1">
            <AlertCircle size={16} /> {errors.length} validation error(s) found
          </div>
          <ul className="text-sm text-tarkov-error/80 list-disc list-inside max-h-32 overflow-y-auto">
            {errors.map((e, i) => <li key={i}>{e.field}: {e.message}</li>)}
          </ul>
        </div>
      )}

      {/* Tabs */}
      <nav className="bg-tarkov-surface border-b border-tarkov-border px-6 flex gap-1">
        {tabs.map(tab => {
          const activeItem = mode === 'ammo' ? activeAmmo : activeGrenade
          return (
            <button
              key={tab.id}
              onClick={() => activeItem && setActiveTab(tab.id)}
              disabled={!activeItem}
              className={`flex items-center gap-1.5 px-4 py-3 text-sm font-medium border-b-2 transition-colors ${
                activeTab === tab.id
                  ? 'border-tarkov-accent text-tarkov-accent'
                  : 'border-transparent text-tarkov-text-dim hover:text-tarkov-text'
              } ${!activeItem ? 'opacity-50 cursor-not-allowed' : ''}`}
            >
              {tab.icon} {tab.label}
            </button>
          )
        })}
      </nav>

      {/* Item selector */}
      <div className="bg-tarkov-surface border-b border-tarkov-border px-6 py-3">
        <div className="max-w-5xl mx-auto flex flex-col sm:flex-row sm:items-center gap-3">
          <div className="text-sm text-tarkov-text-dim">{mode === 'ammo' ? 'Ammo' : 'Grenades'}</div>
          <div className="flex flex-wrap gap-2 flex-1">
            {mode === 'ammo'
              ? pack.ammo.map((ammo, i) => (
                <button
                  key={i}
                  onClick={() => setActiveIndex(i)}
                  className={`flex items-center gap-1.5 text-sm px-3 py-1.5 rounded border transition-colors ${
                    activeIndex === i
                      ? 'bg-tarkov-accent/20 border-tarkov-accent text-tarkov-accent'
                      : 'bg-tarkov-bg border-tarkov-border text-tarkov-text-dim hover:text-tarkov-text'
                  }`}
                >
                  <Crosshair size={14} />
                  {ammo.shortName || `Ammo ${i + 1}`}
                  {pack.ammo.length > 1 && (
                    <span
                      className="ml-1 text-tarkov-error hover:text-tarkov-error/80"
                      onClick={(e) => {
                        e.stopPropagation()
                        removeAmmo(i)
                      }}
                    >
                      <Trash2 size={14} />
                    </span>
                  )}
                </button>
              ))
              : pack.grenades.map((grenade, i) => (
                <button
                  key={i}
                  onClick={() => setActiveIndex(i)}
                  className={`flex items-center gap-1.5 text-sm px-3 py-1.5 rounded border transition-colors ${
                    activeIndex === i
                      ? 'bg-tarkov-accent/20 border-tarkov-accent text-tarkov-accent'
                      : 'bg-tarkov-bg border-tarkov-border text-tarkov-text-dim hover:text-tarkov-text'
                  }`}
                >
                  <Bomb size={14} />
                  {grenade.shortName || `Grenade ${i + 1}`}
                  {pack.grenades.length > 1 && (
                    <span
                      className="ml-1 text-tarkov-error hover:text-tarkov-error/80"
                      onClick={(e) => {
                        e.stopPropagation()
                        removeGrenade(i)
                      }}
                    >
                      <Trash2 size={14} />
                    </span>
                  )}
                </button>
              ))}
          </div>
          <div className="flex items-center gap-3">
            <button onClick={mode === 'ammo' ? addAmmo : addGrenade} className="btn-secondary text-sm flex items-center gap-1.5">
              <Plus size={14} /> {mode === 'ammo' ? 'Add Ammo' : 'Add Grenade'}
            </button>
            <a
              href="https://db.sp-tarkov.com/"
              target="_blank"
              rel="noopener noreferrer"
              className="text-xs text-tarkov-text-dim hover:text-tarkov-accent flex items-center gap-1"
            >
              <ExternalLink size={12} />
              Item DB
            </a>
          </div>
        </div>
      </div>

      {/* Content */}
      <main className="flex-1 p-6 max-w-5xl mx-auto w-full">
        {mode === 'ammo' ? (
          !activeAmmo ? (
            <div className="card text-center text-tarkov-text-dim py-12">
              <Crosshair size={48} className="mx-auto mb-4 text-tarkov-accent/50" />
              <p className="text-lg">No ammo defined yet.</p>
              <p className="text-sm mt-1">Click <span className="text-tarkov-accent">Add Ammo</span> to start building your pack.</p>
            </div>
          ) : (
            <>
              {activeTab === 'identity' && <IdentityTab pack={pack} setPack={setPack} ammo={activeAmmo} onChange={u => updateAmmo(activeIndex, u)} />}
              {activeTab === 'stats' && <StatsTab ammo={activeAmmo} onChange={u => updateAmmo(activeIndex, u)} />}
              {activeTab === 'economy' && <EconomyTab economy={activeAmmo.economy} onChange={u => updateAmmo(activeIndex, { economy: { ...activeAmmo.economy, ...u } })} />}
              {activeTab === 'trader' && <TraderTab traders={activeAmmo.traders} onChange={u => updateAmmo(activeIndex, u)} />}
              {activeTab === 'crafting' && <CraftingTab crafting={activeAmmo.crafting} onChange={u => updateAmmo(activeIndex, u)} />}
              {activeTab === 'filters' && <FiltersTab ammo={activeAmmo} onChange={u => updateAmmo(activeIndex, u)} />}
              {activeTab === 'ammobox' && <AmmoBoxTab ammo={activeAmmo} onChange={u => updateAmmo(activeIndex, u)} />}
              {activeTab === 'loot' && <LootTab ammo={activeAmmo} onChange={u => updateAmmo(activeIndex, u)} />}
              {activeTab === 'preview' && <PreviewTab pack={pack} activeAmmo={activeAmmo} />}
            </>
          )
        ) : (
          !activeGrenade ? (
            <div className="card text-center text-tarkov-text-dim py-12">
              <Bomb size={48} className="mx-auto mb-4 text-tarkov-accent/50" />
              <p className="text-lg">No grenades defined yet.</p>
              <p className="text-sm mt-1">Click <span className="text-tarkov-accent">Add Grenade</span> to start building your pack.</p>
            </div>
          ) : (
            <>
              {activeTab === 'identity' && <GrenadeIdentityTab key={activeGrenade.id} pack={pack} setPack={setPack} grenade={activeGrenade} onChange={u => updateGrenade(activeIndex, u)} />}
              {activeTab === 'stats' && <GrenadeStatsTab key={activeGrenade.id} grenade={activeGrenade} onChange={u => updateGrenade(activeIndex, u)} />}
              {activeTab === 'economy' && <EconomyTab key={activeGrenade.id} economy={activeGrenade.economy} onChange={u => updateGrenade(activeIndex, { economy: { ...activeGrenade.economy, ...u } })} />}
              {activeTab === 'trader' && <TraderTab key={activeGrenade.id} traders={activeGrenade.traders} onChange={u => updateGrenade(activeIndex, u)} />}
              {activeTab === 'crafting' && <CraftingTab key={activeGrenade.id} crafting={activeGrenade.crafting} onChange={u => updateGrenade(activeIndex, u)} />}
              {activeTab === 'loot' && <GrenadeLootTab key={activeGrenade.id} grenade={activeGrenade} onChange={u => updateGrenade(activeIndex, u)} />}
              {activeTab === 'preview' && <PreviewTab pack={pack} activeAmmo={activeAmmo} />}
            </>
          )
        )}
      </main>
    </div>
  )
}

function Field({ label, children, className = '', tooltip }: { label: string; children: React.ReactNode; className?: string; tooltip?: React.ReactNode }) {
  return (
    <div className={className}>
      <label className="label flex items-center gap-1.5">
        {label}
        {tooltip && (
          <span className="relative group">
            <HelpCircle size={13} className="text-tarkov-text-dim hover:text-tarkov-accent cursor-help transition-colors" />
            <span className="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 px-3 py-2 bg-tarkov-bg border border-tarkov-border rounded-lg text-xs text-tarkov-text font-normal w-64 opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-150 z-50 shadow-xl leading-relaxed pointer-events-none">
              {tooltip}
            </span>
          </span>
        )}
      </label>
      {children}
    </div>
  )
}

function Section({ title, icon, children }: { title: string; icon: React.ReactNode; children: React.ReactNode }) {
  return (
    <section className="card">
      <h2 className="text-lg font-semibold text-tarkov-accent mb-4 flex items-center gap-2">
        {icon} {title}
      </h2>
      {children}
    </section>
  )
}

function CollapsibleSection({ title, icon, defaultOpen = true, children }: { title: string; icon: React.ReactNode; defaultOpen?: boolean; children: React.ReactNode }) {
  const [open, setOpen] = useState(defaultOpen)
  const [overflowVisible, setOverflowVisible] = useState(defaultOpen)
  useEffect(() => {
    if (open) {
      const timer = setTimeout(() => setOverflowVisible(true), 200)
      return () => clearTimeout(timer)
    } else {
      setOverflowVisible(false)
    }
  }, [open])
  return (
    <div className="border-b border-tarkov-border/50 py-4 last:border-b-0 last:pb-0">
      <button
        type="button"
        className="w-full flex items-center justify-between text-sm font-semibold text-tarkov-accent hover:text-tarkov-accent/80 transition-colors"
        onClick={() => setOpen(v => !v)}
      >
        <span className="flex items-center gap-2">{icon} {title}</span>
        <ChevronDown size={18} className={`transition-transform duration-200 ${open ? 'rotate-180' : ''}`} />
      </button>
      <div className={`transition-all duration-200 ${open ? 'max-h-[2000px] opacity-100 mt-3' : 'max-h-0 opacity-0'} ${overflowVisible ? 'overflow-visible' : 'overflow-hidden'}`}>
        {children}
      </div>
    </div>
  )
}

function Toggle({ checked, onChange, label }: { checked: boolean; onChange: (v: boolean) => void; label?: string }) {
  return (
    <label className="toggle flex items-center gap-2 cursor-pointer">
      <input type="checkbox" checked={checked} onChange={e => onChange(e.target.checked)} />
      <span className="toggle-track"></span>
      <span className="toggle-thumb"></span>
      {label && <span className="text-sm text-tarkov-text">{label}</span>}
    </label>
  )
}

function IdentityTab({ pack, setPack, ammo, onChange }: {
  pack: AmmoPackDefinition
  setPack: Dispatch<SetStateAction<AmmoPackDefinition>>
  ammo: AmmoDefinition
  onChange: (u: Partial<AmmoDefinition>) => void
}) {
  return (
    <div className="space-y-6">
      <Section title="Pack Identity" icon={<Shield size={18} />}>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Field label="Pack Name">
            <input
              className="input-field"
              value={pack.name}
              onChange={e => setPack({ ...pack, name: e.target.value })}
              placeholder="My Ammo Pack"
            />
          </Field>
          <Field label="Enabled">
            <div className="mt-2">
              <Toggle checked={pack.enabled} onChange={v => setPack({ ...pack, enabled: v })} label="Pack enabled" />
            </div>
          </Field>
        </div>
      </Section>

      <Section title="Ammo Identity" icon={<Crosshair size={18} />}>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Field label="Ammo ID">
            <div className="flex gap-2">
              <input
                className="input-field flex-1 font-mono text-sm"
                value={ammo.id}
                onChange={e => onChange({ id: e.target.value })}
                placeholder="24-char hex string"
                maxLength={24}
              />
              <button onClick={() => onChange({ id: generateMongoId() })} className="btn-secondary text-xs px-2" title="Generate random ID">
                <RefreshCw size={14} />
              </button>
            </div>
          </Field>

          <Field label="Enabled">
            <div className="mt-2">
              <Toggle checked={ammo.enabled} onChange={v => onChange({ enabled: v })} label="Ammo enabled" />
            </div>
          </Field>

          <Field
            label="Base Ammo Template"
            className="md:col-span-2"
            tooltip="Existing ammo item to clone. Selecting one auto-fills the Stats tab with the base ammo's values. Choose 'Other' to enter a custom template ID from a mod."
          >
            <select
              className="input-field"
              value={AMMO_TEMPLATES.some(t => t.id === ammo.baseTpl) ? ammo.baseTpl : ammo.baseTpl ? '__other__' : ''}
              onChange={e => {
                const value = e.target.value
                if (value === '__other__') {
                  onChange({ baseTpl: '__other__', compareToAmmoId: '' })
                } else if (value) {
                  const base = getAmmoStats(value)
                  if (base) {
                    const { name, shortName, ...baseStats } = base
                    onChange({
                      baseTpl: value,
                      compareToAmmoId: '',
                      stats: baseStats as AmmoStats,
                    })
                  } else {
                    onChange({ baseTpl: value, compareToAmmoId: '' })
                  }
                } else {
                  onChange({ baseTpl: '', compareToAmmoId: '' })
                }
              }}
            >
              <option value="">Select a base ammo...</option>
              {AMMO_TEMPLATES.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.name} ({t.caliber}) — {t.id}{t.requiresMod ? ` [Requires: ${t.requiresMod}]` : ''}
                </option>
              ))}
              <option value="__other__">Other (custom ID)...</option>
            </select>
            {(ammo.baseTpl === '__other__' || (!!ammo.baseTpl && !AMMO_TEMPLATES.some(t => t.id === ammo.baseTpl))) && (
              <input
                className="input-field mt-2 font-mono text-sm"
                value={ammo.baseTpl === '__other__' ? '' : ammo.baseTpl}
                onChange={e => onChange({ baseTpl: e.target.value })}
                placeholder="Enter custom ammo template ID"
              />
            )}
            {(() => {
              const selected = AMMO_TEMPLATES.find(t => t.id === ammo.baseTpl)
              if (!selected?.requiresMod) return null
              return (
                <div className="mt-2 flex items-start gap-2 text-xs text-tarkov-warning bg-tarkov-warning/10 border border-tarkov-warning/30 rounded px-3 py-2">
                  <AlertCircle size={14} className="shrink-0 mt-0.5" />
                  <span>
                    This base ammo requires{' '}
                    <a href={selected.requiresModUrl} target="_blank" rel="noopener noreferrer" className="underline hover:text-tarkov-accent">
                      {selected.requiresMod}
                    </a>
                    {' '}to be installed.
                  </span>
                </div>
              )
            })()}
          </Field>

          <Field label="Name">
            <input
              className="input-field"
              value={ammo.name}
              onChange={e => onChange({ name: e.target.value })}
              placeholder="e.g. Custom 5.45 BP"
            />
          </Field>

          <Field label="Short Name">
            <input
              className="input-field"
              value={ammo.shortName}
              onChange={e => onChange({ shortName: e.target.value })}
              placeholder="e.g. cBP"
            />
          </Field>

          <Field label="Description" className="md:col-span-2">
            <textarea
              className="input-field min-h-[80px] resize-y"
              rows={3}
              value={ammo.description}
              onChange={e => onChange({ description: e.target.value })}
              placeholder="A short description of the ammo..."
            />
          </Field>

          <Field
            label="Handbook Parent ID (optional)"
            tooltip="Handbook category ID for trader/flea sorting. Leave blank and the server will look up the base ammo's category automatically."
          >
            <input
              className="input-field font-mono text-sm"
              value={ammo.handbookParentId || ''}
              onChange={e => onChange({ handbookParentId: e.target.value || undefined })}
              placeholder="Leave blank to auto-resolve from base template"
            />
          </Field>
        </div>
      </Section>
    </div>
  )
}

function StatsTab({ ammo, onChange }: { ammo: AmmoDefinition; onChange: (u: Partial<AmmoDefinition>) => void }) {
  const statNames: Record<string, string> = {
    damage: 'Damage',
    penetration: 'Penetration Power',
    armorDamage: 'Armor Damage',
    initialSpeed: 'Initial Speed (m/s)',
    ammoAccr: 'Accuracy Modifier',
    ammoRec: 'Recoil Modifier',
    stackMaxSize: 'Stack Max Size',
    lightBleedingDelta: 'Light Bleed Chance',
    heavyBleedingDelta: 'Heavy Bleed Chance',
    durabilityBurnModificator: 'Durability Burn',
    ballisticCoeficient: 'Ballistic Coefficient',
    projectileCount: 'Projectile Count',
    ricochetChance: 'Ricochet Chance',
    fragmentationChance: 'Fragmentation Chance',
    penetrationDamageMod: 'Penetration Damage Mod',
    penetrationChanceObstacle: 'Penetration Chance (Obstacle)',
    ammoLifeTimeSec: 'Ammo Life Time (sec)',
    bulletMassGram: 'Bullet Mass (g)',
    bulletDiameterMilimeters: 'Bullet Diameter (mm)',
    misfireChance: 'Misfire Chance',
    malfMisfireChance: 'Malf. Misfire Chance',
    malfFeedChance: 'Malf. Feed Chance',
    heatFactor: 'Heat Factor',
    staminaBurnPerDamage: 'Stamina Burn / Damage',
    tracerDistance: 'Tracer Distance',
    fuzeArmTimeSec: 'Fuze Arm Time (sec)',
    minExplosionDistance: 'Min Explosion Distance',
    maxExplosionDistance: 'Max Explosion Distance',
    fragmentsCount: 'Fragments Count',
    explosionStrength: 'Explosion Strength',
    lightAndSoundShotAngle: 'Flash/Sound Angle',
    lightAndSoundShotSelfContusionTime: 'Self Contusion Time',
    lightAndSoundShotSelfContusionStrength: 'Self Contusion Strength',
  }
  const statTooltips: Record<string, string> = {
    damage: 'Hit damage dealt to unarmored body parts.',
    penetration: 'Armor penetration capability. Higher values pierce higher armor classes.',
    armorDamage: 'Durability damage dealt to armor when hit.',
    initialSpeed: 'Muzzle velocity in meters per second. Affects drop and damage.',
    ammoAccr: 'Accuracy modifier. Positive improves accuracy; negative reduces it.',
    ammoRec: 'Recoil modifier. Positive increases recoil; negative reduces it.',
    stackMaxSize: 'Maximum rounds per inventory slot. 0 inherits the base ammo template default.',
    lightBleedingDelta: 'Chance to cause light bleeding on hit (0-1). 0 leaves the base ammo value unchanged.',
    heavyBleedingDelta: 'Chance to cause heavy bleeding on hit (0-1). 0 leaves the base ammo value unchanged.',
    durabilityBurnModificator: 'Multiplier for weapon durability burn per shot. 1 is the base ammo value; 0 disables durability burn.',
    ballisticCoeficient: 'Ballistic coefficient (G1). Lower values drop faster; higher values retain velocity better.',
    projectileCount: 'Number of projectiles fired per shot (1 for normal, higher for buckshot).',
    ricochetChance: 'Probability of ricocheting off hard surfaces.',
    fragmentationChance: 'Probability of fragmenting on impact.',
    penetrationDamageMod: 'Damage retained after penetrating armor.',
    penetrationChanceObstacle: 'Chance to penetrate thin obstacles and barriers.',
    ammoLifeTimeSec: 'How long the projectile stays in the world before being removed.',
    bulletMassGram: 'Projectile mass in grams.',
    bulletDiameterMilimeters: 'Projectile diameter in millimeters.',
    misfireChance: 'Chance of a single misfire on fire.',
    malfMisfireChance: 'Chance contribution to weapon misfire malfunction.',
    malfFeedChance: 'Chance contribution to weapon feed malfunction.',
    heatFactor: 'Multiplier for barrel heat generated per shot.',
    staminaBurnPerDamage: 'Stamina drained per point of damage dealt.',
    tracerDistance: 'Distance over which the tracer effect is visible.',
    fuzeArmTimeSec: 'Time before an explosive round arms after firing.',
    minExplosionDistance: 'Minimum distance at which the explosion deals damage.',
    maxExplosionDistance: 'Maximum radius of the explosion.',
    fragmentsCount: 'Number of fragments released on explosion.',
    explosionStrength: 'Raw power of the explosion.',
    lightAndSoundShotAngle: 'Cone angle for flash/sound effect.',
    lightAndSoundShotSelfContusionTime: 'Duration of self-contusion from firing a flash/sound round.',
    lightAndSoundShotSelfContusionStrength: 'Strength of self-contusion from firing a flash/sound round.',
  }
  const compareToId = ammo.compareToAmmoId === '__other__' ? ammo.baseTpl : (ammo.compareToAmmoId || ammo.baseTpl)
  const base = getAmmoStats(compareToId)

  const coreStats = ['damage', 'penetration', 'armorDamage', 'initialSpeed', 'ammoAccr', 'ammoRec', 'stackMaxSize', 'lightBleedingDelta', 'heavyBleedingDelta', 'durabilityBurnModificator', 'ballisticCoeficient']
  const projectileStats = ['projectileCount', 'ricochetChance', 'fragmentationChance', 'penetrationDamageMod', 'penetrationChanceObstacle', 'ammoLifeTimeSec', 'bulletMassGram', 'bulletDiameterMilimeters']
  const malfunctionStats = ['misfireChance', 'malfMisfireChance', 'malfFeedChance', 'heatFactor', 'staminaBurnPerDamage']
  const explosiveStats = ['fuzeArmTimeSec', 'minExplosionDistance', 'maxExplosionDistance', 'fragmentsCount', 'explosionStrength']
  const lightSoundStats = ['lightAndSoundShotAngle', 'lightAndSoundShotSelfContusionTime', 'lightAndSoundShotSelfContusionStrength']
  const allNumericStats = [...coreStats, ...projectileStats, ...malfunctionStats, 'tracerDistance', ...explosiveStats, ...lightSoundStats]

  const stepForStat = (stat: string): string | number => {
    if (stat === 'lightBleedingDelta' || stat === 'heavyBleedingDelta' || stat === 'ricochetChance' || stat === 'fragmentationChance' || stat === 'penetrationChanceObstacle' || stat === 'misfireChance' || stat === 'malfMisfireChance' || stat === 'malfFeedChance' || stat === 'staminaBurnPerDamage' || stat === 'heatFactor') return 0.01
    if (stat === 'ballisticCoeficient' || stat === 'penetrationDamageMod' || stat === 'bulletMassGram' || stat === 'bulletDiameterMilimeters') return 0.001
    if (stat === 'fuzeArmTimeSec' || stat === 'minExplosionDistance' || stat === 'maxExplosionDistance' || stat === 'tracerDistance' || stat === 'ammoLifeTimeSec' || stat === 'lightAndSoundShotSelfContusionTime' || stat === 'explosionStrength') return 0.1
    return 1
  }

  const updateStat = (stat: string, value: number) => onChange({ stats: { ...ammo.stats, [stat]: value } })

  const renderNumberField = (stat: string) => (
    <Field key={stat} label={statNames[stat]} tooltip={statTooltips[stat]}>
      <input
        className="input-field"
        type="number"
        step={stepForStat(stat)}
        value={ammo.stats[stat as keyof AmmoStats] as number}
        onChange={e => updateStat(stat, parseFloat(e.target.value) || 0)}
      />
    </Field>
  )

  const updateVector3 = (key: 'armorDistanceDistanceDamage' | 'contusion' | 'blindness', axis: 'x' | 'y' | 'z', value: number) => {
    onChange({
      stats: {
        ...ammo.stats,
        [key]: { ...ammo.stats[key], [axis]: value },
      },
    })
  }

  return (
    <Section title="Ammo Stats" icon={<Crosshair size={18} />}>
      <CollapsibleSection title="Core Stats" icon={<Crosshair size={16} />}>
        <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
          {coreStats.map(renderNumberField)}
        </div>
      </CollapsibleSection>

      <CollapsibleSection title="Projectile & Flight" icon={<Crosshair size={16} />} defaultOpen={false}>
        <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
          {projectileStats.map(renderNumberField)}
        </div>
      </CollapsibleSection>

      <CollapsibleSection title="Malfunctions & Durability" icon={<Crosshair size={16} />} defaultOpen={false}>
        <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
          {malfunctionStats.map(renderNumberField)}
        </div>
      </CollapsibleSection>

      <CollapsibleSection title="Tracer" icon={<Crosshair size={16} />} defaultOpen={false}>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <Field label="Tracer" tooltip="Whether the round leaves a tracer trail.">
            <select
              className="input-field"
              value={ammo.stats.tracer ? 'true' : 'false'}
              onChange={e => onChange({ stats: { ...ammo.stats, tracer: e.target.value === 'true' } })}
            >
              <option value="false">No</option>
              <option value="true">Yes</option>
            </select>
          </Field>
          <Field
            label="Tracer Color"
            tooltip={
              <span>
                Preset color keys used by base ammo, or a custom hex color. Custom colors require the{' '}
                <a
                  href="https://forge.sp-tarkov.com/mod/1090/color-converter-api"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-tarkov-accent underline hover:text-tarkov-accent/80"
                  onClick={e => e.stopPropagation()}
                >
                  ColorConverterAPI
                </a>{' '}
                mod.
              </span>
            }
          >
            <select
              className="input-field"
              value={TRACER_COLOR_OPTIONS.includes(ammo.stats.tracerColor) ? ammo.stats.tracerColor : ammo.stats.tracerColor ? '__CUSTOM__' : ''}
              onChange={e => {
                const value = e.target.value
                if (value === '__CUSTOM__') {
                  onChange({ stats: { ...ammo.stats, tracerColor: '#FF0000' } })
                } else {
                  onChange({ stats: { ...ammo.stats, tracerColor: value } })
                }
              }}
            >
              <option value="">None / Default</option>
              {TRACER_COLOR_OPTIONS.map(c => (
                <option key={c} value={c}>{c}</option>
              ))}
              <option value="__CUSTOM__">Custom (requires ColorConverterAPI mod)</option>
            </select>
          </Field>
          {ammo.stats.tracerColor && !TRACER_COLOR_OPTIONS.includes(ammo.stats.tracerColor) && (
            <Field
              label="Custom Tracer Color"
              tooltip={
                <span>
                  Hex color (#RRGGBB). Custom colors require the{' '}
                  <a
                    href="https://forge.sp-tarkov.com/mod/1090/color-converter-api"
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-tarkov-accent underline hover:text-tarkov-accent/80"
                    onClick={e => e.stopPropagation()}
                  >
                    ColorConverterAPI
                  </a>{' '}
                  mod to render in-game.
                </span>
              }
            >
              <div className="flex items-center gap-3">
                <input
                  type="color"
                  className="w-10 h-10 rounded cursor-pointer bg-transparent border-0 p-0"
                  value={ammo.stats.tracerColor}
                  onChange={e => onChange({ stats: { ...ammo.stats, tracerColor: e.target.value.toUpperCase() } })}
                />
                <span className="text-xs font-mono text-tarkov-text-dim">{ammo.stats.tracerColor}</span>
              </div>
            </Field>
          )}
          {renderNumberField('tracerDistance')}
        </div>
      </CollapsibleSection>

      <CollapsibleSection title="Audio / Visual" icon={<Crosshair size={16} />} defaultOpen={false}>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Field label="Ammo SFX" tooltip="Sound effect key for the projectile / impact audio. Pick from the values used by base ammo templates.">
            <select
              className="input-field font-mono text-sm"
              value={ammo.stats.ammoSfx}
              onChange={e => onChange({ stats: { ...ammo.stats, ammoSfx: e.target.value } })}
            >
              <option value="">None / Default</option>
              {AMMO_SFX_OPTIONS.map(s => (
                <option key={s} value={s}>{s}</option>
              ))}
            </select>
          </Field>
          <Field label="Casing Sounds" tooltip="Audio category key for shell casing sounds. Pick from the values used by base ammo templates.">
            <select
              className="input-field font-mono text-sm"
              value={ammo.stats.casingSounds}
              onChange={e => onChange({ stats: { ...ammo.stats, casingSounds: e.target.value } })}
            >
              <option value="">None / Default</option>
              {CASING_SOUNDS_OPTIONS.map(s => (
                <option key={s} value={s}>{s}</option>
              ))}
            </select>
          </Field>
        </div>
      </CollapsibleSection>

      <CollapsibleSection title="Explosive / Grenade" icon={<Crosshair size={16} />} defaultOpen={false}>
        <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
          {explosiveStats.map(renderNumberField)}
        </div>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mt-4">
          <Field label="Fragment Type" tooltip="Fragment type identifier used by the explosion.">
            <input
              className="input-field font-mono text-sm"
              value={ammo.stats.fragmentType}
              onChange={e => onChange({ stats: { ...ammo.stats, fragmentType: e.target.value } })}
            />
          </Field>
          <Field label="Explosion Type" tooltip="Explosion type identifier (e.g. grenade, flamable, etc.).">
            <input
              className="input-field font-mono text-sm"
              value={ammo.stats.explosionType}
              onChange={e => onChange({ stats: { ...ammo.stats, explosionType: e.target.value } })}
            />
          </Field>
          <Field label="Show Hit Effect On Explode" tooltip="Whether to show a hit effect when the explosive detonates.">
            <select
              className="input-field"
              value={ammo.stats.showHitEffectOnExplode ? 'true' : 'false'}
              onChange={e => onChange({ stats: { ...ammo.stats, showHitEffectOnExplode: e.target.value === 'true' } })}
            >
              <option value="false">No</option>
              <option value="true">Yes</option>
            </select>
          </Field>
        </div>
      </CollapsibleSection>

      <CollapsibleSection title="Light & Sound (Flash / CS)" icon={<Crosshair size={16} />} defaultOpen={false}>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Field label="Light & Sound Shot" tooltip="Whether this round acts as a flashbang / sound suppression round.">
            <select
              className="input-field"
              value={ammo.stats.isLightAndSoundShot ? 'true' : 'false'}
              onChange={e => onChange({ stats: { ...ammo.stats, isLightAndSoundShot: e.target.value === 'true' } })}
            >
              <option value="false">No</option>
              <option value="true">Yes</option>
            </select>
          </Field>
        </div>
        <div className="grid grid-cols-2 md:grid-cols-3 gap-4 mt-4">
          {lightSoundStats.map(renderNumberField)}
        </div>
      </CollapsibleSection>

      <CollapsibleSection title="Area Effect Vectors" icon={<Crosshair size={16} />} defaultOpen={false}>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {[
            { key: 'armorDistanceDistanceDamage', label: 'Armor Distance Damage', tooltip: 'Damage falloff over distance against armor.' },
            { key: 'contusion', label: 'Contusion', tooltip: 'Contusion effect intensity vector.' },
            { key: 'blindness', label: 'Blindness', tooltip: 'Blindness effect intensity vector.' },
          ].map(({ key, label, tooltip }) => (
            <Field key={key} label={label} tooltip={tooltip}>
              <div className="grid grid-cols-3 gap-2">
                {(['x', 'y', 'z'] as const).map(axis => (
                  <input
                    key={axis}
                    className="input-field font-mono text-sm"
                    type="number"
                    step={0.01}
                    value={(ammo.stats[key as keyof AmmoStats] as Vector3)[axis]}
                    onChange={e => updateVector3(key as 'armorDistanceDistanceDamage' | 'contusion' | 'blindness', axis, parseFloat(e.target.value) || 0)}
                    placeholder={axis.toUpperCase()}
                  />
                ))}
              </div>
            </Field>
          ))}
        </div>
      </CollapsibleSection>

      {ammo.baseTpl && (
      <CollapsibleSection title="Base Ammo Comparison" icon={<Crosshair size={16} />} defaultOpen={false}>
        <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-3 mb-4">
          <div className="flex flex-col md:flex-row md:items-center gap-2">
            <span className="text-xs text-tarkov-text-dim">Compare to:</span>
            <select
              className="input-field text-sm py-1"
              value={(() => {
                if (!ammo.compareToAmmoId) return ammo.baseTpl
                if (AMMO_TEMPLATES.some(t => t.id === ammo.compareToAmmoId)) return ammo.compareToAmmoId
                return '__other__'
              })()}
              onChange={e => {
                const value = e.target.value
                if (value === '__other__') {
                  onChange({ compareToAmmoId: '__other__' })
                } else if (value === ammo.baseTpl) {
                  onChange({ compareToAmmoId: '' })
                } else {
                  onChange({ compareToAmmoId: value })
                }
              }}
            >
              <option value={ammo.baseTpl}>Original clone ({getAmmoStats(ammo.baseTpl)?.name || ammo.baseTpl})</option>
              {AMMO_TEMPLATES.filter(t => t.id !== ammo.baseTpl).map((t) => (
                <option key={t.id} value={t.id}>
                  {t.name} ({t.caliber})
                </option>
              ))}
              <option value="__other__">Other (custom ID)...</option>
            </select>
            {(ammo.compareToAmmoId === '__other__' || (!!ammo.compareToAmmoId && ammo.compareToAmmoId !== ammo.baseTpl && !AMMO_TEMPLATES.some(t => t.id === ammo.compareToAmmoId))) && (
              <input
                className="input-field text-sm py-1 font-mono"
                value={ammo.compareToAmmoId === '__other__' ? '' : ammo.compareToAmmoId}
                onChange={e => onChange({ compareToAmmoId: e.target.value })}
                placeholder="Enter custom ammo ID to compare"
              />
            )}
          </div>
        </div>
        {base ? (
          <div className="grid grid-cols-2 md:grid-cols-4 gap-3 text-sm">
            {allNumericStats.map((stat) => {
              const custom = ammo.stats[stat as keyof AmmoStats] as number
              const original = base[stat as keyof AmmoTemplateStats] as number
              const diff = custom - original
              const diffClass = diff > 0 ? 'text-tarkov-success' : diff < 0 ? 'text-tarkov-error' : 'text-tarkov-text-dim'
              const diffText = diff === 0 ? '=' : diff > 0 ? `+${diff}` : `${diff}`
              return (
                <div key={stat} className="flex flex-col bg-tarkov-surface border border-tarkov-border rounded p-2">
                  <span className="text-tarkov-text-dim text-xs">{statNames[stat]}</span>
                  <div className="flex items-baseline gap-2">
                    <span className="font-medium text-tarkov-text">{custom}</span>
                    <span className="text-xs text-tarkov-text-dim">/ {original}</span>
                  </div>
                  <span className={`text-xs font-medium ${diffClass}`}>{diffText}</span>
                </div>
              )
            })}
          </div>
        ) : (
          <div className="text-sm text-tarkov-text-dim">Comparison ammo not found.</div>
        )}
      </CollapsibleSection>
      )}
    </Section>
  )
}

function EconomyTab({ economy, onChange }: { economy: AmmoEconomy; onChange: (u: Partial<AmmoEconomy>) => void }) {
  return (
    <Section title="Economy" icon={<Star size={18} />}>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Field label="Handbook Price (₽)" tooltip="Base price used for handbook display and insurance calculations.">
          <input
            className="input-field"
            type="number"
            value={economy.handbookPriceRoubles}
            onChange={e =>
              onChange({
                handbookPriceRoubles: parseInt(e.target.value, 10) || 0,
              })
            }
          />
        </Field>
        <Field label="Flea Price (₽)" tooltip="Price used for Flea Market listings.">
          <input
            className="input-field"
            type="number"
            value={economy.fleaPriceRoubles}
            onChange={e =>
              onChange({
                fleaPriceRoubles: parseInt(e.target.value, 10) || 0,
              })
            }
          />
        </Field>
        <Field label="Rarity PvE" tooltip="Spawn rarity for PvE containers and loot tables.">
          <select
            className="input-field"
            value={economy.rarityPvE}
            onChange={e =>
              onChange({
                rarityPvE: e.target.value,
              })
            }
          >
            {RARITY_OPTIONS.map((r) => (
              <option key={r} value={r}>{r}</option>
            ))}
          </select>
        </Field>
      </div>
    </Section>
  )
}

function TraderTab({ traders, onChange }: { traders: TraderEntry[]; onChange: (u: { traders: TraderEntry[] }) => void }) {
  const updateTrader = (index: number, updates: Partial<TraderEntry>) => {
    const next = [...traders]
    next[index] = { ...next[index], ...updates }
    onChange({ traders: next })
  }

  const addTrader = () => {
    onChange({ traders: [...traders, createDefaultTraderEntry()] })
  }

  const removeTrader = (index: number) => {
    onChange({ traders: traders.filter((_, i) => i !== index) })
  }

  return (
    <Section title="Vanilla Traders" icon={<Package size={18} />}>
      <div className="space-y-4">
        {traders.map((trader, i) => (
          <div key={i} className="bg-tarkov-bg border border-tarkov-border rounded-lg p-4">
            <div className="flex items-center justify-between mb-4">
              <Toggle
                checked={trader.enabled}
                onChange={v => updateTrader(i, { enabled: v })}
                label={`Trader ${i + 1}`}
              />
              {traders.length > 1 && (
                <button className="btn-danger text-xs flex items-center gap-1" onClick={() => removeTrader(i)}>
                  <Trash2 size={14} /> Remove
                </button>
              )}
            </div>

            {trader.enabled && (
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <Field label="Trader" tooltip="Vanilla trader that sells this item.">
                  <select
                    className="input-field"
                    value={trader.traderId}
                    onChange={e => updateTrader(i, { traderId: e.target.value })}
                  >
                    {VANILLA_TRADERS.map((t) => (
                      <option key={t.id} value={t.id}>{t.name}</option>
                    ))}
                  </select>
                </Field>
                <Field label="Loyalty Level" tooltip="Trader level required for this item to appear.">
                  <input
                    className="input-field"
                    type="number"
                    min={1}
                    max={4}
                    value={trader.loyaltyLevel}
                    onChange={e => updateTrader(i, { loyaltyLevel: parseInt(e.target.value, 10) || 1 })}
                  />
                </Field>
                <Field label="Price (₽)" tooltip="Price per round in roubles.">
                  <input
                    className="input-field"
                    type="number"
                    value={trader.priceRoubles}
                    onChange={e => updateTrader(i, { priceRoubles: parseInt(e.target.value, 10) || 0 })}
                  />
                </Field>
                <Field label="Stock Count" tooltip="Amount available after each trader restock. 0 is out of stock. Use the toggle below for unlimited stock.">
                  <input
                    className="input-field"
                    type="number"
                    disabled={trader.unlimitedStock}
                    value={trader.stockCount}
                    onChange={e => updateTrader(i, { stockCount: parseInt(e.target.value, 10) || 0 })}
                  />
                  <Toggle
                    checked={trader.unlimitedStock}
                    onChange={v => updateTrader(i, { unlimitedStock: v })}
                    label="Unlimited stock"
                  />
                </Field>
                <Field label="Buy Restriction Max" tooltip="Maximum rounds a player can buy per restock cycle. 0 means no purchases allowed. Use the toggle below for no restriction.">
                  <input
                    className="input-field"
                    type="number"
                    disabled={trader.unlimitedBuyRestriction}
                    value={trader.buyRestrictionMax}
                    onChange={e => updateTrader(i, { buyRestrictionMax: parseInt(e.target.value, 10) || 0 })}
                  />
                  <Toggle
                    checked={trader.unlimitedBuyRestriction}
                    onChange={v => updateTrader(i, { unlimitedBuyRestriction: v })}
                    label="Unlimited buy restriction"
                  />
                </Field>
              </div>
            )}
          </div>
        ))}

        <button className="btn-secondary flex items-center gap-1.5" onClick={addTrader}>
          <Plus size={14} /> Add Trader
        </button>
      </div>
    </Section>
  )
}

function CraftingTab({ crafting, onChange }: { crafting: CraftingEntry; onChange: (u: { crafting: CraftingEntry }) => void }) {
  return (
    <Section title="Workbench Crafting" icon={<Wrench size={18} />}>
      <div className="mb-4">
        <Toggle
          checked={crafting.enabled}
          onChange={v => onChange({ crafting: { ...crafting, enabled: v } })}
          label="Add workbench craft"
        />
      </div>

      {crafting.enabled && (
        <>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
            <Field label="Workbench Level" tooltip="Hideout workbench level required to craft this item.">
              <input
                className="input-field"
                type="number"
                min={1}
                max={3}
                value={crafting.workbenchLevel}
                onChange={e =>
                  onChange({
                    crafting: { ...crafting, workbenchLevel: parseInt(e.target.value, 10) || 1 },
                  })
                }
              />
            </Field>
            <Field label="Craft Time (seconds)" tooltip="Time in seconds to complete one craft.">
              <input
                className="input-field"
                type="number"
                value={crafting.craftTimeSeconds}
                onChange={e =>
                  onChange({
                    crafting: { ...crafting, craftTimeSeconds: parseInt(e.target.value, 10) || 0 },
                  })
                }
              />
            </Field>
            <Field label="Output Count" tooltip="Number of items produced per craft completion.">
              <input
                className="input-field"
                type="number"
                value={crafting.outputCount}
                onChange={e =>
                  onChange({
                    crafting: { ...crafting, outputCount: parseInt(e.target.value, 10) || 0 },
                  })
                }
              />
            </Field>
          </div>

          <div>
            <label className="label">Requirements</label>
            {crafting.requirements.map((req, i) => (
              <div key={i} className="flex gap-2 mb-2 items-start">
                <div className="flex-1 flex flex-col gap-1">
                  <SearchableSelect
                    value={req.tpl}
                    onChange={id => {
                      const next = [...crafting.requirements]
                      next[i] = { ...next[i], tpl: id }
                      onChange({ crafting: { ...crafting, requirements: next } })
                    }}
                    placeholder="Search item name..."
                  />
                  {req.tpl && getItemName(req.tpl) && (
                    <span className="text-xs text-tarkov-text-dim font-mono truncate">{req.tpl}</span>
                  )}
                </div>
                <input
                  className="input-field w-24"
                  type="number"
                  placeholder="Count"
                  value={req.count}
                  onChange={e => {
                    const next = [...crafting.requirements]
                    next[i] = { ...next[i], count: parseInt(e.target.value, 10) || 0 }
                    onChange({ crafting: { ...crafting, requirements: next } })
                  }}
                />
                <button
                  className="btn-danger"
                  onClick={() => {
                    const next = crafting.requirements.filter((_, idx) => idx !== i)
                    onChange({ crafting: { ...crafting, requirements: next } })
                  }}
                >
                  <Trash2 size={16} />
                </button>
              </div>
            ))}
            <button
              className="btn-secondary mt-2 flex items-center gap-1.5"
              onClick={() =>
                onChange({
                  crafting: {
                    ...crafting,
                    requirements: [...crafting.requirements, { tpl: '', count: 1 }],
                  },
                })
              }
            >
              <Plus size={14} /> Add Requirement
            </button>
          </div>
        </>
      )}
    </Section>
  )
}

function FiltersTab({ ammo, onChange }: { ammo: AmmoDefinition; onChange: (u: Partial<AmmoDefinition>) => void }) {
  const compat = getAmmoCompatibility(ammo.baseTpl)

  const autoFill = () => {
    if (!compat) return
    onChange({
      filters: {
        ...ammo.filters,
        patchMagazines: [...new Set([...ammo.filters.patchMagazines, ...compat.magazines.map(m => m.id)])],
        patchWeapons: [...new Set([...ammo.filters.patchWeapons, ...compat.weapons.map(w => w.id)])],
      },
    })
  }

  return (
    <Section title="Filter Patching" icon={<Filter size={18} />}>
      <p className="text-sm text-tarkov-text-dim mb-4">
        Optional magazine / weapon IDs whose filters should be patched to accept this ammo.
      </p>

      {compat && (
        <div className="mb-4 p-3 bg-tarkov-bg border border-tarkov-border rounded-lg">
          <div className="text-sm text-tarkov-text mb-2">
            Found <span className="text-tarkov-accent font-medium">{compat.magazines.length}</span> compatible magazines and{' '}
            <span className="text-tarkov-accent font-medium">{compat.weapons.length}</span> compatible weapons for the selected base ammo.
          </div>
          <button onClick={autoFill} className="btn-primary text-sm flex items-center gap-1.5">
            <Filter size={14} /> Auto-fill Compatible Magazines & Weapons
          </button>
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <Field label="Patch Magazines" tooltip="Magazine IDs that will have their cartridge filter patched to accept this ammo.">
          <textarea
            className="input-field min-h-[120px] font-mono text-sm resize-y"
            value={ammo.filters.patchMagazines.join('\n')}
            onChange={e =>
              onChange({
                filters: {
                  ...ammo.filters,
                  patchMagazines: e.target.value.split('\n').map(s => s.trim()).filter(Boolean),
                },
              })
            }
            placeholder="One 24-char ID per line"
          />
          <ResolvedNameList ids={ammo.filters.patchMagazines} />
        </Field>
        <Field label="Patch Weapons" tooltip="Weapon IDs that will have their chamber filter patched to accept this ammo.">
          <textarea
            className="input-field min-h-[120px] font-mono text-sm resize-y"
            value={ammo.filters.patchWeapons.join('\n')}
            onChange={e =>
              onChange({
                filters: {
                  ...ammo.filters,
                  patchWeapons: e.target.value.split('\n').map(s => s.trim()).filter(Boolean),
                },
              })
            }
            placeholder="One 24-char ID per line"
          />
          <ResolvedNameList ids={ammo.filters.patchWeapons} />
        </Field>
      </div>
    </Section>
  )
}

function ResolvedNameList({ ids }: { ids: string[] }) {
  const resolved = useMemo(() => {
    return ids.map(id => ({ id, name: getItemName(id) || 'Unknown item' }))
  }, [ids])

  if (ids.length === 0) return null

  return (
    <div className="mt-2 p-2 bg-tarkov-bg border border-tarkov-border rounded-lg max-h-[160px] overflow-y-auto">
      <div className="text-xs text-tarkov-text-dim mb-1">Resolved item names:</div>
      <ul className="text-xs space-y-0.5">
        {resolved.map((entry, i) => (
          <li key={i} className="flex gap-2">
            <span className="font-mono text-tarkov-text-dim truncate max-w-[100px]">{entry.id}</span>
            <span className="text-tarkov-text truncate">{entry.name}</span>
          </li>
        ))}
      </ul>
    </div>
  )
}

function PreviewTab({ pack, activeAmmo }: { pack: AmmoPackDefinition; activeAmmo: AmmoDefinition }) {
  return (
    <Section title="JSON Preview" icon={<FileJson size={18} />}>
      <div className="bg-tarkov-bg border border-tarkov-border rounded p-4 font-mono text-xs text-tarkov-text-dim overflow-auto max-h-[70vh]">
        <pre>{JSON.stringify(buildExportJson({ ...pack, ammo: [activeAmmo] }), null, 2)}</pre>
      </div>
    </Section>
  )
}

function AmmoBoxTab({ ammo, onChange }: { ammo: AmmoDefinition; onChange: (u: Partial<AmmoDefinition>) => void }) {
  const updateBox = (updates: Partial<AmmoBoxEntry>) => {
    onChange({ ammoBox: { ...ammo.ammoBox, ...updates } })
  }

  return (
    <Section title="Ammo Box" icon={<Box size={18} />}>
      <div className="mb-4">
        <Toggle
          checked={ammo.ammoBox.enabled}
          onChange={v => updateBox({ enabled: v })}
          label="Generate a lootable ammo box for this ammo"
        />
      </div>

      {ammo.ammoBox.enabled && (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Field label="Ammo Box ID" className="md:col-span-2" tooltip="Unique 24-character hex ID for the generated ammo box.">
            <div className="flex gap-2">
              <input
                className="input-field flex-1 font-mono text-sm"
                value={ammo.ammoBox.id}
                onChange={e => updateBox({ id: e.target.value })}
                placeholder="24-char hex string"
                maxLength={24}
              />
              <button onClick={() => updateBox({ id: generateMongoId() })} className="btn-secondary text-xs px-2" title="Generate random ID">
                <RefreshCw size={14} />
              </button>
            </div>
          </Field>

          <Field label="Base Ammo Box Template" className="md:col-span-2" tooltip="Existing ammo box to clone. Its model and stack slot count will be reused. Choose 'Other' to enter a custom template ID from a mod.">
            <select
              className="input-field"
              value={AMMO_BOX_TEMPLATES.some(t => t.id === ammo.ammoBox.baseTpl) ? ammo.ammoBox.baseTpl : ammo.ammoBox.baseTpl ? '__other__' : ''}
              onChange={e => {
                const value = e.target.value
                if (value === '__other__') {
                  updateBox({ baseTpl: '__other__' })
                } else if (value) {
                  const template = getAmmoBoxTemplate(value)
                  updateBox({
                    baseTpl: value,
                    count: template?.count || ammo.ammoBox.count,
                  })
                } else {
                  updateBox({ baseTpl: '' })
                }
              }}
            >
              <option value="">Select an ammo box template...</option>
              {AMMO_BOX_TEMPLATES.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.name} ({t.count} rounds) — {t.id}
                </option>
              ))}
              <option value="__other__">Other (custom ID)...</option>
            </select>
            {(ammo.ammoBox.baseTpl === '__other__' || (!!ammo.ammoBox.baseTpl && !AMMO_BOX_TEMPLATES.some(t => t.id === ammo.ammoBox.baseTpl))) && (
              <input
                className="input-field mt-2 font-mono text-sm"
                value={ammo.ammoBox.baseTpl === '__other__' ? '' : ammo.ammoBox.baseTpl}
                onChange={e => updateBox({ baseTpl: e.target.value })}
                placeholder="Enter custom ammo box template ID"
              />
            )}
          </Field>

          <Field label="Ammo Box Name" tooltip="In-game name for the generated box.">
            <input
              className="input-field"
              value={ammo.ammoBox.name}
              onChange={e => updateBox({ name: e.target.value })}
              placeholder="e.g. Box of Custom 5.45 BP"
            />
          </Field>

          <Field label="Short Name" tooltip="Short name shown in tight UI spaces.">
            <input
              className="input-field"
              value={ammo.ammoBox.shortName}
              onChange={e => updateBox({ shortName: e.target.value })}
              placeholder="e.g. cBP box"
            />
          </Field>

          <Field label="Description" className="md:col-span-2" tooltip="Description shown when examining the box.">
            <textarea
              className="input-field min-h-[80px] resize-y"
              rows={3}
              value={ammo.ammoBox.description}
              onChange={e => updateBox({ description: e.target.value })}
              placeholder="A box containing custom ammo..."
            />
          </Field>

          <Field label="Round Count" tooltip="Number of rounds inside the box. Auto-fills from the template but can be overridden.">
            <input
              className="input-field"
              type="number"
              min={1}
              value={ammo.ammoBox.count}
              onChange={e => updateBox({ count: parseInt(e.target.value, 10) || 0 })}
            />
          </Field>

          <Field label="Handbook Price (₽)" tooltip="Handbook price for the box itself.">
            <input
              className="input-field"
              type="number"
              value={ammo.ammoBox.handbookPriceRoubles}
              onChange={e => updateBox({ handbookPriceRoubles: parseInt(e.target.value, 10) || 0 })}
            />
          </Field>

          <Field label="Rarity PvE" tooltip="Loot rarity for the box in PvE.">
            <select
              className="input-field"
              value={ammo.ammoBox.rarityPvE}
              onChange={e => updateBox({ rarityPvE: e.target.value })}
            >
              {RARITY_OPTIONS.map((r) => (
                <option key={r} value={r}>{r}</option>
              ))}
            </select>
          </Field>

          <div className="md:col-span-2 mt-2">
            <Toggle
              checked={ammo.ammoBox.sellToTraders}
              onChange={v => updateBox({ sellToTraders: v })}
              label="Sell ammo box to traders"
            />
          </div>

          {ammo.ammoBox.sellToTraders && (
            <>
              <Field label="Ammo Box Trader Price (₽)" tooltip="Price traders will sell the ammo box for.">
                <input
                  className="input-field"
                  type="number"
                  value={ammo.ammoBox.traderPriceRoubles}
                  onChange={e => updateBox({ traderPriceRoubles: parseInt(e.target.value, 10) || 0 })}
                />
              </Field>

              <Field label="Trader Override" tooltip="Leave blank to use the same trader(s) as the ammo. Enter a trader ID to sell the box from a different trader.">
                <select
                  className="input-field"
                  value={ammo.ammoBox.traderId ?? ''}
                  onChange={e => updateBox({ traderId: e.target.value || undefined })}
                >
                  <option value="">Same as ammo</option>
                  {VANILLA_TRADERS.map((t) => (
                    <option key={t.id} value={t.id}>{t.name}</option>
                  ))}
                </select>
              </Field>

              <Field label="Loyalty Level Override" tooltip="Leave blank to use the ammo's loyalty level. 1-4.">
                <input
                  className="input-field"
                  type="number"
                  min={1}
                  max={4}
                  value={ammo.ammoBox.loyaltyLevel ?? ''}
                  placeholder="Same as ammo"
                  onChange={e => {
                    const value = e.target.value
                    updateBox({ loyaltyLevel: value === '' ? undefined : parseInt(value, 10) || 1 })
                  }}
                />
              </Field>

              <Field label="Stock Count Override" tooltip="Leave blank to use the ammo's stock count. 0 is out of stock. Use the toggle below for unlimited stock.">
                <input
                  className="input-field"
                  type="number"
                  disabled={ammo.ammoBox.unlimitedStock}
                  value={ammo.ammoBox.stockCount ?? ''}
                  placeholder="Same as ammo"
                  onChange={e => {
                    const value = e.target.value
                    updateBox({ stockCount: value === '' ? undefined : parseInt(value, 10) || 0 })
                  }}
                />
                <Toggle
                  checked={!!ammo.ammoBox.unlimitedStock}
                  onChange={v => updateBox({ unlimitedStock: v })}
                  label="Unlimited stock"
                />
              </Field>

              <Field label="Buy Restriction Override" tooltip="Leave blank to use the ammo's buy restriction. 0 means no purchases allowed. Use the toggle below for no restriction.">
                <input
                  className="input-field"
                  type="number"
                  disabled={ammo.ammoBox.unlimitedBuyRestriction}
                  value={ammo.ammoBox.buyRestrictionMax ?? ''}
                  placeholder="Same as ammo"
                  onChange={e => {
                    const value = e.target.value
                    updateBox({ buyRestrictionMax: value === '' ? undefined : parseInt(value, 10) || 0 })
                  }}
                />
                <Toggle
                  checked={!!ammo.ammoBox.unlimitedBuyRestriction}
                  onChange={v => updateBox({ unlimitedBuyRestriction: v })}
                  label="Unlimited buy restriction"
                />
              </Field>
            </>
          )}
        </div>
      )}
    </Section>
  )
}

function LootEntryEditor({
  title,
  loot,
  onChange,
  itemLabel,
  disabled,
  disabledHint,
}: {
  title: string
  loot: LootEntry
  onChange: (u: Partial<LootEntry>) => void
  itemLabel: string
  disabled?: boolean
  disabledHint?: string
}) {
  const [manualId, setManualId] = useState('')
  const [selectedContainer, setSelectedContainer] = useState('')

  const addContainer = (id: string) => {
    const trimmed = id.trim()
    if (!trimmed) return
    if (loot.containerIds.includes(trimmed)) return
    onChange({ containerIds: [...loot.containerIds, trimmed] })
  }

  const removeContainer = (id: string) => {
    onChange({ containerIds: loot.containerIds.filter((c: string) => c !== id) })
  }

  return (
    <div className="mb-6">
      <div className="text-sm font-semibold text-tarkov-text mb-2">{title}</div>
      <div className="mb-4">
        <Toggle
          checked={loot.enabled && !disabled}
          onChange={v => {
            if (!disabled) onChange({ enabled: v })
          }}
          label={disabled && disabledHint ? `${itemLabel} (${disabledHint})` : itemLabel}
        />
      </div>

      {loot.enabled && !disabled && (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Field
            label="Container IDs"
            className="md:col-span-2"
            tooltip="Select containers from the dropdown or type a 24-char ID manually. The selected item will be added as a possible loot spawn inside these containers (e.g., weapon crates, ammo boxes, duffle bags)."
          >
            <div className="flex flex-col gap-3">
              <div className="flex gap-2">
                <select
                  className="input-field flex-1"
                  value={selectedContainer}
                  onChange={e => {
                    const id = e.target.value
                    setSelectedContainer(id)
                    if (id) {
                      addContainer(id)
                      setSelectedContainer('')
                    }
                  }}
                >
                  <option value="">Add a container...</option>
                  {LOOT_CONTAINERS.map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.name} — {c.id}
                    </option>
                  ))}
                </select>
              </div>

              <div className="flex gap-2">
                <input
                  className="input-field flex-1 font-mono text-sm"
                  value={manualId}
                  onChange={e => setManualId(e.target.value)}
                  onKeyDown={e => {
                    if (e.key === 'Enter') {
                      e.preventDefault()
                      addContainer(manualId)
                      setManualId('')
                    }
                  }}
                  placeholder="Or enter a container ID manually and press Enter"
                  maxLength={24}
                />
                <button
                  onClick={() => {
                    addContainer(manualId)
                    setManualId('')
                  }}
                  className="btn-secondary px-3"
                  title="Add container"
                >
                  <Plus size={18} />
                </button>
              </div>

              {loot.containerIds.length > 0 && (
                <div className="flex flex-wrap gap-2">
                  {loot.containerIds.map((id: string) => {
                    const container = getLootContainer(id)
                    const label = container ? `${container.name} (${id})` : id
                    return (
                      <span
                        key={id}
                        className="inline-flex items-center gap-1 px-2 py-1 bg-tarkov-surface border border-tarkov-border rounded text-xs font-mono"
                      >
                        {label}
                        <button
                          onClick={() => removeContainer(id)}
                          className="text-tarkov-error hover:text-tarkov-error/80"
                          title="Remove"
                        >
                          <X size={14} />
                        </button>
                      </span>
                    )
                  })}
                </div>
              )}
            </div>
          </Field>

          <Field label="Rarity" tooltip="Loot rarity for this item in the specified containers.">
            <select
              className="input-field"
              value={loot.rarity}
              onChange={e => onChange({ rarity: e.target.value })}
            >
              {RARITY_OPTIONS.map((r) => (
                <option key={r} value={r}>{r}</option>
              ))}
            </select>
          </Field>
        </div>
      )}
    </div>
  )
}

function LootTab({ ammo, onChange }: { ammo: AmmoDefinition; onChange: (u: Partial<AmmoDefinition>) => void }) {
  const updateAmmoLoot = (updates: Partial<LootEntry>) => {
    onChange({ ammoLoot: { ...ammo.ammoLoot, ...updates } })
  }
  const updateAmmoBoxLoot = (updates: Partial<LootEntry>) => {
    onChange({ ammoBoxLoot: { ...ammo.ammoBoxLoot, ...updates } })
  }

  return (
    <Section title="Loot Table Injection" icon={<MapPin size={18} />}>
      <LootEntryEditor
        title="Ammo Loot"
        loot={ammo.ammoLoot}
        onChange={updateAmmoLoot}
        itemLabel="Add this ammo to container loot tables"
      />
      <LootEntryEditor
        title="Ammo Box Loot"
        loot={ammo.ammoBoxLoot}
        onChange={updateAmmoBoxLoot}
        itemLabel="Add the generated ammo box to container loot tables"
        disabled={!ammo.ammoBox.enabled}
        disabledHint="enable the ammo box first"
      />
    </Section>
  )
}

function GrenadeIdentityTab({ pack, setPack, grenade, onChange }: {
  pack: AmmoPackDefinition
  setPack: Dispatch<SetStateAction<AmmoPackDefinition>>
  grenade: GrenadeDefinition
  onChange: (u: Partial<GrenadeDefinition>) => void
}) {
  return (
    <div className="space-y-6">
      <Section title="Pack Identity" icon={<Shield size={18} />}>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Field label="Pack Name">
            <input
              className="input-field"
              value={pack.name}
              onChange={e => setPack({ ...pack, name: e.target.value })}
              placeholder="My Ammo Pack"
            />
          </Field>
          <Field label="Enabled">
            <div className="mt-2">
              <Toggle checked={pack.enabled} onChange={v => setPack({ ...pack, enabled: v })} label="Pack enabled" />
            </div>
          </Field>
        </div>
      </Section>

      <Section title="Grenade Identity" icon={<Bomb size={18} />}>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Field label="Grenade ID">
            <div className="flex gap-2">
              <input
                className="input-field flex-1 font-mono text-sm"
                value={grenade.id}
                onChange={e => onChange({ id: e.target.value })}
                placeholder="24-char hex string"
                maxLength={24}
              />
              <button onClick={() => onChange({ id: generateMongoId() })} className="btn-secondary text-xs px-2" title="Generate random ID">
                <RefreshCw size={14} />
              </button>
            </div>
          </Field>

          <Field label="Enabled">
            <div className="mt-2">
              <Toggle checked={grenade.enabled} onChange={v => onChange({ enabled: v })} label="Grenade enabled" />
            </div>
          </Field>

          <Field
            label="Base Grenade Template"
            className="md:col-span-2"
            tooltip="Existing grenade item to clone. Selecting one auto-fills the Stats tab with the base grenade's values. Choose 'Other' to enter a custom template ID from a mod."
          >
            <select
              className="input-field"
              value={GRENADE_TEMPLATES.some(t => t.id === grenade.baseTpl) ? grenade.baseTpl : grenade.baseTpl ? '__other__' : ''}
              onChange={e => {
                const value = e.target.value
                if (value === '__other__') {
                  onChange({ baseTpl: '__other__', compareToGrenadeId: '' })
                } else if (value) {
                  const base = getGrenadeStats(value)
                  if (base) {
                    const { name, shortName, ...baseStats } = base
                    onChange({
                      baseTpl: value,
                      compareToGrenadeId: '',
                      stats: baseStats as GrenadeStats,
                    })
                  } else {
                    onChange({ baseTpl: value, compareToGrenadeId: '' })
                  }
                } else {
                  onChange({ baseTpl: '', compareToGrenadeId: '' })
                }
              }}
            >
              <option value="">Select a base grenade...</option>
              {GRENADE_TEMPLATES.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.name} — {t.id}
                </option>
              ))}
              <option value="__other__">Other (custom ID)...</option>
            </select>
            {(grenade.baseTpl === '__other__' || (!!grenade.baseTpl && !GRENADE_TEMPLATES.some(t => t.id === grenade.baseTpl))) && (
              <input
                className="input-field mt-2 font-mono text-sm"
                value={grenade.baseTpl === '__other__' ? '' : grenade.baseTpl}
                onChange={e => onChange({ baseTpl: e.target.value })}
                placeholder="Enter custom grenade template ID"
              />
            )}
          </Field>

          <Field label="Name">
            <input
              className="input-field"
              value={grenade.name}
              onChange={e => onChange({ name: e.target.value })}
              placeholder="e.g. Custom F-1"
            />
          </Field>

          <Field label="Short Name">
            <input
              className="input-field"
              value={grenade.shortName}
              onChange={e => onChange({ shortName: e.target.value })}
              placeholder="e.g. cF-1"
            />
          </Field>

          <Field label="Description" className="md:col-span-2">
            <textarea
              className="input-field min-h-[80px] resize-y"
              rows={3}
              value={grenade.description}
              onChange={e => onChange({ description: e.target.value })}
              placeholder="A short description of the grenade..."
            />
          </Field>

          <Field
            label="Handbook Parent ID (optional)"
            tooltip="Handbook category ID for trader/flea sorting. Leave blank and the server will look up the base grenade's category automatically."
          >
            <input
              className="input-field font-mono text-sm"
              value={grenade.handbookParentId || ''}
              onChange={e => onChange({ handbookParentId: e.target.value || undefined })}
              placeholder="Leave blank to auto-resolve from base template"
            />
          </Field>
        </div>
      </Section>
    </div>
  )
}

function GrenadeStatsTab({ grenade, onChange }: { grenade: GrenadeDefinition; onChange: (u: Partial<GrenadeDefinition>) => void }) {
  const updateStat = <K extends keyof GrenadeStats>(key: K, value: GrenadeStats[K]) => {
    onChange({ stats: { ...grenade.stats, [key]: value } })
  }

  const updateVector3 = (key: keyof GrenadeStats, axis: 'x' | 'y' | 'z', value: number) => {
    const v = grenade.stats[key] as Vector3
    onChange({ stats: { ...grenade.stats, [key]: { ...v, [axis]: value } } })
  }

  const updateSmokeKeyframe = (index: number, field: 'time' | 'value', value: number) => {
    const keyframes = [...grenade.stats.smokeSizeOverTime]
    keyframes[index] = { ...keyframes[index], [field]: value }
    onChange({ stats: { ...grenade.stats, smokeSizeOverTime: keyframes } })
  }

  const addSmokeKeyframe = () => {
    onChange({ stats: { ...grenade.stats, smokeSizeOverTime: [...grenade.stats.smokeSizeOverTime, { time: 0, value: 0 }] } })
  }

  const removeSmokeKeyframe = (index: number) => {
    const keyframes = [...grenade.stats.smokeSizeOverTime]
    keyframes.splice(index, 1)
    onChange({ stats: { ...grenade.stats, smokeSizeOverTime: keyframes } })
  }

  const updateSmokeSpeedRange = (index: number, field: 'x' | 'y', value: number) => {
    const ranges = [...grenade.stats.smokeStartSpeed]
    ranges[index] = { ...ranges[index], [field]: value }
    onChange({ stats: { ...grenade.stats, smokeStartSpeed: ranges } })
  }

  const addSmokeSpeedRange = () => {
    onChange({ stats: { ...grenade.stats, smokeStartSpeed: [...grenade.stats.smokeStartSpeed, { x: 0, y: 0 }] } })
  }

  const removeSmokeSpeedRange = (index: number) => {
    const ranges = [...grenade.stats.smokeStartSpeed]
    ranges.splice(index, 1)
    onChange({ stats: { ...grenade.stats, smokeStartSpeed: ranges } })
  }

  const renderNumberField = (label: string, key: keyof GrenadeStats, tooltip?: string, step?: number) => (
    <Field key={key} label={label} tooltip={tooltip}>
      <input
        className="input-field font-mono text-sm"
        type="number"
        step={step ?? 0.01}
        value={grenade.stats[key] as number}
        onChange={e => updateStat(key, (parseFloat(e.target.value) || 0) as GrenadeStats[keyof GrenadeStats])}
      />
    </Field>
  )

  const renderToggleableNumberField = (
    label: string,
    valueKey: keyof GrenadeStats,
    overrideKey: keyof GrenadeStats,
    tooltip?: string,
    step?: number,
  ) => {
    const enabled = grenade.stats[overrideKey] as boolean
    return (
      <Field key={valueKey} label={label} tooltip={tooltip}>
        <div className="flex items-center gap-3">
          <input
            type="checkbox"
            checked={enabled}
            onChange={e => updateStat(overrideKey, e.target.checked as GrenadeStats[keyof GrenadeStats])}
            title="Override base value"
          />
          <input
            className="input-field font-mono text-sm flex-1"
            type="number"
            step={step ?? 0.01}
            disabled={!enabled}
            value={grenade.stats[valueKey] as number}
            onChange={e => updateStat(valueKey, (parseFloat(e.target.value) || 0) as GrenadeStats[keyof GrenadeStats])}
          />
        </div>
      </Field>
    )
  }

  const renderBooleanField = (label: string, key: keyof GrenadeStats, tooltip?: string) => (
    <Field key={key} label={label} tooltip={tooltip}>
      <select
        className="input-field"
        value={grenade.stats[key] ? 'true' : 'false'}
        onChange={e => updateStat(key, (e.target.value === 'true') as GrenadeStats[keyof GrenadeStats])}
      >
        <option value="false">No</option>
        <option value="true">Yes</option>
      </select>
    </Field>
  )

  const renderSelectWithCustom = (
    label: string,
    value: string,
    options: string[],
    onChange: (v: string) => void,
    tooltip?: string,
  ) => {
    const isCustom = value && !options.includes(value)
    return (
      <Field label={label} tooltip={tooltip}>
        <select
          className="input-field"
          value={isCustom ? '__CUSTOM__' : value || ''}
          onChange={e => {
            const selected = e.target.value
            if (selected === '__CUSTOM__') {
              onChange('')
            } else {
              onChange(selected)
            }
          }}
        >
          <option value="">None / Default</option>
          {options.map(opt => (
            <option key={opt} value={opt}>{opt}</option>
          ))}
          <option value="__CUSTOM__">Custom...</option>
        </select>
        {isCustom && (
          <input
            className="input-field mt-2 font-mono text-sm"
            value={value}
            onChange={e => onChange(e.target.value)}
            placeholder={`Enter custom ${label.toLowerCase()}`}
          />
        )}
      </Field>
    )
  }

  const base = grenade.baseTpl ? getGrenadeStats(grenade.baseTpl) : undefined

  const statNames: Record<string, string> = {
    minExplosionDistance: 'Min Explosion Distance',
    maxExplosionDistance: 'Max Explosion Distance',
    fragmentsCount: 'Fragments Count',
    contusionDistance: 'Contusion Distance',
    explDelay: 'Explosion Delay',
    minTimeToContactExplode: 'Min Time To Contact Explode',
    strength: 'Strength (Fragment Damage)',
    throwDamMax: 'Throw Damage Max',
    weight: 'Weight',
  }

  const allNumericStats = [
    'minExplosionDistance', 'maxExplosionDistance', 'fragmentsCount', 'contusionDistance',
    'explDelay', 'minTimeToContactExplode', 'strength', 'throwDamMax', 'weight',
  ] as const

  return (
    <Section title="Grenade Stats" icon={<Bomb size={18} />}>
      <CollapsibleSection title="Explosion" icon={<Bomb size={16} />} defaultOpen={true}>
        <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
          {renderNumberField('Min Explosion Distance', 'minExplosionDistance', 'Minimum distance at which the explosion deals damage.')}
          {renderNumberField('Max Explosion Distance', 'maxExplosionDistance', 'Maximum distance at which the explosion deals damage.')}
          {renderNumberField('Fragments Count', 'fragmentsCount', 'Number of fragments generated on explosion.', 1)}
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
          {renderSelectWithCustom(
            'Fragment Type',
            grenade.stats.fragmentType,
            GRENADE_FRAGMENT_TYPES,
            v => updateStat('fragmentType', v),
            'Fragment type identifier used by the explosion. Pick from the values used by base grenade templates, or enter a custom ID.'
          )}
          {renderSelectWithCustom(
            'Explosion Effect Type',
            grenade.stats.explosionEffectType,
            GRENADE_EXPLOSION_EFFECT_TYPES,
            v => updateStat('explosionEffectType', v),
            'Explosion effect type identifier. Pick from the values used by base grenade templates, or enter a custom value.'
          )}
        </div>
      </CollapsibleSection>

      <CollapsibleSection title="Effects" icon={<Bomb size={16} />} defaultOpen={false}>
        <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
          {renderNumberField('Contusion Distance', 'contusionDistance', 'Distance at which contusion is applied.')}
          {renderNumberField('Explosion Delay', 'explDelay', 'Delay before the grenade explodes after trigger.')}
          {renderNumberField('Min Time To Contact Explode', 'minTimeToContactExplode', 'Minimum time before contact explosion can occur.')}
          {renderNumberField('Strength', 'strength', 'Max damage per fragment.', 1)}
          {renderNumberField('Min Fragment Damage', 'minFragmentDamage', 'Minimum damage a single fragment can deal.', 0.1)}
          {renderNumberField('Throw Damage Max', 'throwDamMax', 'Maximum damage dealt on direct throw impact.')}
          {renderNumberField('Weight', 'weight', 'Grenade weight in kg.')}
          {renderBooleanField('Play Fuze Sound', 'playFuzeSound', 'Whether the fuze sound is audible before detonation.')}
          {renderBooleanField('Can Plant On Ground', 'canPlantOnGround', 'Whether the grenade can be planted on the ground as a tripwire mine.')}
        </div>
      </CollapsibleSection>

      <CollapsibleSection title="Throw" icon={<Bomb size={16} />} defaultOpen={false}>
        {renderSelectWithCustom(
          'Throw Type',
          grenade.stats.throwType,
          GRENADE_THROW_TYPES,
          v => updateStat('throwType', v),
          'Grenade throwing type / classification. Pick from the values used by base grenade templates, or enter a custom value.'
        )}
      </CollapsibleSection>

      <CollapsibleSection title="Smoke Color" icon={<Bomb size={16} />} defaultOpen={false}>
        <Field
          label="Smoke Color"
          tooltip="Custom hex color for smoke grenades. Requires the AmmoGen Client mod to render in-game."
        >
          <div className="flex items-center gap-3">
            <input
              type="color"
              className="w-10 h-10 rounded cursor-pointer bg-transparent border-0 p-0"
              value={grenade.stats.smokeColor || '#FFFFFF'}
              onChange={e => updateStat('smokeColor', e.target.value.toUpperCase())}
            />
            <input
              className="input-field font-mono text-sm flex-1"
              value={grenade.stats.smokeColor}
              onChange={e => updateStat('smokeColor', e.target.value.toUpperCase())}
              placeholder="#RRGGBB"
            />
            {grenade.stats.smokeColor && (
              <button
                className="btn-secondary text-xs px-2"
                onClick={() => updateStat('smokeColor', '')}
                title="Clear smoke color"
              >
                <X size={14} />
              </button>
            )}
          </div>
        </Field>
        <p className="text-xs text-tarkov-text-dim mt-2">
          Leave blank to use the base grenade's default smoke color. Set a hex value to override it in-game via the AmmoGen Client mod.
        </p>
      </CollapsibleSection>

      <CollapsibleSection title="Body Color" icon={<Bomb size={16} />} defaultOpen={false}>
        <Field
          label="Body Color"
          tooltip="Custom hex color for the grenade body once thrown. Requires the AmmoGen Client mod to render in-game."
        >
          <div className="flex items-center gap-3">
            <input
              type="color"
              className="w-10 h-10 rounded cursor-pointer bg-transparent border-0 p-0"
              value={grenade.stats.bodyColor || '#FFFFFF'}
              onChange={e => updateStat('bodyColor', e.target.value.toUpperCase())}
            />
            <input
              className="input-field font-mono text-sm flex-1"
              value={grenade.stats.bodyColor}
              onChange={e => updateStat('bodyColor', e.target.value.toUpperCase())}
              placeholder="#RRGGBB"
            />
            {grenade.stats.bodyColor && (
              <button
                className="btn-secondary text-xs px-2"
                onClick={() => updateStat('bodyColor', '')}
                title="Clear body color"
              >
                <X size={14} />
              </button>
            )}
          </div>
        </Field>
        <p className="text-xs text-tarkov-text-dim mt-2">
          Leave blank to keep the base grenade's default body color. Set a hex value to recolor the grenade body once thrown via the AmmoGen Client mod.
        </p>
      </CollapsibleSection>

      <CollapsibleSection title="Smoke Settings" icon={<Bomb size={16} />} defaultOpen={false}>
        {renderToggleableNumberField('Smoke Radius', 'smokeRadius', 'overrideSmokeRadius', 'Multiplier for the smoke cloud size. 1 = default. Requires AmmoGen Client.', 0.1)}
        {renderToggleableNumberField('Smoke Duration', 'smokeDuration', 'overrideSmokeDuration', 'How long the smoke emission lasts in seconds. 90 = default. Requires AmmoGen Client.', 1)}
        {renderToggleableNumberField('Smoke Fill Size', 'smokeFillSize', 'overrideSmokeFillSize', 'Initial fill size for the smoke area. 3.5 = default. Requires AmmoGen Client.', 0.1)}

        <Field label="Smoke Size Over Time" tooltip="Animation curve keyframes controlling smoke radius over time. Disabled by default; enable to override the base curve.">
          <div className="flex flex-col gap-2">
            <label className="flex items-center gap-2 text-sm">
              <input
                type="checkbox"
                checked={grenade.stats.overrideSmokeSizeOverTime}
                onChange={e => updateStat('overrideSmokeSizeOverTime', e.target.checked)}
              />
              Override base size curve
            </label>
            {grenade.stats.overrideSmokeSizeOverTime && (
              <div className="flex flex-col gap-2">
                {grenade.stats.smokeSizeOverTime.map((kf, i) => (
                  <div key={i} className="flex items-center gap-2">
                    <input
                      type="number"
                      step={0.01}
                      className="input-field font-mono text-sm w-24"
                      value={kf.time}
                      onChange={e => updateSmokeKeyframe(i, 'time', parseFloat(e.target.value) || 0)}
                      placeholder="Time"
                    />
                    <input
                      type="number"
                      step={0.01}
                      className="input-field font-mono text-sm w-24"
                      value={kf.value}
                      onChange={e => updateSmokeKeyframe(i, 'value', parseFloat(e.target.value) || 0)}
                      placeholder="Value"
                    />
                    <button className="btn-secondary text-xs px-2" onClick={() => removeSmokeKeyframe(i)} title="Remove keyframe">
                      <Trash2 size={14} />
                    </button>
                  </div>
                ))}
                <button className="btn-secondary text-xs px-2 self-start flex items-center gap-1" onClick={addSmokeKeyframe}>
                  <Plus size={14} /> Add Keyframe
                </button>
              </div>
            )}
          </div>
        </Field>

        <Field label="Smoke Start Speed" tooltip="Particle speed ranges. Disabled by default; enable to override the base values.">
          <div className="flex flex-col gap-2">
            <label className="flex items-center gap-2 text-sm">
              <input
                type="checkbox"
                checked={grenade.stats.overrideSmokeStartSpeed}
                onChange={e => updateStat('overrideSmokeStartSpeed', e.target.checked)}
              />
              Override base start speed
            </label>
            {grenade.stats.overrideSmokeStartSpeed && (
              <div className="flex flex-col gap-2">
                {grenade.stats.smokeStartSpeed.map((range, i) => (
                  <div key={i} className="flex items-center gap-2">
                    <input
                      type="number"
                      step={0.1}
                      className="input-field font-mono text-sm w-24"
                      value={range.x}
                      onChange={e => updateSmokeSpeedRange(i, 'x', parseFloat(e.target.value) || 0)}
                      placeholder="Min"
                    />
                    <input
                      type="number"
                      step={0.1}
                      className="input-field font-mono text-sm w-24"
                      value={range.y}
                      onChange={e => updateSmokeSpeedRange(i, 'y', parseFloat(e.target.value) || 0)}
                      placeholder="Max"
                    />
                    <button className="btn-secondary text-xs px-2" onClick={() => removeSmokeSpeedRange(i)} title="Remove range">
                      <Trash2 size={14} />
                    </button>
                  </div>
                ))}
                <button className="btn-secondary text-xs px-2 self-start flex items-center gap-1" onClick={addSmokeSpeedRange}>
                  <Plus size={14} /> Add Range
                </button>
              </div>
            )}
          </div>
        </Field>

        <p className="text-xs text-tarkov-text-dim mt-2">
          Toggle an option to override that base value. Untoggled options are not written to the pack, so the grenade keeps its original behavior. Requires the AmmoGen Client mod.
        </p>
      </CollapsibleSection>

      <CollapsibleSection title="Area Effect Vectors" icon={<Bomb size={16} />} defaultOpen={false}>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {[
            { key: 'armorDistanceDistanceDamage', label: 'Armor Distance Damage', tooltip: 'Damage falloff over distance against armor.' },
            { key: 'contusion', label: 'Contusion', tooltip: 'Contusion effect intensity vector.' },
            { key: 'blindness', label: 'Blindness', tooltip: 'Blindness effect intensity vector.' },
          ].map(({ key, label, tooltip }) => (
            <Field key={key} label={label} tooltip={tooltip}>
              <div className="grid grid-cols-3 gap-2">
                {(['x', 'y', 'z'] as const).map(axis => (
                  <input
                    key={axis}
                    className="input-field font-mono text-sm"
                    type="number"
                    step={0.01}
                    value={(grenade.stats[key as keyof GrenadeStats] as Vector3)[axis]}
                    onChange={e => updateVector3(key as keyof GrenadeStats, axis, parseFloat(e.target.value) || 0)}
                    placeholder={axis.toUpperCase()}
                  />
                ))}
              </div>
            </Field>
          ))}
        </div>
      </CollapsibleSection>

      {grenade.baseTpl && (
      <CollapsibleSection title="Base Grenade Comparison" icon={<Bomb size={16} />} defaultOpen={false}>
        <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-3 mb-4">
          <div className="flex flex-col md:flex-row md:items-center gap-2">
            <span className="text-xs text-tarkov-text-dim">Compare to:</span>
            <select
              className="input-field text-sm py-1"
              value={(() => {
                if (!grenade.compareToGrenadeId) return grenade.baseTpl
                if (GRENADE_TEMPLATES.some(t => t.id === grenade.compareToGrenadeId)) return grenade.compareToGrenadeId
                return '__other__'
              })()}
              onChange={e => {
                const value = e.target.value
                if (value === '__other__') {
                  onChange({ compareToGrenadeId: '__other__' })
                } else if (value === grenade.baseTpl) {
                  onChange({ compareToGrenadeId: '' })
                } else {
                  onChange({ compareToGrenadeId: value })
                }
              }}
            >
              <option value={grenade.baseTpl}>Original clone ({getGrenadeStats(grenade.baseTpl)?.name || grenade.baseTpl})</option>
              {GRENADE_TEMPLATES.filter(t => t.id !== grenade.baseTpl).map((t) => (
                <option key={t.id} value={t.id}>
                  {t.name}
                </option>
              ))}
              <option value="__other__">Other (custom ID)...</option>
            </select>
            {(grenade.compareToGrenadeId === '__other__' || (!!grenade.compareToGrenadeId && grenade.compareToGrenadeId !== grenade.baseTpl && !GRENADE_TEMPLATES.some(t => t.id === grenade.compareToGrenadeId))) && (
              <input
                className="input-field text-sm py-1 font-mono"
                value={grenade.compareToGrenadeId === '__other__' ? '' : grenade.compareToGrenadeId}
                onChange={e => onChange({ compareToGrenadeId: e.target.value })}
                placeholder="Enter custom grenade ID to compare"
              />
            )}
          </div>
        </div>
        {base ? (
          <div className="grid grid-cols-2 md:grid-cols-4 gap-3 text-sm">
            {allNumericStats.map((stat) => {
              const custom = grenade.stats[stat as keyof GrenadeStats] as number
              const original = base[stat as keyof GrenadeTemplateStats] as number
              const diff = custom - original
              const diffClass = diff > 0 ? 'text-tarkov-success' : diff < 0 ? 'text-tarkov-error' : 'text-tarkov-text-dim'
              const diffText = diff === 0 ? '=' : diff > 0 ? `+${diff}` : `${diff}`
              return (
                <div key={stat} className="flex flex-col bg-tarkov-surface border border-tarkov-border rounded p-2">
                  <span className="text-tarkov-text-dim text-xs">{statNames[stat]}</span>
                  <div className="flex items-baseline gap-2">
                    <span className="font-medium text-tarkov-text">{custom}</span>
                    <span className="text-xs text-tarkov-text-dim">/ {original}</span>
                  </div>
                  <span className={`text-xs font-medium ${diffClass}`}>{diffText}</span>
                </div>
              )
            })}
          </div>
        ) : (
          <div className="text-sm text-tarkov-text-dim">Comparison grenade not found.</div>
        )}
      </CollapsibleSection>
      )}
    </Section>
  )
}

function GrenadeLootTab({ grenade, onChange }: { grenade: GrenadeDefinition; onChange: (u: Partial<GrenadeDefinition>) => void }) {
  return (
    <Section title="Loot Table Injection" icon={<MapPin size={18} />}>
      <LootEntryEditor
        title="Grenade Loot"
        loot={grenade.loot}
        onChange={loot => onChange({ loot: { ...grenade.loot, ...loot } })}
        itemLabel="Add this grenade to container loot tables"
      />
    </Section>
  )
}
