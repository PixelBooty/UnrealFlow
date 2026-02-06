using Newtonsoft.Json.Linq;
using System.IO;

namespace UnrealFlow {
  public class AppSettings {
    public AppSettings( string settingsFile ) {
      _instance = this;
      this._settingsFile = settingsFile;
      this.Load();
    }

    private string _settingsFile;

    public void Load() {
      if( File.Exists( this.fileLocation ) ) {
        try {
          JObject settingsObject = JObject.Parse( File.ReadAllText( this.fileLocation ) );
          if( settingsObject.ContainsKey( nameof( this.serviceUri ) ) ) {
            this.serviceUri = (string)settingsObject[nameof( this.serviceUri )];
          }
          if( settingsObject.ContainsKey( nameof( this.apiKey ) ) ) {
            this.apiKey = (string)settingsObject[nameof( this.apiKey )];
          }
          if( settingsObject.ContainsKey( nameof( this.secret ) ) ) {
            this.secret = (string)settingsObject[nameof( this.secret )];
          }
          if( settingsObject.ContainsKey( nameof( this.unrealPath ) ) ) {
            this.unrealPath = (string)settingsObject[nameof( this.unrealPath )];
          }
          if( settingsObject.ContainsKey( nameof( this.projects ) ) ) {
            this.projects.Clear();
            JObject projectObject = (JObject)settingsObject[nameof( this.projects )];
            foreach( KeyValuePair<string, JToken> projectKvp in projectObject ) {
              JObject projectData = projectKvp.Value as JObject;
              ProjectSettings project = new ProjectSettings();
              if( projectData.ContainsKey( nameof( ProjectSettings.displayName ) ) ) {
                project.displayName = (string)projectData[nameof( ProjectSettings.displayName )];
              }
              if( projectData.ContainsKey( nameof( ProjectSettings.projectPath ) ) ) {
                project.projectPath = (string)projectData[nameof( ProjectSettings.projectPath )];
              }
              if( projectData.ContainsKey( nameof( ProjectSettings.syncName ) ) ) {
                project.syncName = (string)projectData[nameof( ProjectSettings.syncName )];
              }
              if( projectData.ContainsKey( nameof( ProjectSettings.projectSyncFolder ) ) ) {
                project.projectSyncFolder = (string)projectData[nameof( ProjectSettings.projectSyncFolder )];
              }
              if( projectData.ContainsKey( nameof( ProjectSettings.syncOverrides ) ) ) {
                JObject overridesData = projectData[nameof( ProjectSettings.syncOverrides )] as JObject;
                if( overridesData != null ) {
                  if( overridesData.ContainsKey( "toggledFolders" ) ) {
                    project.syncOverrides.toggledFolders = overridesData["toggledFolders"].ToObject<string[]>();
                  }
                  if( overridesData.ContainsKey( "pushFiles" ) ) {
                    project.syncOverrides.pushFiles = overridesData["pushFiles"].ToObject<string[]>();
                  }
                  if( overridesData.ContainsKey( "pullFiles" ) ) {
                    project.syncOverrides.pullFiles = overridesData["pullFiles"].ToObject<string[]>();
                  }
                }
              }
              this.projects.Add( projectKvp.Key, project );
            }
          }
          if( settingsObject.ContainsKey( nameof( this.folders ) ) ) {
            this.folders.Clear();
            JObject folderObject = (JObject)settingsObject[nameof( this.folders )];
            foreach( KeyValuePair<string, JToken> folderKvp in folderObject ) {
              JObject folderData = folderKvp.Value as JObject;
              FolderSettings folder = new FolderSettings();
              if( folderData.ContainsKey( nameof( FolderSettings.displayName ) ) ) {
                folder.displayName = (string)folderData[nameof( FolderSettings.displayName )];
              }
              if( folderData.ContainsKey( nameof( FolderSettings.syncName ) ) ) {
                folder.syncName = (string)folderData[nameof( FolderSettings.syncName )];
              }
              if( folderData.ContainsKey( nameof( FolderSettings.folderPath ) ) ) {
                folder.folderPath = (string)folderData[nameof( FolderSettings.folderPath )];
              }
              if( folderData.ContainsKey( nameof( FolderSettings.syncOverrides ) ) ) {
                JObject overridesData = folderData[nameof( FolderSettings.syncOverrides )] as JObject;
                if( overridesData != null ) {
                  if( overridesData.ContainsKey( "toggledFolders" ) ) {
                    folder.syncOverrides.toggledFolders = overridesData["toggledFolders"].ToObject<string[]>();
                  }
                  if( overridesData.ContainsKey( "pushFiles" ) ) {
                    folder.syncOverrides.pushFiles = overridesData["pushFiles"].ToObject<string[]>();
                  }
                  if( overridesData.ContainsKey( "pullFiles" ) ) {
                    folder.syncOverrides.pullFiles = overridesData["pullFiles"].ToObject<string[]>();
                  }
                }
              }
              this.folders.Add( folderKvp.Key, folder );
            }
          }
        }
        catch( Exception ) { }
      }
      else {
        this.Save();
      }
    }

    public string fileLocation => this._settingsFile;

    public string settingsDirectory => this._settingsFile.Replace( Path.GetFileName( this._settingsFile ), "" );

    public void Save() {
      File.WriteAllText( this.fileLocation, this.ToString() );
    }

    public static AppSettings instance => _instance;

    public override string ToString() {
      JObject baseObject = new JObject();
      baseObject[nameof( this.serviceUri )] = this.serviceUri;
      baseObject[nameof( this.apiKey )] = this.apiKey;
      baseObject[nameof( this.secret )] = this.secret;
      baseObject[nameof( this.unrealPath )] = this.unrealPath;
      JObject baseProjects = new JObject();
      foreach( KeyValuePair<string, ProjectSettings> project in this.projects ) {
        JObject projectData = new JObject();
        projectData[nameof( ProjectSettings.displayName )] = project.Value.displayName;
        projectData[nameof( ProjectSettings.projectPath )] = project.Value.projectPath;
        projectData[nameof( ProjectSettings.syncName )] = project.Value.syncName;
        projectData[nameof( ProjectSettings.projectSyncFolder )] = project.Value.projectSyncFolder;
        JObject projectOverrides = new JObject();
        projectOverrides["toggledFolders"] = new JArray( project.Value.syncOverrides.toggledFolders );
        projectOverrides["pushFiles"] = new JArray( project.Value.syncOverrides.pushFiles );
        projectOverrides["pullFiles"] = new JArray( project.Value.syncOverrides.pullFiles );
        projectData[nameof( ProjectSettings.syncOverrides )] = projectOverrides;
        baseProjects.Add( project.Key, projectData );
      }
      JObject baseFolders = new JObject();
      foreach( KeyValuePair<string, FolderSettings> folder in this.folders ) {
        JObject folderData = new JObject();
        folderData[nameof( FolderSettings.displayName )] = folder.Value.displayName;
        folderData[nameof( FolderSettings.syncName )] = folder.Value.syncName;
        folderData[nameof( FolderSettings.folderPath )] = folder.Value.folderPath;
        JObject folderOverrides = new JObject();
        folderOverrides["toggledFolders"] = new JArray( folder.Value.syncOverrides.toggledFolders );
        folderOverrides["pushFiles"] = new JArray( folder.Value.syncOverrides.pushFiles );
        folderOverrides["pullFiles"] = new JArray( folder.Value.syncOverrides.pullFiles );
        folderData[nameof( FolderSettings.syncOverrides )] = folderOverrides;
        baseFolders.Add( folder.Key, folderData );
      }
      baseObject[nameof( this.projects )] = baseProjects;
      baseObject[nameof( this.folders )] = baseFolders;
      return baseObject.ToString();
    }

    public string user => this.apiKey.Contains( "@" ) ? this.apiKey.Split( '@' )[0] : this.apiKey;

    public string serviceUri = "";
    public string apiKey = "";
    public string secret = "";
    public string unrealPath = "";

    public Dictionary<string, ProjectSettings> projects = new Dictionary<string, ProjectSettings>();
    public Dictionary<string, FolderSettings> folders = new Dictionary<string, FolderSettings>();

    private static AppSettings _instance = null;
  }

  static class DateExtentions {

    public static long ToUnixSeconds( this DateTime dateTime ) {
      DateTime utcDateTime = dateTime.Kind == DateTimeKind.Unspecified
        ? DateTime.SpecifyKind( dateTime, DateTimeKind.Utc )
        : dateTime.ToUniversalTime();

      return ( (DateTimeOffset)utcDateTime ).ToUnixTimeSeconds();
    }
  }
};