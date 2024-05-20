using System.Windows;
using CampusHelperApp.UI.ViewModels;

namespace CampusHelperApp.Sandbox;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private void App_OnStartup(object sender, StartupEventArgs e)
    {
        var viewModel = new MainWindowViewModel();
        var window    = new UI.Views.MainWindow { DataContext = viewModel };
        MainWindow = window;
        MainWindow.Show();
    }
}
