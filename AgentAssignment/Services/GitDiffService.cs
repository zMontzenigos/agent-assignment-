using System.Diagnostics;

namespace AgentAssignment.Services;

class GitDiffService(string diffFile)
{
    public async Task<string> GetDiffAsync()
    {
        var resolved = Path.IsPathRooted(diffFile)
            ? diffFile
            : Path.Combine(Directory.GetCurrentDirectory(), diffFile);

        if (File.Exists(resolved))
            return await File.ReadAllTextAsync(resolved);

        var diff = await RunGitAsync("diff HEAD~1 HEAD");
        if (string.IsNullOrWhiteSpace(diff))
            diff = await RunGitAsync("diff HEAD");
        if (string.IsNullOrWhiteSpace(diff))
            throw new InvalidOperationException($"Diff file '{diffFile}' not found and no git diff available.");

        return diff;
    }

    private static async Task<string> RunGitAsync(string args)
    {
        var psi = new ProcessStartInfo("git", args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(psi)!;
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        return output;
    }
}
