using Avalonia.Controls;
using System;
using System.IO;  // ← ADD THIS for Path and File
using AvaloniaWebView;
using Bahyway.SharedKernel.Graph;
using System.Text.Json;
using System.Linq;

namespace Bahyway.KGEditor.UI.Controls
{
    public class WebGraphControl : UserControl
    {
        private WebView _webView;

        public WebGraphControl()
        {
            _webView = new WebView
            {
                Url = new Uri("https://www.google.com")  // Test URL
            };

            this.Content = _webView;
        }

        public void LoadTemplate(string templateName)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string templatePath = Path.Combine(baseDir, "Extensions", templateName, "index.html");

            System.Diagnostics.Debug.WriteLine($"Loading template from: {templatePath}");

            if (File.Exists(templatePath))
            {
                string fileUrl = "file:///" + templatePath.Replace("\\", "/");
                _webView.Url = new Uri(fileUrl);
                System.Diagnostics.Debug.WriteLine($"Template loaded: {fileUrl}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Template not found: {templatePath}");
            }
        }

        public void RenderGraph(GraphTopology graph)
        {
            // Convert graph to Cytoscape format
            var cytoscapeData = new
            {
                nodes = graph.Nodes.Select(n => new
                {
                    id = n.Id,
                    label = n.Label,
                    x = n.X,
                    y = n.Y,
                    color = n.ColorHex,
                    size = 40
                }).ToArray(),

                edges = graph.Edges.Select(e => new
                {
                    id = $"{e.SourceId}_{e.TargetId}",
                    source = e.SourceId,
                    target = e.TargetId,
                    label = ""
                }).ToArray()
            };

            string json = JsonSerializer.Serialize(cytoscapeData);
            System.Diagnostics.Debug.WriteLine($"Sending graph data: {json.Substring(0, Math.Min(100, json.Length))}...");

            try
            {
                _webView.ExecuteScriptAsync($"if(typeof renderGraph === 'function') {{ renderGraph({json}); }}");
                System.Diagnostics.Debug.WriteLine("Graph data sent successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error executing script: {ex.Message}");
            }
        }
    }
}