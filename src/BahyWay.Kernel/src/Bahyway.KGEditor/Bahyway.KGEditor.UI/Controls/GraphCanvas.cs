using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Layout;
using Bahyway.SharedKernel.Graph;
using System;
using System.Linq;
using System.Collections.Generic;


namespace Bahyway.KGEditor.UI.Controls
{
    public class GraphCanvas : Control
    {
        private GraphTopology? _graph;
        private VisualNode? _draggedNode;
        private Point _dragStartPos;
        private bool _isDragging = false;

        private bool _isDrawingEdge = false;
        private VisualNode? _edgeSourceNode;
        private Point _currentMousePos;

        private ContextMenu? _nodeContextMenu;
        private VisualNode? _contextMenuNode;

        public GraphCanvas()
        {
            //Background = Brushes.Transparent;

            this.PointerPressed += OnPointerPressed;
            this.PointerMoved += OnPointerMoved;
            this.PointerReleased += OnPointerReleased;

            CreateContextMenu();
        }

        private void CreateContextMenu()
        {
            _nodeContextMenu = new ContextMenu();

            var editPropsItem = new MenuItem { Header = "Edit Properties" };
            editPropsItem.Click += (s, e) => ShowNodePropertiesDialog();

            var drawEdgeItem = new MenuItem { Header = "Draw Edge From Here" };
            drawEdgeItem.Click += (s, e) => StartDrawingEdge();

            var deleteItem = new MenuItem { Header = "Delete Node" };
            deleteItem.Click += (s, e) => DeleteNode();

            var separator = new Separator();

            var colorMenu = new MenuItem { Header = "Change Color" };

            var greenItem = new MenuItem { Header = "Green" };
            greenItem.Click += (s, e) => ChangeNodeColor("#00FF00");

            var redItem = new MenuItem { Header = "Red" };
            redItem.Click += (s, e) => ChangeNodeColor("#FF0000");

            var blueItem = new MenuItem { Header = "Blue" };
            blueItem.Click += (s, e) => ChangeNodeColor("#3498db");

            var yellowItem = new MenuItem { Header = "Yellow" };
            yellowItem.Click += (s, e) => ChangeNodeColor("#FFFF00");

            // Fix: Use Items.Add() instead of assignment
            colorMenu.Items.Add(greenItem);
            colorMenu.Items.Add(redItem);
            colorMenu.Items.Add(blueItem);
            colorMenu.Items.Add(yellowItem);

            _nodeContextMenu.Items.Add(editPropsItem);
            _nodeContextMenu.Items.Add(drawEdgeItem);
            _nodeContextMenu.Items.Add(separator);
            _nodeContextMenu.Items.Add(colorMenu);
            _nodeContextMenu.Items.Add(deleteItem);
        }

        //private void ShowNodePropertiesDialog()
        //{
        //    if (_contextMenuNode == null) return;

        //    var dialog = new Window
        //    {
        //        Title = "Node Properties",
        //        Width = 400,
        //        Height = 300,
        //        WindowStartupLocation = WindowStartupLocation.CenterOwner,
        //        CanResize = false
        //    };

        //    var panel = new StackPanel { Margin = new Thickness(20), Spacing = 10 };

        //    // ID
        //    panel.Children.Add(new TextBlock
        //    {
        //        Text = "ID:",
        //        Foreground = Brushes.Gray
        //    });

        //    var idBox = new TextBox
        //    {
        //        Text = _contextMenuNode.Id,
        //        IsReadOnly = true
        //    };
        //    panel.Children.Add(idBox);

        //    // Label
        //    panel.Children.Add(new TextBlock
        //    {
        //        Text = "Label:",
        //        Foreground = Brushes.Gray,
        //        Margin = new Thickness(0, 10, 0, 0)
        //    });

        //    var labelBox = new TextBox
        //    {
        //        Text = _contextMenuNode.Label
        //    };
        //    panel.Children.Add(labelBox);

        //    // X Position
        //    panel.Children.Add(new TextBlock
        //    {
        //        Text = "X Position:",
        //        Foreground = Brushes.Gray,
        //        Margin = new Thickness(0, 10, 0, 0)
        //    });

        //    var xBox = new TextBox
        //    {
        //        Text = _contextMenuNode.X.ToString("F0")
        //    };
        //    panel.Children.Add(xBox);

        //    // Y Position
        //    panel.Children.Add(new TextBlock
        //    {
        //        Text = "Y Position:",
        //        Foreground = Brushes.Gray,
        //        Margin = new Thickness(0, 10, 0, 0)
        //    });

        //    var yBox = new TextBox
        //    {
        //        Text = _contextMenuNode.Y.ToString("F0")
        //    };
        //    panel.Children.Add(yBox);

        //    // Buttons
        //    var buttonPanel = new StackPanel
        //    {
        //        Orientation = Orientation.Horizontal,
        //        Spacing = 10,
        //        HorizontalAlignment = HorizontalAlignment.Right,
        //        Margin = new Thickness(0, 20, 0, 0)
        //    };

        //    var saveBtn = new Button { Content = "Save", Width = 80 };
        //    saveBtn.Click += (s, e) =>
        //    {
        //        _contextMenuNode.Label = labelBox.Text ?? _contextMenuNode.Id;
        //        if (double.TryParse(xBox.Text, out double x)) _contextMenuNode.X = x;
        //        if (double.TryParse(yBox.Text, out double y)) _contextMenuNode.Y = y;
        //        InvalidateVisual();
        //        dialog.Close();
        //    };

        //    var cancelBtn = new Button { Content = "Cancel", Width = 80 };
        //    cancelBtn.Click += (s, e) => dialog.Close();

        //    buttonPanel.Children.Add(saveBtn);
        //    buttonPanel.Children.Add(cancelBtn);
        //    panel.Children.Add(buttonPanel);

        //    dialog.Content = panel;
        //    dialog.ShowDialog((Window)this.VisualRoot!);
        //}
        private void ShowNodePropertiesDialog()
        {
            if (_contextMenuNode == null) return;

            var dialog = new Window
            {
                Title = "Node Properties",
                Width = 450,
                Height = 400,  // ← Made taller
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            // Main container
            var mainPanel = new DockPanel { Margin = new Thickness(20) };

            // Buttons at bottom
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var saveBtn = new Button { Content = "Save", Width = 80, Height = 30 };
            saveBtn.Click += (s, e) =>
            {
                // Get references to text boxes
                var content = dialog.Content as DockPanel;
                var scroll = content?.Children.OfType<ScrollViewer>().FirstOrDefault();
                var panel = scroll?.Content as StackPanel;

                if (panel != null)
                {
                    var labelBox = panel.Children.OfType<TextBox>().ElementAtOrDefault(1);
                    var xBox = panel.Children.OfType<TextBox>().ElementAtOrDefault(2);
                    var yBox = panel.Children.OfType<TextBox>().ElementAtOrDefault(3);

                    if (labelBox != null)
                        _contextMenuNode.Label = labelBox.Text ?? _contextMenuNode.Id;
                    if (xBox != null && double.TryParse(xBox.Text, out double x))
                        _contextMenuNode.X = x;
                    if (yBox != null && double.TryParse(yBox.Text, out double y))
                        _contextMenuNode.Y = y;
                }

                InvalidateVisual();
                dialog.Close();
            };

            var cancelBtn = new Button { Content = "Cancel", Width = 80, Height = 30 };
            cancelBtn.Click += (s, e) => dialog.Close();

            buttonPanel.Children.Add(saveBtn);
            buttonPanel.Children.Add(cancelBtn);

            DockPanel.SetDock(buttonPanel, Dock.Bottom);
            mainPanel.Children.Add(buttonPanel);

            // Content panel with ScrollViewer
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
            };

            var contentPanel = new StackPanel { Spacing = 10 };

            // ID (read-only)
            contentPanel.Children.Add(new TextBlock
            {
                Text = "ID:",
                Foreground = Brushes.Gray,
                FontWeight = FontWeight.Bold
            });

            var idBox = new TextBox
            {
                Text = _contextMenuNode.Id,
                IsReadOnly = true,
                Foreground = Brushes.Gray
            };
            contentPanel.Children.Add(idBox);

            // Label
            contentPanel.Children.Add(new TextBlock
            {
                Text = "Label:",
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 15, 0, 0),
                FontWeight = FontWeight.Bold
            });

            var labelBox = new TextBox
            {
                Text = _contextMenuNode.Label,
                Watermark = "Enter node label..."
            };
            contentPanel.Children.Add(labelBox);

            // X Position
            contentPanel.Children.Add(new TextBlock
            {
                Text = "X Position:",
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 15, 0, 0),
                FontWeight = FontWeight.Bold
            });

            var xBox = new TextBox
            {
                Text = _contextMenuNode.X.ToString("F0"),
                Watermark = "X coordinate"
            };
            contentPanel.Children.Add(xBox);

            // Y Position
            contentPanel.Children.Add(new TextBlock
            {
                Text = "Y Position:",
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 15, 0, 0),
                FontWeight = FontWeight.Bold
            });

            var yBox = new TextBox
            {
                Text = _contextMenuNode.Y.ToString("F0"),
                Watermark = "Y coordinate"
            };
            contentPanel.Children.Add(yBox);

            // Color preview
            contentPanel.Children.Add(new TextBlock
            {
                Text = "Current Color:",
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 15, 0, 0),
                FontWeight = FontWeight.Bold
            });

            var colorPreview = new Border
            {
                Width = 50,
                Height = 50,
                Background = new SolidColorBrush(Color.Parse(_contextMenuNode.ColorHex)),
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(25),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            contentPanel.Children.Add(colorPreview);

            scrollViewer.Content = contentPanel;
            mainPanel.Children.Add(scrollViewer);

            dialog.Content = mainPanel;
            dialog.ShowDialog((Window)this.VisualRoot!);
        }
        private void StartDrawingEdge()
        {
            if (_contextMenuNode == null) return;

            _isDrawingEdge = true;
            _edgeSourceNode = _contextMenuNode;
            System.Diagnostics.Debug.WriteLine($"Started edge drawing from: {_contextMenuNode.Label}");
            InvalidateVisual();
        }

        private void DeleteNode()
        {
            if (_contextMenuNode == null || _graph == null) return;

            _graph.Nodes.Remove(_contextMenuNode);

            var edgesToRemove = _graph.Edges
                .Where(e => e.SourceId == _contextMenuNode.Id || e.TargetId == _contextMenuNode.Id)
                .ToList();

            foreach (var edge in edgesToRemove)
            {
                _graph.Edges.Remove(edge);
            }

            System.Diagnostics.Debug.WriteLine($"Deleted node: {_contextMenuNode.Label}");
            _contextMenuNode = null;
            InvalidateVisual();
        }

        private void ChangeNodeColor(string colorHex)
        {
            if (_contextMenuNode == null) return;

            _contextMenuNode.ColorHex = colorHex;
            System.Diagnostics.Debug.WriteLine($"Changed {_contextMenuNode.Label} color to {colorHex}");
            InvalidateVisual();
        }

        public void RenderGraph(GraphTopology graph)
        {
            _graph = graph;
            InvalidateVisual();
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (_graph == null) return;

            DrawGrid(context);

            foreach (var edge in _graph.Edges)
            {
                var source = _graph.Nodes.FirstOrDefault(n => n.Id == edge.SourceId);
                var target = _graph.Nodes.FirstOrDefault(n => n.Id == edge.TargetId);

                if (source != null && target != null)
                {
                    DrawEdge(context, source, target);
                }
            }

            if (_isDrawingEdge && _edgeSourceNode != null)
            {
                var pen = new Pen(Brushes.Yellow, 2, lineCap: PenLineCap.Round);
                context.DrawLine(pen,
                    new Point(_edgeSourceNode.X, _edgeSourceNode.Y),
                    _currentMousePos);
            }

            foreach (var node in _graph.Nodes)
            {
                DrawNode(context, node, node == _draggedNode || node == _edgeSourceNode);
            }
        }

        private void DrawGrid(DrawingContext context)
        {
            var gridPen = new Pen(new SolidColorBrush(Color.Parse("#333333")), 1);
            int gridSize = 50;

            for (int x = 0; x < Bounds.Width; x += gridSize)
            {
                context.DrawLine(gridPen, new Point(x, 0), new Point(x, Bounds.Height));
            }

            for (int y = 0; y < Bounds.Height; y += gridSize)
            {
                context.DrawLine(gridPen, new Point(0, y), new Point(Bounds.Width, y));
            }
        }

        private void DrawNode(DrawingContext context, VisualNode node, bool isHighlighted)
        {
            var color = Color.Parse(node.ColorHex);
            var brush = new SolidColorBrush(color);
            var strokeBrush = isHighlighted ? Brushes.Yellow : Brushes.White;
            var strokeThickness = isHighlighted ? 3 : 2;

            var center = new Point(node.X, node.Y);
            context.DrawEllipse(brush, new Pen(strokeBrush, strokeThickness), center, 20, 20);

            // Fix: Use proper FormattedText constructor
            var typeface = new Typeface("Arial");
            var text = new FormattedText(
                node.Label ?? node.Id,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                12,
                Brushes.White
            );

            var textPos = new Point(node.X - text.Width / 2, node.Y + 25);
            context.DrawText(text, textPos);
        }

        private void DrawEdge(DrawingContext context, VisualNode source, VisualNode target)
        {
            var pen = new Pen(new SolidColorBrush(Color.Parse("#999999")), 2);

            context.DrawLine(pen,
                new Point(source.X, source.Y),
                new Point(target.X, target.Y));

            DrawArrow(context, source, target);
        }

        private void DrawArrow(DrawingContext context, VisualNode source, VisualNode target)
        {
            double angle = Math.Atan2(target.Y - source.Y, target.X - source.X);
            double arrowLength = 10;
            double arrowAngle = Math.PI / 6;

            double endX = target.X - 20 * Math.Cos(angle);
            double endY = target.Y - 20 * Math.Sin(angle);

            Point arrowTip = new Point(endX, endY);
            Point arrowLeft = new Point(
                endX - arrowLength * Math.Cos(angle - arrowAngle),
                endY - arrowLength * Math.Sin(angle - arrowAngle)
            );
            Point arrowRight = new Point(
                endX - arrowLength * Math.Cos(angle + arrowAngle),
                endY - arrowLength * Math.Sin(angle + arrowAngle)
            );

            var segments = new PathSegments();
            segments.Add(new LineSegment { Point = arrowLeft });
            segments.Add(new LineSegment { Point = arrowRight });

            var figure = new PathFigure
            {
                StartPoint = arrowTip,
                Segments = segments,
                IsClosed = true
            };

            var figures = new PathFigures();
            figures.Add(figure);

            var geometry = new PathGeometry { Figures = figures };

            context.DrawGeometry(Brushes.Gray, null, geometry);
        }

        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (_graph == null) return;

            var point = e.GetPosition(this);
            var clickedNode = FindNodeAtPosition(point);

            var props = e.GetCurrentPoint(this).Properties;

            if (props.IsRightButtonPressed)
            {
                if (clickedNode != null)
                {
                    _contextMenuNode = clickedNode;
                    _nodeContextMenu?.Open(this);
                    e.Handled = true;
                }
                return;
            }

            if (props.IsLeftButtonPressed && clickedNode != null)
            {
                _isDragging = true;
                _draggedNode = clickedNode;
                _dragStartPos = point;
                InvalidateVisual();
                e.Handled = true;
            }
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            var point = e.GetPosition(this);

            if (_isDrawingEdge)
            {
                _currentMousePos = point;
                InvalidateVisual();
                return;
            }

            if (_isDragging && _draggedNode != null)
            {
                _draggedNode.X = point.X;
                _draggedNode.Y = point.Y;
                InvalidateVisual();
            }
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (_graph == null) return;

            var point = e.GetPosition(this);

            if (_isDrawingEdge && _edgeSourceNode != null)
            {
                var targetNode = FindNodeAtPosition(point);

                if (targetNode != null && targetNode != _edgeSourceNode)
                {
                    var newEdge = new VisualEdge
                    {
                        SourceId = _edgeSourceNode.Id,
                        TargetId = targetNode.Id
                    };

                    _graph.Edges.Add(newEdge);
                    System.Diagnostics.Debug.WriteLine($"Created edge: {_edgeSourceNode.Label} → {targetNode.Label}");
                }

                _isDrawingEdge = false;
                _edgeSourceNode = null;
                InvalidateVisual();
                e.Handled = true;
                return;
            }

            if (_isDragging)
            {
                _isDragging = false;
                _draggedNode = null;
                e.Handled = true;
            }
        }

        private VisualNode? FindNodeAtPosition(Point position)
        {
            if (_graph == null) return null;

            foreach (var node in _graph.Nodes)
            {
                double distance = Math.Sqrt(
                    Math.Pow(position.X - node.X, 2) +
                    Math.Pow(position.Y - node.Y, 2)
                );

                if (distance <= 20)
                {
                    return node;
                }
            }

            return null;
        }
    }
}