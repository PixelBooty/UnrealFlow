namespace UnrealFlow {

  public class FolderSettings {
    public string displayName = "";
    public string syncName = "";
    public string folderPath = "";

    public SyncOverrides syncOverrides = new SyncOverrides();

    public Bucket bucket => new Bucket() {
      apiKey = AppSettings.instance.apiKey,
      secret = AppSettings.instance.secret,
      name = this.syncName
    };

  }

};