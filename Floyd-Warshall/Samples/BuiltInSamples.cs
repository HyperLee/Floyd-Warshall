using Floyd_Warshall.Models;

namespace Floyd_Warshall.Samples;

/// <summary>
/// 內建範例集合，含教學範例與 LeetCode 題目對照。
/// </summary>
public static class BuiltInSamples
{
    /// <summary>單一範例的 metadata 與建構工廠。</summary>
    /// <param name="Name">顯示名稱。</param>
    /// <param name="Description">說明文字。</param>
    /// <param name="Build">建構 <see cref="Graph"/> 的委派。</param>
    public sealed record Sample(string Name, string Description, Func<Graph> Build);

    /// <summary>取得所有內建範例。</summary>
    /// <returns>順序穩定的範例清單。</returns>
    public static IReadOnlyList<Sample> All { get; } =
    [
        new("教學標準範例（4 節點）",
            "經典正權有向圖；用於教學基本流程。",
            BuildClassic),
        new("含負權邊但無負環",
            "示範 Floyd-Warshall 可處理負權邊。",
            BuildNegativeEdgesNoCycle),
        new("含負環",
            "示範負環偵測：節點 1→2→3→1 形成總權重 -1 的環。",
            BuildNegativeCycle),
        new("無向圖範例",
            "5 節點無向加權圖。",
            BuildUndirected),
        new("LeetCode 743 Network Delay Time",
            "示例輸入：times=[[2,1,1],[2,3,1],[3,4,1]], n=4, k=2。",
            BuildLeetCode743),
        new("LeetCode 1334 Threshold Distance",
            "示例輸入：n=4, edges=[[0,1,3],[0,2,5],[1,2,1],[1,3,4],[2,3,2]], distanceThreshold=4。",
            BuildLeetCode1334),
    ];

    private static Graph BuildClassic()
    {
        // 經典 4 節點範例：
        // 0->1:3, 0->2:8, 0->4:-4, 1->3:1, 1->4:7, 2->1:4, 3->0:2, 3->2:-5, 4->3:6
        return new Graph(
            5,
            GraphDirection.Directed,
            [
                new Edge(0, 1, 3),
                new Edge(0, 2, 8),
                new Edge(0, 4, -4),
                new Edge(1, 3, 1),
                new Edge(1, 4, 7),
                new Edge(2, 1, 4),
                new Edge(3, 0, 2),
                new Edge(3, 2, -5),
                new Edge(4, 3, 6),
            ],
            ["A", "B", "C", "D", "E"]);
    }

    private static Graph BuildNegativeEdgesNoCycle()
    {
        return new Graph(
            4,
            GraphDirection.Directed,
            [
                new Edge(0, 1, 1),
                new Edge(1, 2, -2),
                new Edge(2, 3, 2),
                new Edge(0, 3, 5),
                new Edge(0, 2, 4),
            ]);
    }

    private static Graph BuildNegativeCycle()
    {
        // 1 -> 2 (1) -> 3 (-3) -> 1 (1) → 環總和 = -1
        return new Graph(
            4,
            GraphDirection.Directed,
            [
                new Edge(0, 1, 2),
                new Edge(1, 2, 1),
                new Edge(2, 3, -3),
                new Edge(3, 1, 1),
            ]);
    }

    private static Graph BuildUndirected()
    {
        return new Graph(
            5,
            GraphDirection.Undirected,
            [
                new Edge(0, 1, 2),
                new Edge(0, 3, 6),
                new Edge(1, 2, 3),
                new Edge(1, 3, 8),
                new Edge(1, 4, 5),
                new Edge(2, 4, 7),
                new Edge(3, 4, 9),
            ]);
    }

    private static Graph BuildLeetCode743()
    {
        // 標籤改為 1-based 顯示，但內部以 0-based 索引。
        return new Graph(
            4,
            GraphDirection.Directed,
            [
                new Edge(1, 0, 1),
                new Edge(1, 2, 1),
                new Edge(2, 3, 1),
            ],
            ["1", "2", "3", "4"]);
    }

    private static Graph BuildLeetCode1334()
    {
        return new Graph(
            4,
            GraphDirection.Undirected,
            [
                new Edge(0, 1, 3),
                new Edge(0, 2, 5),
                new Edge(1, 2, 1),
                new Edge(1, 3, 4),
                new Edge(2, 3, 2),
            ]);
    }
}
