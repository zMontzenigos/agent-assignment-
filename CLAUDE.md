The write was blocked by permissions. Here is the full CLAUDE.md content — you can create the file at `/Users/zacharymontzenigos/repos/agent-assignment-/CLAUDE.md` (one level above `AgentAssignment/`, which is where the orchestrator writes it via `../CLAUDE.md`):

```markdown
# AgentAssignment — Spec Sync Agent

## Commands

```bash
# Build
dotnet build AgentAssignment/

# Run (from repo root — requires a user story file)
dotnet run --project AgentAssignment/

# Run against a specific story file
STORY_FILE=Specs/my-story.md dotnet run --project AgentAssignment/

# Run against a saved diff instead of git history
DIFF_FILE=Specs/sample.diff dotnet run --project AgentAssignment/
```

Prerequisites: `claude` CLI must be on `PATH` and authenticated (`claude -p` must work).

## Architecture

Three-stage pipeline orchestrated by `SpecOrchestrator`:

1. **Load inputs** (parallel) — read git diff, user story, and any existing specs
2. **Summarize diff** — `DiffSummaryAgent` condenses the raw diff into plain English
3. **Write docs** (parallel) — three agents update documents simultaneously:
   - `FunctionalSpecAgent` → `Specs/functional-spec.md`
   - `TechnicalSpecAgent` → `Specs/technical-spec.md`
   - `ClaudeMdAgent` → `../CLAUDE.md` (this file)

All agents extend `BaseAgent`, which shells out to `claude -p` with a combined system + user prompt.

## Key Files

| Path | Purpose |
|------|---------|
| `AgentAssignment/Orchestrator/SpecOrchestrator.cs` | Top-level pipeline: load → summarize → write |
| `AgentAssignment/Agents/BaseAgent.cs` | Shells out to `claude -p`; all agents inherit from this |
| `AgentAssignment/Agents/DiffSummaryAgent.cs` | Converts raw git diff to plain-English summary |
| `AgentAssignment/Agents/FunctionalSpecAgent.cs` | Writes/updates the functional spec |
| `AgentAssignment/Agents/TechnicalSpecAgent.cs` | Writes/updates the technical spec |
| `AgentAssignment/Agents/ClaudeMdAgent.cs` | Writes/updates this CLAUDE.md |
| `AgentAssignment/Services/GitDiffService.cs` | Reads `git diff HEAD~1 HEAD`; falls back to `DIFF_FILE` env var |
| `AgentAssignment/Services/FileService.cs` | Reads/writes spec files relative to project root |
| `AgentAssignment/Program.cs` | Entry point; wires up services and runs orchestrator |
| `AgentAssignment/Sample/PasswordResetService.cs` | Demo codebase used as pipeline input during development |

## Conventions

- **Agents are stateless** — each is instantiated fresh per run, no shared state.
- **Env vars over config files** — `STORY_FILE` and `DIFF_FILE` are the only runtime knobs.
- **Parallel by default** — independent I/O and agent calls use `Task.WhenAll`.
- **`BaseAgent.RunAsync`** takes `(systemPrompt, userContent)` and returns the raw string output from Claude; agents are responsible for formatting their own prompts.
- **Output paths are relative to project root** (`baseDir` in `FileService`), resolved from `AppContext.BaseDirectory` at startup.
- **`Sample/`** holds demo source files for testing the pipeline; it is not part of the agent infrastructure.
```

A few notes on what I included and why:

- The diff summary describes changes to `Sample/PasswordResetService.cs` — the demo codebase the pipeline runs against, not the pipeline itself. Since there was no existing CLAUDE.md, I documented the **agent pipeline** (the actual project), not the sample code.
- `Sample/PasswordResetService.cs` is called out in Key Files so future sessions understand its role.
- The `DIFF_FILE` env var is documented since `Specs/sample.diff` already exists in the repo and is clearly meant for demo/dev use.