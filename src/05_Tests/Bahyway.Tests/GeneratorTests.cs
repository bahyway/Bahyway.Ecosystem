using Xunit;
using Bahyway.SharedKernel.Compiler;

namespace Bahyway.Tests
{
    public class GeneratorTests
    {
        [Fact]
        public void Should_Generate_SQL_And_Cypher_From_Model()
        {
            // 1. Create a Fake Model (Simulating what Parser produces)
            var model = new AkkadianModel
            {
                Context = new ContextDefinition { Name = "Najaf" },
                Concepts = new List<ConceptDefinition>
                {
                    new ConceptDefinition
                    {
                        Name = "Person",
                        Properties = new List<PropertyDefinition>
                        {
                            new PropertyDefinition { Name = "FullName" },
                            new PropertyDefinition { Name = "Tribe" }
                        }
                    }
                }
            };

            // 2. Run Generator
            var generator = new AkkadianGenerator();
            var result = generator.Generate(model);

            // 3. Verify SQL (Data Vault)
            Assert.Contains("CREATE TABLE \"Hub_Person\"", result.SqlScript);
            Assert.Contains("\"PersonHash\" CHAR(64) PRIMARY KEY", result.SqlScript);
            Assert.Contains("CREATE TABLE \"Sat_Person_Attributes\"", result.SqlScript);
            Assert.Contains("\"FullName\" TEXT", result.SqlScript);

            // 4. Verify Cypher (Graph)
            Assert.Contains("CREATE (:Person { _type: 'Concept' })", result.CypherScript);

            // Debug Output (Optional: To see what it generated)
            System.Console.WriteLine(result.SqlScript);
        }
    }
}