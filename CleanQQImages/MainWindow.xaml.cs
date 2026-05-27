using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using Wpf.Ui.Appearance;
using Path = System.IO.Path;

namespace CleanQQImages;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
[INotifyPropertyChanged]
public partial class MainWindow
{
    [ObservableProperty]
    public partial string? TargetPath { get; set; }

    [ObservableProperty]
    public partial bool IsOnEntry { get; set; } = true;
    [ObservableProperty]
    public partial bool IsOnOp { get; set; }

    public MainWindow()
    {
        SystemThemeWatcher.Watch(this);
        InitializeComponent();
    }

    private void EntryNext(string target_path)
    {
        TargetPath = target_path;
        IsOnEntry = false;
        IsOnOp = true;
        TheOp.Init(target_path);
    }
    private void OpReturn()
    {
        TargetPath = null;
        IsOnEntry = true;
        IsOnOp = false;
        TheEntry.Init();
    }
}
