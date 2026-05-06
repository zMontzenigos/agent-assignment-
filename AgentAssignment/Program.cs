using AgentAssignment.Orchestrator;
using AgentAssignment.Services;

var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
var storyFile = Environment.GetEnvironmentVariable("STORY_FILE") ?? "Specs/user-story.md";
var storyLabel = System.IO.Path.GetFileNameWithoutExtension(storyFile);

var orchestrator = new SpecOrchestrator(
    new GitDiffService(),
    new FileService(projectRoot, storyFile),
    storyLabel
);

await orchestrator.RunAsync();
