$mainPath = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI/Views/MainWindow.axaml.cs"
$mainCode = @"
using Avalonia.Controls;
using Bahyway.KGEditor.UI.Controls;
using Bahyway.KGEditor.UI.Actors;
using Bahyway.SharedKernel.Graph;
using Akka.Actor;
using Akka.Configuration;
using System;

namespace Bahyway.KGEditor.UI.Views
{
    public partial class MainWindow : Window
    {
        private ActorSystem _uiSystem;
        private GraphTopology _currentGraph;

        public MainWindow()
        {
            InitializeComponent();
            this.Opened += OnWindowOpened;
            this.Closing += OnWindowClosing;
        }

        private void OnWindowOpened(object? sender, EventArgs e)
        {
            // 1. Initialize Graph Data
            _currentGraph = new GraphTopology();
            // Sensor Node (ID matches what the Orchestrator sends)
            _currentGraph.Nodes.Add(new VisualNode { Id = ""Sensor_03"", Label = ""Sensor 03"", X = 400, Y = 300, ColorHex = ""#333333"" }); // Default Gray
            _currentGraph.Nodes.Add(new VisualNode { Id = ""Valve_A"", Label = ""Valve A"", X = 200, Y = 150, ColorHex = ""#00FF00"" });
            _currentGraph.Edges.Add(new VisualEdge { SourceId = ""Valve_A"", TargetId = ""Sensor_03"" });

            var canvas = this.FindControl<GraphCanvas>(""MyGraphCanvas"");
            if (canvas != null) canvas.RenderGraph(_currentGraph);

            // 2. Start Akka.NET on Port 8091 (The Receiver)
            var config = ConfigurationFactory.ParseString(@""
                akka {
                    actor.provider = """"Akka.Remote.RemoteActorRefProvider, Akka.Remote""""
                    remote.dot-netty.tcp {
                        port = 8091
                        hostname = """"localhost""""
                    }
                }
            "");

            _uiSystem = ActorSystem.Create(""BahywayUI"", config);

            // 3. Spawn the Bridge Actor named 'bridge'
            // Address becomes: akka.tcp://BahywayUI@localhost:8091/user/bridge
            if (canvas != null)
            {
                _uiSystem.ActorOf(Props.Create(() => new UIBridgeActor(canvas, _currentGraph)), ""bridge"");
            }
        }

        private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _uiSystem?.Terminate();
        }
    }
}
"@
Set-Content -Path $mainPath -Value $mainCode
Write-Host "✅ MainWindow updated to listen on Port 8091." -ForegroundColor Green
