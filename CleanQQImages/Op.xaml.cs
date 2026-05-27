using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using SearchOption = System.IO.SearchOption;

namespace CleanQQImages;

public enum TimeBeforeMode
{
    Custom,
    OneWeek,
    OneMonth,
    TwoMonth,
    HalfYear,
    OneYear,
    TwoYear,
}

public enum DeleteMode
{
    Recycle,
    Delete,
    Search,
}

[INotifyPropertyChanged]
public partial class Op
{
    public Op()
    {
        InitializeComponent();
        SelectDate((TimeBeforeMode)TimeBefore);
        var dpd = DependencyPropertyDescriptor.FromProperty(CalendarDatePicker.DateProperty, typeof(CalendarDatePicker));
        dpd.AddValueChanged(TheCalendarDatePicker, OnCalendarDatePickerDateChange);
    }

    private void OnCalendarDatePickerDateChange(object? sender, EventArgs e)
    {
        TimeBefore = (int)TimeBeforeMode.Custom;
        BeforeData = TheCalendarDatePicker.Date!.Value;
        BeforeDataText = $"{BeforeData:yyyy-MM-dd}";
    }

    [ObservableProperty]
    public partial Visibility ProgressBarVisibility { get; set; } = Visibility.Hidden;
    [ObservableProperty]
    public partial bool IsIndeterminate { get; set; }
    [ObservableProperty]
    public partial bool AllHitTestVisible { get; set; } = true;

    [ObservableProperty]
    public partial string TargetPath { get; set; } = "";

    [ObservableProperty]
    public partial int TimeBefore { get; set; } = (int)TimeBeforeMode.OneYear;
    [ObservableProperty]
    public partial DateTime BeforeData { get; set; }
    [ObservableProperty]
    public partial string BeforeDataText { get; set; }

    [ObservableProperty]
    public partial int RunMode { get; set; } = 2;

    [ObservableProperty]
    public partial string State { get; set; } = "";

    public event Action? OnReturn;

    public void Init(string TargetPath)
    {
        this.TargetPath = Path.GetFullPath(@$"{TargetPath}\Image\Group2\");
        State = "";
    }

    private void SelectDate(TimeBeforeMode Mode)
    {
        BeforeData = Mode switch
        {
            TimeBeforeMode.OneWeek => DateTime.Today.AddDays(-7),
            TimeBeforeMode.OneMonth => DateTime.Today.AddMonths(-1),
            TimeBeforeMode.TwoMonth => DateTime.Today.AddMonths(-2),
            TimeBeforeMode.HalfYear => DateTime.Today.AddMonths(-6),
            TimeBeforeMode.OneYear => DateTime.Today.AddYears(-1),
            TimeBeforeMode.TwoYear => DateTime.Today.AddYears(-2),
            _ => BeforeData,
        };
        BeforeDataText = $"{BeforeData:yyyy-MM-dd}";
    }

    private void OnSelectDateModeChange(object sender, SelectionChangedEventArgs e) =>
        SelectDate((TimeBeforeMode)SelectDateModeCombo.SelectedIndex);

    private void OnReturnClicked(object sender, RoutedEventArgs e)
    {
        OnReturn?.Invoke();
    }

    private void OnCancelClicked(object sender, RoutedEventArgs e)
    {
        cts?.Cancel();
        Done();
    }

    private void OnStartClicked(object sender, RoutedEventArgs e)
    {
        ProgressBarVisibility = Visibility.Visible;
        IsIndeterminate = true;
        AllHitTestVisible = false;
        State = "正在扫描...";
        ReturnButton.Visibility = Visibility.Collapsed;
        CancelButton.Visibility = Visibility.Visible;
        cts = new();
        Task.Run(() => Start(cts.Token), cts.Token);
    }

    private void Done(bool ResetState = true)
    {
        ProgressBarVisibility = Visibility.Hidden;
        ReturnButton.Visibility = Visibility.Visible;
        CancelButton.Visibility = Visibility.Collapsed;
        IsIndeterminate = false;
        AllHitTestVisible = true;
        if (ResetState) State = "";
    }

    private CancellationTokenSource? cts;

    public sealed class Counter
    {
        public static NumberFormatInfo Format = new()
        {
            NumberGroupSizes = [4],
            NumberGroupSeparator = ",",
        };

        public int Total;
        public int Found;
        public int Removed;
        public int Error;
        public long TotalSize;
        public long Done;
        public long StartTime = Stopwatch.GetTimestamp();

        public string BuildMessage(string msg)
        {
            var duration = new TimeSpan((long)((Stopwatch.GetTimestamp() - StartTime) * ((double)TimeSpan.TicksPerSecond / Stopwatch.Frequency)));

            var total = Total.ToString("N0", Format);
            var found = Found.ToString("N0", Format);
            var removed = Removed.ToString("N0", Format);
            var error = Error.ToString("N0", Format);
            var total_size = Utils.FormatByteSize(TotalSize);
            var time = Utils.FormatTime(duration);
            return $"{msg}        已扫描 {total} | 已找到 {found} | 总大小 {total_size} | 已删除 {removed} | 出错 {error} | 耗时 {time}";
        }
    }

    private async Task Start(CancellationToken ct)
    {
        Counter counter = new();
        try
        {
            var root_directory = new DirectoryInfo(TargetPath).EnumerateDirectories("*", SearchOption.TopDirectoryOnly).ToList();
            ct.ThrowIfCancellationRequested();
            var before = BeforeData;
            var mode = (DeleteMode)RunMode;
            var tasks = root_directory
                .Select(d => Task.Factory.StartNew(() => ScanSubTask(ct, counter, d, before, mode), TaskCreationOptions.LongRunning))
                .ToList();
            var msg = MessageTask(ct, counter);
            State = counter.BuildMessage("扫描中");
            await Task.WhenAll(tasks);
            Interlocked.Exchange(ref counter.Done, 1);
            await msg;
            State = counter.BuildMessage("已完成");
            _ = Dispatcher.BeginInvoke(() => Done(false));
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task MessageTask(CancellationToken ct, Counter counter)
    {
        for (;;)
        {
            if (Interlocked.Read(ref counter.Done) != 0) return;
            ct.ThrowIfCancellationRequested();
            await Task.Delay(100, ct);
            State = counter.BuildMessage("扫描中");
        }
    }

    private void ScanSubTask(CancellationToken ct, Counter counter, DirectoryInfo directory, DateTime before, DeleteMode mode)
    {
        foreach (var file in directory.EnumerateFiles("*", SearchOption.AllDirectories))
        {
            ct.ThrowIfCancellationRequested();
            Interlocked.Increment(ref counter.Total);
            if (file.LastWriteTime >= before) continue;
            Interlocked.Increment(ref counter.Found);
            Interlocked.Add(ref counter.TotalSize, file.Length);
            try
            {
                switch (mode)
                {
                    case DeleteMode.Recycle:
                        RecycleBin.Send(file.FullName);
                        break;
                    case DeleteMode.Delete:
                        File.Delete(file.FullName);
                        break;
                    case DeleteMode.Search:
                    default:
                        // 不删除
                        continue;
                }
                Interlocked.Increment(ref counter.Removed);
            }
            catch (Exception e)
            {
                Interlocked.Increment(ref counter.Error);
                Console.WriteLine(e.Message);
            }
        }
    }
}
