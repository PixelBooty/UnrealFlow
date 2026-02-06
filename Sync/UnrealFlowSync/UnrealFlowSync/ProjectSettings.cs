namespace UnrealFlow {

  public class ProjectSettings {
    public string displayName = "";
    public string syncName = "";
    public string projectPath = "";
    public string projectSyncFolder = "";

    public SyncOverrides syncOverrides = new SyncOverrides();

    public Bucket bucket => new Bucket() {
      apiKey = AppSettings.instance.apiKey,
      secret = AppSettings.instance.secret,
      name = this.syncName
    };

    public string projectFile => Directory
      .GetFiles( this.projectPath )
      .Select( projectFile => projectFile.Replace( this.projectPath, "" ).Substring( 1 ) )
      .FirstOrDefault( path => path.EndsWith( ".uproject" ) );

  }

};