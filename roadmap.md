# AmmoGen Roadmap

This is a living list of planned features and improvements for the AmmoGen mod and tool.

## Implemented

- [x] Ammo pack JSON editor with live preview
- [x] Clone base ammo template and override stats
- [x] Trader, crafting, economy, and filter configuration
- [x] Tooltips on all major fields
- [x] Auto-fill stats when selecting a base ammo template
- [x] Searchable item picker for crafting requirements
- [x] Hamburger sidebar with links to related tools
- [x] Custom favicon and dark theme
- [x] Auto-suggest compatible magazines and weapons for filter patching
- [x] Ammo stats comparison panel (custom vs base)
- [x] Export full `user/mods/AmmoGen/` folder as a ZIP
- [x] Optional generated ammo boxes that contain the custom ammo
- [x] Optional loot table injection into container distributions
- [x] `stackMaxSize` override

## Future Ideas

### Ammo Variants (Phase 1)
Support creating tracer, subsonic, armor-piercing, and hollow-point variants from the same base ammo with one click. Each variant could inherit the base stats and apply preset modifiers (e.g. AP: +pen, -dmg). This needs:
- Variant presets in the data model
- A "Create Variant" button in the tool
- Server support for variant-specific visual/sound properties (tracer color, muzzle flash, etc.)

### Weapon / Magazine Compatibility Browser
Show a read-only list of every magazine and weapon that accepts the selected base ammo, with names and IDs. This would make the auto-suggest feature even more useful and help users decide what to patch manually.

### Damage & Penetration Visualizer
A small graph or table showing estimated damage and penetration against each armor class (1–6). This helps balance new ammo without launching the game.

### Local Pack Presets / Library
Save and load partial pack templates (e.g., "balanced 5.45 pack", "high-pen sniper ammo") to local storage. This speeds up creating families of related ammo.

### Ammo Family Generator
Create a set of related ammo types from one base template (e.g., FMJ → AP → Tracer) with incremental stat changes and matching names.

### Import from Existing Ammo Pack
Open another AmmoGen pack to remix or extend it.

### In-Game Validation / Playtest Helper
Generate a quick test command or config snippet to give the player a stack of each custom ammo and a compatible weapon for fast in-game verification.

### Multi-Language Locale Support
Allow exporting ammo packs with names and descriptions for multiple languages, not just English.

### Mod Conflict Checker
Warn if generated ammo or ammo box IDs collide with known vanilla items or other popular mods.
