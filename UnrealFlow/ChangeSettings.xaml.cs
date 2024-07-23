
using System;
using CommunityToolkit.Maui.Storage;

namespace UnrealFlow{
  public partial class ChangeSettings : ContentPage{
    public ChangeSettings(){
      this.InitializeComponent();
      this.AppDataDirectoryLabel.Text = "App data directory:" + FileSystem.AppDataDirectory;
      this.serviceUri.Text = AppSettings.instance.serviceUri;
      this.unrealPath.Text = AppSettings.instance.unrealPath;
      this.defaultSyncFolders.Text = String.Join( ", ", AppSettings.instance.defaultSyncFolders );
    }

    public void OnCancel( object sender, EventArgs e ){
      Shell.Current.GoToAsync( ".." );
    }

    public async void OnLookupUnreal( object sender, EventArgs e ){
      CancellationTokenSource source = new();
      CancellationToken token = source.Token;
      FolderPickerResult result = await FolderPicker.Default.PickAsync(token);
      
      if( result.IsSuccessful ){
        this.unrealPath.Text = result.Folder.Path;
      }
    }

    public void OnApply( object sender, EventArgs e ){
      AppSettings.instance.serviceUri = this.serviceUri.Text;
      if( this.unrealPath.Text != null && this.unrealPath.Text != "" ){
        AppSettings.instance.unrealPath = this.unrealPath.Text;
      }
      if( this.apiKey.Text != null && this.apiKey.Text != "" ){
        AppSettings.instance.apiKey = this.apiKey.Text;
      }
      if( this.secret.Text != null && this.secret.Text != "" ){
        AppSettings.instance.secret = this.secret.Text;
      }
      if( this.defaultSyncFolders.Text != null && this.defaultSyncFolders.Text != "" ){
        AppSettings.instance.defaultSyncFolders = new List<String>(
          this.defaultSyncFolders.Text.Split(",").Select( defaultSyncFolder => defaultSyncFolder.Trim() )
        );
      }
      AppSettings.instance.Save();
      Shell.Current.GoToAsync( ".." );
    }
  }
};