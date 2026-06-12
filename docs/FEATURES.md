# BioRand RECV — Feature Roadmap

**Legend:** ✅ Done 🚧 In Progress ⬜ Pending

---

## ELF/Exe Patches

New `ICvPatch` classes in `src/BioRand.RECV/Patches/`. Each entry is a single-address NOP or byte write in `SLUS_201.84`.

- [x] Door Skip
- [ ] Disable Nosferatu Poison
- [ ] Fix Rifle Stacking
- [ ] Hack Item Pickup
- [ ] Randomize Item Pickup Quantities

---

## Item Randomization

New `ICvModifier` class + config toggles in `ReCvConfigurationDefinition.cs`. Requires item placement logic and progression graph generation.

- [ ] Randomize Item Placements
- [ ] Random Starting Inventory
- [ ] Item Quantity Multiplier
- [ ] Include Documents toggle

---

## Door Randomization

New `ICvModifier` class + room connection graph logic. Most complex feature — requires lock mapping, segmented generation, and fixed-link constraints.

- [ ] Randomize Doors
- [ ] Segmented Generation
- [ ] Lock System
- [ ] Fixed Links

---

## Script Patches

SCD patches applied as part of pipeline. Fixes softlocks and enables progression when items/doors are randomized. Applied via `ICvPatch` or dedicated script patching infrastructure.

- [ ] Keep Lighter after giving medicine to Rodrigo
- [ ] Fix ladder/silver key softlock
- [ ] Force Steve at airport
- [ ] Skip Steve/Alfred cutscene
- [ ] Prevent forced swap to Chris
- [ ] Force RDT 1021 version with briefcase
- [ ] Force RDT 1031 version with medal
- [ ] Change 4011 transition condition

---

## Cosmetics

Asset swaps in ADV.AFS, SYSTEM.AFS, and RDT textures.

- [ ] Randomize BGM
- [ ] Change Player Character (PLD swap)
- [ ] Randomize Title Screen
- [ ] Randomize Portraits

---

## Enemy Randomization

**Last priority** — historically crash-prone. New `ICvModifier` class + config. Enemy type limits and unique boss constraints required.

- [ ] Randomize Enemy Types
- [ ] Enemy Difficulty / Quantity / Room Count
- [ ] Allow Enemies Any Room
- [ ] Unique Enemy Types
- [ ] Randomize Enemy Placement
- [ ] Randomize Enemy Skins
