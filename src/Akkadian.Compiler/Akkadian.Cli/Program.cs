using Akkadian.Core.Compiler;
using Akkadian.Core.Generators;
using System;
using System.IO;

namespace Akkadian.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=============================================");
            Console.WriteLine("   AKKADIAN COMPILER v3.0 (beeMDM Edition)   ");
            Console.WriteLine("=============================================");
            Console.ResetColor();

            // 1. Setup File Path
            string filePath = args.Length > 0 ? args[0] : "test.akk";

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, GetSampleCode());
                Console.WriteLine($"Created sample file: {filePath}");
            }

            // 2. Read and Compile
            string sourceCode = File.ReadAllText(filePath);
            Console.WriteLine($"Compiling {filePath}...");

            var compiler = new AkkadianCompilerEngine();
            var ast = compiler.Compile(sourceCode);

            // 3. Generate Artifacts (Only if compilation succeeded)
            if (ast != null)
            {
                Console.WriteLine("\n[1] AST Generation Successful.");

                // --- A. Generate SQL ---
                Console.WriteLine("[2] Generating SQL Backend...");
                var sqlGenerator = new SqlGenerator();
                string sqlOutput = sqlGenerator.Generate(ast);
                string sqlFilename = Path.ChangeExtension(filePath, ".sql");
                File.WriteAllText(sqlFilename, sqlOutput);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✅ SQL script saved to: {Path.GetFullPath(sqlFilename)}");
                Console.ResetColor();

                // --- B. Generate C# Models ---
                Console.WriteLine("[3] Generating C# Domain Models...");
                var csGenerator = new CSharpGenerator();
                string csOutput = csGenerator.Generate(ast);
                string csFilename = Path.ChangeExtension(filePath, ".cs");
                File.WriteAllText(csFilename, csOutput);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✅ C# Models saved to: {Path.GetFullPath(csFilename)}");
                Console.ResetColor();

                // --- C. Generate AKKA Actors ---
                Console.WriteLine("[4] Generating AKKA.NET Actors...");
                var actorGenerator = new ActorGenerator();
                string actorOutput = actorGenerator.Generate(ast);
                string actorFilename = Path.ChangeExtension(filePath, ".Actors.cs");
                File.WriteAllText(actorFilename, actorOutput);

                // --- 4. Python AI Service Generation ---
                Console.WriteLine("[5] Generating Python AI Microservice...");
                var pyGenerator = new Akkadian.Core.Generators.PythonGenerator(); // Ensure namespace is imported or use full path
                string pyOutput = pyGenerator.Generate(ast);

                string pyFilename = Path.ChangeExtension(filePath, ".py");
                File.WriteAllText(pyFilename, pyOutput);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n--- PYTHON PREVIEW ---");
                Console.WriteLine(pyOutput);
                Console.WriteLine("----------------------");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n✅ Python Service saved to: {Path.GetFullPath(pyFilename)}");
                Console.ResetColor();

                // --- 5. Avalonia UI Generation ---
                Console.WriteLine("[5] Generating Avalonia ViewModels...");
                var uiGenerator = new AvaloniaGenerator(); // Add namespace Akkadian.Core.Generators
                string uiOutput = uiGenerator.Generate(ast);

                string uiFilename = Path.ChangeExtension(filePath, ".ViewModels.cs");
                File.WriteAllText(uiFilename, uiOutput);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n--- UI PREVIEW ---");
                Console.WriteLine(uiOutput);
                Console.WriteLine("------------------");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n✅ UI ViewModels saved to: {Path.GetFullPath(uiFilename)}");
                Console.ResetColor();

                // Show Preview of Actors
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n--- ACTOR PREVIEW ---");
                Console.WriteLine(actorOutput);
                Console.WriteLine("---------------------");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✅ Actor Code saved to: {Path.GetFullPath(actorFilename)}");
                Console.ResetColor();


            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
                
        static string GetSampleCode()
        {
            return @"
CONTEXT beeMDM_Governance {

    IDENTITY MasterEntity {
        BUSINESS_KEY: tax_id
        FUZZY_MATCH: name USING Levenshtein THRESHOLD 0.85
        SPATIAL_COLOR: GPS_to_RGB(lat, lon)
    }

    STORAGE DataVault {
        HUB: hub_customer WITH {
            customer_key: UUID PRIMARY KEY,
            name: VARCHAR(200)
        }

        SATELLITE: sat_customer_details WITH temporal_tracking {
            customer_key: UUID,
            email: VARCHAR(100)
        }
    }

    COMMAND IngestCustomerData {
        VALIDATION {
            CHECK: email_is_valid
            CHECK: name_not_empty
        }
        EXECUTION {
            INSERT_EVENT: sat_customer_details { payload }
        }
    }

    VECTORIZATION {
        MODEL: 'sentence-transformers/all-MiniLM-L6-v2'
        EMBEDDINGS {
            customer_profile: [name, bio]
        }
    }
}";
        }
    }
}