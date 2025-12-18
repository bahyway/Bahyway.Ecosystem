using SkiaSharp;
using System;
using System.IO;

namespace Bahyway.KGEditor.UI.Services
{
    public static class PreviewGenerator
    {
        private const int WIDTH = 400;
        private const int HEIGHT = 300;

        public static void GenerateAllPreviews(string extensionsPath)
        {
            GenerateForceDirectedPreview(Path.Combine(extensionsPath, "Cytoscape_ForceDirected", "preview.png"));
            GenerateHierarchicalPreview(Path.Combine(extensionsPath, "Cytoscape_Hierarchical", "preview.png"));
            GenerateRadialPreview(Path.Combine(extensionsPath, "Cytoscape_Radial", "preview.png"));
            GenerateNetworkPreview(Path.Combine(extensionsPath, "Cytoscape_Network", "preview.png"));
            GenerateCompoundPreview(Path.Combine(extensionsPath, "Cytoscape_Compound", "preview.png"));
        }

        private static void GenerateForceDirectedPreview(string outputPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            using var surface = SKSurface.Create(new SKImageInfo(WIDTH, HEIGHT));
            var canvas = surface.Canvas;
            canvas.Clear(SKColor.Parse("#1e1e1e"));

            var random = new Random(42);
            var nodes = new (float x, float y)[12];

            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = (random.Next(50, WIDTH - 50), random.Next(50, HEIGHT - 50));
            }

            // Draw edges (organic connections)
            var edgePaint = new SKPaint
            {
                Color = SKColor.Parse("#666666"),
                StrokeWidth = 2,
                IsAntialias = true
            };

            for (int i = 0; i < nodes.Length - 1; i++)
            {
                if (random.Next(0, 100) > 40)
                {
                    canvas.DrawLine(nodes[i].x, nodes[i].y, nodes[i + 1].x, nodes[i + 1].y, edgePaint);
                }
            }

            // Draw nodes
            var nodePaint = new SKPaint
            {
                Color = SKColor.Parse("#3498db"),
                IsAntialias = true
            };

            foreach (var node in nodes)
            {
                canvas.DrawCircle(node.x, node.y, 12, nodePaint);
            }

            SaveImage(surface, outputPath);
        }

        private static void GenerateHierarchicalPreview(string outputPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            using var surface = SKSurface.Create(new SKImageInfo(WIDTH, HEIGHT));
            var canvas = surface.Canvas;
            canvas.Clear(SKColor.Parse("#1e1e1e"));

            var edgePaint = new SKPaint
            {
                Color = SKColor.Parse("#666666"),
                StrokeWidth = 2,
                IsAntialias = true
            };

            var nodePaint = new SKPaint
            {
                Color = SKColor.Parse("#2ecc71"),
                IsAntialias = true
            };

            // Root node
            float rootX = WIDTH / 2;
            float rootY = 40;
            canvas.DrawCircle(rootX, rootY, 15, nodePaint);

            // Level 1 (3 nodes)
            float[] level1X = { WIDTH / 4, WIDTH / 2, 3 * WIDTH / 4 };
            float level1Y = 120;

            foreach (var x in level1X)
            {
                canvas.DrawLine(rootX, rootY, x, level1Y, edgePaint);
                canvas.DrawCircle(x, level1Y, 12, nodePaint);
            }

            // Level 2 (6 nodes)
            float level2Y = 200;
            for (int i = 0; i < 3; i++)
            {
                float x1 = level1X[i] - 40;
                float x2 = level1X[i] + 40;

                canvas.DrawLine(level1X[i], level1Y, x1, level2Y, edgePaint);
                canvas.DrawLine(level1X[i], level1Y, x2, level2Y, edgePaint);

                canvas.DrawCircle(x1, level2Y, 10, nodePaint);
                canvas.DrawCircle(x2, level2Y, 10, nodePaint);
            }

            SaveImage(surface, outputPath);
        }

        private static void GenerateRadialPreview(string outputPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            using var surface = SKSurface.Create(new SKImageInfo(WIDTH, HEIGHT));
            var canvas = surface.Canvas;
            canvas.Clear(SKColor.Parse("#1e1e1e"));

            float centerX = WIDTH / 2;
            float centerY = HEIGHT / 2;
            float radius = 100;

            var edgePaint = new SKPaint
            {
                Color = SKColor.Parse("#666666"),
                StrokeWidth = 2,
                IsAntialias = true
            };

            var centerPaint = new SKPaint
            {
                Color = SKColor.Parse("#e74c3c"),
                IsAntialias = true
            };

            var outerPaint = new SKPaint
            {
                Color = SKColor.Parse("#f39c12"),
                IsAntialias = true
            };

            // Center node
            canvas.DrawCircle(centerX, centerY, 20, centerPaint);

            // Outer ring
            int nodeCount = 8;
            for (int i = 0; i < nodeCount; i++)
            {
                double angle = 2 * Math.PI * i / nodeCount;
                float x = centerX + (float)(radius * Math.Cos(angle));
                float y = centerY + (float)(radius * Math.Sin(angle));

                canvas.DrawLine(centerX, centerY, x, y, edgePaint);
                canvas.DrawCircle(x, y, 12, outerPaint);
            }

            SaveImage(surface, outputPath);
        }

        private static void GenerateNetworkPreview(string outputPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            using var surface = SKSurface.Create(new SKImageInfo(WIDTH, HEIGHT));
            var canvas = surface.Canvas;
            canvas.Clear(SKColor.Parse("#1e1e1e"));

            var random = new Random(123);
            var nodes = new (float x, float y)[15];

            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = (random.Next(40, WIDTH - 40), random.Next(40, HEIGHT - 40));
            }

            var edgePaint = new SKPaint
            {
                Color = SKColor.Parse("#555555"),
                StrokeWidth = 1.5f,
                IsAntialias = true
            };

            // Dense connections
            for (int i = 0; i < nodes.Length; i++)
            {
                for (int j = i + 1; j < nodes.Length; j++)
                {
                    float distance = (float)Math.Sqrt(
                        Math.Pow(nodes[i].x - nodes[j].x, 2) +
                        Math.Pow(nodes[i].y - nodes[j].y, 2)
                    );

                    if (distance < 120)
                    {
                        canvas.DrawLine(nodes[i].x, nodes[i].y, nodes[j].x, nodes[j].y, edgePaint);
                    }
                }
            }

            var nodePaint = new SKPaint
            {
                Color = SKColor.Parse("#9b59b6"),
                IsAntialias = true
            };

            foreach (var node in nodes)
            {
                canvas.DrawCircle(node.x, node.y, 8, nodePaint);
            }

            SaveImage(surface, outputPath);
        }

        private static void GenerateCompoundPreview(string outputPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            using var surface = SKSurface.Create(new SKImageInfo(WIDTH, HEIGHT));
            var canvas = surface.Canvas;
            canvas.Clear(SKColor.Parse("#1e1e1e"));

            var groupPaint = new SKPaint
            {
                Color = SKColor.Parse("#34495e"),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3,
                IsAntialias = true
            };

            var nodePaint = new SKPaint
            {
                Color = SKColor.Parse("#1abc9c"),
                IsAntialias = true
            };

            // Group 1
            canvas.DrawRoundRect(40, 40, 150, 120, 10, 10, groupPaint);
            canvas.DrawCircle(80, 80, 12, nodePaint);
            canvas.DrawCircle(120, 80, 12, nodePaint);
            canvas.DrawCircle(100, 120, 12, nodePaint);

            // Group 2
            canvas.DrawRoundRect(210, 40, 150, 120, 10, 10, groupPaint);
            canvas.DrawCircle(250, 80, 12, nodePaint);
            canvas.DrawCircle(290, 80, 12, nodePaint);
            canvas.DrawCircle(270, 120, 12, nodePaint);

            // Group 3
            canvas.DrawRoundRect(120, 180, 150, 100, 10, 10, groupPaint);
            canvas.DrawCircle(160, 210, 12, nodePaint);
            canvas.DrawCircle(210, 210, 12, nodePaint);

            SaveImage(surface, outputPath);
        }

        private static void SaveImage(SKSurface surface, string path)
        {
            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = File.OpenWrite(path);
            data.SaveTo(stream);

            Console.WriteLine($"Generated: {path}");
        }
    }
}