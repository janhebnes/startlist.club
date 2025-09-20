using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace FlightJournal.Host
{
    internal static class Program
    {
        private static Process? _iisProcess;
        private static ManualResetEvent _shutdown = new ManualResetEvent(false);

        static int Main(string[] args)
        {
            Console.Title = "FlightJournal Host (IIS Express Wrapper)";
            var port = GetPort(args) ?? 5000;
            var projectPath = ResolveWebProjectPath();
            if (!Directory.Exists(projectPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Web project path not found: {projectPath}");
                Console.ResetColor();
                return 1;
            }

            var iisExe = ResolveIisExpress();
            if (iisExe == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("IIS Express executable not found. Install IIS Express or Visual Studio workload containing it.");
                Console.ResetColor();
                return 2;
            }

            var arguments = $"/path:\"{projectPath}\" /port:{port} /clr:v4.0 /systray:false";
            Console.WriteLine($"Launching IIS Express: \n  {iisExe} {arguments}\n");

            var psi = new ProcessStartInfo
            {
                FileName = iisExe,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            try
            {
                _iisProcess = Process.Start(psi);
                if (_iisProcess == null)
                {
                    Console.WriteLine("Failed to start IIS Express process.");
                    return 3;
                }
                _iisProcess.OutputDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
                _iisProcess.ErrorDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
                _iisProcess.BeginOutputReadLine();
                _iisProcess.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error starting IIS Express: " + ex.Message);
                return 4;
            }

            var url = $"http://localhost:{port}/";
            Console.WriteLine($"Site running at {url}");
            TryOpenBrowser(url);
            Console.WriteLine("Press ENTER (or Ctrl+C) to stop...");

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                _shutdown.Set();
            };
            _shutdown.WaitOne();
            StopIis();
            return 0;
        }

        private static void StopIis()
        {
            if (_iisProcess != null && !_iisProcess.HasExited)
            {
                try
                {
                    _iisProcess.Kill();
                }
                catch { }
            }
        }

        private static string? ResolveIisExpress()
        {
            string?[] candidates = new[]
            {
                Environment.GetEnvironmentVariable("PROGRAMFILES") + "\\IIS Express\\iisexpress.exe",
                Environment.GetEnvironmentVariable("PROGRAMFILES(X86)") + "\\IIS Express\\iisexpress.exe"
            };
            return candidates.FirstOrDefault(f => !string.IsNullOrWhiteSpace(f) && File.Exists(f!));
        }

        private static string ResolveWebProjectPath()
        {
            // Host/bin/Debug/... -> go up until solution root then to FlightJournal.Web
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dir = new DirectoryInfo(baseDir);
            // climb up until we find the solution root marker (FlightJournal.Web folder exists sibling)
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "FlightJournal.Web")))
            {
                dir = dir.Parent;
            }
            return Path.Combine(dir?.FullName ?? baseDir, "FlightJournal.Web");
        }

        private static int? GetPort(string[] args)
        {
            foreach (var a in args)
            {
                if (a.StartsWith("--port="))
                {
                    if (int.TryParse(a.Substring(8), out var p)) return p;
                }
            }
            return null;
        }

        private static void TryOpenBrowser(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch { }
        }
    }
}
