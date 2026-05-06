namespace AgentAssignment.Agents;

class FunctionalSpecAgent() : BaseAgent("FunctionalSpecAgent")
{
    private const string SystemPrompt = """
        You are a business analyst writing and maintaining a functional specification document.
        You will receive: a diff summary, a user story, and optionally the current functional spec.

        Rules:
        - Write from a user/business perspective — no code, no technical jargon.
        - If an existing spec is provided, update only the sections affected by the changes.
          Preserve all other content exactly.
        - If no existing spec is provided, create a full document from scratch.
        - Add a changelog entry at the bottom under "## Changelog" with today's date and a
          one-line description of what changed.
        - Cross-reference the user story acceptance criteria against the diff summary.
          Under "## Open Items", list any acceptance criteria that do not appear to be addressed.
          If all criteria appear covered, write "None."
        - Return the full markdown document. Nothing before or after it.
        """;

    public Task<string> UpdateAsync(string diffSummary, string userStory, string? existingSpec)
    {
        var existingSection = existingSpec is not null
            ? $"## Existing Functional Spec\n{existingSpec}"
            : "## Existing Functional Spec\nNone — create from scratch.";

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
