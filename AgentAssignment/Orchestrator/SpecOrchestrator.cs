using AgentAssignment.Agents;
using AgentAssignment.Services;

namespace AgentAssignment.Orchestrator;

class SpecOrchestrator(GitDiffService gitDiff, FileService files, string storyLabel = "")
{
    public async Task RunAsync()
    {
        var label = string.IsNullOrWhiteSpace(storyLabel) ? "" : $" — {storyLabel}";
        Console.WriteLine($"=== Spec Sync Agent{label} ===\n");

        // Stage 0: load inputs in parallel
        Console.WriteLine("[1/3] Reading diff and user story...");
        var (rawDiff, userStory, existingFuncSpec, existingTechSpec, existingClaudeMd) = await LoadInputsAsync();

        var storyTitle = ExtractTitle(userStory);
        if (!string.IsNullOrWhiteSpace(storyTitle))
            Console.WriteLine($"      Story: {storyTitle}");
        Console.WriteLine($"      Diff: {rawDiff.Split('\n').Length} lines | User story: {userStory.Split('\n').Length} lines");

        // Stage 1: summarize
        Console.WriteLine("\n[2/3] Summarizing diff...");
        var diffSummary = await new DiffSummaryAgent().SummarizeAsync(rawDiff);
        Console.WriteLine(Indent(diffSummary));

        // Stage 2: write all three documents in parallel
        Console.WriteLine("\n[3/3] Writing specs and CLAUDE.md in parallel...");
        var (funcSpec, techSpec, claudeMd) = await WriteDocsAsync(diffSummary, userStory, existingFuncSpec, existingTechSpec, existingClaudeMd);

        await Task.WhenAll(
            files.WriteSpecAsync("Specs/functional-spec.md", funcSpec),
            files.WriteSpecAsync("Specs/technical-spec.md", techSpec),
            files.WriteSpecAsync("../CLAUDE.md", claudeMd)
        );

        Console.WriteLine("\n=== Done ===");
        Console.WriteLine("  Specs/functional-spec.md  updated");
        Console.WriteLine("  Specs/technical-spec.md   updated");
        Console.WriteLine("  CLAUDE.md                 updated");
    }

    private async Task<(string rawDiff, string userStory, string? existingFuncSpec, string? existingTechSpec, string? existingClaudeMd)> LoadInputsAsync()
    {
        var diffTask = gitDiff.GetDiffAsync();
        var storyTask = files.ReadUserStoryAsync();
        var funcTask = files.ReadSpecIfExistsAsync("Specs/functional-spec.md");
        var techTask = files.ReadSpecIfExistsAsync("Specs/technical-spec.md");
        var claudeTask = files.ReadSpecIfExistsAsync("../CLAUDE.md");

        await Task.WhenAll((Task)diffTask, storyTask, funcTask, techTask, claudeTask);

        return (diffTask.Result, storyTask.Result, funcTask.Result, techTask.Result, claudeTask.Result);
    }

    private static async Task<(string funcSpec, string techSpec, string claudeMd)> WriteDocsAsync(
        string diffSummary, string userStory,
        string? existingFuncSpec, string? existingTechSpec, string? existingClaudeMd)
    {
        var funcTask = new FunctionalSpecAgent().UpdateAsync(diffSummary, userStory, existingFuncSpec);
        var techTask = new TechnicalSpecAgent().UpdateAsync(diffSummary, userStory, existingTechSpec);
        var claudeTask = new ClaudeMdAgent().UpdateAsync(diffSummary, existingClaudeMd);

        await Task.WhenAll(funcTask, techTask, claudeTask);
        return (funcTask.Result, techTask.Result, claudeTask.Result);
    }

    private static string ExtractTitle(string markdown)
    {
        var line = markdown.Split('\n').FirstOrDefault(l => l.TrimStart().StartsWith("# "));
        return line?.TrimStart('#').Trim() ?? "";
    }

    private static string Indent(string text) =>
        string.Join('\n', text.Split('\n').Select(l => "      " + l));
}
