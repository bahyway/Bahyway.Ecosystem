using Akkadian.Core.Ast;
using System.Text;
using System.Linq;

namespace Akkadian.Core.Generators
{
    public class AvaloniaGenerator
    {
        public string Generate(AkkadianProgram program)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using CommunityToolkit.Mvvm.ComponentModel;");
            sb.AppendLine("using Avalonia.Media;");
            sb.AppendLine("using System;");
            sb.AppendLine();

            foreach (var context in program.Contexts)
            {
                if (context.Presentation == null && context.Storage == null) continue;

                string ns = $"{ToPascalCase(context.Name)}.UI.ViewModels";
                sb.AppendLine($"namespace {ns}");
                sb.AppendLine("{");

                // Generate a Base Node ViewModel for this Context
                sb.AppendLine($"    public partial class {ToPascalCase(context.Name)}NodeViewModel : ObservableObject");
                sb.AppendLine("    {");
                sb.AppendLine("        [ObservableProperty] private double _x;");
                sb.AppendLine("        [ObservableProperty] private double _y;");
                sb.AppendLine("        [ObservableProperty] private bool _isVisible = true;");
                sb.AppendLine("        public virtual string Color => \"#808080\";"); // Default Gray
                sb.AppendLine("        public virtual string Shape => \"CIRCLE\";");
                sb.AppendLine("    }");
                sb.AppendLine();

                // Generate specific ViewModels for each Hub based on Styles
                if (context.Storage != null)
                {
                    foreach (var hub in context.Storage.Hubs)
                    {
                        // Find matching style in Presentation block
                        var style = context.Presentation?.Styles.FirstOrDefault(s => s.TargetEntity == hub.Name);

                        string className = $"{ToPascalCase(hub.Name)}ViewModel";
                        sb.AppendLine($"    public partial class {className} : {ToPascalCase(context.Name)}NodeViewModel");
                        sb.AppendLine("    {");

                        // Apply DSL Styling
                        if (style != null)
                        {
                            sb.AppendLine($"        public override string Color => \"{style.Color}\";");
                            sb.AppendLine($"        public override string Shape => \"{style.Shape}\";");
                        }

                        // Generate Properties from Data Columns
                        foreach (var col in hub.Columns)
                        {
                            string propType = MapToCSharpType(col.DataType);
                            string propName = ToPascalCase(col.Name);
                            // Using CommunityToolkit.Mvvm for automatic PropertyChanged notification
                            sb.AppendLine($"        [ObservableProperty] private {propType} _{col.Name.ToLower()};");
                        }

                        sb.AppendLine("    }");
                        sb.AppendLine();
                    }
                }
                sb.AppendLine("}");
            }
            return sb.ToString();
        }

        private string ToPascalCase(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return char.ToUpper(text[0]) + text.Substring(1);
        }

        private string MapToCSharpType(string akkadianType)
        {
            if (akkadianType.StartsWith("VARCHAR")) return "string";
            if (akkadianType.StartsWith("DECIMAL")) return "decimal";
            return akkadianType switch
            {
                "UUID" => "Guid",
                "INT" => "int",
                "BIGINT" => "long",
                "TIMESTAMP" => "DateTime",
                "BOOLEAN" => "bool",
                _ => "string"
            };
        }
    }
}