using System.Diagnostics;
using System.Text;

namespace Lode.Drivers.AccessDb;

public static class ProcessRunner
{
    public static async Task<bool> IsMdbToolsAvailable()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "mdb-ver",
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var process = Process.Start(psi);

            if (process == null)
                return false;

            await process.WaitForExitAsync();

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
    public static async Task<ProcessResult> RunAsync(string fileName, string arguments = "", string workingDirectory = "")
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (!string.IsNullOrWhiteSpace(workingDirectory))
            startInfo.WorkingDirectory = workingDirectory;

        using var process = new Process();
        process.StartInfo = startInfo;

        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        var stdoutCompletion = new TaskCompletionSource<bool>();
        var stderrCompletion = new TaskCompletionSource<bool>();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data == null) stdoutCompletion.TrySetResult(true);
            else stdout.AppendLine(e.Data);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data == null) stderrCompletion.TrySetResult(true);
            else stderr.AppendLine(e.Data);
        };

        process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();
        await Task.WhenAll(stdoutCompletion.Task, stderrCompletion.Task);

        return new ProcessResult
        {
            ExitCode = process.ExitCode,
            StdOut = stdout.ToString(),
            StdErr = stderr.ToString()
        };
    }
}

public class ProcessResult
{
    public int ExitCode { get; init; }
    public string StdOut { get; init; } = "";
    public string StdErr { get; init; } = "";
}
