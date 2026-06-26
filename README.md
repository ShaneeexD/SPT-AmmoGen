# AmmoGen

A server-side framework for SPTarkov that lets players and mod authors add custom ammo to the game by dropping simple JSON packs into a folder. No bundles, no client plugin, no C# required for users.

## What it does

- Clones an existing ammo template and overrides its stats, name, price, etc.
- Registers the new ammo in the item database with locales and handbook entries.
- Adds the ammo to vanilla trader assortments (optional).
- Adds a hideout workbench crafting recipe (optional).
- Patches magazine / weapon filters so the ammo can be loaded (optional).

## Project Structure

```
AmmoGen/
├── Server/                 # SPT server mod
│   ├── AmmoGen.csproj
│   ├── AmmoGenPlugin.cs
│   ├── Models/             # JSON schema models
│   ├── Services/           # Loading, registration, traders, crafting
│   ├── Validation/         # Pack validation
│   ├── config/
│   │   └── config.json
│   └── package.json
├── Tool/                   # Online tool for generating ammo packs
│   ├── src/
│   │   ├── App.tsx
│   │   ├── types.ts
│   │   └── ...
│   ├── index.html
│   ├── package.json
│   └── vite.config.ts
├── ammo/                   # User ammo packs go here
│   └── example-pack.json
└── SPT-Mod-Template.sln
```

## Building

1. Open `SPT-Mod-Template.sln` in Visual Studio / Rider (the solution now contains only the Server project).
2. Build the solution.
3. The post-build target copies the DLL, `package.json`, and `config/` to `C:\SPT\SPT\user\mods\Serenity-AmmoGen`.

## How Users Create Ammo Packs

Users can either write JSON manually or use the `Tool/` web app.

### Using the Tool

```bash
cd Tool
npm install
npm run dev
```

The tool lets you pick a base ammo, set stats, configure one or more vanilla traders, configure a craft, and download the JSON pack.

### Manual JSON

Place a JSON file in `SPT/user/mods/AmmoGen/ammo/` (or a subfolder). See `ammo/example-pack.json` for a complete example.

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
      }
    }
  ]
}
```

## Ammo Pack Fields

| Field | Description |
|-------|-------------|
| `id` | Unique 24-character hex item ID for the new ammo. |
| `baseTpl` | Existing ammo template to clone. |
| `name` / `shortName` / `description` | Locale strings. |
| `handbookParentId` | Optional handbook category. Defaults to the base ammo's category. |
| `stats` | Damage, penetration, armor damage, initial speed, accuracy, recoil, **stack max size**. |
| `economy` | Handbook price, flea price, PvE rarity. |
| `traders` | Array of optional vanilla trader listings. |
| `crafting` | Optional workbench recipe with searchable item requirements. |
| `filters` | Optional magazine / weapon IDs to patch. |

## Vanilla Trader IDs

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

## Notes

- No bundles are included or required. The cloned ammo uses the base ammo's existing model/texture.
- The mod is server-only; no client plugin is needed.
- Packs are validated at load time. Invalid packs are logged and skipped.

## License

MIT
