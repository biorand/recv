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
- [x] Patch special-slot item in ELF (0x3340B0, 0x2A6CE8)
- [x] Patch first inventory item in ELF (0x2A6CF0)

---

## Item Randomization

`ICvModifier` classes + config toggles in `ReCvConfigurationDefinition.cs`. Uses embedded `graph.json` (165 rooms, 75 keys, 79 item types, 330 edges, 400 slots).

- [x] Randomize Key Items — `ItemModifier.KeyRouting()` via `ReCvKeyRandomizer` + graph routing
- [x] Randomize Non-Key Items — `ItemModifier.LootFilling()` with configurable distribution ratios
- [x] Apply Item Changes to RDTs — `ItemModifier.RdtEditing()` (AOT tables + byte offsets)
- [x] Item Quantity Multiplier — config slider (`items/quantity-multiplier`, 0–7)
- [x] Random Starting Inventory — `InventoryModifier` picks from 4 early-game weapon kinds (handgun, shotgun, bow-gun, knife), writes to ELF 0x2A6CF0, NOPs Steve cutscene in RDT 1030
- [x] Initial Key Items — classic starts Claire with Lighter; always applied via `InitialLighterPatch`
- [ ] Randomize Non-Key Weapon Placement — weapons currently excluded from loot pool; only 3 weapons (Shotgun, Gold Lugers, M1P) are in the keys array and get placed by routing; all other weapons stay in vanilla positions. Needs Weapon Group Deduplication to avoid placing 6 handgun variants.
- [ ] Ammo-to-Weapon Matching — exclude ammo types whose weapon was not placed
- [x] Include Documents toggle — config item (`items/allow-documents`) filters documents out of kind weights
- [x] Richer Item Pool — `ReCvItemPool` groups items per kind from graph.json, `Pick()` selects randomly from the pool (multi-item per kind with configurable ratio sliders)

---

## Seed Output

Assets emitted alongside the randomized ISO.

- [x] Key Hints HTML — `KeyHintsModifier` builds a `hints.html` asset listing every randomized key placement, ordered by the depth of its room (BFS) from the start room. Columns: Depth, Item, Room Id, Room Name, Global Item Id, Local Item Id. Rooms unreachable in the graph render `—` and sort last. Emitted as a `hints` output asset; the CLI writes it next to the ISO as `*.hints.html`.

---

## Door Randomization

`ICvModifier` class + room connection graph logic. Most complex feature — requires lock mapping, segmented generation, and fixed-link constraints. The classic randomizer has a full `DoorRandomizer` with `LockRandomizer`.

- [ ] Randomize Doors — config toggle (`doors/random`) exists but no implementation
- [ ] Segmented Generation
- [ ] Lock System
- [ ] Fixed Links

---

## Script Patches

SCD patches applied at the RDT level. Fixes softlocks and enables progression when items/doors are randomized. The classic randomizer applies these in `ReCvDoorHelper.Begin()`. Patches now live in `ReCvRdtPatcher` (`src/BioRand.RECV/Patches/ReCvRdtPatcher.cs`) using the `[RdtPatch]` attribute pattern from the classic `Re1RdtPatcher`, with byte-level SCD NOP (0xF4) writes via `ReCvRdtPatcherRoom`.

- [x] Keep Lighter after giving medicine to Rodrigo — `KeepLighterPatch` NOPs RDT 1000 (0x18CD7A, 0x18DB74)
- [x] Don't put Rodrigo's gift into special slot — `KeepLighterPatch` NOPs RDT 1000 (0x18CD74, 0x18DB7C) + RDT 8170 (0x14D26E, 0x14EB68)
- [x] Force RDT 1021 version with briefcase — `ReCvRdtPatcher.ForceBriefcaseVersion` (gated on `!doors/random`)
- [x] Force RDT 1031 version with medal — `ReCvRdtPatcher.ForceMedalVersion` (gated on `!doors/random`)
- [x] Force window cutscene on item interaction — `ReCvRdtPatcher.ForceWindowCutscene`
- [x] Skip Steve/Alfred cutscene — `ReCvRdtPatcher.SkipSteveAlfredCutscene`
- [x] Fix ladder/silver key softlock — `ReCvRdtPatcher.FixLadderSilverKeySoftlock` (gated on `!doors/random`)
- [x] Change 4011 transition condition — `ReCvRdtPatcher.FixTransitionCondition` (gated on `!doors/random`)
- [x] Force Steve at airport — `ReCvRdtPatcher.ForceSteveAtAirport`
- [ ] Prevent forced swap to Chris — RDT 70A0 (0x1F3140, gated on `doors/random`; pending door randomization implementation)
- [ ] Door randomization flag sets — RDTs 20E0, 5040, 4080, 6000, 8080, 80A0 (pending door randomization implementation)

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
