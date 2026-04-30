using System.Text;
using Floyd_Warshall.Algorithms;
using Floyd_Warshall.Models;

namespace Floyd_Warshall.IO;

/// <summary>
/// 將矩陣以對齊欄位的方式格式化為字串，方便在主控台顯示。
/// </summary>
public static class GraphPrinter
{
    private const string InfinitySymbol = "∞";
    private const string NegInfinitySymbol = "-∞";

    /// <summary>
    /// 將距離矩陣格式化為字串。
    /// <see cref="GraphConstants.Infinity"/> 會被顯示為 "∞"，
    /// <see cref="GraphConstants.NegativeInfinity"/> 顯示為 "-∞"。
    /// </summary>
    /// <param name="matrix">距離矩陣。</param>
    /// <param name="labels">節點標籤；若 null 則使用索引字串。</param>
    /// <returns>對齊後的多行字串。</returns>
    public static string FormatDistance(int[,] matrix, IReadOnlyList<string>? labels = null)
    {
        var n = matrix.GetLength(0);
        var cells = new string[n, n];

        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j < n; j++)
            {
                cells[i, j] = FormatDistanceCell(matrix[i, j]);
            }
        }

        return RenderTable(cells, labels, n);
    }

    /// <summary>
    /// 將 next 矩陣格式化為字串，null 顯示為 "-"。
    /// </summary>
    public static string FormatNext(int?[,] matrix, IReadOnlyList<string>? labels = null)
    {
        var n = matrix.GetLength(0);
        var cells = new string[n, n];

        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j < n; j++)
            {
                cells[i, j] = matrix[i, j] is { } v ? v.ToString() : "-";
            }
        }

        return RenderTable(cells, labels, n);
    }

    private static string FormatDistanceCell(int value) => value switch
    {
        >= GraphConstants.Infinity => InfinitySymbol,
        <= GraphConstants.NegativeInfinity => NegInfinitySymbol,
        _ => value.ToString(),
    };

    private static string RenderTable(string[,] cells, IReadOnlyList<string>? labels, int n)
    {
        var labelArr = labels ?? Enumerable.Range(0, n).Select(i => i.ToString()).ToArray();

        // 計算每欄寬度：max(label, 該欄所有資料儲存格)。
        var widths = new int[n];

        for (var j = 0; j < n; j++)
        {
            widths[j] = labelArr[j].Length;

            for (var i = 0; i < n; i++)
            {
                if (cells[i, j].Length > widths[j])
                {
                    widths[j] = cells[i, j].Length;
                }
            }
        }

        var rowLabelWidth = labelArr.Max(s => s.Length);
        var sb = new StringBuilder();

        // 表頭。
        sb.Append(' ', rowLabelWidth).Append(" |");

        for (var j = 0; j < n; j++)
        {
            sb.Append(' ').Append(labelArr[j].PadLeft(widths[j]));
        }

        sb.AppendLine();
        sb.Append('-', rowLabelWidth).Append("-+");

        for (var j = 0; j < n; j++)
        {
            sb.Append('-', widths[j] + 1);
        }

        sb.AppendLine();

        for (var i = 0; i < n; i++)
        {
            sb.Append(labelArr[i].PadLeft(rowLabelWidth)).Append(" |");

            for (var j = 0; j < n; j++)
            {
                sb.Append(' ').Append(cells[i, j].PadLeft(widths[j]));
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}
