using Akkadian.Core.Ast;
using Akkadian.Core.Grammar;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System.Collections.Generic;
using System.Linq;

namespace Akkadian.Core.Visitors
{
    public class AkkadianAstVisitor : AkkadianBaseVisitor<object>
    {
        public override object VisitProgram(AkkadianParser.ProgramContext context)
        {
            var program = new AkkadianProgram();
            foreach (var ctx in context.context()) program.Contexts.Add((ContextNode)Visit(ctx));
            return program;
        }

        public override object VisitContext(AkkadianParser.ContextContext context)
        {
            var node = new ContextNode { Name = context.IDENTIFIER().GetText() };
            foreach (var body in context.contextBody())
            {
                var result = Visit(body);
                if (result is IdentityNode id) node.Identities.Add(id);
                else if (result is StorageNode st) node.Storage = st;
                else if (result is VectorizationNode vec) node.Vectorization = vec;
                else if (result is CommandNode cmd) node.Commands.Add(cmd);
                else if (result is RagQueryNode rag) node.RagQueries.Add(rag);
                else if (result is RuleSetNode rules) node.RuleSet = rules;
                else if (result is PresentationNode pres) node.Presentation = pres;
                else if (result is MetaAlgorithmicsNode meta) node.MetaAlgorithmics = meta;
            }
            return node;
        }

        // --- IDENTITY ---
        public override object VisitIdentity(AkkadianParser.IdentityContext context)
        {
            var node = new IdentityNode { Name = context.IDENTIFIER().GetText() };
            foreach (var body in context.identityBody())
            {
                var txt = body.GetText();
                if (txt.StartsWith("BUSINESS_KEY")) node.BusinessKeys = ProcessKeyList(body.keyList());
                else if (txt.StartsWith("FUZZY_RESOLUTION"))
                {
                    foreach (var rule in body.fuzzyRule())
                    {
                        node.FuzzyRules.Add(new FuzzyRuleConfig
                        {
                            Fields = ProcessKeyList(rule.keyList()),
                            Algorithm = rule.IDENTIFIER().GetText(),
                            Threshold = double.Parse(rule.FLOAT().GetText())
                        });
                    }
                }
                else if (txt.StartsWith("SPATIAL_ID"))
                {
                    node.SpatialId = new SpatialIdConfig { Algorithm = "EXTENDED_COLOR_64", Precision = 64 };
                }
            }
            return node;
        }

        // --- STORAGE ---
        public override object VisitStorage(AkkadianParser.StorageContext context)
        {
            var node = new StorageNode();
            foreach (var body in context.storageBody())
            {
                var table = (TableNode)Visit(body.GetChild(0));
                if (table.Type == "HUB") node.Hubs.Add(table);
                else if (table.Type == "SATELLITE") node.Satellites.Add(table);
                else if (table.Type == "LINK") node.Links.Add(table);
            }
            return node;
        }

        // Updated Calls to ProcessTable (Passing arrays now)
        public override object VisitHub(AkkadianParser.HubContext context)
            => ProcessTable(context.IDENTIFIER().GetText(), "HUB", context.tableOptions(), context.columnList(), context.indexBlock());

        public override object VisitSatellite(AkkadianParser.SatelliteContext context)
        {
            var t = ProcessTable(context.IDENTIFIER().GetText(), "SATELLITE", context.tableOptions(), context.columnList(), context.indexBlock());
            t.HasTemporalTracking = context.GetText().Contains("temporal_tracking");
            return t;
        }

        public override object VisitLink(AkkadianParser.LinkContext context)
            => ProcessTable(context.IDENTIFIER().GetText(), "LINK", context.tableOptions(), context.columnList(), context.indexBlock());

        // FIX: Changed 'opts' from single object (?) to array ([])
        private TableNode ProcessTable(string name, string type, AkkadianParser.TableOptionsContext[] opts, AkkadianParser.ColumnListContext cols, AkkadianParser.IndexBlockContext? idx)
        {
            var node = new TableNode { Name = name, Type = type, Columns = ProcessColumnList(cols) };

            // FIX: Iterate through the array of options
            if (opts != null && opts.Length > 0)
            {
                foreach (var opt in opts)
                {
                    var txt = opt.GetText();
                    if (txt.Contains("PARTITION_BY")) node.PartitionStrategy = txt;
                    if (txt.Contains("CLUSTERED_BY"))
                    {
                        node.ClusteredBy = opt.Stop.Text;
                    }
                }
            }
            return node;
        }

        // --- META & PRESENTATION ---
        public override object VisitMetaAlgorithmics(AkkadianParser.MetaAlgorithmicsContext context)
        {
            var node = new MetaAlgorithmicsNode();
            foreach (var body in context.metaBody())
            {
                if (body.GetText().StartsWith("EXECUTION_MODEL")) node.ExecutionModel = body.IDENTIFIER().GetText();
                else if (body.GetText().StartsWith("ACTORS"))
                {
                    foreach (var a in body.actorDef())
                    {
                        var cfg = new ActorConfig { Name = a.IDENTIFIER().GetText() };
                        foreach (var p in a.configProp())
                        {
                            // Keep index (0) here because grammar structure is fixed
                            string key = p.IDENTIFIER(0).GetText();
                            string val = p.GetChild(2).GetText();
                            cfg.Properties[key] = val;
                        }
                        node.Actors.Add(cfg);
                    }
                }
            }
            return node;
        }

        public override object VisitPresentation(AkkadianParser.PresentationContext context)
        {
            var node = new PresentationNode();
            foreach (var rule in context.styleRule())
            {
                var style = new StyleNode { TargetEntity = rule.IDENTIFIER().GetText() };

                foreach (var p in rule.styleProp())
                {
                    var txt = p.GetText();
                    if (txt.StartsWith("COLOR")) style.Color = p.STRING().GetText().Trim('\'');
                    if (txt.StartsWith("SHAPE")) style.Shape = p.GetChild(2).GetText();
                }
                node.Styles.Add(style);
            }
            return node;
        }

        // --- BOILERPLATE ---
        public override object VisitVectorization(AkkadianParser.VectorizationContext context) => new VectorizationNode();
        public override object VisitCommand(AkkadianParser.CommandContext context) => new CommandNode { Name = context.IDENTIFIER().GetText() };
        public override object VisitRagQuery(AkkadianParser.RagQueryContext context) => new RagQueryNode { Name = context.IDENTIFIER().GetText() };
        public override object VisitRuleSet(AkkadianParser.RuleSetContext context) => new RuleSetNode { Name = context.IDENTIFIER().GetText() };

        public override object VisitContextBody(AkkadianParser.ContextBodyContext context) => Visit(context.GetChild(0));
        public override object VisitIdentityBody(AkkadianParser.IdentityBodyContext context) => null!;

        // --- HELPERS ---
        private List<string> ProcessKeyList(AkkadianParser.KeyListContext context)
            => context?.IDENTIFIER().Select(x => x.GetText()).ToList() ?? new List<string>();

        private List<ColumnNode> ProcessColumnList(AkkadianParser.ColumnListContext context)
        {
            var list = new List<ColumnNode>();
            foreach (var col in context.columnDef())
            {
                list.Add(new ColumnNode
                {
                    Name = col.IDENTIFIER().GetText(),
                    DataType = col.typeDef().GetText(),
                    IsPrimaryKey = col.constraint().Any(c => c.GetText().Contains("PRIMARY")),
                    IsUnique = col.constraint().Any(c => c.GetText().Contains("UNIQUE"))
                });
            }
            return list;
        }
    }
}