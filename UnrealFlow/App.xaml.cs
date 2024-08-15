namespace UnrealFlow;

public partial class App : Application{
  public App(){
    this.InitializeComponent();

    MainPage = new AppShell();
  }

  protected override Window CreateWindow(IActivationState activationState){
    var window = base.CreateWindow(activationState);
    window.MaximumHeight = 600; // set minimal window height
    window.MaximumWidth = 800; // set minimal window width
    return window;
  }
}
