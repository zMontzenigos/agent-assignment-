using System.Diagnostics;

namespace AgentAssignment.Services;

class GitDiffService
{
    public async Task<string> GetDiffAsync()
    {
        var diffFile = Environment.GetEnvironmentVariable("DIFF_FILE");
        if (!string.IsNullOrWhiteSpace(diffFile))
        {
            var resolved = System.IO.Path.IsPathRooted(diffFile)
                ? diffFile
                : System.IO.Path.Combine(Directory.GetCurrentDirectory(), diffFile);
            if (File.Exists(resolved))
                return await File.ReadAllTextAsync(resolved);
        }

        var diff = await RunGitAsync("diff HEAD~1 HEAD");

        if (string.IsNullOrWhiteSpace(diff))
            diff = await RunGitAsync("diff HEAD");

        if (string.IsNullOrWhiteSpace(diff))
            throw new InvalidOperationException("No git diff found. Set DIFF_FILE or ensure there is at least one commit with changes.");

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
