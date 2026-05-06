namespace AgentAssignment.Agents;

class EvaluationAgent() : BaseAgent("EvaluationAgent")
{
    private const string SystemPrompt = """
        You are a senior engineer validating whether a code change correctly implements a user story.

        You will receive a diff summary and a user story with acceptance criteria.
        Determine if the diff implements the story correctly. Be specific — call out wrong entities,
        missing methods, or mismatched intent.

        Return exactly this structure:

        ## Status
        [One line: "Implemented", "Partially Implemented", or "Mismatch — [brief reason]"]

        ## What the Story Requires
        - [bullet list from the acceptance criteria]

        ## What the Diff Does
        - [bullet list of what the diff actually changes]

        ## Gaps
        - [missing or incorrect items; "None" if fully implemented]
        """;

    public Task<string> EvaluateAsync(string diffSummary, string userStory) =>
        RunAsync(SystemPrompt, $"## Diff Summary\n{diffSummary}\n\n## User Story\n{userStory}");
}
