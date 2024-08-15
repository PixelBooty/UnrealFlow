using System.Diagnostics;

namespace UnrealFlow{

  public partial class MainPage : ContentPage{

    public MainPage(){
      this.InitializeComponent();

      if( AppSettings.instance.projects.Count > 0 ){
        this._SetProject( AppSettings.instance.projects.Keys.ToArray()[0] );
      }

      this._ResetProjectList();
      
      
    }

    private void _SetProject( string projectId ){
      if( projectId != this._selectedProjectId ){
        this._selectedProjectId = projectId;
        this.selectedProject.Text = AppSettings.instance.projects[projectId].displayName;
        this.buildButton.IsEnabled = true;
        this.openButton.IsEnabled = true;
        this.resetButton.IsEnabled = true;
        this.syncButton.IsEnabled = true;
        this.editProject.IsEnabled = true;
      }
      else{
        this._selectedProjectId = null;
        this.selectedProject.Text = "None";
        this.buildButton.IsEnabled = false;
        this.openButton.IsEnabled = false;
        this.resetButton.IsEnabled = false;
        this.syncButton.IsEnabled = false;
        this.editProject.IsEnabled = false;
      }

      this._ResetProjectList();
    }

    private static Color _projectColor = new Color( 35, 115, 103 );
    private static Color _selectedProjectColor = new Color( 57, 88, 55 );

    private string _selectedProjectId = null;

    private void _ResetProjectList(){
      this.projectList.Children.Clear();
      this._projectButtons.Clear();
      foreach( KeyValuePair<string, ProjectSettings> project in AppSettings.instance.projects ){
        Button nextProject = new Button(){
          BackgroundColor = this._selectedProjectId != null && this._selectedProjectId == project.Key ? MainPage._selectedProjectColor : MainPage._projectColor,
          WidthRequest = 425,
          HeightRequest = 35,
          Padding = new Thickness( 10, 0 ),
          Text = project.Value.displayName,
          TextColor = new Color( 255, 255, 255 )
        };
        nextProject.Clicked += ( sender, args ) => {
          this._SetProject( project.Key );
        };

        this.projectList.Children.Add( nextProject );
        this._projectButtons.Add( project.Key, nextProject );
      }
    }

    private Dictionary<string, Button> _projectButtons = new Dictionary<string, Button>();

    public async void OnChangeSettings( object sender, EventArgs e ){
      await Shell.Current.GoToAsync( "ChangeSettings" );
    }

    public async void OnAddProject( object sender, EventArgs e ){
      ChangeProjectSettings.passedId = null;
      await Shell.Current.GoToAsync( "ChangeProjectSettings" );
      this._ResetProjectList();
    }

    public async void OnEditProject( object sender, EventArgs e ){
      ChangeProjectSettings.passedId = this._selectedProjectId;
      await Shell.Current.GoToAsync( "ChangeProjectSettings" );
      this._ResetProjectList();
    }

    ProjectSettings _currentProject => AppSettings.instance.projects[this._selectedProjectId];

    private void _RunUnrealBuildToolInNewWindow( Action onEnd ) {
      string toolPath = AppSettings.instance.unrealPath + @"\Engine\Binaries\DotNET\UnrealBuildTool\UnrealBuildTool.exe";
      string projectPath = System.IO.Path.Combine( this._currentProject.projectPath, this._currentProject.projectFile );
      string arguments = this._currentProject.projectFile.Replace(".uproject", "" ) + $"Editor Win64 Development -project=\"{projectPath}\" -WaitMutex -FromMsBuild -progress";
      Console.WriteLine( toolPath );
      Console.WriteLine( projectPath );
      Console.WriteLine( arguments );

      ProcessStartInfo startInfo = new ProcessStartInfo() {
        FileName = toolPath,
        Arguments = arguments,
        UseShellExecute = true, // Important for opening a new window
        CreateNoWindow = false, // No effect when UseShellExecute is true, but here for clarity
      };

      // No need to redirect output since we're displaying it in a new console window
      // No need to set the working directory if the project path is absolute

      try {
        Process process = new Process() {
          StartInfo = startInfo,
          EnableRaisingEvents = true,
        };
        process.Exited += ( sender, e ) => {
          MainThread.BeginInvokeOnMainThread( onEnd );
        };

        process.Start();
        // No need to wait for exit or read output since it's running in a new window
      }
      catch( Exception ex ) {
        // Handle any exceptions that occur
        // For example, log the exception or show a message to the user
      }
    }

    private void _RunUnrealVSRebuildToolInNewWindow( Action onEnd ) {
      string toolPath = AppSettings.instance.unrealPath + @"\Engine\Binaries\DotNET\UnrealBuildTool\UnrealBuildTool.exe";
      string projectPath = System.IO.Path.Combine( this._currentProject.projectPath, this._currentProject.projectFile );
      string arguments = $"-projectfiles -project=\"{projectPath}\" -game -rocket -progress";

      ProcessStartInfo startInfo = new ProcessStartInfo() {
        FileName = toolPath,
        Arguments = arguments,
        UseShellExecute = true, // Important for opening a new window
        CreateNoWindow = false, // No effect when UseShellExecute is true, but here for clarity
      };

      // No need to redirect output since we're displaying it in a new console window
      // No need to set the working directory if the project path is absolute

      try {
        Process process = new Process() {
          StartInfo = startInfo,
          EnableRaisingEvents = true,
        };
        process.Exited += ( sender, e ) => {
          MainThread.BeginInvokeOnMainThread( onEnd );
        };

        process.Start();
      }
      catch( Exception ex ) {
        // Handle any exceptions that occur
        // For example, log the exception or show a message to the user
      }
    }

    private void _ToggleButtons( bool state ){
        this.buildButton.IsEnabled = state;
        this.openButton.IsEnabled = state;
        this.resetButton.IsEnabled = state;
        this.syncButton.IsEnabled = state;
        this.editProject.IsEnabled = state;
        this.changeSettings.IsEnabled = state;
        this.addProject.IsEnabled = state;
    }

    public async void OnBuild( object sender, EventArgs e ){
      this._ToggleButtons( false );
      this._RunUnrealBuildToolInNewWindow( () => this._ToggleButtons( true ) );
    }

    public async void OnOpen( object sender, EventArgs e ){

      string toolPath = AppSettings.instance.unrealPath + @"\Engine\Binaries\DotNET\UnrealBuildTool\UnrealBuildTool.exe";
      string projectPath = System.IO.Path.Combine( this._currentProject.projectPath, this._currentProject.projectFile );

      ProcessStartInfo startInfo = new ProcessStartInfo() {
        FileName = toolPath,
        Arguments = projectPath,
        WorkingDirectory = System.IO.Path.GetDirectoryName( projectPath ),
        UseShellExecute = true, // Important for opening a new window
        Verb = "OPEN",
      };

      Process.Start( startInfo );

    }

    public async void OnReset( object sender, EventArgs e ){
      this._ToggleButtons( false );
      this._RunUnrealVSRebuildToolInNewWindow( () => this._ToggleButtons( true ) );
    }

    public async void OnSync( object sender, EventArgs e ){
      this._ToggleButtons( false );
      await (new SyncSystem(this.status, this._currentProject.projectPath)).SyncBucket( this._currentProject );
      this._ToggleButtons( true );
    }

  }

}