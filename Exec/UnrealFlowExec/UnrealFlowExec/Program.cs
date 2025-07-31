using System;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Collections.Generic;
using System.Linq;

namespace UnrealFlowExec {

  class Program {
    static void Main( string[] args ) {
      if( args.Length == 0 ) {
        Console.WriteLine( "Usage: CSharpFileExecutor.exe <path-to-csharp-file>" );
        Console.WriteLine( "Example: CSharpFileExecutor.exe \"C:\\MyProgram.cs\"" );
        Console.WriteLine( "         CSharpFileExecutor.exe \"./scripts/test.cs\"" );
        Console.WriteLine( "         CSharpFileExecutor.exe \"../MyProgram.cs\"" );
        return;
      }

      string filePath = args[0];

      try {
        ExecuteCSharpFile( filePath );
      }
      catch( Exception ex ) {
        Console.WriteLine( $"Error: {ex.Message}" );
      }
    }

    static void ExecuteCSharpFile( string filePath ) {
      // Convert relative path to absolute path
      string absolutePath = Path.GetFullPath( filePath );

      // Check if file exists
      if( !File.Exists( absolutePath ) ) {
        throw new FileNotFoundException( $"File not found: {absolutePath}" );
      }

      // Read the source code
      string sourceCode = File.ReadAllText( absolutePath );
      Console.WriteLine( $"Compiling and executing: {absolutePath}" );

      // Wrap the source code in a class and Main method if needed
      string wrappedCode = WrapCodeIfNeeded( sourceCode );

      // Create syntax tree
      SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText( wrappedCode );

      // Get references to required assemblies
      var references = new List<MetadataReference>
      {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location)
            };

      // Add additional references if available
      try {
        references.Add( MetadataReference.CreateFromFile( typeof( System.Runtime.AssemblyTargetedPatchBandAttribute ).Assembly.Location ) );
      }
      catch {
        // Not available in this framework, continue without it
      }

      // Try to add System.Runtime if available
      try {
        references.Add( MetadataReference.CreateFromFile( Assembly.Load( "System.Runtime" ).Location ) );
      }
      catch {
        // System.Runtime not available, continue without it
      }

      // Try to add netstandard if available
      try {
        references.Add( MetadataReference.CreateFromFile( Assembly.Load( "netstandard" ).Location ) );
      }
      catch {
        // netstandard not available, continue without it
      }

      // Add mscorlib for .NET Framework compatibility
      try {
        references.Add( MetadataReference.CreateFromFile( typeof( string ).Assembly.Location ) );
      }
      catch {
        // Already included or not needed
      }

      // Create compilation
      CSharpCompilation compilation = CSharpCompilation.Create(
          "DynamicAssembly",
          new[] { syntaxTree },
          references,
          new CSharpCompilationOptions( OutputKind.ConsoleApplication ) );

      // Compile to memory stream
      using( var ms = new MemoryStream() ) {
        EmitResult result = compilation.Emit( ms );

        if( !result.Success ) {
          Console.WriteLine( "Compilation failed:" );
          foreach( Diagnostic diagnostic in result.Diagnostics.Where( d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error ) ) {
            Console.WriteLine( $"  {diagnostic.Id}: {diagnostic.GetMessage()}" );
          }
          return;
        }

        // Load and execute the assembly
        ms.Seek( 0, SeekOrigin.Begin );
        byte[] assemblyBytes = ms.ToArray();
        Assembly assembly = Assembly.Load( assemblyBytes );

        // Find the entry point (Main method)
        Type programType = assembly.GetTypes().FirstOrDefault( t =>
            t.GetMethod( "Main", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic ) != null );

        if( programType == null ) {
          throw new InvalidOperationException( "No Main method found in the compiled assembly" );
        }

        MethodInfo mainMethod = programType.GetMethod( "Main", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );

        Console.WriteLine( "--- Execution Output ---" );

        // Execute the Main method
        var parameters = mainMethod.GetParameters();
        if( parameters.Length == 0 ) {
          mainMethod.Invoke( null, null );
        }
        else if( parameters.Length == 1 && parameters[0].ParameterType == typeof( string[] ) ) {
          // Pass empty string array for args
          mainMethod.Invoke( null, new object[] { new string[0] } );
        }
        else {
          throw new InvalidOperationException( "Unsupported Main method signature" );
        }

        Console.WriteLine( "--- Execution Complete ---\nPress enter to exit." );
        Console.ReadLine();
      }
    }

    static string WrapCodeIfNeeded( string sourceCode ) {
      // Check if the code already has a Main method or is a complete program
      if( sourceCode.Contains( "static void Main" ) ||
          sourceCode.Contains( "static int Main" ) ||
          sourceCode.Contains( "static async Task Main" ) ||
          sourceCode.Contains( "static async Task<int> Main" ) ) {
        // Code already has a Main method, return as is
        return sourceCode;
      }

      // Check if the code has class definitions
      bool hasClass = sourceCode.Contains( "class " ) ||
                     sourceCode.Contains( "public class " ) ||
                     sourceCode.Contains( "internal class " ) ||
                     sourceCode.Contains( "partial class " );

      if( hasClass ) {
        // Has classes but no Main method, wrap in a Main method
        return $@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

{sourceCode}

class Program
{{
    static void Main()
    {{
        // Auto-generated Main method
        // The original code with classes is above
    }}
}}";
      }
      else {
        // No classes, wrap everything in a Main method
        return $@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

class Program
{{
    static void Main()
    {{
{IndentCode( sourceCode, 2 )}
    }}
}}";
      }
    }

    static string IndentCode( string code, int spaces ) {
      string indent = new string( ' ', spaces * 4 );
      return string.Join( Environment.NewLine,
          code.Split( new[] { Environment.NewLine, "\n", "\r\n" }, StringSplitOptions.None )
              .Select( line => string.IsNullOrWhiteSpace( line ) ? line : indent + line ) );
    }
  }

};