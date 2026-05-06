namespace AgentAssignment.Agents;

class TechnicalSpecAgent() : BaseAgent("TechnicalSpecAgent")
{
    private const string SystemPrompt = """
        You are a software architect writing and maintaining a technical specification document.
        You will receive: a diff summary, a user story (for context), an evaluation report,
        and optionally the current technical spec.

        Rules:
        - Write from an engineering perspective — components, data flow, dependencies, decisions.
        - If an existing spec is provided, update only the sections affected by the changes.
          Preserve all other content exactly.
        - If no existing spec is provided, create a full document from scratch.
        - Add a changelog entry at the bottom under "## Changelog" with today's date.
        - Under "## Design Decisions", document all items from the evaluation's "Design Decisions"
          section, plus any pre-existing decisions already in the spec.
        - Under "## Open Items", carry forward any unresolved items from the evaluation's
          "Potentially Missing" section with a technical framing.
        - Do not re-evaluate or add your own findings — only document what you receive.
        - Return the full markdown document. Nothing before or after it.
        """;

    public Task<string> UpdateAsync(string diffSummary, string userStory, string? evaluationReport, string? existingSpec)
    {
        var existingSection = existingSpec is not null
            ? $"## Existing Technical Spec\n{existingSpec}"
            : "## Existing Technical Spec\nNone — create from scratch.";

        var evalSection = evaluationReport is not null
            ? $"## Evaluation Report\n{evaluationReport}"
            : "## Evaluation Report\nNot run.";

        var userContent = $"""
            ## Diff Summary
            {diffSummary}

            ## User Story
            {userStory}

            {evalSection}

            {existingSection}
            """;

        return RunAsync(SystemPrompt, userContent);
    }
}
