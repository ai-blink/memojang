using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace Memojang.App.Shell;

// One Memojang window per user session: a later launch (Explorer "Open", a second icon click) hands its file
// paths to the first process over a named pipe and exits, so documents land as tabs instead of new windows.
// The mutex decides who is primary; the pipe carries newline-separated absolute paths (empty = "new tab").
public sealed class SingleInstanceChannel : IDisposable
{
    private const int AllowAnyProcess = -1;

    private readonly Mutex _mutex;
    private readonly string _pipeName;
    private readonly CancellationTokenSource _listenCancellation = new();

    public SingleInstanceChannel(string name)
    {
        _pipeName = name;
        _mutex = new Mutex(initiallyOwned: true, $@"Local\{name}", out bool createdNew);
        IsPrimary = createdNew;
    }

    public bool IsPrimary { get; }

    public static string DefaultName
    {
        get
        {
            string user = WindowsIdentity.GetCurrent().User?.Value ?? Environment.UserName;
            using var process = System.Diagnostics.Process.GetCurrentProcess();
            return $"Memojang-{user}-{process.SessionId}";
        }
    }

    public bool TrySendToPrimary(IReadOnlyList<string> paths, TimeSpan timeout)
    {
        try
        {
            // The launching process owns the foreground right; lend it so the primary can raise its window.
            AllowSetForegroundWindow(AllowAnyProcess);
            using var client = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out, PipeOptions.CurrentUserOnly);
            client.Connect((int)timeout.TotalMilliseconds);
            using var writer = new StreamWriter(client, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            writer.Write(string.Join('\n', paths));
            writer.Flush();
            return true;
        }
        catch (Exception ex) when (ex is IOException or TimeoutException or UnauthorizedAccessException)
        {
            AppDiagnostics.Log($"SingleInstance send failed pipe={_pipeName} {ex.Message}");
            return false;
        }
    }

    public void StartListening(Action<IReadOnlyList<string>> onPathsReceived)
    {
        if (!IsPrimary)
        {
            throw new InvalidOperationException("Only the primary instance listens for forwarded paths.");
        }

        CancellationToken token = _listenCancellation.Token;
        _ = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await using var server = new NamedPipeServerStream(
                        _pipeName,
                        PipeDirection.In,
                        1,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
                    await server.WaitForConnectionAsync(token);
                    using var reader = new StreamReader(server, Encoding.UTF8);
                    string message = await reader.ReadToEndAsync(token);
                    onPathsReceived(ParseMessage(message));
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    AppDiagnostics.Log($"SingleInstance listen error pipe={_pipeName} {ex.Message}");
                }
            }
        }, token);
    }

    public static IReadOnlyList<string> ParseMessage(string message)
    {
        return message
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    public void Dispose()
    {
        _listenCancellation.Cancel();
        if (IsPrimary)
        {
            try
            {
                _mutex.ReleaseMutex();
            }
            catch (ApplicationException)
            {
                // Released from a different thread than the owner at shutdown; the handle close below frees it anyway.
            }
        }

        _mutex.Dispose();
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AllowSetForegroundWindow(int processId);
}
