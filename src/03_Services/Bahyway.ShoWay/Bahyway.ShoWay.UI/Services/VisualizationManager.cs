using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bahyway.ShoWay.UI.Services
{
    public class VisualizationTemplate
    {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
    }

    public class VisualizationManager
    {
        private readonly string _extensionsDir;

        public VisualizationManager()
        {
            // We look for a folder named "Extensions" next to the app
            _extensionsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Extensions");
            if (!Directory.Exists(_extensionsDir))
            {
                Directory.CreateDirectory(_extensionsDir);
            }
        }

        public List<VisualizationTemplate> LoadTemplates()
        {
            var templates = new List<VisualizationTemplate>();

            // Scan for subfolders containing 'index.html'
            var dirs = Directory.GetDirectories(_extensionsDir);
            foreach (var dir in dirs)
            {
                string indexFile = Path.Combine(dir, "index.html");
                if (File.Exists(indexFile))
                {
                    templates.Add(new VisualizationTemplate
                    {
                        Name = new DirectoryInfo(dir).Name,
                        Path = indexFile
                    });
                }
            }
            return templates;
        }
    }
}
