using System.Linq;

namespace UnrealFlow {

  public class SyncOverrides {
    public string[] toggledFolders = {};
    public string[] pushFiles = {};
    public string[] pullFiles = {};

    public bool IsPushOnly( string filePath ) {
      if( !_IsInToggledFolder( filePath ) ) return false;
      bool inPush = pushFiles.Any( p => filePath == p || filePath.StartsWith( p + "/" ) );
      bool inPull = pullFiles.Any( p => filePath == p || filePath.StartsWith( p + "/" ) );
      return inPush && !inPull;
    }

    public bool IsPullOnly( string filePath ) {
      if( !_IsInToggledFolder( filePath ) ) return false;
      bool inPush = pushFiles.Any( p => filePath == p || filePath.StartsWith( p + "/" ) );
      bool inPull = pullFiles.Any( p => filePath == p || filePath.StartsWith( p + "/" ) );
      return inPull && !inPush;
    }

    private bool _IsInToggledFolder( string filePath ) {
      return toggledFolders.Any( f => filePath.StartsWith( f + "/" ) || filePath == f );
    }

  }

};
