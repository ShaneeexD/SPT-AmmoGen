# AmmoGen - Custom Ammo Framework for SPTarkov 4.0.13

A server-side framework for SPTarkov that lets anyone, including non-programmers, create custom ammo, ammo boxes, and loot injections using simple JSON packs. Includes a web-based **AmmoGen Tool** for generating packs visually.

**Tool**: [AmmoGen](https://ammogen-tool.netlify.app).

> **New**: The tool supports exporting a ready-to-install ZIP and importing existing packs (JSON or ZIP) for editing.

---

## Quick Start

### For Players (Installing an Ammo Pack)

1. Install the AmmoGen mod by dragging the `SPT/` folder into your SPT install directory.
2. Place the ammo pack file into `SPT/user/mods/AmmoGen/ammo/` (or a subfolder).
3. Start the SPT server — the custom ammo appears automatically in traders, crafting, and loot.

### For Creators (Making an Ammo Pack)

1. Open the [AmmoGen Tool](https://ammogen-tool.netlify.app).
2. Pick a base ammo template, set stats, name/description, and configure traders, crafting, ammo boxes, and loot.
3. Click **Export** to download a ready-to-use ZIP.
4. Extract the ZIP and drag the `SPT/` folder into your SPT install directory to test.
5. Publish your pack - users need **AmmoGen** installed as a dependency.

> **Tip**: Always test on a new developer profile so you can verify trader purchases, crafting, and loot without affecting your main save.

---

## Export Format

The AmmoGen Tool exports a pre-packaged ZIP that drops straight into any SPT install:

```
SPT/
└── user/
    └── mods/
        └── AmmoGen/
            └── ammo/
                └── my-ammo-pack.json
```

You can also click **Export JSON** to download a single `.json` file and place it manually in `SPT/user/mods/AmmoGen/ammo/`.

---

## Features

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

See [`roadmap.md`](roadmap.md) for upcoming features and planned improvements.

---

## Ammo Pack JSON

A pack is a single JSON file containing one or more custom ammo definitions.

```json
{
  "enabled": true,
  "name": "My Ammo Pack",
  "ammo": [
    {
      "id": "010000000000000000000001",
      "enabled": true,
      "baseTpl": "59e655cb86f77411dc52a77b",
      "name": "Custom Ammo",
      "shortName": "Custom",
      "description": "A custom ammo cloned from .366 TKM EKO.",
      "handbookParentId": "5b47574386f77428ca22b33b",
      "stats": {
        "damage": 110,
        "penetration": 45,
        "armorDamage": 55,
        "initialSpeed": 850,
        "ammoAccr": 2,
        "ammoRec": -5,
        "stackMaxSize": 30
      },
      "economy": {
        "handbookPriceRoubles": 1200,
        "fleaPriceRoubles": 1400,
        "rarityPvE": "Rare"
      },
      "traders": [
        {
          "enabled": true,
          "traderId": "54cb50c76803fa8b248b4571",
          "loyaltyLevel": 2,
          "priceRoubles": 900,
          "stockCount": 200,
          "buyRestrictionMax": 200
        }
      ],
      "crafting": {
        "enabled": true,
        "workbenchLevel": 2,
        "craftTimeSeconds": 10800,
        "outputCount": 100,
        "requirements": [
          { "tpl": "59e655cb86f77411dc52a77b", "count": 60 },
          { "tpl": "590c5a7286f7747884343aea", "count": 2 }
        ]
      },
      "filters": {
        "patchMagazines": [],
        "patchWeapons": []
      },
      "ammoBox": {
        "id": "020000000000000000000001",
        "enabled": true,
        "baseTpl": "543be5cb4bdc2deb348b4568",
        "count": 50,
        "name": "Box of Custom Ammo",
        "shortName": "Custom Ammo Box",
        "description": "A box containing 50 rounds of custom ammo.",
        "handbookPriceRoubles": 60000,
        "rarityPvE": "Rare",
        "sellToTraders": true,
        "traderPriceRoubles": 45000
      },
      "loot": {
        "enabled": true,
        "containerIds": ["578f8782245977355405de3a"],
        "rarity": "Rare",
        "lootItem": "ammo"
      }
    }
  ]
}
```

Find item template IDs at: https://db.sp-tarkov.com/search

---

## Field Reference

| Field | Description |
|-------|-------------|
| `id` | Unique 24-character hex item ID for the new ammo. |
| `baseTpl` | Existing ammo template to clone. |
| `name` / `shortName` / `description` | Locale strings shown in-game. |
| `handbookParentId` | Optional handbook category. Defaults to the base ammo's category. |
| `stats` | Damage, penetration, armor damage, initial speed, accuracy, recoil, and stack max size. |
| `economy` | Handbook price, flea price, and PvE rarity. |
| `traders` | Optional vanilla trader listings. |
| `crafting` | Optional workbench recipe with item requirements. |
| `filters` | Optional magazine / weapon IDs to patch so the ammo fits. |
| `ammoBox` | Optional custom ammo box containing this ammo. |
| `loot` | Optional loot table injection for containers. |

### Vanilla Trader IDs

| Trader | ID |
|--------|-----|
| Prapor | `54cb50c76803fa8b248b4571` |
| Therapist | `54cb57776803fa99248b456e` |
| Skier | `58330581ace78e27b8b10cee` |
| Peacekeeper | `5935c25fb3acc3127c3d8cd9` |
| Fence | `579dc571d53a0658a154fbec` |
| Mechanic | `5a7c2eca46aef81a7ca2145d` |
| Ragman | `5ac3b934156ae10c4430e83c` |
| Jaeger | `5c0647fdd443bc2504c2d371` |
| Caretaker | `638f541a29ffd1183d187f57` |
| BTR | `656f0f98d80a697f855d34b1` |
| Arena | `6617beeaa9cfa777ca915b7c` |
| Storyteller | `6864e812f9fe664cb8b8e152` |

---

## Validation & Error Handling

AmmoGen validates every pack on load and logs clear errors to the server console:

- Missing required fields (`id`, `baseTpl`, `name`, `stats`, etc.)
- Invalid ID format (must be 24-character hex)
- Invalid trader IDs
- Invalid crafting requirement template IDs
- Invalid ammo box base templates

Invalid packs are **skipped** — other packs still load normally.

---

## Publishing an Ammo Pack

The AmmoGen Tool export ZIP is already structured for distribution. When publishing:

1. **State the dependency**: Your pack requires AmmoGen for SPT 4.0.13.
2. **Do not include** the AmmoGen DLL or other authors' packs in your ZIP.
3. **Test** by extracting and running the server before publishing.

---

## Technical Details

- **SPT Version**: 4.0.13
- **Framework**: .NET 9.0, C#
- **DI Pattern**: `[Injectable]` + `IOnLoad`
- **NuGet Packages**: `SPTarkov.Common`, `SPTarkov.DI`, `SPTarkov.Server.Core` (4.0.13)
- **Tool**: React + TypeScript + Vite + TailwindCSS
- **No bundles or client plugin required**: cloned ammo uses the base ammo's existing model/texture.

## License

MIT — Use freely for your SPT mods.
