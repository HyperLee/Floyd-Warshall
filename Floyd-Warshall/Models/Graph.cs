using Floyd_Warshall.Algorithms;

namespace Floyd_Warshall.Models;

/// <summary>
/// 以鄰接矩陣為核心、含 metadata 的圖資料結構。
/// 建構時會依 <see cref="Direction"/> 規則驗證並（必要時）補上對稱邊。
/// </summary>
public sealed class Graph
{
    private readonly List<Edge> edges;
    private readonly string[] labels;

    /// <summary>節點數（必須 &gt; 0）。</summary>
    public int NodeCount { get; }

    /// <summary>圖的方向性。</summary>
    public GraphDirection Direction { get; }

    /// <summary>節點顯示用標籤；若未指定則為 "0"…"n-1"。</summary>
    public IReadOnlyList<string> Labels => labels;

    /// <summary>規範化後的邊集合（無向圖會包含補上的對稱邊）。</summary>
    public IReadOnlyList<Edge> Edges => edges;

    /// <summary>
    /// 建立一個圖。
    /// </summary>
    /// <param name="nodeCount">節點數。</param>
    /// <param name="direction">方向性。</param>
    /// <param name="edges">原始邊集合。</param>
    /// <param name="labels">顯示用標籤；若為 null 則自動產生 "0"…"n-1"。</param>
    /// <exception cref="ArgumentOutOfRangeException">節點數 ≤ 0。</exception>
    /// <exception cref="ArgumentException">邊資料不合法（重複、權重衝突或索引越界）。</exception>
    public Graph(
        int nodeCount,
        GraphDirection direction,
        IEnumerable<Edge> edges,
        IReadOnlyList<string>? labels = null)
    {
        if (nodeCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(nodeCount), "節點數必須大於 0。");
        }

        if (edges is null)
        {
            throw new ArgumentNullException(nameof(edges));
        }

        NodeCount = nodeCount;
        Direction = direction;

        if (labels is null)
        {
            this.labels = Enumerable.Range(0, nodeCount).Select(i => i.ToString()).ToArray();
        }
        else
        {
            if (labels.Count != nodeCount)
            {
                throw new ArgumentException("Labels 數量需等於節點數。", nameof(labels));
            }

            this.labels = [.. labels];
        }

        this.edges = NormalizeEdges(edges, nodeCount, direction);
    }

    /// <summary>
    /// 將圖轉成鄰接矩陣；不存在的邊以 <see cref="GraphConstants.Infinity"/> 表示，對角線為 0。
    /// </summary>
    /// <returns>n×n 的鄰接矩陣。</returns>
    public int[,] ToAdjacencyMatrix()
    {
        var n = NodeCount;
        var matrix = new int[n, n];

        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j < n; j++)
            {
                matrix[i, j] = i == j ? 0 : GraphConstants.Infinity;
            }
        }

        foreach (var edge in edges)
        {
            // 取較小權重以容忍同方向重複邊（驗證已通過：等值或衝突已先擋下）。
            if (edge.Weight < matrix[edge.From, edge.To])
            {
                matrix[edge.From, edge.To] = edge.Weight;
            }
        }

        return matrix;
    }

    private static List<Edge> NormalizeEdges(
        IEnumerable<Edge> source,
        int nodeCount,
        GraphDirection direction)
    {
        // 以 (from,to) 為 key 收集；無向圖會補入反向邊並驗證一致性。
        var map = new Dictionary<(int From, int To), int>();

        foreach (var edge in source)
        {
            ValidateBounds(edge, nodeCount);

            AddOrValidate(map, edge.From, edge.To, edge.Weight);

            if (direction == GraphDirection.Undirected && edge.From != edge.To)
            {
                AddOrValidate(map, edge.To, edge.From, edge.Weight);
            }
        }

        return [.. map.Select(kv => new Edge(kv.Key.From, kv.Key.To, kv.Value))];
    }

    private static void ValidateBounds(Edge edge, int nodeCount)
    {
        if ((uint)edge.From >= (uint)nodeCount || (uint)edge.To >= (uint)nodeCount)
        {
            throw new ArgumentException(
                $"邊 ({edge.From}->{edge.To}) 的節點索引超出範圍 [0,{nodeCount - 1}]。",
                nameof(edge));
        }
    }

    private static void AddOrValidate(
        Dictionary<(int From, int To), int> map,
        int from,
        int to,
        int weight)
    {
        var key = (from, to);

        if (map.TryGetValue(key, out var existing))
        {
            if (existing != weight)
            {
                throw new ArgumentException(
                    $"邊 ({from}->{to}) 出現權重衝突：{existing} 與 {weight}。");
            }

            return;
        }

        map[key] = weight;
    }
}
