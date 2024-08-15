using Newtonsoft.Json.Linq;
using System.IO;

namespace UnrealFlow{
  public class AppSettings{
    AppSettings(){
      this.Load();
    }

    public void Load(){
      if( File.Exists( this._fileLocation ) ) {
        try {
          JObject settingsObject = JObject.Parse( File.ReadAllText( this._fileLocation ) );
          if( settingsObject.ContainsKey( nameof( this.serviceUri ) ) ) {
            this.serviceUri = (string)settingsObject[nameof( this.serviceUri )];
          }
          if( settingsObject.ContainsKey( nameof( this.apiKey ) ) ) {
            this.apiKey = (string)settingsObject[nameof( this.apiKey )];
          }
          if( settingsObject.ContainsKey( nameof( this.secret ) ) ) {
            this.secret = (string)settingsObject[nameof( this.secret )];
          }
          if( settingsObject.ContainsKey( nameof( this.unrealPath ) ) ){
            this.unrealPath = (string)settingsObject[nameof( this.unrealPath )];
          }
          if( settingsObject.ContainsKey( nameof( this.projects ) ) ) {
            this.projects.Clear();
            JObject projectObject = (JObject)settingsObject[nameof( this.projects )];
            foreach( KeyValuePair<string, JToken> projectKvp in projectObject ){
              JObject projectData = projectKvp.Value as JObject;
              ProjectSettings project = new ProjectSettings();
              if( projectData.ContainsKey( nameof(ProjectSettings.displayName) ) ){
                project.displayName = (string)projectData[nameof(ProjectSettings.displayName)];
              }
              if( projectData.ContainsKey( nameof(ProjectSettings.projectPath) ) ){
                project.projectPath = (string)projectData[nameof(ProjectSettings.projectPath)];
              }
              if( projectData.ContainsKey( nameof(ProjectSettings.syncName) ) ){
                project.syncName = (string)projectData[nameof(ProjectSettings.syncName)];
              }
              if( projectData.ContainsKey( nameof(ProjectSettings.versionsToKeep) ) ){
                project.versionsToKeep = (int)projectData[nameof(ProjectSettings.versionsToKeep)];
              }
              if( projectData.ContainsKey( nameof( ProjectSettings.syncPaths ) ) ){
                JArray syncFoldersArray = (JArray)projectData[nameof( ProjectSettings.syncPaths )];
                foreach( JToken syncFolder in syncFoldersArray ){
                  project.syncPaths.Add( (string)syncFolder );
                }
              }
              this.projects.Add( projectKvp.Key, project );
            }
          }
          if( settingsObject.ContainsKey( nameof( this.defaultSyncFolders ) ) ){
            this.defaultSyncFolders.Clear();
            JArray defaultSyncFoldersArray = (JArray)settingsObject[nameof( this.defaultSyncFolders )];
            foreach( JToken defaultSyncFolder in defaultSyncFoldersArray ){
              this.defaultSyncFolders.Add( (string)defaultSyncFolder );
            }
          }
        }
        catch( Exception ) { }
      }
      else {
        this.Save();
      }
    }

    private string _fileLocation =>  Path.Combine( FileSystem.AppDataDirectory, "settings.json" );

    public void Save(){
      File.WriteAllText( this._fileLocation, this.ToString() );
    }

    public const string settingsFile = "settings.json";

    public static AppSettings instance{
      get{
        if( _instance == null){
          _instance = new AppSettings();
        }

        return _instance;
      }
    }

    public override string ToString(){
      JObject baseObject = new JObject();
      baseObject[nameof( this.serviceUri )] = this.serviceUri;
      baseObject[nameof( this.apiKey )] = this.apiKey;
      baseObject[nameof( this.secret )] = this.secret;
      baseObject[nameof( this.unrealPath )] = this.unrealPath;
      JArray baseDefaultFoldersArray = new JArray();
      foreach( string defaultSyncFolder in this.defaultSyncFolders ){
        baseDefaultFoldersArray.Add( defaultSyncFolder );
      }
      baseObject[nameof( this.defaultSyncFolders )] = baseDefaultFoldersArray;
      JObject baseProjects = new JObject();
      foreach( KeyValuePair<string, ProjectSettings> project in this.projects ){
        JObject projectData = new JObject();
        projectData[nameof( ProjectSettings.displayName )] = project.Value.displayName;
        projectData[nameof( ProjectSettings.projectPath )] = project.Value.projectPath;
        projectData[nameof( ProjectSettings.syncName )] = project.Value.syncName;
        projectData[nameof( ProjectSettings.versionsToKeep )] = project.Value.versionsToKeep;
        JArray foldersArray = new JArray();
        foreach( string syncFolder in project.Value.syncPaths ){
          foldersArray.Add( syncFolder );
        }
        projectData[nameof( ProjectSettings.syncPaths )] = foldersArray;
        baseProjects.Add( project.Key, projectData );
      }
      baseObject[nameof( this.projects )] = baseProjects;
      return baseObject.ToString();
    }

    public string serviceUri = "";
    public string apiKey = "";
    public string secret = "";
    public string unrealPath = "";
    public List<string> defaultSyncFolders = new List<string>();

    public Dictionary<string, ProjectSettings> projects = new Dictionary<string, ProjectSettings>();

    private static AppSettings _instance = null;
  }
};