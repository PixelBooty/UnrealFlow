namespace UnrealFlow;

public partial class App : Application{
  public App(){
    this.InitializeComponent();

    MainPage = new AppShell();
  }
}
