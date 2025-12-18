using System.Collections.Generic;

namespace Akkadian.Core.Ast
{
    public class AkkadianProgram
    {
        public List<ContextNode> Contexts { get; set; } = new List<ContextNode>();
    }

    public class ContextNode
    {
        public string Name { get; set; } = string.Empty;
        public List<IdentityNode> Identities { get; set; } = new List<IdentityNode>();
        public StorageNode? Storage { get; set; }
        public VectorizationNode? Vectorization { get; set; }
        public List<CommandNode> Commands { get; set; } = new List<CommandNode>();
        public List<RagQueryNode> RagQueries { get; set; } = new List<RagQueryNode>();

        // NEW V3.1 BLOCKS
        public RuleSetNode? RuleSet { get; set; }
        public PresentationNode? Presentation { get; set; }
        public MetaAlgorithmicsNode? MetaAlgorithmics { get; set; }
    }

    // --- IDENTITY (Enhanced) ---
    public class IdentityNode
    {
        public string Name { get; set; } = string.Empty;
        public List<string> BusinessKeys { get; set; } = new List<string>();
        public List<FuzzyRuleConfig> FuzzyRules { get; set; } = new List<FuzzyRuleConfig>();
        public SpatialIdConfig? SpatialId { get; set; }
    }

    public class FuzzyRuleConfig
    {
        public List<string> Fields { get; set; } = new List<string>();
        public string Algorithm { get; set; } = string.Empty;
        public double Threshold { get; set; }
    }

    public class SpatialIdConfig
    {
        public string Algorithm { get; set; } = string.Empty;
        public List<string> Dimensions { get; set; } = new List<string>();
        public int Precision { get; set; }
    }

    // --- STORAGE (Enhanced) ---
    public class StorageNode
    {
        public List<TableNode> Hubs { get; set; } = new List<TableNode>();
        public List<TableNode> Satellites { get; set; } = new List<TableNode>();
        public List<TableNode> Links { get; set; } = new List<TableNode>();
    }

    public class TableNode
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // HUB, SATELLITE, LINK
        public bool HasTemporalTracking { get; set; }

        // NEW: Physical Storage Options
        public string? PartitionStrategy { get; set; }
        public string? ClusteredBy { get; set; }

        public List<ColumnNode> Columns { get; set; } = new List<ColumnNode>();
        public Dictionary<string, List<string>> Indexes { get; set; } = new Dictionary<string, List<string>>();
    }

    public class ColumnNode
    {
        public string Name { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsPrimaryKey { get; set; }
        public bool IsUnique { get; set; }
    }

    // --- NEW BLOCKS ---
    public class PresentationNode
    {
        public List<StyleNode> Styles { get; set; } = new List<StyleNode>();
    }

    public class StyleNode
    {
        public string TargetEntity { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Shape { get; set; } = string.Empty;
        public string SizeByField { get; set; } = string.Empty;
    }

    public class RuleSetNode
    {
        public string Name { get; set; } = string.Empty;
        public string Algorithm { get; set; } = "MAMDANI";
        public List<string> LogicRules { get; set; } = new List<string>();
    }

    public class MetaAlgorithmicsNode
    {
        public string ExecutionModel { get; set; } = "actor_based";
        public List<ActorConfig> Actors { get; set; } = new List<ActorConfig>();
    }

    public class ActorConfig
    {
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }

    // --- STANDARD NODES ---
    public class VectorizationNode
    {
        public string ModelName { get; set; } = string.Empty;
        public Dictionary<string, List<string>> Embeddings { get; set; } = new Dictionary<string, List<string>>();
    }

    public class CommandNode
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Validations { get; set; } = new List<string>();
        public List<StatementNode> ExecutionSteps { get; set; } = new List<StatementNode>();
    }

    public class StatementNode
    {
        public string Action { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public string JsonPayload { get; set; } = string.Empty;
    }

    public class RagQueryNode
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public RetrievalNode? Retrieval { get; set; }
        public GenerationNode? Generation { get; set; }
    }

    public class RetrievalNode
    {
        public string VectorTarget { get; set; } = string.Empty;
        public int TopK { get; set; }
        public int GraphHops { get; set; }
        public List<string> GraphEdges { get; set; } = new List<string>();
        public string TemporalWindow { get; set; } = string.Empty;
    }

    public class GenerationNode
    {
        public string Model { get; set; } = string.Empty;
        public string PromptTemplate { get; set; } = string.Empty;
    }
}