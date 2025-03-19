using UnrealFlow;

class Program {
  static async Task Main( string[] args ) {

    string projectKey = null;
    string settingsPath = null;

    foreach( string arg in args ) {
      string[] parts = arg.Split( new[] { '=' }, 2 );
      if( parts.Length == 2 ) {
        string key = parts[0].Trim();
        string value = parts[1].Trim().Trim( '\'', '"' );

        switch( key.ToLower() ) {
          case "-projectkey":
            projectKey = value;
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
    if( !string.IsNullOrEmpty( projectKey ) && !string.IsNullOrEmpty( settingsPath ) ) {
      AppSettings appSettings = new AppSettings( settingsPath );
      if( appSettings.projects.ContainsKey( projectKey ) ) {
        Console.WriteLine( "Sync Started for: " + appSettings.projects[projectKey].displayName + ": " + appSettings.projects[projectKey].projectPath + " " + appSettings.projects[projectKey].syncName );
        await ( new SyncSystem() ).SyncBucket( appSettings.projects[projectKey] );
        Console.WriteLine( "Sync finished " + appSettings.projects[projectKey].displayName );
      }
      else {
        Console.WriteLine( "Sync failed to find project with key " + projectKey );
      }
      
    }
    else {
      Console.WriteLine( "Error: Both projectKey and settingsPath are required" );
      Console.WriteLine( "Usage: app.exe projectKey=path settingsPath=path" );
      Console.WriteLine( "Example: app.exe projectKey=MyProject settingsPath=C:\\Settings\\config.json" );
    }
  }
}