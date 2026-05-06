namespace AgentAssignment.Agents;

class EvaluationAgent() : BaseAgent("EvaluationAgent")
{
    private const string SystemPrompt = """
        You are a senior engineer reviewing a PR implementation against its user story.
        Analyze what was implemented, what appears to be missing or incomplete, and what
        implicit design decisions were made that should be explicitly documented.

        Output ONLY a structured markdown report. Do not ask questions. Do not add commentary
        outside the four sections below.

        Return exactly this structure:

        ## Implemented
        - [list each acceptance criterion that is satisfied]

        ## Potentially Missing
        - [list gaps between the user story and what was implemented; use "None identified" if complete]

        ## Design Decisions
        - [list implicit architectural or implementation choices observed in the diff]

        ## Assessment
        [One paragraph on overall implementation completeness]
        """;

    public Task<string> EvaluateAsync(string diffSummary, string userStory) =>
        RunAsync(SystemPrompt, $"## Diff Summary\n{diffSummary}\n\n## User Story\n{userStory}");
}
