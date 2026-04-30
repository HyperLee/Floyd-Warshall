namespace Floyd_Warshall.Algorithms;

/// <summary>
/// 依 <see cref="FloydWarshallResult.Next"/> 矩陣重建最短路徑。
/// </summary>
public static class PathReconstructor
{
    /// <summary>
    /// 重建從 <paramref name="from"/> 到 <paramref name="to"/> 的最短路徑節點序列。
    /// </summary>
    /// <param name="result">演算法輸出。</param>
    /// <param name="from">起點索引。</param>
    /// <param name="to">終點索引。</param>
    /// <returns>
    /// 節點索引序列（含起點與終點）；
    /// 若不可達或路徑受負環影響，回傳空集合。
    /// </returns>
    /// <exception cref="ArgumentNullException">result 為 null。</exception>
    /// <exception cref="ArgumentOutOfRangeException">from / to 超出節點範圍。</exception>
    public static IReadOnlyList<int> Reconstruct(FloydWarshallResult result, int from, int to)
    {
        if (result is null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        var n = result.NodeCount;

        if ((uint)from >= (uint)n)
        {
            throw new ArgumentOutOfRangeException(nameof(from));
        }

        if ((uint)to >= (uint)n)
        {
            throw new ArgumentOutOfRangeException(nameof(to));
        }

        // 受負環影響或不可達 → 空集合。
        if (result.Next[from, to] is null)
        {
            return [];
        }

        if (result.Distance[from, to] >= GraphConstants.Infinity)
        {
            return [];
        }

        var path = new List<int> { from };
        var current = from;
        // 追加保險上限以避免任何不可預期循環導致無限迴圈。
        var safety = n + 1;

        while (current != to)
        {
            var step = result.Next[current, to];

            if (step is null)
            {
                return [];
            }

            current = step.Value;
            path.Add(current);

            if (--safety < 0)
            {
                return [];
            }
        }

        return path;
    }
}
