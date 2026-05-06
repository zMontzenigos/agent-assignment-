using AgentAssignment.Orchestrator;
using AgentAssignment.Services;

var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
var enableEvaluation = Environment.GetEnvironmentVariable("ENABLE_EVALUATION") == "true";

var orchestrator = new SpecOrchestrator(
    new GitDiffService(),
    new FileService(projectRoot),
    enableEvaluation
);

await orchestrator.RunAsync();
