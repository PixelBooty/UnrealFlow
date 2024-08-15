using CommunityToolkit.Maui.Storage;

namespace UnrealFlow{

	public partial class ChangeProjectSettings : ContentPage{
		public ChangeProjectSettings(){
			InitializeComponent();
			this.id = ChangeProjectSettings.passedId;
			if( this.id == "" ){
				this.nameHeader.Text = "Add Project";
				this.syncPaths.Text = String.Join( ", ", AppSettings.instance.defaultSyncFolders );
			}
			else {
				ProjectSettings projectSettings = AppSettings.instance.projects[this.id];
				this.nameHeader.Text = "Project settings " + projectSettings.displayName;
				this.displayName.Text = projectSettings.displayName;
				this.syncName.Text = projectSettings.syncName;
				this.versionsToKeep.Text = projectSettings.versionsToKeep + "";
				this.projectPath.Text = projectSettings.projectPath;
				this.syncPaths.Text = String.Join( ", ", projectSettings.syncPaths );
			}
		}

		public static string passedId = null;

		public void OnCancel( object sender, EventArgs e ){
      Shell.Current.GoToAsync( ".." );
    }

		public async void OnLookupProjectPath( object sender, EventArgs e ){
			CancellationTokenSource source = new();
      CancellationToken token = source.Token;
      FolderPickerResult result = await FolderPicker.Default.PickAsync(token);
      
      if( result.IsSuccessful ){
        this.projectPath.Text = result.Folder.Path;
      }
		}

		public void OnApply( object sender, EventArgs e ){
			if( this.id == "" ){
				string newId = Guid.NewGuid().ToString();
				AppSettings.instance.projects.Add( newId, new ProjectSettings(){
					projectPath = this.projectPath.Text,
					displayName = this.displayName.Text,
					syncName = this.syncName.Text,
					versionsToKeep = int.Parse( this.versionsToKeep.Text ),
					syncPaths = new List<string>(
						this.syncPaths.Text.Split(',').Select( syncFolder => syncFolder.Trim() ).ToArray()
					)
				} );
			}
			else {
				ProjectSettings project = AppSettings.instance.projects[this.id];
				project.projectPath = this.projectPath.Text;
				project.displayName = this.displayName.Text;
				project.syncName = this.syncName.Text;
				project.versionsToKeep = int.Parse( this.versionsToKeep.Text );
				project.syncPaths = new List<string>(
					this.syncPaths.Text.Split(',').Select( syncFolder => syncFolder.Trim() ).ToArray()
				);
			}
			AppSettings.instance.Save();
      Shell.Current.GoToAsync( ".." );
    }

		public string id { get; set; } = "";
	}
};