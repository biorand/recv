# BioRand RECV

## Project

Randomizer for Resident Evil Code: Veronica (PS2). Outputs full modified ISO.

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
dotnet run --project src/biorand-recv -- generate -i <input.iso> -o <output.iso> [--seed <s>]
dotnet run --project src/biorand-recv -- agent <url> -k <apikey> -i <input.iso>
```

## Config definition

`ReCvConfigurationDefinition.cs` — add new toggles/sliders here as features are added.

## Roadmap

See `docs/FEATURES.md`

## Old randomizer

This is a fresh start from scratch. The old randomizer was here: /home/ted/repos/biorand-classic
It was a randomizer for RE 1, 2, 3, and CVX. You can see how all the features were implemented previously.
