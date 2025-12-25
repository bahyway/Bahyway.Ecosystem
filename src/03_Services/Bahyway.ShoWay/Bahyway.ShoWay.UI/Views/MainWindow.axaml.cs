using Akka.Actor;
using Akka.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Bahyway.ShoWay.UI.Actors;
using Bahyway.ShoWay.UI.Controls;
using Bahyway.ShoWay.UI.Services;
using Bahyway.ShoWay.UI.ViewModels;
using Bahyway.ShoWay.UI.ViewModels.Monitoring; // <--- NEW IMPORT
using Bahyway.ShoWay.UI.Views.Monitoring;     // <--- NEW IMPORT
using Bahyway.SharedKernel.Graph;
using BahyWay.SharedKernel.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Bahyway.ShoWay.UI.Views
{
    public partial class MainWindow : Window
    {
        // --- CORE SYSTEMS ---
        private ActorSystem? _uiSystem;
        private GraphTopology? _currentGraph;
        private List<IAkkadianModule> _loadedModules = new List<IAkkadianModule>();

        // --- UI STATE ---
        private string? _selectedTool = null;
        private Window? _currentVizWindow = null;

        // --- SPATIAL / BILLIONS LOGIC ---
        private Point _lastMousePosition;
        private bool _isPanning;
        private double _currentOffsetX = 0;
        private double _currentOffsetY = 0;
        private readonly SpatialViewportService _spatialService;

        public MainWindow()
        {
            InitializeComponent();
            _spatialService = new SpatialViewportService();

            // ---------------------------------------------------------
            // 1. INITIALIZE PIPELINE MONITOR (Tab 2 Logic)
            // ---------------------------------------------------------
            // We manually find the control defined in XAML and assign its ViewModel
            // This allows Tab 2 to connect to Redis independently of the Graph Editor
            //var monitorView = this.FindControl<PipelineMonitorView>("MonitorView");
            var monitorView = this.FindControl<Bahyway.ShoWay.UI.Views.Monitoring.PipelineMonitorView>("MonitorView");
            if (monitorView != null)
            {
                // This starts the Redis Listener immediately
                monitorView.DataContext = new PipelineMonitorViewModel();
            }

            this.Opened += OnWindowOpened;
            this.Closing += OnWindowClosing;
        }

        // =========================================================
        // 2. INITIALIZATION & LIFECYCLE
        // =========================================================
        private void OnWindowOpened(object? sender, EventArgs e)
        {
            // A. Setup Infinite Canvas Hooks (Pan/Zoom/Spatial)
            var nativeCanvas = this.FindControl<GraphCanvas>("NativeCanvas");
            if (nativeCanvas != null)
            {
                nativeCanvas.PointerPressed += OnCanvasPointerPressed;
                nativeCanvas.PointerMoved += OnCanvasPointerMoved;
                nativeCanvas.PointerReleased += OnCanvasPointerReleased;
            }

            // B. Generate Previews for Gallery
            string extensionsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Extensions");
            try
            {
                Bahyway.ShoWay.UI.Services.PreviewGenerator.GenerateAllPreviews(extensionsPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Preview generation warning: {ex.Message}");
            }

            // C. Initialize Graph Data (Default State)
            _currentGraph = new GraphTopology();
            _currentGraph.Nodes.Add(new VisualNode { Id = "Valve_A", Label = "Valve A", X = 300, Y = 200, ColorHex = "#00FF00" });
            _currentGraph.Nodes.Add(new VisualNode { Id = "Sensor_03", Label = "Sensor 03", X = 500, Y = 200, ColorHex = "#333333" });
            _currentGraph.Edges.Add(new VisualEdge { SourceId = "Valve_A", TargetId = "Sensor_03" });

            // Render Initial Graph
            if (nativeCanvas != null) nativeCanvas.RenderGraph(_currentGraph);

            // D. Start Akka Actor System (The UI Backend)
            var config = ConfigurationFactory.ParseString(@"
                akka {
                    actor.provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                    remote.dot-netty.tcp { port = 8091, hostname = ""localhost"" }
                }
            ");
            _uiSystem = ActorSystem.Create("BahywayUI", config);
            if (nativeCanvas != null && _currentGraph != null)
            {
                _uiSystem.ActorOf(Props.Create(() => new UIBridgeActor(nativeCanvas, _currentGraph)), "bridge");
            }

            // E. CRITICAL: Sync View to ViewModel (Allows "Build" Button to work)
            if (DataContext is MainWindowViewModel vm)
            {
                vm.ActiveGraph = _currentGraph;
            }

            // F. Load Akkadian Extensions (WPDWay, beeMDM, etc.)
            LoadExtensions();

            // G. Update HUD (Heads Up Display)
            UpdateSpatialHud();
        }

        private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _uiSystem?.Terminate();
        }

        // =========================================================
        // 3. EXTENSION / PLUGIN SYSTEM (The "Brain" - Akkadian v3.1)
        // =========================================================
        private void LoadExtensions()
        {
            string extensionsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Extensions");
            if (!Directory.Exists(extensionsPath)) Directory.CreateDirectory(extensionsPath);

            foreach (string dllPath in Directory.GetFiles(extensionsPath, "*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(dllPath);
                    // Find classes that implement IAkkadianModule
                    var types = assembly.GetTypes()
                        .Where(t => typeof(IAkkadianModule).IsAssignableFrom(t) && !t.IsInterface);

                    foreach (var type in types)
                    {
                        var module = (IAkkadianModule)Activator.CreateInstance(type)!;
                        _loadedModules.Add(module);
                        System.Diagnostics.Debug.WriteLine($"[System] Loaded Extension: {module.ModuleName}");

                        // Load data from the module immediately
                        LoadModuleData(module);
                    }
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error loading extension: {ex.Message}"); }
            }
        }

        private async void LoadModuleData(IAkkadianModule module)
        {
            // Ask the module for data based on current view (Mocked Viewport for now)
            var viewport = new Rect(0, 0, 5000, 5000);
            var newNodes = await module.LoadNodesAsync(viewport);

            foreach (var node in newNodes)
            {
                // Prevent duplicates
                if (!_currentGraph.Nodes.Any(n => n.Id == node.Id))
                {
                    _currentGraph?.Nodes.Add(node);
                }
            }

            // Refresh Canvas
            var canvas = this.FindControl<GraphCanvas>("NativeCanvas");
            canvas?.RenderGraph(_currentGraph);

            // Re-sync ViewModel
            if (DataContext is MainWindowViewModel vm) vm.ActiveGraph = _currentGraph;
        }

        // =========================================================
        // 4. SPATIAL INTERACTION (The "Billions" UI Logic)
        // =========================================================
        private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // Middle or Right click to Pan
            if (e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed || e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                _isPanning = true;
                _lastMousePosition = e.GetPosition(this);
                this.Cursor = new Cursor(StandardCursorType.Hand);
            }
        }

        private void OnCanvasPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_isPanning)
            {
                var currentPos = e.GetPosition(this);
                var delta = currentPos - _lastMousePosition;
                _lastMousePosition = currentPos;

                // Update Virtual Offset
                _currentOffsetX -= delta.X;
                _currentOffsetY -= delta.Y;

                UpdateSpatialHud();
            }
        }

        private void OnCanvasPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _isPanning = false;
            this.Cursor = Cursor.Default;
        }

        private void UpdateSpatialHud()
        {
            // 1. Calculate Spatial ID (Simulated 64-bit Color ID)
            long sectorX = (long)(_currentOffsetX / 1000);
            long sectorY = (long)(_currentOffsetY / 1000);
            long spatialId = (sectorX << 32) | (sectorY & 0xFFFFFFFF);

            // 2. Update UI Elements
            var txtId = this.FindControl<TextBlock>("TxtSpatialID");
            var txtCoords = this.FindControl<TextBlock>("TxtCoordinates");
            var txtCount = this.FindControl<TextBlock>("TxtNodeCount");

            if (txtId != null) txtId.Text = $"0x{spatialId:X8}";
            if (txtCoords != null) txtCoords.Text = $"X: {_currentOffsetX:F0}, Y: {_currentOffsetY:F0}";
            if (txtCount != null)
            {
                txtCount.Text = $"{_currentGraph?.Nodes.Count ?? 0} / 1,000,000,000";
            }
        }

        // =========================================================
        // 5. EDITOR TOOLS & BUILD BUTTON
        // =========================================================

        // This links the XAML button to the ViewModel Logic via a Dialog
        private async void OnGenerateClicked(object? sender, RoutedEventArgs e)
        {
            await PromptAndBuild();
        }

        public async Task PromptAndBuild()
        {
            var msgBox = new Window { Title = "Commit & Generate?", Width = 400, Height = 200, WindowStartupLocation = WindowStartupLocation.CenterOwner, Background = SolidColorBrush.Parse("#1e1e1e") };
            var stack = new StackPanel { Margin = new Thickness(20), Spacing = 20 };
            stack.Children.Add(new TextBlock { Text = "This will generate the Akkadian Ecosystem (SQL, C#, Python).\nExisting files will be overwritten.\n\nProceed?", TextWrapping = TextWrapping.Wrap, Foreground = Brushes.White });

            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Spacing = 10 };
            var btnYes = new Button { Content = "Yes, Generate", Background = Brushes.Green, Foreground = Brushes.White };
            var btnNo = new Button { Content = "Cancel", Background = Brushes.Gray, Foreground = Brushes.White };

            bool confirmed = false;
            btnYes.Click += (s, e) => { confirmed = true; msgBox.Close(); };
            btnNo.Click += (s, e) => { msgBox.Close(); };

            btnPanel.Children.Add(btnNo);
            btnPanel.Children.Add(btnYes);
            stack.Children.Add(btnPanel);
            msgBox.Content = stack;

            await msgBox.ShowDialog(this);

            if (confirmed && DataContext is MainWindowViewModel vm)
            {
                await vm.BuildAndDeploy();
            }
        }

        private void OnTemplateChanged(object? sender, SelectionChangedEventArgs e)
        {
            // Handled dynamically in ShowTemplateGallery
        }

        private void OnToolPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is StackPanel panel && panel.Tag is string toolType)
            {
                _selectedTool = toolType;
                AddNodeToCanvas(toolType);
            }
        }

        private void AddNodeToCanvas(string nodeType)
        {
            if (_currentGraph == null) return;

            // --- SMART GRID PLACEMENT (No Overlap) ---
            int nodeCount = _currentGraph.Nodes.Count;
            int cols = 5;
            int spacing = 150;
            int startX = 100;
            int startY = 100;

            int row = nodeCount / cols;
            int col = nodeCount % cols;

            double x = startX + (col * spacing);
            double y = startY + (row * spacing);
            // -----------------------------------------

            string color = nodeType switch
            {
                "Valve" => "#00FF00",
                "Sensor" => "#FF0000",
                "Concept" => "#3498db",
                "Hub_Customer" => "#3498db",
                "Sat_Details" => "#f1c40f",
                _ => "#FFFFFF"
            };

            var newNode = new VisualNode
            {
                Id = $"{nodeType}_{DateTime.Now.Ticks}",
                Label = $"{nodeType} {nodeCount + 1}",
                X = x,
                Y = y,
                ColorHex = color
            };

            _currentGraph.Nodes.Add(newNode);

            // Sync ViewModel
            if (DataContext is MainWindowViewModel vm)
            {
                vm.ActiveGraph = _currentGraph;
            }

            var nativeCanvas = this.FindControl<GraphCanvas>("NativeCanvas");
            if (nativeCanvas != null) nativeCanvas.RenderGraph(_currentGraph);
        }

        // =========================================================
        // 6. TEMPLATE GALLERY & LAYOUTS (FULL Implementation)
        // =========================================================
        private void ShowTemplateGallery(object? sender, RoutedEventArgs e)
        {
            string selectedEngine = "Cytoscape";
            var combo = this.FindControl<ComboBox>("TemplateSelector");
            if (combo != null && combo.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                selectedEngine = tag;
            }

            if (_currentGraph == null || _currentGraph.Nodes.Count == 0) return;

            var galleryWindow = new Window
            {
                Title = $"{selectedEngine} Template Gallery",
                Width = 1000,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = new SolidColorBrush(Color.Parse("#1e1e1e"))
            };

            var mainPanel = new DockPanel { Margin = new Thickness(20) };
            var titlePanel = new StackPanel { Spacing = 10 };
            titlePanel.Children.Add(new TextBlock { Text = "Choose Visualization Template", FontSize = 24, FontWeight = FontWeight.Bold, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center });
            DockPanel.SetDock(titlePanel, Dock.Top);
            mainPanel.Children.Add(titlePanel);

            var templateGrid = new UniformGrid { Columns = 3, Rows = 2 };

            // Dynamic Buttons based on Engine
            if (selectedEngine.Contains("ThreeJS"))
            {
                AddTemplateButton(templateGrid, "3D Molecular", "ThreeJS_Molecular", "Nodes as atoms in 3D space");
                AddTemplateButton(templateGrid, "3D City", "ThreeJS_City", "Data represented as buildings");
                AddTemplateButton(templateGrid, "Galaxy", "ThreeJS_Galaxy", "Spiral galaxy layout");
            }
            else if (selectedEngine.Contains("D3"))
            {
                AddTemplateButton(templateGrid, "Force Cluster", "D3_Cluster", "Physics clustering by group");
                AddTemplateButton(templateGrid, "Sankey Flow", "D3_Sankey", "Left-to-right flow diagram");
                AddTemplateButton(templateGrid, "Sunburst", "D3_Sunburst", "Radial hierarchy visualization");
            }
            else // Cytoscape (Default)
            {
                AddTemplateButton(templateGrid, "Force-Directed", "Cytoscape_ForceDirected", "Organic, physics-based layout");
                AddTemplateButton(templateGrid, "Hierarchical", "Cytoscape_Hierarchical", "Tree-like parent-child structure");
                AddTemplateButton(templateGrid, "Compound", "Cytoscape_Compound", "Nested groups and containers");
                AddTemplateButton(templateGrid, "Radial", "Cytoscape_Radial", "Circular layout around central hub");
                AddTemplateButton(templateGrid, "Network Grid", "Cytoscape_Network", "Ordered grid layout");
            }

            mainPanel.Children.Add(templateGrid);
            galleryWindow.Content = mainPanel;
            galleryWindow.Show();
        }

        private void AddTemplateButton(UniformGrid grid, string name, string template, string description)
        {
            var button = new Button
            {
                Margin = new Thickness(10),
                Padding = new Thickness(0),
                Background = new SolidColorBrush(Color.Parse("#2c3e50")),
                BorderBrush = new SolidColorBrush(Color.Parse("#3498db")),
                BorderThickness = new Thickness(2),
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch
            };

            var content = new DockPanel();

            // Preview Image Placeholder
            var previewBorder = new Border { Height = 120, Background = new SolidColorBrush(Color.Parse("#34495e")) };
            string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Extensions", template, "preview.png");

            if (File.Exists(imagePath))
            {
                try { previewBorder.Child = new Avalonia.Controls.Image { Source = new Bitmap(imagePath), Stretch = Stretch.UniformToFill }; } catch { }
            }
            else
            {
                previewBorder.Child = new TextBlock { Text = "📊", FontSize = 40, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            }

            DockPanel.SetDock(previewBorder, Dock.Top);
            content.Children.Add(previewBorder);

            var infoPanel = new StackPanel { Spacing = 5, Margin = new Thickness(10), Background = new SolidColorBrush(Color.Parse("#2c3e50")) };
            infoPanel.Children.Add(new TextBlock { Text = name, FontSize = 14, FontWeight = FontWeight.Bold, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center });
            infoPanel.Children.Add(new TextBlock { Text = description, FontSize = 10, Foreground = Brushes.LightGray, HorizontalAlignment = HorizontalAlignment.Center, TextWrapping = TextWrapping.Wrap });
            content.Children.Add(infoPanel);

            button.Content = content;
            button.Click += (s, e) => LoadCytoscapeTemplate(template);

            grid.Children.Add(button);
        }

        private void LoadCytoscapeTemplate(string templateName)
        {
            if (_currentGraph == null) return;

            var vizWindow = new Window
            {
                Title = $"Ontoway Visualizer - {templateName}",
                Width = 1200,
                Height = 800,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = new SolidColorBrush(Color.Parse("#1e1e1e"))
            };

            var canvas = new GraphCanvas();
            var layoutGraph = ApplyLayout(_currentGraph, templateName);
            canvas.RenderGraph(layoutGraph);
            vizWindow.Content = canvas;
            vizWindow.Show();
        }

        // =========================================================
        // 7. LAYOUT ALGORITHMS (FULL IMPLEMENTATION)
        // =========================================================
        private GraphTopology ApplyLayout(GraphTopology graph, string templateName)
        {
            var layoutGraph = new GraphTopology();
            foreach (var node in graph.Nodes)
                layoutGraph.Nodes.Add(new VisualNode { Id = node.Id, Label = node.Label, ColorHex = node.ColorHex, X = node.X, Y = node.Y });
            foreach (var edge in graph.Edges)
                layoutGraph.Edges.Add(new VisualEdge { SourceId = edge.SourceId, TargetId = edge.TargetId });

            switch (templateName)
            {
                case "Cytoscape_ForceDirected": ApplyForceDirectedLayout(layoutGraph); break;
                case "Cytoscape_Hierarchical": ApplyHierarchicalLayout(layoutGraph); break;
                case "Cytoscape_Radial": ApplyRadialLayout(layoutGraph); break;
                case "Cytoscape_Network": ApplyNetworkLayout(layoutGraph); break;
                case "Cytoscape_Compound": ApplyCompoundLayout(layoutGraph); break;

                // New D3/ThreeJS Simulators (Math-based approximation for now)
                case "D3_Cluster": ApplyCompoundLayout(layoutGraph); break;
                case "ThreeJS_Molecular": ApplyRadialLayout(layoutGraph); break;
            }
            return layoutGraph;
        }

        private void ApplyForceDirectedLayout(GraphTopology graph)
        {
            var r = new Random();
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                graph.Nodes[i].X = 600 + r.Next(-250, 250);
                graph.Nodes[i].Y = 400 + r.Next(-200, 200);
            }
        }

        private void ApplyHierarchicalLayout(GraphTopology graph)
        {
            if (graph.Nodes.Count == 0) return;
            int levels = (int)Math.Ceiling(Math.Sqrt(graph.Nodes.Count));
            int nodesPerLevel = (int)Math.Ceiling((double)graph.Nodes.Count / levels);
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                int level = i / nodesPerLevel;
                int pos = i % nodesPerLevel;
                graph.Nodes[i].X = 200 + (800.0 / (nodesPerLevel + 1)) * (pos + 1);
                graph.Nodes[i].Y = 100 + level * 150;
            }
        }

        private void ApplyRadialLayout(GraphTopology graph)
        {
            if (graph.Nodes.Count == 0) return;
            double cx = 600, cy = 400, rad = 300;
            // Put first node in center
            graph.Nodes[0].X = cx; graph.Nodes[0].Y = cy;

            for (int i = 1; i < graph.Nodes.Count; i++)
            {
                double angle = 2 * Math.PI * (i - 1) / (graph.Nodes.Count - 1);
                graph.Nodes[i].X = cx + rad * Math.Cos(angle);
                graph.Nodes[i].Y = cy + rad * Math.Sin(angle);
            }
        }

        private void ApplyNetworkLayout(GraphTopology graph)
        {
            int cols = (int)Math.Ceiling(Math.Sqrt(graph.Nodes.Count));
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                int row = i / cols;
                int col = i % cols;
                graph.Nodes[i].X = 100 + col * 150;
                graph.Nodes[i].Y = 100 + row * 150;
            }
        }

        private void ApplyCompoundLayout(GraphTopology graph)
        {
            int groupSize = Math.Max(2, graph.Nodes.Count / 3);
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                int group = i / groupSize;
                int pos = i % groupSize;
                double gx = 200 + (group % 3) * 350;
                double gy = 200 + (group / 3) * 300;

                // Arrange in circle within group
                double angle = 2 * Math.PI * pos / groupSize;
                graph.Nodes[i].X = gx + 100 * Math.Cos(angle);
                graph.Nodes[i].Y = gy + 100 * Math.Sin(angle);
            }
        }
    }
}