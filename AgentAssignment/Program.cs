using AgentAssignment.Orchestrator;
using AgentAssignment.Services;

var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));

static SpecOrchestrator Build(string projectRoot, string diffFile, string storyFile) =>
    new(
        new GitDiffService(diffFile),
        new FileService(projectRoot, storyFile),
        Path.GetFileNameWithoutExtension(storyFile)
    );

Console.WriteLine("Press Enter to run Story 1 — Add a User...");
Console.ReadLine();
await Build(projectRoot, "Specs/story-1.diff", "Specs/stories/story-1-add-user.md").RunAsync();

Console.WriteLine("\nPress Enter to run Story 2 — Get Enterprise & Update User...");
Console.ReadLine();
await Build(projectRoot, "Specs/story-2.diff", "Specs/stories/story-2-enterprise-and-user.md").RunAsync();

Console.WriteLine("\nPress Enter to run Story 3 — Update User Status (watch the evaluation)...");
Console.ReadLine();
await Build(projectRoot, "Specs/story-3.diff", "Specs/stories/story-3-update-user-status.md").RunAsync();
