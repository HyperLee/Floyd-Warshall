namespace Floyd_Warshall.Models;

/// <summary>
/// 表示圖中的一條邊。
/// </summary>
/// <param name="From">起點節點索引（0-based）。</param>
/// <param name="To">終點節點索引（0-based）。</param>
/// <param name="Weight">邊的權重，可為負值。</param>
public sealed record Edge(int From, int To, int Weight);
