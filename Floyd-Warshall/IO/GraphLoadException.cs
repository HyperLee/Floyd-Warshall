namespace Floyd_Warshall.IO;

/// <summary>
/// 載入圖資料時發生的錯誤；可附帶來源檔案的行號方便除錯。
/// </summary>
public sealed class GraphLoadException : Exception
{
    /// <summary>來源檔案行號（1-based）；若無法定位則為 null。</summary>
    public int? LineNumber { get; }

    /// <summary>建立一個新的 <see cref="GraphLoadException"/>。</summary>
    /// <param name="message">錯誤訊息。</param>
    /// <param name="lineNumber">行號（1-based），可選。</param>
    /// <param name="inner">內部例外。</param>
    public GraphLoadException(string message, int? lineNumber = null, Exception? inner = null)
        : base(FormatMessage(message, lineNumber), inner)
    {
        LineNumber = lineNumber;
    }

    private static string FormatMessage(string message, int? lineNumber)
        => lineNumber is { } n ? $"[行 {n}] {message}" : message;
}
