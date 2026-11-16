using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
namespace VaceHub { sealed partial class App : Application {
 public App() { InitializeComponent(); RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested; }
 protected override void OnLaunched(LaunchActivatedEventArgs e) { if (Window.Current.Content == null) Window.Current.Content = new MainPage(); Window.Current.Activate(); }
} }
