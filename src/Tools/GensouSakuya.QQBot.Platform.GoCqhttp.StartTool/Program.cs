using System.Diagnostics;
using System.Runtime.InteropServices;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine(string.Join(Environment.NewLine, args));
        if (args == null || args.Length < 1)
            return;
        var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
        GetConsoleMode(iStdOut, out uint outConsoleMode);
        outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN;
        SetConsoleMode(iStdOut, outConsoleMode);

        var waitStartTask = new TaskCompletionSource<bool>();
        var botPath = args[0];
        var p = Process.Start(new ProcessStartInfo
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            RedirectStandardInput = true,
            FileName = "start.cmd",
            UseShellExecute = false
        });
        var isLogin = false;
        p.BeginOutputReadLine();
        p.OutputDataReceived += (s, e) =>
        {
            if (e?.Data == null)
                return;
            Console.WriteLine("[GoCqhttp]" + e.Data);
            if (!isLogin)
            {
                if (e.Data.Contains("服务器已启动", StringComparison.OrdinalIgnoreCase))
                {
                    isLogin = true;
                    waitStartTask.TrySetResult(true);
                }
            }
        };

        await waitStartTask.Task;
        var botP = Process.Start(new ProcessStartInfo
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            RedirectStandardInput = true,
            WorkingDirectory = Path.GetDirectoryName(botPath),
            FileName = botPath,
            UseShellExecute = false
        });
        botP.BeginOutputReadLine();
        botP.OutputDataReceived += (s, e) =>
        {
            Console.WriteLine("[Bot]" + e.Data);
        };

        while (true)
        {
            var readline = await Console.In.ReadLineAsync();
            if (readline.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                if (!botP.HasExited)
                    botP.Kill();
                if (!p.HasExited)
                    p.Kill();
                return;
            }
            else if (readline.StartsWith("m:", StringComparison.OrdinalIgnoreCase))
            {
                await p.StandardInput.WriteLineAsync(readline.Substring(2));
            }
            else if (readline.StartsWith("b:", StringComparison.OrdinalIgnoreCase))
            {
                await botP.StandardInput.WriteLineAsync(readline.Substring(2));
            }
        }
    }

    private const int STD_OUTPUT_HANDLE = -11;
    private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
    private const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);
}
