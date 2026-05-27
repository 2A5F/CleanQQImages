using System.Configuration;
using System.Data;
using System.Windows;
using Wpf.Ui.Appearance;

namespace CleanQQImages;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private void OnStartup(object sender, StartupEventArgs e)
    {
        ApplicationThemeManager.ApplySystemTheme();
    }
}
