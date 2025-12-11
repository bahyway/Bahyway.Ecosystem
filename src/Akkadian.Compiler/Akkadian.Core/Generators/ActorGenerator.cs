using Akkadian.Core.Ast;
using System.Text;
using System.Linq; // Needed for Select mapping

namespace Akkadian.Core.Generators
{
    public class ActorGenerator
    {
        public string Generate(AkkadianProgram program)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using Akka.Actor;");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine();

            foreach (var context in program.Contexts)
            {
                // Only generate actors if there are commands
                if (context.Commands.Count == 0) continue;

                string ns = $"{ToPascalCase(context.Name)}.Actors";
                sb.AppendLine($"namespace {ns}");
                sb.AppendLine("{");

                // Generate the Actor Class
                string actorName = $"{ToPascalCase(context.Name)}IngestionActor";
                sb.AppendLine($"    public class {actorName} : ReceiveActor");
                sb.AppendLine("    {");
                sb.AppendLine("        public {actorName}()");
                sb.AppendLine("        {");

                // Generate Receive<T> handlers for each Command
                foreach (var cmd in context.Commands)
                {
                    string msgType = $"{ToPascalCase(cmd.Name)}Command";
                    sb.AppendLine($"            ReceiveAsync<{msgType}>(Handle{cmd.Name});");
                }

                sb.AppendLine("        }");
                sb.AppendLine();

                // Generate Handler Methods
                foreach (var cmd in context.Commands)
                {
                    sb.AppendLine($"        private async Task Handle{cmd.Name}({ToPascalCase(cmd.Name)}Command cmd)");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            Console.WriteLine($\"Processing {cmd.Name}...\");");

                    // Generate Validation Logic
                    if (cmd.Validations.Count > 0)
                    {
                        sb.AppendLine("            // 1. Validation Phase");
                        foreach (var check in cmd.Validations)
                        {
                            // In a real compiler, we would parse the expression.
                            // For now, we output it as a comment or psuedo-check.
                            sb.AppendLine($"            // Rule: {check}");
                            sb.AppendLine($"            if (!CheckRule(\"{check}\")) throw new Exception(\"Validation Failed: {check}\");");
                        }
                    }

                    // Generate Execution Logic
                    sb.AppendLine("            // 2. Execution Phase (Write to Data Vault)");
                    foreach (var step in cmd.ExecutionSteps)
                    {
                        sb.AppendLine($"            // Action: {step.Action} -> Target: {step.Target}");
                        // Example: Dao.InsertSat(step.JsonPayload);
                    }

                    sb.AppendLine("        }");
                    sb.AppendLine();
                }

                // Helper for demo purposes
                sb.AppendLine("        private bool CheckRule(string rule) => true; // Implement expression parser logic here");

                sb.AppendLine("    }");
                sb.AppendLine();

                // Generate Message Classes (DTOs for the Commands)
                foreach (var cmd in context.Commands)
                {
                    sb.AppendLine($"    public class {ToPascalCase(cmd.Name)}Command");
                    sb.AppendLine("    {");
                    sb.AppendLine("        public Guid RequestId { get; set; }");
                    sb.AppendLine("        public string Payload { get; set; }");
                    sb.AppendLine("    }");
                }

                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        private string ToPascalCase(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var words = text.Split(new[] { '_', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join("", words.Select(w => char.ToUpper(w[0]) + w.Substring(1)));
        }
    }
}