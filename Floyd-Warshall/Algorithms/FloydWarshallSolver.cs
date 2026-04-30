using Floyd_Warshall.Models;

namespace Floyd_Warshall.Algorithms;

/// <summary>
/// Floyd-Warshall 演算法的主要進入點。
/// </summary>
/// <remarks>
/// 演算法以三層迴圈執行 O(n³)：對每個中介節點 k，以 dist[i,k] + dist[k,j] 嘗試鬆弛 dist[i,j]。
/// 同步維護 next 矩陣以便日後重建路徑。
/// 完成後再走訪對角線以偵測負環，並將受負環影響的節點對標記為 <see cref="GraphConstants.NegativeInfinity"/>。
/// </remarks>
public static class FloydWarshallSolver
{
    /// <summary>
    /// 對輸入圖求解所有點對最短路徑。
    /// </summary>
    /// <param name="graph">輸入圖（不可為 null）。</param>
    /// <returns>包含距離矩陣、next 矩陣與負環資訊的結果物件。</returns>
    /// <exception cref="ArgumentNullException">graph 為 null。</exception>
    /// <example>
    /// <code>
    /// var result = FloydWarshallSolver.Solve(graph);
    /// var dist = result.Distance[0, 3];
    /// </code>
    /// </example>
    public static FloydWarshallResult Solve(Graph graph)
    {
        if (graph is null)
        {
            throw new ArgumentNullException(nameof(graph));
        }

        var n = graph.NodeCount;
        var dist = graph.ToAdjacencyMatrix();
        var next = new int?[n, n];

        // 初始化 next：i==j 時為 i；可直接到達的邊指向 j；其餘 null。
        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j < n; j++)
            {
                if (i == j)
                {
                    next[i, j] = i;
                }
                else if (dist[i, j] < GraphConstants.Infinity)
                {
                    next[i, j] = j;
                }
            }
        }

        // 三層迴圈執行鬆弛操作。
        for (var k = 0; k < n; k++)
        {
            for (var i = 0; i < n; i++)
            {
                if (dist[i, k] >= GraphConstants.Infinity)
                {
                    continue;
                }

                for (var j = 0; j < n; j++)
                {
                    if (dist[k, j] >= GraphConstants.Infinity)
                    {
                        continue;
                    }

                    var candidate = dist[i, k] + dist[k, j];

                    if (candidate < dist[i, j])
                    {
                        dist[i, j] = candidate;
                        next[i, j] = next[i, k];
                    }
                }
            }
        }

        var negativeNodes = new HashSet<int>();

        for (var i = 0; i < n; i++)
        {
            if (dist[i, i] < 0)
            {
                negativeNodes.Add(i);
            }
        }

        // 將任何「路徑會穿越負環節點」的位置標記為 -∞，並讓 next 失效。
        if (negativeNodes.Count > 0)
        {
            MarkNegativeCycleAffected(dist, next, negativeNodes, n);
        }

        return new FloydWarshallResult
        {
            Distance = dist,
            Next = next,
            HasNegativeCycle = negativeNodes.Count > 0,
            NegativeCycleNodes = negativeNodes,
        };
    }

    private static void MarkNegativeCycleAffected(
        int[,] dist,
        int?[,] next,
        HashSet<int> negativeNodes,
        int n)
    {
        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j < n; j++)
            {
                // 任何能透過某個負環節點 k 抵達的 (i,j)，距離可被無限縮小。
                foreach (var k in negativeNodes)
                {
                    if (dist[i, k] < GraphConstants.Infinity
                        && dist[k, j] < GraphConstants.Infinity)
                    {
                        dist[i, j] = GraphConstants.NegativeInfinity;
                        next[i, j] = null;
                        break;
                    }
                }
            }
        }
    }
}
