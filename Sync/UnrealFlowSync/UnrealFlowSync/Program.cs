using Amazon;
using UnrealFlow;

class Program {
  static async Task Main( string[] args ) {

    Environment.SetEnvironmentVariable( "AWS_DEFAULT_REGION", "us-east-1" );
    /*AWSConfigs.LoggingConfig.LogTo = LoggingOptions.Console;
    AWSConfigs.LoggingConfig.LogMetrics = true;
    AWSConfigs.LoggingConfig.LogResponses = ResponseLoggingOption.Always;
    AWSConfigs.LoggingConfig.LogResponsesSizeLimit = 1024 * 1024;
    AWSConfigs.LoggingConfig.LogMetricsFormat = LogMetricsFormatOption.JSON;*/

    string keyId = null;
    string settingsPath = null;
    bool locksOnly = false;

    foreach( string arg in args ) {
      if( arg.Trim().ToLower() == "-locksonly" ) {
        locksOnly = true;
        continue;
      }
      string[] parts = arg.Split( new[] { '=' }, 2 );
      if( parts.Length == 2 ) {
        string key = parts[0].Trim();
        string value = parts[1].Trim().Trim( '\'', '"' );

        switch( key.ToLower() ) {
          case "-key":
            keyId = value;
            break;
          case "-settingspath":
            settingsPath = value;
            break;
          default:
            Console.WriteLine( $"Unknown argument: {key}" );
            break;
        }
      }
    }

    // Validate and use the arguments
    if( !string.IsNullOrEmpty( keyId ) && !string.IsNullOrEmpty( settingsPath ) ) {
      AppSettings appSettings = new AppSettings( settingsPath );
      if( appSettings.projects.ContainsKey( keyId ) ) {
        Console.WriteLine( "Sync Started for: " + appSettings.projects[keyId].displayName + ": " + appSettings.projects[keyId].projectPath + " " + appSettings.projects[keyId].syncName );
        if( locksOnly ) {
          await ( new SyncSystem() ).SyncLocks( appSettings.projects[keyId] );
        }
        else {
          await ( new SyncSystem() ).SyncBucket( appSettings.projects[keyId] );
        }
        Console.WriteLine( "Sync finished " + appSettings.projects[keyId].displayName );
      }
      else if( appSettings.folders.ContainsKey( keyId ) ) {
        Console.WriteLine( "Sync Started for: " + appSettings.folders[keyId].displayName + ": " + appSettings.folders[keyId].folderPath + " " + appSettings.folders[keyId].syncName );
        await ( new SyncSystem() ).SyncFolder( appSettings.folders[keyId] );
        Console.WriteLine( "Sync finished " + appSettings.folders[keyId].displayName );
      }
      else { 
        Console.WriteLine( "Sync failed to find project with key " + keyId );
      }
      
    }
    else {
      Console.WriteLine( "Error: Both projectKey and settingsPath are required" );
      Console.WriteLine( "Usage: app.exe projectKey=path settingsPath=path" );
      Console.WriteLine( "Example: app.exe projectKey=MyProject settingsPath=C:\\Settings\\config.json" );
    }
  }
}