using AgentAssignment.Agents;
using AgentAssignment.Services;

namespace AgentAssignment.Orchestrator;

class SpecOrchestrator(GitDiffService gitDiff, FileService files, bool enableEvaluation)
{
    public async Task RunAsync()
    {
        Console.WriteLine("=== Spec Sync Agent ===\n");

        // Stage 0: load inputs in parallel
        Console.WriteLine("[1/4] Reading diff and user story...");
        var (rawDiff, userStory, existingFuncSpec, existingTechSpec) = await LoadInputsAsync();
        Console.WriteLine($"      Diff: {rawDiff.Split('\n').Length} lines | User story: {userStory.Split('\n').Length} lines");

        // Stage 1: summarize
        Console.WriteLine("\n[2/4] Summarizing diff...");
        var diffSummary = await new DiffSummaryAgent().SummarizeAsync(rawDiff);
        Console.WriteLine(Indent(diffSummary));

        // Stage 2: evaluate (optional)
        string? evaluationReport = null;
        if (enableEvaluation)
        {
            Console.WriteLine("\n[3/4] Evaluating implementation against user story...");
            evaluationReport = await new EvaluationAgent().EvaluateAsync(diffSummary, userStory);
            Console.WriteLine(Indent(evaluationReport));
        }
        else
        {
            Console.WriteLine("\n[3/4] Evaluation skipped (ENABLE_EVALUATION not set).");
        }

        // Stage 3: write specs in parallel
        Console.WriteLine("\n[4/4] Writing specs in parallel...");
        var (funcSpec, techSpec) = await WriteSpecsAsync(diffSummary, userStory, evaluationReport, existingFuncSpec, existingTechSpec);

        await Task.WhenAll(
            files.WriteSpecAsync("Specs/functional-spec.md", funcSpec),
            files.WriteSpecAsync("Specs/technical-spec.md", techSpec)
        );

        Console.WriteLine("\n=== Done ===");
        Console.WriteLine("  Specs/functional-spec.md  updated");
        Console.WriteLine("  Specs/technical-spec.md   updated");
    }

    private async Task<(string rawDiff, string userStory, string? existingFuncSpec, string? existingTechSpec)> LoadInputsAsync()
    {
        var diffTask = gitDiff.GetDiffAsync();
        var storyTask = files.ReadUserStoryAsync();
        var funcTask = files.ReadSpecIfExistsAsync("Specs/functional-spec.md");
        var techTask = files.ReadSpecIfExistsAsync("Specs/technical-spec.md");

        await Task.WhenAll(diffTask, storyTask, funcTask, techTask);

        return (diffTask.Result, storyTask.Result, funcTask.Result, techTask.Result);
    }

    private async Task<(string funcSpec, string techSpec)> WriteSpecsAsync(
        string diffSummary, string userStory, string? eval,
        string? existingFuncSpec, string? existingTechSpec)
    {
        var funcTask = new FunctionalSpecAgent().UpdateAsync(diffSummary, userStory, eval, existingFuncSpec);
        var techTask = new TechnicalSpecAgent().UpdateAsync(diffSummary, userStory, eval, existingTechSpec);

        await Task.WhenAll(funcTask, techTask);
        return (funcTask.Result, techTask.Result);
    }

    private static string Indent(string text) =>
        string.Join('\n', text.Split('\n').Select(l => "      " + l));
}
