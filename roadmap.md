# AmmoGen Roadmap

A living list of what AmmoGen can do today and what is planned for future updates.

For setup, usage, and JSON examples, see [`README.md`](README.md).

---

## Implemented

- **Ammo pack JSON editor** with live preview and validation.
- **Clone base ammo template** and override stats, stack size, economy, and name.
- **Vanilla trader integration** with optional ammo box listings.
- **Workbench crafting** with a searchable item requirement picker.
- **Filter patching** to make custom ammo fit existing magazines and weapons.
- **Generated ammo boxes** that contain the custom ammo and can be sold by traders.
- **Loot table injection** into container distributions (ammo, box, or both).
- **Auto-fill stats** when selecting a base ammo template.
- **Ammo stats comparison** panel (custom vs base).
- **Export / import** packs as JSON or ready-to-install ZIP.
- **Tooltips** on every major field.
- **Dark theme** and responsive layout.
- **Custom background colors** for ammo, grenades, flares, and ammo boxes with an opacity slider.
- **Crowd-sourced Mod Patches** tab in the tool for grouping and merging custom weapon/magazine filter patches.
- **Built-in ammo compatibility patches** for MTs-255-12, Chiappa Rhino, RSh-12, and related magazines.

---

## Future Ideas

### Ammo Variants (Phase 1)
Support creating tracer, subsonic, armor-piercing, and hollow-point variants from the same base ammo with one click. Each variant would inherit the base stats and apply preset modifiers (e.g. AP: +pen, -dmg). This needs:
- Variant presets in the data model.
- A "Create Variant" button in the tool.
- Server support for variant-specific visual/sound properties (tracer color, muzzle flash, etc.).

### Weapon / Magazine Compatibility Browser
Show a read-only list of every magazine and weapon that accepts the selected base ammo, with names and IDs. This would make the auto-suggest feature even more useful and help users decide what to patch manually.

### Damage & Penetration Visualizer
A small graph or table showing estimated damage and penetration against each armor class (1–6). This helps balance new ammo without launching the game.

### Local Pack Presets / Library
Save and load partial pack templates (e.g., "balanced 5.45 pack", "high-pen sniper ammo") to local storage. This speeds up creating families of related ammo.

### Ammo Family Generator
Create a set of related ammo types from one base template (e.g., FMJ → AP → Tracer) with incremental stat changes and matching names.

### In-Game Validation / Playtest Helper
Generate a quick test command or config snippet to give the player a stack of each custom ammo and a compatible weapon for fast in-game verification.

### Multi-Language Locale Support
Allow exporting ammo packs with names and descriptions for multiple languages, not just English.

### Mod Conflict Checker
Warn if generated ammo or ammo box IDs collide with known vanilla items or other popular mods.
