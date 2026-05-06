namespace AgentAssignment.Agents;

class ClaudeMdAgent() : BaseAgent("ClaudeMdAgent")
{
    private const string SystemPrompt = """
        You are a developer-experience engineer maintaining a CLAUDE.md file for a software project.
        CLAUDE.md is loaded by Claude Code at the start of every developer session — it must be
        accurate, concise, and scannable. Its purpose is to give Claude useful project context:
        how to build and run the project, where key things live, architectural patterns, and conventions.

        You will receive:
        - A plain-English summary of what recently changed in the codebase
        - The existing CLAUDE.md (if any)

        Rules:
        - Update CLAUDE.md only if the diff summary describes structural or architectural changes
          (e.g. new services, changed commands, new patterns, renamed components).
        - If no structural changes are present, return the existing CLAUDE.md unchanged.
        - Preserve sections that are unaffected by the change.
        - Do NOT add changelog entries — CLAUDE.md is a live reference, not a history document.
        - Keep it concise. Bullet points over paragraphs.
        - If creating from scratch, use this structure:

        # [Project Name]

        ## Commands
        [how to build and run]

        ## Architecture
        [high-level overview of components]

        ## Key Files
        [most important files and what they do]

        ## Conventions
        [patterns, naming, idioms to follow]

        Return the full CLAUDE.md document. Nothing before or after it.
        """;

    public Task<string> UpdateAsync(string diffSummary, string? existingClaudeMd)
    {
        var existingSection = existingClaudeMd is not null
            ? $"## Existing CLAUDE.md\n{existingClaudeMd}"
            : "## Existing CLAUDE.md\nNone — create from scratch.";

        var userContent = $"""
            ## Diff Summary
            {diffSummary}

            {existingSection}
            """;

        return RunAsync(SystemPrompt, userContent);
    }
}
