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
            JArray projectArray = (JArray)settingsObject[nameof( this.projects )];
            foreach( JToken project in projectArray ){
              this.projects.Add( (string)project );
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
      JArray baseProjectsArray = new JArray();
      foreach( string project in this.projects ){
        baseProjectsArray.Add( project );
      }
      baseObject[nameof( this.projects )] = baseProjectsArray;
      return baseObject.ToString();
    }

    public string serviceUri = "";
    public string apiKey = "";
    public string secret = "";
    public string unrealPath = "";
    public List<string> defaultSyncFolders = new List<string>();

    public List<string> projects = new List<string>();

    private static AppSettings _instance = null;
  }
};