# Spec Sync Agent

A .NET 8 console application that uses Claude to automatically generate and update specification documents from a git diff and a user story.

## What It Does

Given a code diff and a user story, the agent runs a four-stage pipeline:

1. **Load** — reads the diff, user story, and any existing spec files in parallel
2. **Summarize** — `DiffSummaryAgent` converts the raw diff into plain-English bullet points
3. **Evaluate** — `EvaluationAgent` checks whether the diff actually implements the user story, flagging gaps or mismatches
4. **Write** — three agents run in parallel to update:
   - `Specs/functional-spec.md` — what the system does, from a product perspective
   - `Specs/technical-spec.md` — how the system does it, from an engineering perspective
   - `CLAUDE.md` — developer context file for Claude Code sessions

All agents call `claude -p` under the hood via `BaseAgent`, which pipes a combined system + user prompt to the CLI and returns the response.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Claude Code CLI](https://docs.anthropic.com/en/docs/claude-code) installed and authenticated (`claude -p` must work in your terminal)

## Running

```bash
# Build
dotnet build AgentAssignment/

# Run the demo pipeline (3 pre-built stories, press Enter between each)
dotnet run --project AgentAssignment/
```

The demo is designed to be run as a pipeline — it processes three user stories in sequence, each going through the full four-stage pipeline (load → summarize → evaluate → write). Press Enter between stories to step through them and review the output at each stage:

| Story | Diff file | User story file |
|-------|-----------|-----------------|
| 1 — Add a User | `Specs/story-1.diff` | `Specs/stories/story-1-add-user.md` |
| 2 — Get Enterprise & Update User | `Specs/story-2.diff` | `Specs/stories/story-2-enterprise-and-user.md` |
| 3 — Update User Status | `Specs/story-3.diff` | `Specs/stories/story-3-update-user-status.md` |

After each run, the three output files are updated in place.

## Project Structure

```
AgentAssignment/
├── Agents/
│   ├── BaseAgent.cs            # Shells out to `claude -p`; all agents inherit this
│   ├── DiffSummaryAgent.cs     # Stage 2: plain-English diff summary
│   ├── EvaluationAgent.cs      # Stage 3: validates diff against user story
│   ├── FunctionalSpecAgent.cs  # Stage 4: writes/updates functional-spec.md
│   ├── TechnicalSpecAgent.cs   # Stage 4: writes/updates technical-spec.md
│   └── ClaudeMdAgent.cs        # Stage 4: writes/updates CLAUDE.md
├── Orchestrator/
│   └── SpecOrchestrator.cs     # Wires up the four-stage pipeline
├── Services/
│   ├── GitDiffService.cs       # Reads a .diff file; falls back to `git diff HEAD~1 HEAD`
│   └── FileService.cs          # Reads/writes spec files relative to project root
├── Specs/
│   ├── stories/                # Input user story markdown files
│   ├── *.diff                  # Pre-saved diffs for the demo stories
│   ├── functional-spec.md      # Generated output
│   └── technical-spec.md       # Generated output
└── Program.cs                  # Entry point; runs the three demo stories
```

## How the Pipeline Works

```
[diff file]  [user story]  [existing specs]
      \            |            /
       \           |           /
        --- LoadInputsAsync ---         (parallel reads)
                   |
           DiffSummaryAgent             (claude -p)
                   |
           EvaluationAgent              (claude -p)
                   |
     +-------------+-------------+
     |             |             |
FunctionalSpec  TechnicalSpec  ClaudeMd  (parallel, claude -p each)
     |             |             |
     +-------------+-------------+
                   |
           write all three files        (parallel writes)
```

## Key Design Decisions

- **Agents are stateless** — each is instantiated fresh per run.
- **Parallel by default** — independent I/O and agent calls use `Task.WhenAll`.
- **Diff source priority** — `GitDiffService` uses the provided `.diff` file if it exists; otherwise falls back to `git diff HEAD~1 HEAD`, then `git diff HEAD`.
- **Incremental updates** — existing spec files are read and passed to agents so content is amended rather than overwritten from scratch.
