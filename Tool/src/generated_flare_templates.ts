// Generated from SPT database. Do not edit manually.
// Run: node scripts/generate-flare-stats.cjs

export interface FlareTemplate {
  id: string
  name: string
  shortName: string
}

export const HANDHELD_FLARE_TEMPLATES: FlareTemplate[] = [
  { id: '62178be9d0050232da3485d9', name: "ROP-30 reactive flare cartridge (White)", shortName: "Flare" },
  { id: '66d98233302686954b0c6f81', name: "RSP-30 reactive signal cartridge (Blue)", shortName: "Blue" },
  { id: '675ea3d6312c0a5c4e04e317', name: "RSP-30 reactive signal cartridge (Firework)", shortName: "Firework" },
  { id: '6217726288ed9f0845317459', name: "RSP-30 reactive signal cartridge (Green)", shortName: "Green" },
  { id: '62178c4d4ecf221597654e3d', name: "RSP-30 reactive signal cartridge (Red)", shortName: "Red" },
  { id: '66d9f1abb16d9aacf5068468', name: "RSP-30 reactive signal cartridge (Special Yellow)", shortName: "S-Yellow" },
  { id: '624c0b3340357b5f566e8766', name: "RSP-30 reactive signal cartridge (Yellow)", shortName: "Yellow" }
]

export const CARTRIDGE_TEMPLATES: FlareTemplate[] = [
  { id: '635267f063651329f75a4ee8', name: "26x75mm flare cartridge (Acid Green)", shortName: "AG" },
  { id: '62389bc9423ed1685422dc57', name: "26x75mm flare cartridge (White)", shortName: "Flare" },
  { id: '62389be94d5d474bf712e709', name: "26x75mm flare cartridge (Yellow)", shortName: "Yellow" },
  { id: '66d97834d2985e11480d5c1e', name: "Signal flare (Blue)", shortName: "Signal flare (Blue)" },
  { id: '675ea4891b2579e8fe0250aa', name: "Signal flare (New Year)", shortName: "Signal flare (New Year)" },
  { id: '624c09cfbc2e27219346d955', name: "Signal flare (Red)", shortName: "Signal flare (Red)" },
  { id: '66d9f3047b82b9a9aa055d81', name: "Signal flare (Special Yellow)", shortName: "Signal flare (Special Yellow)" },
  { id: '624c09da2cec124eb67c1046', name: "Signal flare (White)", shortName: "Signal flare (White)" },
  { id: '624c09e49b98e019a3315b66', name: "Signal flare (Yellow)", shortName: "Signal flare (Yellow)" }
]

export function getHandheldFlareTemplate(id: string): FlareTemplate | undefined {
  return HANDHELD_FLARE_TEMPLATES.find(t => t.id === id)
}

export function getCartridgeTemplate(id: string): FlareTemplate | undefined {
  return CARTRIDGE_TEMPLATES.find(t => t.id === id)
}
