using Xunit;
using Bahyway.SharedKernel.Compiler;

namespace Bahyway.Tests
{
    public class ParserTests
    {
        [Fact]
        public void Should_Parse_Basic_Akkadian_Script()
        {
            // 1. The DSL Script (Akkadian v2.0)
            var script = @"
                CONTEXT Najaf_Sector_1 {
                }

                IDENTITY VisualGeoKey {
                    TYPE: SpatialColor3D
                    ALGORITHM: RGB_Infinite_Mapping
                }

                CONCEPT DeceasedPerson {
                    ID: VisualGeoKey
                    PROPERTY FullName { }
                }
            ";

            // 2. Run Compiler
            var parser = new AkkadianParser();
            var model = parser.Parse(script);

            // 3. Verify Context
            Assert.NotNull(model.Context);
            Assert.Equal("Najaf_Sector_1", model.Context.Name);

            // 4. Verify Identity
            Assert.Single(model.Identities);
            Assert.Equal("VisualGeoKey", model.Identities[0].Name);
            Assert.Equal("SpatialColor3D", model.Identities[0].Type);

            // 5. Verify Concept
            Assert.Single(model.Concepts);
            Assert.Equal("DeceasedPerson", model.Concepts[0].Name);
            Assert.Equal("VisualGeoKey", model.Concepts[0].IdentityRef);
            Assert.Single(model.Concepts[0].Properties);
            Assert.Equal("FullName", model.Concepts[0].Properties[0].Name);
        }
    }
}