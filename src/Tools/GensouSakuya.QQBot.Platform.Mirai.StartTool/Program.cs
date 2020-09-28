using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GensouSakuya.QQBot.Platform.Mirai.StartTool
{
    class Program
    {
        private static EventWaitHandle botWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        static async Task Main(string[] args)
        {
            if (args == null || args.Length < 4)
                return;
            var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
            GetConsoleMode(iStdOut, out uint outConsoleMode);
            outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN;
            SetConsoleMode(iStdOut, outConsoleMode);

            var miraiLibsPath = args[0];
            var botPath = args[2];
            var p = Process.Start(new ProcessStartInfo
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                FileName = "java",
                WorkingDirectory = miraiLibsPath,
                Arguments = $@"-cp ""{miraiLibsPath}\libs\*"" net.mamoe.mirai.console.pure.MiraiConsolePureLoader {args[1]}",
                UseShellExecute = false
            });
            var isLogin = false;
            p.BeginOutputReadLine();
            p.OutputDataReceived += (s,e)=>
            {
                Console.WriteLine("[Mirai]"+e.Data);
                if (!isLogin)
                {
                    if (e.Data.Contains("login successful",StringComparison.OrdinalIgnoreCase))
                    {
                        isLogin = true;
                        botWaitHandle.Set();
                    }
                }
            };

            botWaitHandle.WaitOne();
            var botP = Process.Start(new ProcessStartInfo
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                WorkingDirectory = Path.GetDirectoryName(botPath),
                FileName = botPath,
                Arguments = args[3],
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
                if (readline.Equals("exit",StringComparison.OrdinalIgnoreCase))
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
}
