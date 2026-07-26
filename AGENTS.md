# BioRand RECV

## Project

Randomizer for Resident Evil Code: Veronica (PS2). Outputs full modified ISO.
Cross-platform .NET project — no Windows or Linux specific code, runs anywhere .NET is supported.

## Layout

- `src/BioRand.RECV/` — library (all randomization logic)
- `src/biorand-recv/` — CLI app
- `lib/biohazard-utils/` — submodule (RDT, AFS, ISO types)
- `lib/Ps2IsoTools/` — submodule (UDF ISO reader/writer)

## Architecture

Two-tier pipeline applied in order:

1. **ICvPatch** — unordered structural/ELF fixes
2. **ICvModifier** — ordered randomization (via `[Order]` attribute)

Pipeline always runs full cycle: open ISO → extract ELF + AFS → decompress RDTs → apply patches → apply modifiers → recompress → repack → rebuild

## Build & Run

```
dotnet build
dotnet run --project src/biorand-recv -- generate -i <input.iso> -c <config.json> -o <output.iso> [-c <config.json>] [--seed <s>]
dotnet run --project src/biorand-recv -- agent <url> -k <apikey> -i <input.iso>
```

- DO: set working directory to `generated_seeds/seed_<n>` **IMPORTANT!**
- DO: set `-o` to `generated_seeds/seed_<n>` to keep output organized.
- Use `src\BioRand.RECV\data\default-config.json` as base config example.

## Tests

```
dotnet test test/BioRand.RECV.Tests
```

8 tests — mix of small graph fixtures (50x each seed) + full graph (10 seeds). All 8 pass.
`RouteSolver.Solve()` has non-deterministic `possibleWays[0]` selection
(`RouteSolver.cs:90`) because `ImmutableHashSet<Edge>` iteration order depends on
`Edge.GetHashCode()` which uses `RuntimeHelpers.GetHashCode()` on `ImmutableArray`
backing arrays — varies between process runs.

## Config definition

`ReCvConfigurationDefinition.cs` — add new toggles/sliders here as features are added.

## Roadmap

See `docs/FEATURES.md` for feature status and `docs/ISSUES.md` for known issues with current implementations.

## Old randomizer

This is a fresh start from scratch. The old randomizer was here: /home/ted/repos/biorand-classic
It was a randomizer for RE 1, 2, 3, and CVX. You can see how all the features were implemented previously.

## Key randomizer

- `ReCvKeyRandomizer.cs` — builds GraphBuilder from graph.json, calls GenerateRoute
- Labels use `"ID|NAME"` format, rooms w/o name get `"ID|"` suffix
- Edge dedup per room: `HashSet<string>` on target ID, keeps first edge
- JSON deserialization uses `PropertyNamingPolicy = JsonNamingPolicy.CamelCase`, no `[JsonPropertyName]`
- Items use `item(N)` for key dependencies, `node(N)` for room prerequisites (NOT `flag()`)
- ALL 400 items become graph item nodes (key-placement slots). Items with `Requires.Length > 0` get locked edges; the 380 without requires are freely accessible slots.
- `ParseRequirements` handles `item`, `flag`, `room`, and `node` prefixes

## graph.json

165 rooms, 75 keys, 79 item types, 330 edges, 400 slots.
Room names populated from CSV callouts on 114 rooms (placed right after `id`).
Embedded as `IntelOrca.Biohazard.BioRand.RECV.data.graph.json`.
