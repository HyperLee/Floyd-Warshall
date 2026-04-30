using System.Globalization;
using Floyd_Warshall.Models;

namespace Floyd_Warshall.IO;

/// <summary>
/// 以 UTF-8 編碼讀取 CSV 圖檔的載入器。
/// 第一行（非註解）為 metadata：<c>nodeCount,direction</c>；之後每行為 <c>from,to,weight</c>。
/// 以 '#' 起始的行為註解，將被忽略。
/// </summary>
public sealed class CsvGraphLoader : IGraphLoader
{
    /// <inheritdoc />
    public Graph Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new GraphLoadException("路徑不可為空。");
        }

        if (!File.Exists(path))
        {
            throw new GraphLoadException($"找不到檔案：{path}");
        }

        string[] rawLines;

        try
        {
            rawLines = File.ReadAllLines(path);
        }
        catch (Exception ex)
        {
            throw new GraphLoadException($"讀取檔案失敗：{ex.Message}", inner: ex);
        }

        int? nodeCount = null;
        GraphDirection? direction = null;
        var edges = new List<Edge>();

        for (var idx = 0; idx < rawLines.Length; idx++)
        {
            var lineNumber = idx + 1;
            var line = rawLines[idx].Trim();

            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            var parts = line.Split(',', StringSplitOptions.TrimEntries);

            if (nodeCount is null)
            {
                (nodeCount, direction) = ParseHeader(parts, lineNumber);
                continue;
            }

            edges.Add(ParseEdge(parts, nodeCount.Value, lineNumber));
        }

        if (nodeCount is null || direction is null)
        {
            throw new GraphLoadException("缺少 metadata 行（nodeCount,direction）。");
        }

        try
        {
            return new Graph(nodeCount.Value, direction.Value, edges);
        }
        catch (ArgumentException ex)
        {
            throw new GraphLoadException(ex.Message, inner: ex);
        }
    }

    private static (int NodeCount, GraphDirection Direction) ParseHeader(
        string[] parts,
        int lineNumber)
    {
        if (parts.Length != 2)
        {
            throw new GraphLoadException(
                $"metadata 行需為 2 個欄位（nodeCount,direction），實際 {parts.Length}。",
                lineNumber);
        }

        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var nc) || nc <= 0)
        {
            throw new GraphLoadException($"nodeCount 不是合法正整數：'{parts[0]}'。", lineNumber);
        }

        var dir = parts[1].ToLowerInvariant() switch
        {
            "directed" => GraphDirection.Directed,
            "undirected" => GraphDirection.Undirected,
            _ => throw new GraphLoadException(
                $"direction 必須是 'directed' 或 'undirected'：'{parts[1]}'。",
                lineNumber),
        };

        return (nc, dir);
    }

    private static Edge ParseEdge(string[] parts, int nodeCount, int lineNumber)
    {
        if (parts.Length != 3)
        {
            throw new GraphLoadException(
                $"邊資料需為 3 個欄位（from,to,weight），實際 {parts.Length}。",
                lineNumber);
        }

        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var from))
        {
            throw new GraphLoadException($"from 不是整數：'{parts[0]}'。", lineNumber);
        }

        if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var to))
        {
            throw new GraphLoadException($"to 不是整數：'{parts[1]}'。", lineNumber);
        }

        if (!int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var weight))
        {
            throw new GraphLoadException($"weight 不是整數：'{parts[2]}'。", lineNumber);
        }

        if ((uint)from >= (uint)nodeCount || (uint)to >= (uint)nodeCount)
        {
            throw new GraphLoadException(
                $"節點索引越界：from={from}, to={to}, nodeCount={nodeCount}。",
                lineNumber);
        }

        return new Edge(from, to, weight);
    }
}
