// Generated from SPT 4.0.13 database. Do not edit manually.
// Run: node scripts/generate-grenade-stats.cjs

export interface GrenadeTemplate {
  id: string
  name: string
  shortName: string
}

export const GRENADE_TEMPLATES: GrenadeTemplate[] = [
  { id: '5710c24ad2720bc3458b45a3', name: "F-1 hand grenade", shortName: "F-1" },
  { id: '67b49e7335dec48e3e05e057', name: "F-1 hand grenade (Reduced delay)", shortName: "F-1 RD" },
  { id: '617aa4dd8166f034d57de9c5', name: "M18 smoke grenade (Green)", shortName: "M18" },
  { id: '58d3db5386f77426186285a0', name: "M67 hand grenade", shortName: "M67" },
  { id: '619256e5f8af2c1a4e1f5d92', name: "Model 7290 Flash Bang grenade", shortName: "M7290" },
  { id: '5a2a57cfc4a2826c6e06d44a', name: "RDG-2B smoke grenade", shortName: "RDG-2B" },
  { id: '5448be9a4bdc2dfd2f8b456a', name: "RGD-5 hand grenade", shortName: "RGD-5" },
  { id: '617fd91e5539a84ec44ce155', name: "RGN hand grenade", shortName: "RGN" },
  { id: '618a431df1eb8e24b8741deb', name: "RGO hand grenade", shortName: "RGO" },
  { id: '66dae7cbeb28f0f96809f325', name: "V40 Mini-Grenade", shortName: "V40" },
  { id: '5e32f56fcb6d5863cc5e5ee4', name: "VOG-17 Khattabka improvised hand grenade", shortName: "VOG-17" },
  { id: '5e340dcdcb6d5863cc5e5efb', name: "VOG-25 Khattabka improvised hand grenade", shortName: "VOG-25" },
  { id: '5a0c27731526d80618476ac4', name: "Zarya stun grenade", shortName: "Zarya" },
]

export function getGrenadeTemplate(id: string): GrenadeTemplate | undefined {
  return GRENADE_TEMPLATES.find(t => t.id === id)
}
