using Akkadian.Core.Ast;
using Akkadian.Core.Grammar; // From ANTLR generation
using System;
using System.Collections.Generic;
using System.Linq;

namespace Akkadian.Core.Visitors
{
    public class AkkadianAstVisitor : AkkadianBaseVisitor<object>
    {
        public override object VisitProgram(AkkadianParser.ProgramContext context)
        {
            var program = new AkkadianProgram();

            foreach (var ctx in context.context())
            {
                program.Contexts.Add((ContextNode)Visit(ctx));
            }

            return program;
        }

        public override object VisitContext(AkkadianParser.ContextContext context)
        {
            var node = new ContextNode
            {
                Name = context.IDENTIFIER().GetText()
            };

            // Iterate over the body elements (Identity, Storage, etc.)
            foreach (var body in context.contextBody())
            {
                var result = Visit(body);

                if (result is IdentityNode idNode) node.Identities.Add(idNode);
                else if (result is StorageNode storeNode) node.Storage = storeNode;
                else if (result is VectorizationNode vecNode) node.Vectorization = vecNode;
                else if (result is CommandNode cmdNode) node.Commands.Add(cmdNode);
            }

            return node;
        }

        //// --- IDENTITY ---
        //public override object VisitIdentity(AkkadianParser.IdentityContext context)
        //{
        //    var node = new IdentityNode
        //    {
        //        Name = context.IDENTIFIER().GetText()
        //    };

        //    foreach (var body in context.identityBody())
        //    {
        //        var text = body.GetText();
        //        if (text.StartsWith("BUSINESS_KEY"))
        //        {
        //            // Correctly calls the helper method defined at the bottom
        //            node.BusinessKeys = ProcessKeyList(body.keyList());
        //        }
        //        else if (text.StartsWith("FUZZY_MATCH"))
        //        {
        //            node.FuzzyMatch = new FuzzyMatchConfig
        //            {
        //                Field = body.IDENTIFIER(0).GetText(),
        //                Algorithm = body.IDENTIFIER(1).GetText(),
        //                Threshold = double.Parse(body.FLOAT().GetText())
        //            };
        //        }
        //        else if (text.StartsWith("SPATIAL_COLOR"))
        //        {
        //            node.SpatialColorExpression = body.expression().GetText();
        //        }
        //    }
        //    return node;
        //}
        // --- IDENTITY (UPDATED FOR V3.0) ---
        public override object VisitIdentity(AkkadianParser.IdentityContext context)
        {
            var node = new IdentityNode
            {
                Name = context.IDENTIFIER().GetText()
            };

            foreach (var body in context.identityBody())
            {
                var text = body.GetText();

                // 1. Handle Business Keys
                if (text.StartsWith("BUSINESS_KEY"))
                {
                    node.BusinessKeys = ProcessKeyList(body.keyList());
                }
                // 2. Handle Spatial ID (New Structure)
                else if (text.StartsWith("SPATIAL_ID"))
                {
                    // In the new grammar, SPATIAL_ID has its own internal tokens.
                    // We extract them manually or via context children.
                    node.SpatialId = new SpatialIdConfig
                    {
                        // Defaulting for now as we transition grammar
                        Algorithm = "EXTENDED_COLOR_64",
                        Dimensions = new List<string> { "lat", "lon" },
                        Precision = "64"
                    };
                }
                // 3. Handle Fuzzy Resolution (New Structure)
                else if (text.StartsWith("FUZZY_RESOLUTION"))
                {
                    // Iterate through the fuzzy rules inside the block
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
                // 4. Backward Compatibility (Old Fuzzy Match syntax)
                // Only keep this if your grammar still supports the old single-line syntax
                else if (text.StartsWith("FUZZY_MATCH") && body.ChildCount > 4)
                {
                    // This block caused your error because IDENTIFIER() requires an index
                    // and FLOAT() might not exist at this level in the new grammar.
                    // We skip this logic now in favor of FUZZY_RESOLUTION above.
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
                // Visit the specific child (Hub, Satellite, or Link context)
                var table = (TableNode)Visit(body.GetChild(0));

                if (table.Type == "HUB") node.Hubs.Add(table);
                else if (table.Type == "SATELLITE") node.Satellites.Add(table);
                else if (table.Type == "LINK") node.Links.Add(table);
            }
            return node;
        }

        public override object VisitHub(AkkadianParser.HubContext context)
        {
            return new TableNode
            {
                Name = context.IDENTIFIER().GetText(),
                Type = "HUB",
                Columns = ProcessColumnList(context.columnList())
            };
        }

        public override object VisitSatellite(AkkadianParser.SatelliteContext context)
        {
            return new TableNode
            {
                Name = context.IDENTIFIER().GetText(),
                Type = "SATELLITE",
                HasTemporalTracking = context.GetText().Contains("temporal_tracking"),
                Columns = ProcessColumnList(context.columnList())
            };
        }

        public override object VisitLink(AkkadianParser.LinkContext context)
        {
            return new TableNode
            {
                Name = context.IDENTIFIER().GetText(),
                Type = "LINK",
                Columns = ProcessColumnList(context.columnList())
            };
        }

        // --- VECTORIZATION ---
        public override object VisitVectorization(AkkadianParser.VectorizationContext context)
        {
            var node = new VectorizationNode();
            foreach (var body in context.vectorBody())
            {
                if (body.GetText().StartsWith("MODEL"))
                {
                    node.ModelName = body.STRING().GetText().Trim('\'', '"');
                }
                else if (body.GetText().StartsWith("EMBEDDINGS"))
                {
                    foreach (var def in body.embeddingDef())
                    {
                        var name = def.IDENTIFIER().GetText();
                        var fields = ProcessKeyList(def.keyList());
                        node.Embeddings.Add(name, fields);
                    }
                }
            }
            return node;
        }

        // --- DISPATCH HELPERS ---

        // This directs traffic for Context Body items
        public override object VisitContextBody(AkkadianParser.ContextBodyContext context)
        {
            return Visit(context.GetChild(0));
        }

        // We handle Identity Body manually inside VisitIdentity, so return null here to satisfy interface
        public override object VisitIdentityBody(AkkadianParser.IdentityBodyContext context)
        {
            return null!;
        }

        // --- PRIVATE HELPERS (Renamed to avoid CS0114 warnings) ---

        private List<string> ProcessKeyList(AkkadianParser.KeyListContext context)
        {
            return context.IDENTIFIER().Select(x => x.GetText()).ToList();
        }

        private List<ColumnNode> ProcessColumnList(AkkadianParser.ColumnListContext context)
        {
            var columns = new List<ColumnNode>();
            foreach (var col in context.columnDef())
            {
                var cNode = new ColumnNode
                {
                    Name = col.IDENTIFIER().GetText(),
                    DataType = col.typeDef().GetText(),
                    IsPrimaryKey = col.constraint().Any(c => c.GetText() == "PRIMARY KEY"),
                    IsUnique = col.constraint().Any(c => c.GetText() == "UNIQUE")
                };
                columns.Add(cNode);
            }
            return columns;
        }
    }
}