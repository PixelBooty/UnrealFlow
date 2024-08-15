namespace UnrealFlow{

  public class ProjectSettings{
    public string displayName = "";
    public string syncName = "";
    public int versionsToKeep = 0;
    public string projectPath = "";
    public List<string> syncPaths = new List<string>();

    public Bucket bucket => new Bucket(){
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