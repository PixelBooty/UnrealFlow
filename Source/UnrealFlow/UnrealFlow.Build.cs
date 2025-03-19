// Copyright Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;
using System.IO;
using System.Linq;

public class UnrealFlow : ModuleRules
{
	public UnrealFlow(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;
	
		PublicDependencyModuleNames.AddRange(new string[] { "Core", "CoreUObject", "Engine", "InputCore", "EnhancedInput", "AWSSDK" });

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
			RuntimeDependencies.Add(Path.Combine(DestDir, fileName), Path.Combine(SourceDir, fileName));
		} );
		
		bEnableUndefinedIdentifierWarnings = false;
	}
}
