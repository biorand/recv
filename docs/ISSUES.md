# BioRand RECV — Known Issues & Gaps

Issues found during comparison of the new codebase against the classic BioRand randomizer (`M:\git\biorand-classic`). These are deficiencies in **currently implemented** features — not missing features (those are in [FEATURES.md](FEATURES.md)).

---

## 1. RDT File Count Mismatch

**Severity:** High
**Files:** `ReCvRandomizerContext.cs` (`RdxFileNames` array)

The new codebase has ~142 RDT entries compared to the classic's 205. Most of stages 5, 6, 7, 8, and 9/A are incomplete:

| Stage | Classic | New | Status |
|-------|---------|-----|--------|
| 0 | 19 files | 19 files | ✅ |
| 1 | 19 files | 19 files | ✅ |
| 2 | 8 files | 8 files | ✅ |
| 3 | 26 files | 26 files | ✅ |
| 4 | 19 files | ~19 with gaps | ⚠️ |
| 5 | ~5 files | ~5 with gaps | ⚠️ |
| 6 | ~10 files | 1 file (6000) | ❌ |
| 7 | ~30 files | ~8 with gaps | ❌ |
| 8 | ~25 files | ~10 with gaps | ⚠️ |
| 9/A | ~42 files | ~20 with gaps | ⚠️ |

Missing variants include 4011, 4012, 5011, 6051, 7031, 7051, 7071, 7081, 7181, 7191, 7221, 7231, 8001, 8031, 9091, 9101–9103, 9301, 9302. This prevents correct item placement and key routing for the latter half of the game (Chris's section and Antarctica).

---

## 2. ~~`ItemModifier.FindItemIdByKind` Is Too Narrow~~ ✅ RESOLVED

**Resolution:** `FindItemIdByKind` was replaced with `ReCvItemPool` (in `src/BioRand.RECV/ReCvItemPool.cs`). The pool groups all items per kind from `graph.json` and `Pick(kind, rng)` selects randomly within each pool. Kind-level distribution weights are configured via ratio sliders (`items/ratio/{kind}`) in `BuildKindWeights()`. This matches the classic's richer weighted-item-pool system.

---

## 3. Weapons Excluded from Non-Key Randomization

**Severity:** Medium
**Files:** `src/BioRand.RECV/Modifiers/ItemModifier.cs` (`BuildKindWeights`)

Line 212 skips all `weapon/` items: `if (kind.StartsWith("key/") || kind.StartsWith("weapon/")) continue;`. Only 3 weapons (Shotgun, Gold Lugers, M1P) are in the graph.json keys array and get placed by the key routing system. All other weapons remain at their vanilla positions — they are never randomized.

The classic randomizer includes all 12 weapons in `GetWeapons()` and shuffles them into available item slots, with a group-deduplication check (`WeaponInfo.Group`) to prevent placing multiple weapons of the same Kind group. If weapons are added to the non-key pool, similar deduplication should be added to avoid placing 6 different handgun variants in one seed.

---

## 4. No Ammo-to-Weapon Matching

**Severity:** Medium
**Files:** `src/BioRand.RECV/Modifiers/ItemModifier.cs`

The classic randomizer checks which weapons were placed and disables ammo types for weapons that were not placed (ItemRandomizer.cs:68-78: `if (definition.Kind.StartsWith("ammo/")) { if (!wpplaced.Any(x => x.Enabled && x.SupportsAmmo(itemType))) weights[i].Item3 = 0; }`). The new codebase does not check whether the corresponding weapon exists, which can result in dead ammo pickups with no matching weapon in the seed.

---

## 5. ~~Missing ELF Patches for Initial Game State~~ ✅ RESOLVED

**Resolution:** `InitialLighterPatch` (in `src/BioRand.RECV/Patches/InitialLighterPatch.cs`) now patches the ELF at offsets 0x3340B0, 0x2A6CE8 (special slot) and 0x2A6CF0 (first inventory slot) with the Lighter item ID (0x37). `KeepLighterPatch` (in `src/BioRand.RECV/Patches/KeepLighterPatch.cs`) NOPs the Rodrigo scene scripts so the Lighter is preserved after giving medicine.

---

## 6. ~~No Initial Key Items (Lighter)~~ ✅ RESOLVED

**Resolution:** The Lighter is now always placed in the starting inventory via `InitialLighterPatch` (ELF patches). The `KeepLighterPatch` prevents it from being removed during the Rodrigo medicine scene. The lighter's slot (globalId 1001) has `tags: ["nokey", "nospecial"]` in graph.json, and both `ReCvKeyRandomizer` and `ItemModifier.LootFilling()` respect the `nokey` tag.

---

## 7. No `ReCvEnemyIds` Equivalent

**Severity:** Low (enemy randomization not yet implemented)
**Files:** No corresponding code exists

The classic randomizer has a complete enemy ID enumeration for RECV (`ReCvEnemyIds.cs`). The new codebase has no enemy-related types at all. This will be needed when enemy randomization is implemented.