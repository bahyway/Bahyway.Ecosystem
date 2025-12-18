using System;
using System.Text.RegularExpressions;

namespace Bahyway.SharedKernel.Compiler
{
    public class AkkadianParser
    {
        public AkkadianModel Parse(string dslScript)
        {
            var model = new AkkadianModel();
            // Split by new lines
            var lines = dslScript.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // State Machine Variables
            string currentBlock = "";
            dynamic currentObject = null;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                // Skip empty lines or comments
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("//")) continue;

                // 1. Detect Context Block
                if (trimmed.StartsWith("CONTEXT"))
                {
                    var name = ExtractName(trimmed, "CONTEXT");
                    model.Context = new ContextDefinition { Name = name };
                    currentBlock = "CONTEXT";
                    currentObject = model.Context;
                    continue;
                }

                // 2. Detect Identity Strategy Block
                if (trimmed.StartsWith("IDENTITY"))
                {
                    var name = ExtractName(trimmed, "IDENTITY");
                    var identity = new IdentityStrategy { Name = name };
                    model.Identities.Add(identity);
                    currentBlock = "IDENTITY";
                    currentObject = identity;
                    continue;
                }

                // 3. Detect Concept Block
                if (trimmed.StartsWith("CONCEPT"))
                {
                    var name = ExtractName(trimmed, "CONCEPT");
                    var concept = new ConceptDefinition { Name = name };
                    model.Concepts.Add(concept);
                    currentBlock = "CONCEPT";
                    currentObject = concept;
                    continue;
                }

                // --- Parsing Inside Blocks ---

                // Parsing properties inside IDENTITY { ... }
                if (currentBlock == "IDENTITY")
                {
                    if (trimmed.StartsWith("TYPE:"))
                        ((IdentityStrategy)currentObject).Type = ExtractValue(trimmed);
                    if (trimmed.StartsWith("ALGORITHM:"))
                        ((IdentityStrategy)currentObject).Algorithm = ExtractValue(trimmed);
                }

                // Parsing properties inside CONCEPT { ... }
                if (currentBlock == "CONCEPT")
                {
                    if (trimmed.StartsWith("ID:"))
                        ((ConceptDefinition)currentObject).IdentityRef = ExtractValue(trimmed);

                    if (trimmed.StartsWith("PROPERTY"))
                    {
                        var propName = ExtractName(trimmed, "PROPERTY");
                        ((ConceptDefinition)currentObject).Properties.Add(new PropertyDefinition { Name = propName });
                    }
                }
            }

            return model;
        }

        // Helper: Extracts "MyContext" from "CONTEXT MyContext {"
        private string ExtractName(string line, string keyword)
        {
            var pattern = $@"{keyword}\s+(\w+)";
            var match = Regex.Match(line, pattern);
            return match.Success ? match.Groups[1].Value : "Unknown";
        }

        // Helper: Extracts "Value" from "KEY: Value"
        private string ExtractValue(string line)
        {
            var parts = line.Split(':');
            return parts.Length > 1 ? parts[1].Trim() : "";
        }
    }
}