namespace AgentAssignment.Agents;

class DiffSummaryAgent() : BaseAgent("DiffSummaryAgent")
{
    private const string SystemPrompt = """
        You are a senior developer summarizing a git diff for a non-technical audience.
        Given a raw git diff, produce a concise plain-English summary of what changed:
        which files were modified, what functionality was added or removed, and why the
        change appears to have been made (infer from context). Keep it to 3–6 bullet points.
        Do not include raw code snippets. Output plain markdown.
        """;

    public Task<string> SummarizeAsync(string rawDiff) =>
        RunAsync(SystemPrompt, $"Git diff:\n\n{rawDiff}");
}
