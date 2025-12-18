using System.Collections.Generic;

namespace Bahyway.SharedKernel.Compiler
{
    // The Root Object (The file itself)
    public class AkkadianModel
    {
        public ContextDefinition Context { get; set; }
        public List<IdentityStrategy> Identities { get; set; } = new();
        public List<ConceptDefinition> Concepts { get; set; } = new();
    }

    // 1. CONTEXT (Metadata)
    public class ContextDefinition
    {
        public string Name { get; set; }
    }

    // 2. IDENTITY (The RGB Key strategy)
    public class IdentityStrategy
    {
        public string Name { get; set; }
        public string Type { get; set; }      // e.g., "SpatialColor3D"
        public string Algorithm { get; set; } // e.g., "RGB_Infinite_Mapping"
    }

    // 3. CONCEPT (The Entity)
    public class ConceptDefinition
    {
        public string Name { get; set; }
        public string IdentityRef { get; set; } // Links to IdentityStrategy
        public List<PropertyDefinition> Properties { get; set; } = new();
    }

    public class PropertyDefinition
    {
        public string Name { get; set; }
    }
}