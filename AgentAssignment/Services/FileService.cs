namespace AgentAssignment.Services;

class FileService(string baseDir)
{
    private string Path(string relativePath) => System.IO.Path.Combine(baseDir, relativePath);

    public Task<string> ReadUserStoryAsync() =>
        File.ReadAllTextAsync(Path("Specs/user-story.md"));

    public async Task<string?> ReadSpecIfExistsAsync(string relativePath)
    {
        var path = Path(relativePath);
        return File.Exists(path) ? await File.ReadAllTextAsync(path) : null;
    }

    public Task WriteSpecAsync(string relativePath, string content) =>
        File.WriteAllTextAsync(Path(relativePath), content);
}
