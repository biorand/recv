---
name: implement-randomizer-feature
description: 'Implement a feature or fix an issue in the BioRand RECV randomizer following the full plan-review-implement-review-docs workflow'
argument-hint: 'Describe the issue or feature to implement. Include any constraints, related features, or preferred approach.'
---

# Implement Randomizer Feature — BioRand RECV

Full-stack workflow for adding features or fixing issues in the Resident Evil Code: Veronica randomizer project at `M:\git\recv`.

## When to Use

Use this skill whenever you need to:
- Add a new randomization feature (key randomizer, item randomizer, enemy placement, etc.)
- Fix a bug or known issue in the randomizer
- Extend an existing feature with new toggles/config options
- Refactor or improve randomizer pipeline code

Do NOT use for:
- CI/CD configuration changes
- Documentation-only updates
- Non-randomizer project maintenance

## Workflow

### 1. Understand the current state

Read `docs/FEATURES.md` to understand feature status and `docs/ISSUES.md` to find known bugs and limitations. These are the primary sources of truth for what needs work.

### 2. Plan the approach

Read the relevant source code to understand the area you'll be changing. Key project areas:
- `src/BioRand.RECV/` — library (all randomization logic, modifiers, patches, graph)
- `src/biorand-recv/` — CLI app
- `test/BioRand.RECV.Tests/` — xUnit tests
- `AGENTS.md` at repo root — project instructions with architecture details
- `docs/FEATURES.md` and `docs/ISSUES.md`

Create a plan.md session artifact in the workspace state directory (`C:\Users\Ted\.copilot\session-state\...\files\`) outlining:
- What you'll change and why
- Files to modify
- Test strategy
- How to update docs

### 3. Review the plan

Launch two review agents in parallel:
- **deepseek-v4-flash** (code-review agent) — reviews for bugs, security, logic errors
- **deepseek-v4-pro** (Refactor Reviewer) — reviews for clarity, consistency, maintainability

Provide them with the plan.md content and relevant context about the project's architecture (from `AGENTS.md`). Do NOT proceed to implementation until both reviews are complete and any blocking issues are resolved.

### 4. Implement

Apply the changes. Test iteratively as you go:

```powershell
dotnet build
dotnet test test/BioRand.RECV.Tests
```

All tests must pass. There are currently 22 tests (mix of small graph fixtures × 50 seeds each + full graph × 10 seeds + item pool tests).

Project-specific notes:
- Configuration toggles go in `ReCvConfigurationDefinition.cs`
- JSON deserialization uses `PropertyNamingPolicy = JsonNamingPolicy.CamelCase`, no `[JsonPropertyName]`
- Graph is embedded as `IntelOrca.Biohazard.BioRand.RECV.data.graph.json`
- Labels use `"ID|NAME"` format, rooms without names get `"ID|"` suffix
- `RouteSolver.Solve()` has non-deterministic `possibleWays[0]` selection at `RouteSolver.cs:90`
- Commit messages should include this trailer: `Co-authored-by: Copilot App <223556219+Copilot@users.noreply.github.com>`

### 5. Review the implementation

Launch two review agents in parallel:
- **deepseek-v4-flash** (code-review agent based on code-reviewer) — reads the diff, reports high-confidence bugs, security issues, logic errors
- **deepseek-v4-pro** (Refactor Reviewer) — suggests refactors for clarity, consistency, and maintainability

### 6. Apply fixes from reviews

Address all blocking/high-confidence issues reported by the review agents. Iterate on build + test as needed.

### 7. Behavior verification against original codebase

Review the old randomizer at `M:\git\biorand-classic` to verify the implementation matches the original behavior and intentions:
- Compare the logic against the original feature implementation (RE 1, 2, 3, or CVX equivalents)
- Check that no feature behavior was dropped, simplified, or forgotten
- Confirm the new implementation doesn't use completely different logic that might change gameplay outcomes
- Flag any discrepancies as issues to resolve before proceeding

### 8. Update documentation

Update `docs/FEATURES.md` to reflect any new, changed, or completed features.
Update `docs/ISSUES.md` to document any new known issues, or mark fixed issues as resolved.

### 9. Final validation

Run final build and test pass:

```powershell
dotnet build
dotnet test test/BioRand.RECV.Tests
```

Confirm all tests pass before considering the task complete.
