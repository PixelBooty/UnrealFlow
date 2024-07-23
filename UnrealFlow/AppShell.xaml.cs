namespace UnrealFlow;

public partial class AppShell : Shell{
  public AppShell(){
    this.InitializeComponent();
    Routing.RegisterRoute( "ChangeSettings", typeof( ChangeSettings ) );
    Routing.RegisterRoute( "ChangeProjectSettings", typeof( ChangeProjectSettings ) );
  }
}
