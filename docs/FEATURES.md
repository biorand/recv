# BioRand RECV — Feature Roadmap

**Legend:** ✅ Done 🚧 In Progress ⬜ Pending

Comparison baseline: the classic BioRand randomizer at `M:\git\biorand-classic`.
Known issues and gaps with current implementations are tracked in [ISSUES.md](ISSUES.md).

---

## ELF/Exe Patches

`ICvPatch` classes in `src/BioRand.RECV/Patches/`. Each entry is a single-address NOP or byte write in `SLUS_201.84`.

- [x] Door Skip — `DoorSkipPatch` (NOPs at 0x133D4C, 0x133D54)
- [x] Disable Nosferatu Poison — `NosferatuPoisonPatch` (0x1E6DC4; ⚠️ gated on `doors/random`)
- [x] Fix Rifle Stacking — `RifleStackingPatch` (0x35B1F4, 0x35B200)
- [x] Hack Item Pickup — `ItemPickupPatch` (0x266E30 → 0x06)
- [x] Randomize Item Pickup Quantities — `ItemQuantityModifier` (ammo table at 0x35BCC0)
- [ ] Patch special-slot item in ELF (0x3340B0, 0x2A6CE8)
- [ ] Patch first inventory item in ELF (0x2A6CF0)

---

## Item Randomization

`ICvModifier` classes + config toggles in `ReCvConfigurationDefinition.cs`. Uses embedded `graph.json` (165 rooms, 75 keys, 79 item types, 330 edges, 400 slots).

- [x] Randomize Key Items — `ItemModifier.KeyRouting()` via `ReCvKeyRandomizer` + graph routing
- [x] Randomize Non-Key Items — `ItemModifier.LootFilling()` with configurable distribution ratios
- [x] Apply Item Changes to RDTs — `ItemModifier.RdtEditing()` (AOT tables + byte offsets)
- [x] Item Quantity Multiplier — config slider (`items/quantity-multiplier`, 0–7)
- [ ] Random Starting Inventory — needs `InventoryModifier` (weapon selection, health/ink, ELF patches)
- [ ] Initial Key Items — classic starts Claire with Lighter; not implemented
- [ ] Randomize Non-Key Weapon Placement — weapons currently excluded from loot pool; only 3 weapons (Shotgun, Gold Lugers, M1P) are in the keys array and get placed by routing; all other weapons stay in vanilla positions. Needs Weapon Group Deduplication to avoid placing 6 handgun variants.
- [ ] Ammo-to-Weapon Matching — exclude ammo types whose weapon was not placed
- [x] Include Documents toggle — config item (`items/allow-documents`) filters documents out of kind weights
- [x] Richer Item Pool — `ReCvItemPool` groups items per kind from graph.json, `Pick()` selects randomly from the pool (multi-item per kind with configurable ratio sliders)

---

## Door Randomization

`ICvModifier` class + room connection graph logic. Most complex feature — requires lock mapping, segmented generation, and fixed-link constraints. The classic randomizer has a full `DoorRandomizer` with `LockRandomizer`.

- [ ] Randomize Doors — config toggle (`doors/random`) exists but no implementation
- [ ] Segmented Generation
- [ ] Lock System
- [ ] Fixed Links

---

## Script Patches

SCD patches applied at the RDT level. Fixes softlocks and enables progression when items/doors are randomized. The classic randomizer applies these in `ReCvDoorHelper.Begin()`. **None currently implemented** — this is the highest-priority gap because it unblocks door and inventory randomization.

- [ ] Keep Lighter after giving medicine to Rodrigo — RDT 1000 (0x18CD7A, 0x18DB74)
- [ ] Don't put Rodrigo's gift into special slot — RDT 1000 (0x18CD74, 0x18DB7C) + RDT 8170 (0x14D26E, 0x14EB68)
- [ ] Force RDT 1021 version with briefcase — RDT 1010 (0x3EF2C, 0x3EF38–0x3EF4C, 0x3EF50–0x3EF5A)
- [ ] Force RDT 1031 version with medal — RDT 1050 (0x1DF2AA–0x1DF2BE, 0x1DF2C2–0x1DF2CC)
- [ ] Force window cutscene on item interaction — RDT 1070 (0x1819AE)
- [ ] Skip Steve/Alfred cutscene — RDT 3050 (0x15F288, 0x15F2DA, 0x15EEDC, 0x15EEF6)
- [ ] Fix ladder/silver key softlock — RDT 3060 (0x70A10+6 → 0x00)
- [ ] Change 4011 transition condition — RDT 4080 (0x9F86C+2 → 0xC5), RDT 40F0 (0x7241C+2 → 0xC5)
- [ ] Force Steve at airport — RDT 5000 (0x187778, 0x18777A, 0x187784, 0x18779C)
- [ ] Prevent forced swap to Chris — RDT 70A0 (0x1F3140)
- [ ] Door randomization flag sets — RDTs 20E0, 5040, 4080, 6000, 8080, 80A0

---

## Cosmetics

Asset swaps in ADV.AFS, SYSTEM.AFS, and RDT textures. Classic had PLD swap and portrait randomization working for RECV.

- [ ] Change Player Character — PLD swap in SYSTEM.AFS (indices 10, 11)
- [ ] Randomize Portraits — RDT texture group replacement from portrait data files
- [ ] Randomize Title Screen — background replacement in ADV.AFS index 2
- [ ] Randomize BGM — was disabled in classic too; framework existed

---

## Enemy Randomization

**Last priority** — historically crash-prone. `ICvModifier` class + config. Enemy type limits and unique boss constraints required. The classic randomizer has `EnemyRandomizer` with full placement, skin, and type-limit support. No `ReCvEnemyIds` exists in the new codebase yet.

- [ ] Randomize Enemy Types
- [ ] Enemy Difficulty / Quantity / Room Count
- [ ] Allow Enemies Any Room
- [ ] Unique Enemy Types (boss constraints)
- [ ] Randomize Enemy Placement
- [ ] Randomize Enemy Skins
