using System.Text;

namespace CleanQQImages;

public static class Utils
{
    public static string FormatByteSize(long Size, int DecimalPlaces = 2)
    {
        if (Size < 0) return $"{Size} B";
        if (Size == 0) return "0 B";

        ReadOnlySpan<string> suffixes = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];

        var i = (int)Math.Floor(Math.Log(Size, 1024));
        if (i >= suffixes.Length) i = suffixes.Length - 1;
        var num = Size / Math.Pow(1024, i);
        var format_specifier = "0." + new string('0', DecimalPlaces);
        if (DecimalPlaces <= 0) format_specifier = "0";
        return $"{num.ToString(format_specifier)} {suffixes[i]}";
    }

    public static string FormatTime(TimeSpan t)
    {
        if (t < TimeSpan.Zero) return $"- {FormatTime(-t)}";
        if (t == TimeSpan.Zero) return "0秒";

        var sb = new StringBuilder();

        if (t.Days > 0) sb.Append($"{t.Days}天 ");
        if (t.Hours > 0) sb.Append($"{t.Hours}小时 ");
        if (t.Minutes > 0) sb.Append($"{t.Minutes}分钟 ");

        var remainingTicks = t.Ticks % TimeSpan.TicksPerMinute;
        var seconds = (double)remainingTicks / TimeSpan.TicksPerSecond;

        if (seconds > 0 || sb.Length == 0)
        {
            sb.Append($"{seconds:0.00}秒");
        }

        return sb.ToString().Trim();
    }
}
