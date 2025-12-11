using Akkadian.Core.Ast;
using System.Text;
using System.Linq;
using System;

namespace Akkadian.Core.Generators
{
    public class CSharpGenerator
    {
        public string Generate(AkkadianProgram program)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// =============================================");
            sb.AppendLine("// AKKADIAN GENERATED C# MODELS");
            sb.AppendLine($"// Generated: {DateTime.Now}");
            sb.AppendLine("// =============================================");
            sb.AppendLine("using System;");
            sb.AppendLine();

            foreach (var context in program.Contexts)
            {
                sb.AppendLine($"namespace {ToPascalCase(context.Name)}.Domain");
                sb.AppendLine("{");

                if (context.Storage != null)
                {
                    // Generate Classes for Hubs, Links, Satellites
                    var allTables = context.Storage.Hubs
                        .Concat(context.Storage.Links)
                        .Concat(context.Storage.Satellites);

                    foreach (var table in allTables)
                    {
                        sb.AppendLine(GenerateClass(table));
                    }
                }

                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        private string GenerateClass(TableNode table)
        {
            var sb = new StringBuilder();
            string className = ToPascalCase(table.Name);

            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// Represents the {table.Type} : {table.Name}");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    public class {className}");
            sb.AppendLine("    {");

            // 1. User Defined Properties
            foreach (var col in table.Columns)
            {
                sb.AppendLine($"        public {MapToCSharpType(col.DataType)} {ToPascalCase(col.Name)} {{ get; set; }}");
            }

            // 2. Standard Audit Properties
            sb.AppendLine($"        public DateTime LoadDate {{ get; set; }}");
            sb.AppendLine($"        public string RecordSource {{ get; set; }}");

            // 3. Temporal Properties
            if (table.HasTemporalTracking)
            {
                sb.AppendLine($"        public DateTime ValidFrom {{ get; set; }}");
                sb.AppendLine($"        public DateTime? ValidTo {{ get; set; }}");
                sb.AppendLine($"        public bool IsCurrent {{ get; set; }}");
            }

            sb.AppendLine("    }");
            return sb.ToString();
        }

        // Helper: snake_case -> PascalCase (e.g., hub_customer -> HubCustomer)
        private string ToPascalCase(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var words = text.Split(new[] { '_', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join("", words.Select(w => char.ToUpper(w[0]) + w.Substring(1)));
        }

        private string MapToCSharpType(string akkadianType)
        {
            if (akkadianType.StartsWith("VARCHAR")) return "string";
            if (akkadianType.StartsWith("DECIMAL")) return "decimal";

            return akkadianType switch
            {
                "UUID" => "Guid",
                "INT" => "int",
                "TIMESTAMP" => "DateTime",
                "BOOLEAN" => "bool",
                _ => "string" // Default fallback
            };
        }
    }
}