// Copyright Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;
using System.IO;
using System.Linq;

public class UnrealFlow : ModuleRules
{
	public UnrealFlow(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;
	
		PublicDependencyModuleNames.AddRange(new string[] { "Core", "CoreUObject", "Engine", "InputCore", "EnhancedInput" });

		PrivateDependencyModuleNames.AddRange(new string[] {  });

		// Uncomment if you are using Slate UI
		// PrivateDependencyModuleNames.AddRange(new string[] { "Slate", "SlateCore" });
		
		// Uncomment if you are using online features
		// PrivateDependencyModuleNames.Add("OnlineSubsystem");
		

		// To include OnlineSubsystemSteam, add it to the plugins section in your uproject file with the Enabled attribute set to true
		
		// Define the source and destination paths
        string SourceDir = Path.Combine(ModuleDirectory, "..", "..", "Sync", "UnrealFlowSync", "UnrealFlowSync", "bin", "Release", "net8.0");
        string DestDir = Path.Combine("$(ProjectDir)", "Content", "Sync");

    // Add the executable and DLLs
    ( new[] { "UnrealFlowSync.exe", "UnrealFlowSync.dll", "UnrealFlowSync.deps.json", "Newtonsoft.Json.dll", "AWSSDK.S3.dll", "AWSSDK.Core.dll", "UnrealFlowSync.pdb", "UnrealFlowSync.runtimeconfig.json" } ).ToList().ForEach( fileName => {
      RuntimeDependencies.Add( Path.Combine( DestDir, fileName ), Path.Combine( SourceDir, fileName ) );
    } );

    SourceDir = Path.Combine( ModuleDirectory, "..", "..", "Exec", "UnrealFlowExec", "UnrealFlowExec", "bin", "Release", "net8.0" );
    DestDir = Path.Combine( "$(ProjectDir)", "Content", "Exec" );

    ( new[] { "UnrealFlowExec.exe", "UnrealFlowExec.dll", "UnrealFlowExec.deps.json", "Newtonsoft.Json.dll", "Microsoft.CodeAnalysis.CSharp.dll", "Microsoft.CodeAnalysis.dll", "Microsoft.CodeAnalysis.Scripting.dll", "System.Collections.Immutable.dll", "System.Reflection.Metadata.dll", "UnrealFlowExec.pdb", "UnrealFlowExec.runtimeconfig.json" } ).ToList().ForEach( fileName => {
      RuntimeDependencies.Add( Path.Combine( DestDir, fileName ), Path.Combine( SourceDir, fileName ) );
    } );
   
    if( Target.Platform == UnrealTargetPlatform.Linux ) {
      // Define paths
      string scriptSourcePath = Path.Combine( ModuleDirectory, "..", "..", "Sync", "unreal_file_dialog.sh" );
      string scriptDestPath = Path.Combine( "$(ProjectDir)", "Content", "Sync", "unreal_file_dialog.sh" );

      // Ensure the script is copied during packaging
      RuntimeDependencies.Add( scriptDestPath, scriptSourcePath );

      // Alternative: Copy to multiple locations to ensure it's found
      //RuntimeDependencies.Add( "$(ProjectDir)/Sync/unreal_file_dialog.sh", scriptSourcePath );
      //RuntimeDependencies.Add( "$(BinaryOutputDir)/Sync/unreal_file_dialog.sh", scriptSourcePath );
    }


    bEnableUndefinedIdentifierWarnings = false;
	}
}
