using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Bahyway.ShoWay.UI.Services;
using Bahyway.SharedKernel.Graph; // Ensure this matches where GraphTopology is
using System.Collections.ObjectModel;

namespace Bahyway.ShoWay.UI.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        // Console Log Output
        [ObservableProperty]
        private string _consoleLog = "Akkadian Kernel v3.1 Initialized...";

        // Selected Node for Property Grid
        [ObservableProperty]
        private VisualNode? _selectedNode;

        // The Graph Data (Bound to the UI)
        // We initialize it here so it is never null
        public GraphTopology ActiveGraph { get; set; } = new GraphTopology();

        [RelayCommand]
        public async Task BuildAndDeploy()
        {
            if (ActiveGraph == null || ActiveGraph.Nodes.Count == 0)
            {
                ConsoleLog += "\n[Error] Graph is empty. Draw something first!";
                return;
            }

            ConsoleLog += "\n--- STARTING AUTOMATED BUILD ---";

            try
            {
                // 1. Generate DSL
                var generator = new AkkadianSourceGenerator();
                string projectName = $"VisualProject_{DateTime.Now:HHmmss}";

                // FIX: Use ActiveGraph property instead of _currentGraph field
                string dslCode = generator.GenerateSource(projectName, ActiveGraph);

                // 2. Save .akk File
                string projectDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Projects");
                if (!Directory.Exists(projectDir)) Directory.CreateDirectory(projectDir);

                string akkFilePath = Path.Combine(projectDir, $"{projectName}.akk");
                await File.WriteAllTextAsync(akkFilePath, dslCode);

                ConsoleLog += $"\n[1] DSL Generated: {projectName}.akk";

                // 3. Invoke Akkadian Compiler CLI
                // Ensure this path points to your actual CLI exe
                string cliPath = Path.GetFullPath(@"..\..\..\..\..\Akkadian.Compiler\Akkadian.Cli\bin\Debug\net8.0\Akkadian.Cli.exe");

                if (!File.Exists(cliPath))
                {
                    ConsoleLog += $"\n[CRITICAL] Compiler not found at: {cliPath}";
                    return;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = cliPath,
                    Arguments = $"\"{akkFilePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                ConsoleLog += "\n[2] Compiling Infrastructure...";

                await Task.Run(() =>
                {
                    using (var process = Process.Start(startInfo))
                    {
                        if (process == null) return;

                        string output = process.StandardOutput.ReadToEnd();
                        string err = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        if (process.ExitCode == 0)
                        {
                            ConsoleLog += "\n[3] Compilation Success!";
                            ConsoleLog += "\n    - SQL Schema Created";
                            ConsoleLog += "\n    - C# Actors Generated";
                            ConsoleLog += "\n    - Python AI Service Generated";
                            ConsoleLog += "\n[4] System Ready.";
                        }
                        else
                        {
                            ConsoleLog += $"\n[ERROR] Compilation Failed:\n{output}\n{err}";
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                ConsoleLog += $"\n[EXCEPTION] {ex.Message}";
            }
        }
    }
}