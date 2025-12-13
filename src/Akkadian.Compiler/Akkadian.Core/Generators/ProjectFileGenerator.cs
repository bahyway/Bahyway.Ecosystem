using Akkadian.Core.Ast;
using System.Text;

namespace Akkadian.Core.Generators
{
    public class ProjectFileGenerator
    {
        public string Generate(ContextNode context)
        {
            return $@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>

    <!-- OPTIMIZATION 2: Native AOT Compilation -->
    <!-- This compiles C# directly to Machine Code (Assembly), removing the JIT. -->
    <PublishAot>true</PublishAot>
    <InvariantGlobalization>true</InvariantGlobalization>

    <!-- High Performance GC Settings -->
    <ServerGarbageCollection>true</ServerGarbageCollection>
    <ConcurrentGarbageCollection>true</ConcurrentGarbageCollection>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Akka"" Version=""1.5.15"" />
    <PackageReference Include=""System.Numerics.Vectors"" Version=""4.5.0"" />
  </ItemGroup>

</Project>";
        }
    }
}