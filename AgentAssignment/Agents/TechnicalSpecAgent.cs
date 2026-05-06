namespace AgentAssignment.Agents;

class TechnicalSpecAgent() : BaseAgent("TechnicalSpecAgent")
{
    private const string SystemPrompt = """
        You are a software architect writing and maintaining a technical specification document.
        You will receive: a diff summary, a user story (for context), and optionally the current
        technical spec.

        Rules:
        - Write from an engineering perspective — components, data flow, dependencies, decisions.
        - If an existing spec is provided, update only the sections affected by the changes.
          Preserve all other content exactly.
        - If no existing spec is provided, create a full document from scratch.
        - Add a changelog entry at the bottom under "## Changelog" with today's date.
        - Under "## Design Decisions", document implicit architectural choices visible in the diff
          summary. Preserve any pre-existing decisions already in the spec.
        - Under "## Open Items", note any technical gaps or unresolved decisions visible in the diff.
          If none, write "None."
        - Return the full markdown document. Nothing before or after it.
        """;

    public Task<string> UpdateAsync(string diffSummary, string userStory, string? existingSpec)
    {
        var existingSection = existingSpec is not null
            ? $"## Existing Technical Spec\n{existingSpec}"
            : "## Existing Technical Spec\nNone — create from scratch.";

        var userContent = $"""
            ## Diff Summary
            {diffSummary}

            ## User Story
            {userStory}

            {existingSection}
            """;

        return RunAsync(SystemPrompt, userContent);
    }
}
