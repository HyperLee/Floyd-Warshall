using System.Text.Json;
using System.Text.Json.Serialization;
using Floyd_Warshall.Models;

namespace Floyd_Warshall.IO;

/// <summary>
/// 以 <see cref="System.Text.Json"/> 解析 JSON 圖檔的載入器。
/// </summary>
public sealed class JsonGraphLoader : IGraphLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

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

        GraphJsonDto? dto;

        try
        {
            using var stream = File.OpenRead(path);
            dto = JsonSerializer.Deserialize<GraphJsonDto>(stream, Options);
        }
        catch (JsonException ex)
        {
            throw new GraphLoadException(
                $"JSON 解析失敗：{ex.Message}",
                lineNumber: (int?)(ex.LineNumber + 1),
                inner: ex);
        }
        catch (Exception ex)
        {
            throw new GraphLoadException($"讀取檔案失敗：{ex.Message}", inner: ex);
        }

        if (dto is null)
        {
            throw new GraphLoadException("JSON 內容為空。");
        }

        if (dto.NodeCount <= 0)
        {
            throw new GraphLoadException($"nodeCount 不是合法正整數：{dto.NodeCount}。");
        }

        var direction = dto.Direction?.ToLowerInvariant() switch
        {
            "directed" => GraphDirection.Directed,
            "undirected" => GraphDirection.Undirected,
            null => throw new GraphLoadException("缺少 direction 欄位。"),
            _ => throw new GraphLoadException($"direction 必須是 'directed' 或 'undirected'：'{dto.Direction}'。"),
        };

        if (dto.Edges is null)
        {
            throw new GraphLoadException("缺少 edges 欄位。");
        }

        var edges = new List<Edge>(dto.Edges.Count);

        for (var i = 0; i < dto.Edges.Count; i++)
        {
            var e = dto.Edges[i];

            if ((uint)e.From >= (uint)dto.NodeCount || (uint)e.To >= (uint)dto.NodeCount)
            {
                throw new GraphLoadException(
                    $"第 {i + 1} 條邊節點索引越界：from={e.From}, to={e.To}, nodeCount={dto.NodeCount}。");
            }

            edges.Add(new Edge(e.From, e.To, e.Weight));
        }

        try
        {
            return new Graph(dto.NodeCount, direction, edges, dto.Labels);
        }
        catch (ArgumentException ex)
        {
            throw new GraphLoadException(ex.Message, inner: ex);
        }
    }

    // 內部 DTO，僅供反序列化使用。
    private sealed class GraphJsonDto
    {
        [JsonPropertyName("nodeCount")] public int NodeCount { get; set; }

        [JsonPropertyName("direction")] public string? Direction { get; set; }

        [JsonPropertyName("labels")] public List<string>? Labels { get; set; }

        [JsonPropertyName("edges")] public List<EdgeDto>? Edges { get; set; }
    }

    private sealed class EdgeDto
    {
        [JsonPropertyName("from")] public int From { get; set; }

        [JsonPropertyName("to")] public int To { get; set; }

        [JsonPropertyName("weight")] public int Weight { get; set; }
    }
}
