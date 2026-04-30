namespace Floyd_Warshall.Models;

/// <summary>
/// 圖的方向性。
/// </summary>
public enum GraphDirection
{
    /// <summary>有向圖：每條邊只在指定方向存在。</summary>
    Directed,

    /// <summary>無向圖：每條邊會雙向存在，兩方向權重需一致。</summary>
    Undirected,
}
