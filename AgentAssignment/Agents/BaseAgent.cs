using System.Diagnostics;

namespace AgentAssignment.Agents;

abstract class BaseAgent(string name)
{
    protected string Name => name;

    protected async Task<string> RunAsync(string systemPrompt, string userContent)
    {
        var fullPrompt = $"{systemPrompt}\n\n---\n\n{userContent}";

        var psi = new ProcessStartInfo("claude", "-p")
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        psi.EnvironmentVariables.Remove("CLAUDECODE");

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start claude process. Ensure 'claude' is on PATH.");

        await process.StandardInput.WriteAsync(fullPrompt);
        process.StandardInput.Close();

        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (string.IsNullOrWhiteSpace(output))
            throw new InvalidOperationException($"{Name} returned empty output. Check that 'claude -p' works in your terminal.");

        return output.Trim();
    }
}
