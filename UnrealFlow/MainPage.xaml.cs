namespace UnrealFlow;

public partial class MainPage : ContentPage{
  int count = 0;

  public MainPage(){
    this.InitializeComponent();
  }

  public async void OnChangeSettings( object sender, EventArgs e ){
    await Shell.Current.GoToAsync( "ChangeSettings" );
  }

  public async void OnAddProject( object sender, EventArgs e ){
    await Shell.Current.GoToAsync( "ChangeProjectSettings?id=" );
  }
}

