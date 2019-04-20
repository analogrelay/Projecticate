using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Projecticate
{
    [Command(Description = "Playing with projected file systems.")]
    internal class Program
    {
        private static int Main(string[] args)
        {
#if DEBUG
            if (args.Any(a => a == "--debug"))
            {
                args = args.Where(a => a != "--debug").ToArray();
                Console.WriteLine($"Ready for debugger to attach. Process ID: {Process.GetCurrentProcess().Id}.");
                Console.WriteLine("Press ENTER to continue.");
                Console.ReadLine();
            }
#endif

            try
            {
                return CommandLineApplication.Execute<Program>(args);
            }
            catch (CommandLineException clex)
            {
                var oldFg = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(clex.Message);
                Console.ForegroundColor = oldFg;
                return 1;
            }
            catch (Exception ex)
            {
                var oldFg = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Unhandled exception:");
                Console.Error.WriteLine(ex.ToString());
                Console.ForegroundColor = oldFg;
                return 1;
            }
        }

        [Option("-d|--directory <DIR>", Description = "A directory in which to project.")]
        public string TargetDirectory { get; set; }

        public async Task OnExecuteAsync(IConsole console)
        {
            console.WriteLine($"Preparing to project into {TargetDirectory}");

            TargetDirectory = Path.GetFullPath(TargetDirectory);

            var tcs = new TaskCompletionSource<object>();
            console.CancelKeyPress += (e, a) =>
            {
                if (!tcs.Task.IsCompleted)
                {
                    console.WriteLine("Shutting down, press Ctrl-C again to terminate forcibly...");
                    a.Cancel = true;
                    tcs.TrySetResult(null);
                }
            };

            using (var demoFs = new DemoFileSystem(TargetDirectory))
            {
                demoFs.Start();
                console.WriteLine("Now virtualizing. Press Ctrl-C to stop.");
                await tcs.Task;
            }
        }

        internal class DemoFileSystem : ProjectedFileSystem
        {
            public DemoFileSystem(string rootPath) : base(rootPath)
            {
            }

            public override IEnumerable<ProjectedDirectoryEntry> EnumerateDirectory(string relativePath, string searchPattern)
            {
                Console.WriteLine($"Enumerating: '{relativePath}' (search pattern: {searchPattern})");
                if (string.IsNullOrEmpty(relativePath))
                {
                    yield return new ProjectedDirectoryEntry("a", FileBasicInfo.File(42, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now));
                    yield return new ProjectedDirectoryEntry("b", FileBasicInfo.File(24, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now));
                    yield return new ProjectedDirectoryEntry("c", FileBasicInfo.Directory(DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now));
                }
                else
                {
                    yield return new ProjectedDirectoryEntry("d", FileBasicInfo.File(80, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now));
                }
            }
        }
    }
}