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
} from 'lucide-react'
import {
  AmmoDefinition,
  AmmoPackDefinition,
  AmmoBoxEntry,
  LootEntry,
  AMMO_TEMPLATES,
  createDefaultAmmo,
  createDefaultPack,
  createDefaultTraderEntry,
  generateMongoId,
  RARITY_OPTIONS,
  TraderEntry,
  VANILLA_TRADERS,
  ValidationError,
} from './types'
import { ITEMS, getItemName } from './generated_items'
import { getAmmoStats } from './generated_ammo_stats'
import { getAmmoCompatibility } from './generated_ammo_compatibility'
import { AMMO_BOX_TEMPLATES, getAmmoBoxTemplate } from './generated_ammo_box_templates'
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

    if (ammo.loot.enabled) {
      ammo.loot.containerIds.forEach((id, j) => {
        if (!hex24.test(id)) errors.push({ field: `${prefix}.loot.containerIds[${j}]`, message: 'Container ID must be 24 hex chars' })
      })
    }
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
      loot: ammo.loot,
    })),
  }
}

type Tab = 'identity' | 'stats' | 'economy' | 'trader' | 'crafting' | 'filters' | 'ammobox' | 'loot' | 'preview'

export default function App() {
  const [pack, setPack] = useState<AmmoPackDefinition>(createDefaultPack())
  const [activeIndex, setActiveIndex] = useState(0)
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
    const modFolder = zip.folder('AmmoGen')
    if (!modFolder) return

    const serverFiles = [
      'AmmoGen.Server.deps.json',
      'AmmoGen.Server.dll',
      'AmmoGen.Server.pdb',
      'package.json',
      'config/config.json',
    ]

    try {
      await Promise.all(
        serverFiles.map(async (file) => {
          const response = await fetch(`/AmmoGen/${file}`)
          if (!response.ok) throw new Error(`Failed to fetch ${file}`)
          const blob = await response.blob()
          modFolder.file(file, blob)
        })
      )
    } catch (err) {
      alert('Failed to package server files. Make sure the server build is in Tool/public/AmmoGen.')
      console.error(err)
      return
    }

    const json = buildExportJson(pack)
    const packName = pack.name.toLowerCase().replace(/\s+/g, '-')
    modFolder.file(`ammo/${packName}.json`, JSON.stringify(json, null, 2))

    const zipBlob = await zip.generateAsync({ type: 'blob' })
    saveAs(zipBlob, `${packName}.zip`)
    setShowExportSuccess(true)
    setTimeout(() => setShowExportSuccess(false), 3000)
  }

  const importPack = () => {
    const input = document.createElement('input')
    input.type = 'file'
    input.accept = '.json'
    input.onchange = (e) => {
      const file = (e.target as HTMLInputElement).files?.[0]
      if (!file) return
      const reader = new FileReader()
      reader.onload = (ev) => {
        try {
          const raw = String(ev.target?.result ?? '')
            .replace(/\/\/.*$/gm, '')
            .replace(/\/\*[\s\S]*?\*\//g, '')
            .replace(/,\s*([\]}])/g, '$1')
          const parsed = JSON.parse(raw)
          const ammo = (parsed.ammo ?? []).map((a: AmmoDefinition & { trader?: Partial<TraderEntry> }) => {
            const normalized: AmmoDefinition = { ...createDefaultAmmo(), ...a }
            // Backward compatibility: old single "trader" field -> new "traders" array
            if (a.trader && !Array.isArray(a.traders)) {
              normalized.traders = [a.trader as TraderEntry]
            }
            // Backward compatibility: missing ammoBox / loot fields
            if (!a.ammoBox) normalized.ammoBox = createDefaultAmmo().ammoBox
            if (!a.loot) normalized.loot = createDefaultAmmo().loot
            return normalized
          })
          const merged: AmmoPackDefinition = {
            ...createDefaultPack(),
            ...parsed,
            ammo,
          }
          setPack(merged)
          setActiveIndex(0)
          setErrors([])
          setActiveTab('identity')
        } catch (err) {
          alert('Failed to parse JSON file. Check the file format.')
          console.error(err)
        }
      }
      reader.readAsText(file)
    }
    input.click()
  }

  const activeAmmo = pack.ammo[activeIndex]

  const tabs: { id: Tab; label: string; icon: React.ReactNode }[] = [
    { id: 'identity', label: 'Identity', icon: <Shield size={16} /> },
    { id: 'stats', label: 'Stats', icon: <Crosshair size={16} /> },
    { id: 'economy', label: 'Economy', icon: <Star size={16} /> },
    { id: 'trader', label: 'Trader', icon: <Package size={16} /> },
    { id: 'crafting', label: 'Crafting', icon: <Wrench size={16} /> },
    { id: 'filters', label: 'Filters', icon: <Filter size={16} /> },
    { id: 'ammobox', label: 'Ammo Box', icon: <Box size={16} /> },
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
            <p className="text-xs text-tarkov-text-dim">SPTarkov 4.0.13 Ammo Pack Editor</p>
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
            <Download size={14} /> Export Mod ZIP
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
        {tabs.map(tab => (
          <button
            key={tab.id}
            onClick={() => activeAmmo && setActiveTab(tab.id)}
            disabled={!activeAmmo}
            className={`flex items-center gap-1.5 px-4 py-3 text-sm font-medium border-b-2 transition-colors ${
              activeTab === tab.id
                ? 'border-tarkov-accent text-tarkov-accent'
                : 'border-transparent text-tarkov-text-dim hover:text-tarkov-text'
            } ${!activeAmmo ? 'opacity-50 cursor-not-allowed' : ''}`}
          >
            {tab.icon} {tab.label}
          </button>
        ))}
      </nav>

      {/* Ammo selector */}
      <div className="bg-tarkov-surface border-b border-tarkov-border px-6 py-3">
        <div className="max-w-5xl mx-auto flex flex-col sm:flex-row sm:items-center gap-3">
          <div className="text-sm text-tarkov-text-dim">Ammo</div>
          <div className="flex flex-wrap gap-2 flex-1">
            {pack.ammo.map((ammo, i) => (
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
            ))}
          </div>
          <button onClick={addAmmo} className="btn-secondary text-sm flex items-center gap-1.5">
            <Plus size={14} /> Add Ammo
          </button>
        </div>
      </div>

      {/* Content */}
      <main className="flex-1 p-6 max-w-5xl mx-auto w-full">
        {!activeAmmo ? (
          <div className="card text-center text-tarkov-text-dim py-12">
            <Crosshair size={48} className="mx-auto mb-4 text-tarkov-accent/50" />
            <p className="text-lg">No ammo defined yet.</p>
            <p className="text-sm mt-1">Click <span className="text-tarkov-accent">Add Ammo</span> to start building your pack.</p>
          </div>
        ) : (
          <>
            {activeTab === 'identity' && <IdentityTab pack={pack} setPack={setPack} ammo={activeAmmo} onChange={u => updateAmmo(activeIndex, u)} />}
            {activeTab === 'stats' && <StatsTab ammo={activeAmmo} onChange={u => updateAmmo(activeIndex, u)} />}
            {activeTab === 'economy' && <EconomyTab ammo={activeAmmo} onChange={u => updateAmmo(activeIndex, u)} />}
            {activeTab === 'trader' && <TraderTab ammo={activeAmmo} onChange={u => updateAmmo(activeIndex, u)} />}
            {activeTab === 'crafting' && <CraftingTab ammo={activeAmmo} onChange={u => updateAmmo(activeIndex, u)} />}
            {activeTab === 'filters' && <FiltersTab ammo={activeAmmo} onChange={u => updateAmmo(activeIndex, u)} />}
            {activeTab === 'ammobox' && <AmmoBoxTab ammo={activeAmmo} onChange={u => updateAmmo(activeIndex, u)} />}
            {activeTab === 'loot' && <LootTab ammo={activeAmmo} onChange={u => updateAmmo(activeIndex, u)} />}
            {activeTab === 'preview' && <PreviewTab pack={pack} activeAmmo={activeAmmo} />}
          </>
        )}
      </main>
    </div>
  )
}

function Field({ label, children, className = '', tooltip }: { label: string; children: React.ReactNode; className?: string; tooltip?: string }) {
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
            tooltip="Existing ammo item to clone. Selecting one auto-fills the Stats tab with the base ammo's values."
          >
            <select
              className="input-field"
              value={ammo.baseTpl}
              onChange={e => {
                const baseTpl = e.target.value
                const base = getAmmoStats(baseTpl)
                if (base) {
                  onChange({
                    baseTpl,
                    stats: {
                      damage: base.damage,
                      penetration: base.penetration,
                      armorDamage: base.armorDamage,
                      initialSpeed: base.initialSpeed,
                      ammoAccr: base.ammoAccr,
                      ammoRec: base.ammoRec,
                      stackMaxSize: base.stackMaxSize,
                    },
                  })
                } else {
                  onChange({ baseTpl })
                }
              }}
            >
              <option value="">Select a base ammo...</option>
              {AMMO_TEMPLATES.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.name} ({t.caliber}) — {t.id}
                </option>
              ))}
            </select>
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
  }
  const statTooltips: Record<string, string> = {
    damage: 'Hit damage dealt to unarmored body parts.',
    penetration: 'Armor penetration capability. Higher values pierce higher armor classes.',
    armorDamage: 'Durability damage dealt to armor when hit.',
    initialSpeed: 'Muzzle velocity in meters per second. Affects drop and damage.',
    ammoAccr: 'Accuracy modifier. Positive improves accuracy; negative reduces it.',
    ammoRec: 'Recoil modifier. Positive increases recoil; negative reduces it.',
    stackMaxSize: 'Maximum rounds per inventory slot. 0 inherits the base ammo template default.',
  }
  const base = getAmmoStats(ammo.baseTpl)

  return (
    <Section title="Ammo Stats" icon={<Crosshair size={18} />}>
      <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
        {(['damage', 'penetration', 'armorDamage', 'initialSpeed', 'ammoAccr', 'ammoRec', 'stackMaxSize'] as const).map((stat) => (
          <Field key={stat} label={statNames[stat]} tooltip={statTooltips[stat]}>
            <input
              className="input-field"
              type="number"
              min={0}
              value={ammo.stats[stat]}
              onChange={e =>
                onChange({
                  stats: { ...ammo.stats, [stat]: parseInt(e.target.value, 10) || 0 },
                })
              }
            />
          </Field>
        ))}
      </div>

      {base && (
        <div className="mt-6 p-4 bg-tarkov-bg border border-tarkov-border rounded-lg">
          <h3 className="text-sm font-semibold text-tarkov-accent mb-3 flex items-center gap-2">
            <Crosshair size={16} /> Base Ammo Comparison: {base.name}
          </h3>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-3 text-sm">
            {(['damage', 'penetration', 'armorDamage', 'initialSpeed', 'ammoAccr', 'ammoRec', 'stackMaxSize'] as const).map((stat) => {
              const custom = ammo.stats[stat]
              const original = base[stat === 'damage' ? 'damage' : stat === 'penetration' ? 'penetration' : stat === 'armorDamage' ? 'armorDamage' : stat === 'initialSpeed' ? 'initialSpeed' : stat === 'ammoAccr' ? 'ammoAccr' : stat === 'ammoRec' ? 'ammoRec' : 'stackMaxSize']
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
        </div>
      )}
    </Section>
  )
}

function EconomyTab({ ammo, onChange }: { ammo: AmmoDefinition; onChange: (u: Partial<AmmoDefinition>) => void }) {
  return (
    <Section title="Economy" icon={<Star size={18} />}>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Field label="Handbook Price (₽)" tooltip="Base price used for handbook display and insurance calculations.">
          <input
            className="input-field"
            type="number"
            value={ammo.economy.handbookPriceRoubles}
            onChange={e =>
              onChange({
                economy: { ...ammo.economy, handbookPriceRoubles: parseInt(e.target.value, 10) || 0 },
              })
            }
          />
        </Field>
        <Field label="Flea Price (₽)" tooltip="Price used for Flea Market listings.">
          <input
            className="input-field"
            type="number"
            value={ammo.economy.fleaPriceRoubles}
            onChange={e =>
              onChange({
                economy: { ...ammo.economy, fleaPriceRoubles: parseInt(e.target.value, 10) || 0 },
              })
            }
          />
        </Field>
        <Field label="Rarity PvE" tooltip="Spawn rarity for PvE containers and loot tables.">
          <select
            className="input-field"
            value={ammo.economy.rarityPvE}
            onChange={e =>
              onChange({
                economy: { ...ammo.economy, rarityPvE: e.target.value },
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

function TraderTab({ ammo, onChange }: { ammo: AmmoDefinition; onChange: (u: Partial<AmmoDefinition>) => void }) {
  const updateTrader = (index: number, updates: Partial<TraderEntry>) => {
    const next = [...ammo.traders]
    next[index] = { ...next[index], ...updates }
    onChange({ traders: next })
  }

  const addTrader = () => {
    onChange({ traders: [...ammo.traders, createDefaultTraderEntry()] })
  }

  const removeTrader = (index: number) => {
    onChange({ traders: ammo.traders.filter((_, i) => i !== index) })
  }

  return (
    <Section title="Vanilla Traders" icon={<Package size={18} />}>
      <div className="space-y-4">
        {ammo.traders.map((trader, i) => (
          <div key={i} className="bg-tarkov-bg border border-tarkov-border rounded-lg p-4">
            <div className="flex items-center justify-between mb-4">
              <Toggle
                checked={trader.enabled}
                onChange={v => updateTrader(i, { enabled: v })}
                label={`Trader ${i + 1}`}
              />
              {ammo.traders.length > 1 && (
                <button className="btn-danger text-xs flex items-center gap-1" onClick={() => removeTrader(i)}>
                  <Trash2 size={14} /> Remove
                </button>
              )}
            </div>

            {trader.enabled && (
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <Field label="Trader" tooltip="Vanilla trader that sells this ammo.">
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
                <Field label="Stock Count" tooltip="Amount available after each trader restock.">
                  <input
                    className="input-field"
                    type="number"
                    value={trader.stockCount}
                    onChange={e => updateTrader(i, { stockCount: parseInt(e.target.value, 10) || 0 })}
                  />
                </Field>
                <Field label="Buy Restriction Max" tooltip="Maximum rounds a player can buy per restock cycle.">
                  <input
                    className="input-field"
                    type="number"
                    value={trader.buyRestrictionMax}
                    onChange={e => updateTrader(i, { buyRestrictionMax: parseInt(e.target.value, 10) || 0 })}
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

function CraftingTab({ ammo, onChange }: { ammo: AmmoDefinition; onChange: (u: Partial<AmmoDefinition>) => void }) {
  return (
    <Section title="Workbench Crafting" icon={<Wrench size={18} />}>
      <div className="mb-4">
        <Toggle
          checked={ammo.crafting.enabled}
          onChange={v => onChange({ crafting: { ...ammo.crafting, enabled: v } })}
          label="Add workbench craft"
        />
      </div>

      {ammo.crafting.enabled && (
        <>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
            <Field label="Workbench Level" tooltip="Hideout workbench level required to craft this ammo.">
              <input
                className="input-field"
                type="number"
                min={1}
                max={3}
                value={ammo.crafting.workbenchLevel}
                onChange={e =>
                  onChange({
                    crafting: { ...ammo.crafting, workbenchLevel: parseInt(e.target.value, 10) || 1 },
                  })
                }
              />
            </Field>
            <Field label="Craft Time (seconds)" tooltip="Time in seconds to complete one craft.">
              <input
                className="input-field"
                type="number"
                value={ammo.crafting.craftTimeSeconds}
                onChange={e =>
                  onChange({
                    crafting: { ...ammo.crafting, craftTimeSeconds: parseInt(e.target.value, 10) || 0 },
                  })
                }
              />
            </Field>
            <Field label="Output Count" tooltip="Number of rounds produced per craft completion.">
              <input
                className="input-field"
                type="number"
                value={ammo.crafting.outputCount}
                onChange={e =>
                  onChange({
                    crafting: { ...ammo.crafting, outputCount: parseInt(e.target.value, 10) || 0 },
                  })
                }
              />
            </Field>
          </div>

          <div>
            <label className="label">Requirements</label>
            {ammo.crafting.requirements.map((req, i) => (
              <div key={i} className="flex gap-2 mb-2 items-start">
                <div className="flex-1 flex flex-col gap-1">
                  <SearchableSelect
                    value={req.tpl}
                    onChange={id => {
                      const next = [...ammo.crafting.requirements]
                      next[i] = { ...next[i], tpl: id }
                      onChange({ crafting: { ...ammo.crafting, requirements: next } })
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
                    const next = [...ammo.crafting.requirements]
                    next[i] = { ...next[i], count: parseInt(e.target.value, 10) || 0 }
                    onChange({ crafting: { ...ammo.crafting, requirements: next } })
                  }}
                />
                <button
                  className="btn-danger"
                  onClick={() => {
                    const next = ammo.crafting.requirements.filter((_, idx) => idx !== i)
                    onChange({ crafting: { ...ammo.crafting, requirements: next } })
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
                    ...ammo.crafting,
                    requirements: [...ammo.crafting.requirements, { tpl: '', count: 1 }],
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
        Optional magazine / weapon IDs whose filters should be patched to accept this ammo. Leave empty if the cloned base already shares filters.
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
        </Field>
      </div>
    </Section>
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

          <Field label="Base Ammo Box Template" className="md:col-span-2" tooltip="Existing ammo box to clone. Its model and stack slot count will be reused.">
            <select
              className="input-field"
              value={ammo.ammoBox.baseTpl}
              onChange={e => {
                const baseTpl = e.target.value
                const template = getAmmoBoxTemplate(baseTpl)
                updateBox({
                  baseTpl,
                  count: template?.count || ammo.ammoBox.count,
                })
              }}
            >
              <option value="">Select an ammo box template...</option>
              {AMMO_BOX_TEMPLATES.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.name} ({t.count} rounds) — {t.id}
                </option>
              ))}
            </select>
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
        </div>
      )}
    </Section>
  )
}

function LootTab({ ammo, onChange }: { ammo: AmmoDefinition; onChange: (u: Partial<AmmoDefinition>) => void }) {
  const updateLoot = (updates: Partial<LootEntry>) => {
    onChange({ loot: { ...ammo.loot, ...updates } })
  }

  return (
    <Section title="Loot Table Injection" icon={<MapPin size={18} />}>
      <div className="mb-4">
        <Toggle
          checked={ammo.loot.enabled}
          onChange={v => updateLoot({ enabled: v })}
          label="Add this ammo to container loot tables"
        />
      </div>

      {ammo.loot.enabled && (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Field
            label="Container IDs"
            className="md:col-span-2"
            tooltip="One container item ID per line. The ammo will be added as a possible loot spawn inside these containers (e.g., weapon crates, ammo boxes, duffle bags)."
          >
            <textarea
              className="input-field min-h-[120px] font-mono text-sm resize-y"
              value={ammo.loot.containerIds.join('\n')}
              onChange={e =>
                updateLoot({
                  containerIds: e.target.value.split('\n').map(s => s.trim()).filter(Boolean),
                })
              }
              placeholder="One 24-char container ID per line"
            />
          </Field>

          <Field label="Rarity" tooltip="Loot rarity for this ammo in the specified containers.">
            <select
              className="input-field"
              value={ammo.loot.rarity}
              onChange={e => updateLoot({ rarity: e.target.value })}
            >
              {RARITY_OPTIONS.map((r) => (
                <option key={r} value={r}>{r}</option>
              ))}
            </select>
          </Field>
        </div>
      )}
    </Section>
  )
}
