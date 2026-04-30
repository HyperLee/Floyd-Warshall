namespace Floyd_Warshall.Algorithms;

/// <summary>
/// Floyd-Warshall 演算法輸出結果。
/// </summary>
public sealed class FloydWarshallResult
{
    /// <summary>距離矩陣 dist[i,j]；不可達為 <see cref="GraphConstants.Infinity"/>。</summary>
    public required int[,] Distance { get; init; }

    /// <summary>next[i,j]：i 走向 j 的下一個節點；不可達為 null。</summary>
    public required int?[,] Next { get; init; }

    /// <summary>是否偵測到負環。</summary>
    public required bool HasNegativeCycle { get; init; }

    /// <summary>受負環影響的節點集合（dist[i,i] &lt; 0 的節點）。</summary>
    public required IReadOnlySet<int> NegativeCycleNodes { get; init; }

    /// <summary>節點數，等於 <c>Distance.GetLength(0)</c>。</summary>
    public int NodeCount => Distance.GetLength(0);
}
