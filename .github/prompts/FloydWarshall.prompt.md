# Floyd_Warshall

Floyd-Warshall 算法：找出圖中所有點對間的最短路徑
Floyd-Warshall 算法是一種用於找出圖中所有點對間最短路徑的動態規劃算法。它可以處理帶有負邊權重的圖（但不能有負權重的回路），並且能有效地求出任意兩點之間的最短路徑。
算法原理
動態規劃思想：
將圖中的所有點編號為 1 到 n。
設 dist[i][j] 表示從點 i 到點 j 的最短路徑長度。
對於每個點 k，考慮是否經過點 k 能夠縮短 i 和 j 之間的路徑。
如果 dist[i][k] + dist[k][j] 小於 dist[i][j]，則更新 dist[i][j]。
算法步驟：
初始化：將 dist 陣列初始化為圖的鄰接矩陣，表示直接相連的點之間的距離，對於不可達的點對，初始化為無窮大。
迭代更新：
對於每個點 k = 1 到 n：
對於每個點對 i 和 j：
如果 dist[i][k] + dist[k][j] 小於 dist[i][j]，則更新 dist[i][j]。

程式碼範例 
```csharp
public static int[,] FloydWarshall(int[,] graph)
{
    int n = graph.GetLength(0);
    int[,] dist = new int[n, n];
    Array.Copy(graph, dist, graph.Length);

    for (int k = 0; k < n; k++)
    {
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (dist[i, k] == int.MaxValue || dist[k, j] == int.MaxValue)
                    continue;
                dist[i, j] = Math.Min(dist[i, j], dist[i, k] + dist[k, j]);
            }
        }
    }

    return dist;
}
```

算法優點
簡單易懂：算法思路清晰，實現相對簡單。
通用性強：適用於各種圖結構，包括帶有負邊權重的圖（但不能有負權重的回路）。
完整性：可以求出所有點對之間的最短路徑。
算法缺點
時間複雜度高：時間複雜度為 O(n^3)，對於大型圖形計算量較大。
空間複雜度高：需要額外一個 n*n 的二維陣列來存儲中間結果。
應用場景
Floyd-Warshall 算法廣泛應用於各種需要計算圖中所有點對間最短路徑的場景，例如：
路徑規劃：計算城市之間的最短路徑。
資訊傳遞：計算訊息在網絡中傳遞的最短路徑。
資源分配：計算不同資源之間的最小成本。
總結
Floyd-Warshall 算法是一個經典的圖算法，雖然時間複雜度較高，但其簡單性和通用性使其在很多場合都有應用。在選擇算法時，需要根據具體問題的規模和要求，權衡時間和空間複雜度。


本專案專門詳細深度廣泛的介紹  Floyd-Warshall 算法，從算法原理、步驟、程式碼範例、優缺點到應用場景，全面解析這個經典的圖算法，幫助讀者深入理解並能夠靈活運用於實際問題中。

.cs 檔案存放程式碼 以及 程式碼加上註解說明

readme.md 檔案存放算法原理、步驟、優缺點和應用場景的詳細介紹，幫助讀者全面理解 Floyd-Warshall 算法的各個方面。
Floyd-Warshall 算法是一個經典的圖算法，適用於計算圖中所有點對間的最短路徑。它的核心思想是通過動態規劃來逐步更新最短路徑的長度，最終得到所有點對之間的最短路徑長度。雖然時間複雜度較高，但其簡單性和通用性使其在很多場合都有應用。在選擇算法時，需要根據具體問題的規模和要求，權衡時間和空間複雜度。


最後提供一些leetcode上的相關題目，幫助讀者練習和鞏固對 Floyd-Warshall 算法的理解和應用。
LeetCode 题目推荐：
1. [LeetCode 743. Network Delay Time](https://leetcode.com/problems/network-delay-time/)
2. [LeetCode 787. Cheapest Flights Within K Stops](https://leetcode.com/problems/cheapest-flights-within-k-stops/)
3. [LeetCode 1334. Find the City With the Smallest Number of Neighbors at a Threshold Distance](https://leetcode.com/problems/find-the-city-with-the-smallest-number-of-neighbors-at-a-threshold-distance/)
4. [LeetCode 847. Shortest Path Visiting All Nodes](https://leetcode.com/problems/shortest-path-visiting-all-nodes/)
5. [LeetCode 743. Network Delay Time](https://leetcode.com/problems/network-delay-time/)
6. [LeetCode 787. Cheapest Flights Within K Stops](https://leetcode.com/problems/cheapest-flights-within-k-stops/)
7. [LeetCode 1334. Find the City With the Smallest Number of Neighbors at a Threshold Distance](https://leetcode.com/problems/find-the-city-with-the-smallest-number-of-neighbors-at-a-threshold-distance/)
8. [LeetCode 847. Shortest Path Visiting All Nodes](https://leetcode.com/problems/shortest-path-visiting-all-nodes/)
這些題目涵蓋了 Floyd-Warshall 算法的不同應用場景，從計算網絡延遲時間到尋找最便宜的航班，再到尋找特定距離內的鄰居城市，幫助讀者在實際問題中靈活運用 Floyd-Warshall 算法，提升解題能力和算法理解。