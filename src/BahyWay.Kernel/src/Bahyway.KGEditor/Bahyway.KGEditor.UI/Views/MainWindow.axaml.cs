using Akka.Actor;
using Akka.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Bahyway.KGEditor.UI.Actors;
using Bahyway.KGEditor.UI.Controls;
using Bahyway.SharedKernel.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bahyway.KGEditor.UI.Views
{
    public partial class MainWindow : Window
    {
        private ActorSystem? _uiSystem;
        private GraphTopology? _currentGraph;
        private string? _selectedTool = null;
        private Window? _currentVizWindow = null;

        public MainWindow()
        {
            InitializeComponent();
            this.Opened += OnWindowOpened;
            this.Closing += OnWindowClosing;
        }

        private void OnWindowOpened(object? sender, EventArgs e)
        {
            // Generate preview images if they don't exist
            string extensionsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Extensions");
            try
            {
                Bahyway.KGEditor.UI.Services.PreviewGenerator.GenerateAllPreviews(extensionsPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Preview generation error: {ex.Message}");
            }

            // Initialize Graph Data
            _currentGraph = new GraphTopology();
            _currentGraph.Nodes.Add(new VisualNode { Id = "Valve_A", Label = "Valve A", X = 300, Y = 100, ColorHex = "#00FF00" });
            _currentGraph.Nodes.Add(new VisualNode { Id = "Sensor_03", Label = "Sensor 03", X = 500, Y = 500, ColorHex = "#333333" });
            _currentGraph.Edges.Add(new VisualEdge { SourceId = "Valve_A", TargetId = "Sensor_03" });

            // Render to canvas
            var canvas = this.FindControl<GraphCanvas>("NativeCanvas");
            if (canvas != null) canvas.RenderGraph(_currentGraph);

            // Start Akka system
            var config = ConfigurationFactory.ParseString(@"
                akka {
                    actor.provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                    remote.dot-netty.tcp { port = 8091, hostname = ""localhost"" }
                }
            ");
            _uiSystem = ActorSystem.Create("BahywayUI", config);
            if (canvas != null && _currentGraph != null)
            {
                _uiSystem.ActorOf(Props.Create(() => new UIBridgeActor(canvas, _currentGraph)), "bridge");
            }
        }

        private void OnTemplateChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem item)
            {
                string? templateName = item.Tag as string;
                if (templateName != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Template selected: {templateName}");
                }
            }
        }

        private void OnToolPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is StackPanel panel && panel.Tag is string toolType)
            {
                System.Diagnostics.Debug.WriteLine($"Tool selected: {toolType}");
                _selectedTool = toolType;
                AddNodeToCanvas(toolType);
            }
        }

        private void AddNodeToCanvas(string nodeType)
        {
            if (_currentGraph == null) return;

            Random rand = new Random();
            int x = rand.Next(100, 700);
            int y = rand.Next(100, 500);

            string color = nodeType switch
            {
                "Valve" => "#00FF00",
                "Sensor" => "#FF0000",
                "Concept" => "#3498db",
                _ => "#FFFFFF"
            };

            var newNode = new VisualNode
            {
                Id = $"{nodeType}_{DateTime.Now.Ticks}",
                Label = $"{nodeType} {_currentGraph.Nodes.Count + 1}",
                X = x,
                Y = y,
                ColorHex = color
            };

            _currentGraph.Nodes.Add(newNode);

            var nativeCanvas = this.FindControl<GraphCanvas>("NativeCanvas");
            if (nativeCanvas != null)
            {
                nativeCanvas.RenderGraph(_currentGraph);
            }

            System.Diagnostics.Debug.WriteLine($"Added node: {newNode.Id} at ({x}, {y})");
        }

        private void ShowTemplateGallery(object? sender, RoutedEventArgs e)
        {
            if (_currentGraph == null || _currentGraph.Nodes.Count == 0)
            {
                var msgBox = new Window
                {
                    Title = "No Data",
                    Width = 300,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    CanResize = false
                };

                var panel = new StackPanel
                {
                    Margin = new Thickness(20),
                    Spacing = 15,
                    VerticalAlignment = VerticalAlignment.Center
                };

                panel.Children.Add(new TextBlock
                {
                    Text = "Please create some nodes first!",
                    TextAlignment = TextAlignment.Center,
                    FontSize = 14
                });

                var okBtn = new Button
                {
                    Content = "OK",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Width = 80
                };
                okBtn.Click += (s, args) => msgBox.Close();
                panel.Children.Add(okBtn);

                msgBox.Content = panel;
                msgBox.ShowDialog(this);
                return;
            }

            var galleryWindow = new Window
            {
                Title = "Cytoscape Template Gallery",
                Width = 1000,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var mainPanel = new DockPanel { Margin = new Thickness(20) };
            var titlePanel = new StackPanel { Spacing = 10 };

            titlePanel.Children.Add(new TextBlock
            {
                Text = "Choose a Visualization Template",
                FontSize = 24,
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            titlePanel.Children.Add(new TextBlock
            {
                Text = $"Current graph: {_currentGraph.Nodes.Count} nodes, {_currentGraph.Edges.Count} edges",
                FontSize = 14,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            });

            DockPanel.SetDock(titlePanel, Dock.Top);
            mainPanel.Children.Add(titlePanel);

            var templateGrid = new UniformGrid { Columns = 3, Rows = 2 };

            AddTemplateButton(templateGrid, "Force-Directed", "Cytoscape_ForceDirected",
                "Dynamic network layout\nIdeal for general graphs");
            AddTemplateButton(templateGrid, "Hierarchical", "Cytoscape_Hierarchical",
                "Tree-like structure\nBest for parent-child relationships");
            AddTemplateButton(templateGrid, "Radial", "Cytoscape_Radial",
                "Circular layout\nGood for central node focus");
            AddTemplateButton(templateGrid, "Network", "Cytoscape_Network",
                "Network analysis style\nFor complex connections");
            AddTemplateButton(templateGrid, "Compound", "Cytoscape_Compound",
                "Nested groups\nSupports hierarchical grouping");

            mainPanel.Children.Add(templateGrid);
            galleryWindow.Content = mainPanel;
            galleryWindow.ShowDialog(this);
        }

        private void AddTemplateButton(UniformGrid grid, string name, string template, string description)
        {
            var button = new Button
            {
                Margin = new Thickness(10),
                Padding = new Thickness(0),
                Background = new SolidColorBrush(Color.Parse("#1e1e1e")),
                BorderBrush = new SolidColorBrush(Color.Parse("#3498db")),
                BorderThickness = new Thickness(2),
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch
            };

            var content = new DockPanel();
            var previewBorder = new Border
            {
                Height = 150,
                Background = new SolidColorBrush(Color.Parse("#2c3e50")),
                BorderBrush = new SolidColorBrush(Color.Parse("#34495e")),
                BorderThickness = new Thickness(0, 0, 0, 1)
            };

            string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Extensions", template, "preview.png");

            if (File.Exists(imagePath))
            {
                var image = new Avalonia.Controls.Image
                {
                    Source = new Avalonia.Media.Imaging.Bitmap(imagePath),
                    Stretch = Avalonia.Media.Stretch.UniformToFill
                };
                previewBorder.Child = image;
            }
            else
            {
                var placeholder = new StackPanel
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                placeholder.Children.Add(new TextBlock
                {
                    Text = "📊",
                    FontSize = 48,
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                placeholder.Children.Add(new TextBlock
                {
                    Text = "Preview",
                    FontSize = 12,
                    Foreground = Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 10, 0, 0)
                });
                previewBorder.Child = placeholder;
            }

            DockPanel.SetDock(previewBorder, Dock.Top);
            content.Children.Add(previewBorder);

            var infoPanel = new StackPanel
            {
                Spacing = 5,
                Margin = new Thickness(15, 10, 15, 10),
                Background = new SolidColorBrush(Color.Parse("#1e1e1e"))
            };

            infoPanel.Children.Add(new TextBlock
            {
                Text = name,
                FontSize = 16,
                FontWeight = FontWeight.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            infoPanel.Children.Add(new TextBlock
            {
                Text = description,
                FontSize = 11,
                Foreground = Brushes.LightGray,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center
            });

            content.Children.Add(infoPanel);
            button.Content = content;
            button.Click += (s, e) => LoadCytoscapeTemplate(template);

            button.PointerEntered += (s, e) =>
            {
                button.BorderBrush = new SolidColorBrush(Color.Parse("#2ecc71"));
                button.BorderThickness = new Thickness(3);
            };
            button.PointerExited += (s, e) =>
            {
                button.BorderBrush = new SolidColorBrush(Color.Parse("#3498db"));
                button.BorderThickness = new Thickness(2);
            };

            grid.Children.Add(button);
        }

        private void LoadCytoscapeTemplate(string templateName)
        {
            System.Diagnostics.Debug.WriteLine($"Loading template: {templateName}");

            if (_currentGraph == null) return;

            // Close previous visualization window
            if (_currentVizWindow != null && _currentVizWindow.IsVisible)
            {
                _currentVizWindow.Close();
                _currentVizWindow = null;
            }

            var vizWindow = new Window
            {
                Title = $"Ontoway - {templateName}",
                Width = 1200,
                Height = 800,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = new SolidColorBrush(Color.Parse("#1e1e1e"))
            };

            _currentVizWindow = vizWindow;

            vizWindow.Closing += (s, e) =>
            {
                if (_currentVizWindow == vizWindow)
                {
                    _currentVizWindow = null;
                }
            };

            var canvas = new GraphCanvas();
            var layoutGraph = ApplyLayout(_currentGraph, templateName);
            canvas.RenderGraph(layoutGraph);

            vizWindow.Content = canvas;
            vizWindow.Show();

            System.Diagnostics.Debug.WriteLine($"Opened {templateName} visualization window");
        }

        private GraphTopology ApplyLayout(GraphTopology graph, string templateName)
        {
            var layoutGraph = new GraphTopology();

            foreach (var node in graph.Nodes)
            {
                layoutGraph.Nodes.Add(new VisualNode
                {
                    Id = node.Id,
                    Label = node.Label,
                    ColorHex = node.ColorHex,
                    X = node.X,
                    Y = node.Y
                });
            }

            foreach (var edge in graph.Edges)
            {
                layoutGraph.Edges.Add(new VisualEdge
                {
                    SourceId = edge.SourceId,
                    TargetId = edge.TargetId
                });
            }

            switch (templateName)
            {
                case "Cytoscape_ForceDirected":
                    ApplyForceDirectedLayout(layoutGraph);
                    break;
                case "Cytoscape_Hierarchical":
                    ApplyHierarchicalLayout(layoutGraph);
                    break;
                case "Cytoscape_Radial":
                    ApplyRadialLayout(layoutGraph);
                    break;
                case "Cytoscape_Network":
                    ApplyNetworkLayout(layoutGraph);
                    break;
                case "Cytoscape_Compound":
                    ApplyCompoundLayout(layoutGraph);
                    break;
            }

            return layoutGraph;
        }

        private void ApplyForceDirectedLayout(GraphTopology graph)
        {
            var random = new Random(42);
            int i = 0;

            foreach (var node in graph.Nodes)
            {
                double angle = 2 * Math.PI * i / graph.Nodes.Count;
                double radius = 250 + random.Next(-30, 30);

                node.X = 600 + radius * Math.Cos(angle);
                node.Y = 400 + radius * Math.Sin(angle);
                i++;
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
                int positionInLevel = i % nodesPerLevel;
                int currentLevelCount = Math.Min(nodesPerLevel, graph.Nodes.Count - level * nodesPerLevel);

                double spacing = 1000.0 / (currentLevelCount + 1);

                graph.Nodes[i].X = 100 + spacing * (positionInLevel + 1);
                graph.Nodes[i].Y = 100 + level * 180;
            }
        }

        private void ApplyRadialLayout(GraphTopology graph)
        {
            if (graph.Nodes.Count == 0) return;

            var edgeCounts = new Dictionary<string, int>();
            foreach (var node in graph.Nodes)
                edgeCounts[node.Id] = 0;

            foreach (var edge in graph.Edges)
            {
                if (edgeCounts.ContainsKey(edge.SourceId))
                    edgeCounts[edge.SourceId]++;
                if (edgeCounts.ContainsKey(edge.TargetId))
                    edgeCounts[edge.TargetId]++;
            }

            var hubNode = graph.Nodes[0];
            int maxConnections = 0;

            foreach (var node in graph.Nodes)
            {
                if (edgeCounts[node.Id] > maxConnections)
                {
                    maxConnections = edgeCounts[node.Id];
                    hubNode = node;
                }
            }

            hubNode.X = 600;
            hubNode.Y = 400;

            var otherNodes = graph.Nodes.Where(n => n.Id != hubNode.Id).ToList();
            for (int i = 0; i < otherNodes.Count; i++)
            {
                double angle = 2 * Math.PI * i / otherNodes.Count;
                otherNodes[i].X = 600 + 280 * Math.Cos(angle);
                otherNodes[i].Y = 400 + 280 * Math.Sin(angle);
            }
        }

        private void ApplyNetworkLayout(GraphTopology graph)
        {
            int cols = (int)Math.Ceiling(Math.Sqrt(graph.Nodes.Count));
            int rows = (int)Math.Ceiling((double)graph.Nodes.Count / cols);

            double spacingX = 1000.0 / (cols + 1);
            double spacingY = 700.0 / (rows + 1);

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                int row = i / cols;
                int col = i % cols;

                graph.Nodes[i].X = 100 + spacingX * (col + 1);
                graph.Nodes[i].Y = 50 + spacingY * (row + 1);
            }
        }

        private void ApplyCompoundLayout(GraphTopology graph)
        {
            int groupSize = Math.Max(2, graph.Nodes.Count / 3);

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                int group = i / groupSize;
                int posInGroup = i % groupSize;

                double groupX = 300 + (group % 3) * 350;
                double groupY = 200 + (group / 3) * 300;

                double angle = 2 * Math.PI * posInGroup / groupSize;
                graph.Nodes[i].X = groupX + 80 * Math.Cos(angle);
                graph.Nodes[i].Y = groupY + 80 * Math.Sin(angle);
            }
        }

        private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _uiSystem?.Terminate();
        }
    }
}