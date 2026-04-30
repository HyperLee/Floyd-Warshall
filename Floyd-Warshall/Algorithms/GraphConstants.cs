namespace Floyd_Warshall.Algorithms;

/// <summary>
/// 共用常數：以 <see cref="Infinity"/> 表示「無窮大」，並避免兩個 Infinity 相加溢位。
/// </summary>
public static class GraphConstants
{
    /// <summary>
    /// 距離矩陣中代表「不可達」的常數。
    /// 使用 <see cref="int.MaxValue"/> / 2 是為了避免在三層迴圈
    /// 進行 dist[i,k] + dist[k,j] 加總時整數溢位。
    /// </summary>
    public const int Infinity = int.MaxValue / 2;

    /// <summary>
    /// 受負環影響時，距離矩陣對應位置的標記值。
    /// </summary>
    public const int NegativeInfinity = int.MinValue / 2;
}
