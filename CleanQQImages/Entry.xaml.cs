using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.WindowsAPICodePack.Dialogs;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace CleanQQImages;

[INotifyPropertyChanged]
public partial class Entry
{
    [ObservableProperty]
    public partial string TencentFiles { get; set; }

    [ObservableProperty]
    public partial List<string> QQFolders { get; set; } = new();

    public event Action<string>? OnNext;

    public Entry()
    {
        InitializeComponent();

        TencentFiles = Path.GetFullPath($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\Tencent Files\\");
        FindTargets();
    }

    public void Init()
    {
        QQFolderListView.SelectedIndex = -1;
    }

    private void SelectTencentFilesPath(object sender, RoutedEventArgs e)
    {
        var dialog = new CommonOpenFileDialog
        {
            Title = "选择 Tencent Files 路径",
            IsFolderPicker = true,
            InitialDirectory = TencentFiles,
        };
        if (dialog.ShowDialog() is CommonFileDialogResult.Ok)
        {
            TencentFiles = dialog.FileName;
            FindTargets();
        }
    }

    private void FindTargets()
    {
        var sub_dirs = Directory.EnumerateDirectories(TencentFiles);
        QQFolders = (from path in sub_dirs let name = Path.GetFileName(path) where AllNumber.IsMatch(name) select path).ToList();
        QQFolderListView.SelectedIndex = -1;
    }

    private static readonly Regex AllNumber = new(@"^\d+$", RegexOptions.Compiled);

    private void OnSelectQQFolder(object sender, SelectionChangedEventArgs e)
    {
        var index = QQFolderListView.SelectedIndex;
        if (index < 0) return;
        OnNext?.Invoke(QQFolders[index]);
    }
}
