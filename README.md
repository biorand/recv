# BioRand RECV

Resident Evil Code: Veronica randomizer (cloud-based).

## Dependencies

- biohazard-utils (submodule at `lib/biohazard-utils`)
- Ps2IsoTools (submodule at `lib/Ps2IsoTools`)

## Build

```bash
git submodule update --init --recursive
dotnet build
```

## Usage

```bash
# Local generation
dotnet run --project src/biorand-recv -- generate -i recvx.iso -o recvx_biorand.iso --seed 0

# Cloud agent mode
dotnet run --project src/biorand-recv -- agent --base-uri https://api.example.com --api-key KEY
```
