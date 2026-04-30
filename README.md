# Floyd-Warshall

> 以 .NET 10 / C# 14 實作的 **Floyd-Warshall（所有點對最短路徑）** 教學專案。
> 包含可重用的核心函式庫類別、互動式 Console 應用，以及 xUnit 測試。

---

## 1. 專案簡介

本專案以教學與實務並重的方式，深入介紹 Floyd-Warshall 演算法：

- **Floyd_Warshall.Algorithms**：演算法核心（`FloydWarshallSolver`、`PathReconstructor`、`FloydWarshallResult`）。
- **Floyd_Warshall.Models**：圖資料結構（`Graph`、`Edge`、`GraphDirection`）。
- **Floyd_Warshall.IO**：CSV / JSON 圖檔載入、矩陣列印。
- **Floyd_Warshall.UI**：互動式 Console 主選單。
- **Floyd_Warshall.Tests**：xUnit 單元測試。

---

## 2. 演算法原理

Floyd-Warshall 是以動態規劃求解 **All-Pairs Shortest Path (APSP)** 的經典演算法。

### 狀態定義

令 `dist[k][i][j]` 為「只允許使用編號 ≤ k 的中介節點時，i 到 j 的最短距離」，則遞推式為：

```
dist[k][i][j] = min(
    dist[k-1][i][j],            // 不使用 k
    dist[k-1][i][k] + dist[k-1][k][j]   // 使用 k 作為中介
)
```

由於 `dist[k][..][..]` 只依賴 `dist[k-1][..][..]`，可降為二維滾動陣列 `dist[i,j]`。

---

## 3. 演算法步驟

1. 以鄰接矩陣初始化 `dist`：自身為 0、有邊填權重、其餘填 `+∞`。
2. 同步初始化 `next[i,j]`：`i==j` 為 i；有邊指向 j；否則 null。
3. 對每個中介節點 `k`、起點 `i`、終點 `j`：
   - 若 `dist[i,k] + dist[k,j] < dist[i,j]`，更新 `dist[i,j]` 並令 `next[i,j] = next[i,k]`。
4. 結束後檢查對角線：若 `dist[i,i] < 0`，代表 i 在某個負環上。

---

## 4. C# 實作範例

### 最簡版

```csharp
const int INF = int.MaxValue / 2;
int n = graph.Length;

for (int k = 0; k < n; k++)
    for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
            if (dist[i, k] + dist[k, j] < dist[i, j])
                dist[i, j] = dist[i, k] + dist[k, j];
```

### 本專案差異

- 同步維護 `next` 矩陣以重建路徑（`PathReconstructor`）。
- 以 `GraphConstants.Infinity = int.MaxValue / 2` 與外層提前 continue 防止整數溢位。
- 偵測負環後，將 **任何會穿越負環節點** 的 `(i,j)` 標記為 `-∞` 並讓對應 `next` 失效。
- 公開 API 全數附 XML 註解、條件分支以 pattern matching 撰寫。

---

## 5. 路徑重建與 next 矩陣

`next[i,j]` 紀錄「從 i 走向 j 的下一個節點」。重建演算法：

```
path = [from]
while current != to:
    current = next[current, to]
    path.Add(current)
```

若 `next[i,j] == null` 則代表 `i` 無法（或不應）到達 `j`，回傳空集合。

---

## 6. 負環偵測原理

執行完三層迴圈後：

- 若存在某個 `i` 使 `dist[i,i] < 0`，代表存在以 i 為起點/終點且總權重為負的環。
- 對於任何能透過該節點 `k` 中轉的 `(i, j)`（即 `dist[i,k]` 與 `dist[k,j]` 皆有限），路徑距離可被無限縮短。
- 本專案會將這類 `(i, j)` 標記為 `NegativeInfinity`，並令 `next[i,j] = null`，以防止重建出錯。

---

## 7. 複雜度與適用情境

| 指標 | 值 |
| --- | --- |
| 時間複雜度 | O(n³) |
| 空間複雜度 | O(n²) |
| 邊負權 | ✅ 支援（無負環） |
| 偵測負環 | ✅ 支援 |
| 規模建議 | n ≲ 500（含 K=1000 為延伸目標） |

### 何時選 Floyd-Warshall？

- 圖**密集**且需要**所有點對**距離。
- 含負權邊但無負環。
- 圖規模不大（n ≤ 500）。

### 何時不選？

- 稀疏圖 + 單源 → Dijkstra（非負權）或 Bellman-Ford（含負權）。
- 含「最多 K 跳」之類限制 → BFS / Bellman-Ford 變形。
- n 很大（> 1000）→ 改用 Johnson’s algorithm（先 Bellman-Ford 重新加權，再對每點跑 Dijkstra）。

---

## 8. 優缺點

**優點**

- 程式碼極短、實作簡單。
- 同時得出所有點對距離。
- 支援負權邊與負環偵測。

**缺點**

- O(n³) 對大圖不可行。
- 不適合稀疏圖（Johnson's 通常更快）。
- 不適合「最多 K 跳」等帶限制的最短路徑問題。

---

## 9. 與 Dijkstra / Bellman-Ford / Johnson 比較

| 演算法 | 適用 | 負權邊 | 負環偵測 | 時間複雜度 |
| --- | --- | --- | --- | --- |
| Dijkstra（堆） | 單源 SSSP | ❌ | ❌ | O((V+E) log V) |
| Bellman-Ford | 單源 SSSP | ✅ | ✅ | O(V·E) |
| Floyd-Warshall | 所有點對 APSP | ✅ | ✅ | O(V³) |
| Johnson | 所有點對 APSP | ✅ | ✅（前處理） | O(V·E + V² log V) |

---

## 10. 應用場景

- 路由 / 路徑規劃（小型網路全表）。
- 通訊網路延遲計算。
- 多階段資源分配。
- 傳遞閉包（將權重視為布林）。

---

## 11. 專案使用方式

### 建置與執行

```bash
# 從 repo 根目錄
dotnet build
dotnet run --project Floyd-Warshall

# 跑測試
dotnet test
```

### 互動式選單示範

```
=== Floyd-Warshall Demo ===
1) 載入內建範例
2) 從 CSV 載入
3) 從 JSON 載入
4) 顯示目前圖
5) 執行 Floyd-Warshall
6) 顯示距離矩陣
7) 顯示 next 矩陣
8) 查詢最短路徑
9) 檢查負環
0) 離開
> 1
> 5
✔ 已完成計算（n=5, 用時 0.4 ms）

> 8
請輸入起點: 0
請輸入終點: 3
最短距離: 2
路徑: A → E → D
```

---

## 12. 檔案格式

### CSV

第一行為 metadata（`nodeCount,direction`），之後每行 `from,to,weight`。`#` 開頭為註解。

```csv
# nodeCount,direction
5,directed
# from,to,weight
0,1,3
0,2,8
1,3,1
```

### JSON

```json
{
  "nodeCount": 4,
  "direction": "undirected",
  "labels": ["A", "B", "C", "D"],
  "edges": [
    { "from": 0, "to": 1, "weight": 5 },
    { "from": 1, "to": 2, "weight": 2 }
  ]
}
```

`labels` 為選用，僅作顯示。`direction` 必須是 `directed` 或 `undirected`。

範例檔位於 `Floyd-Warshall/data/`。

---

## 13. LeetCode 練習題

| # | 題目 | 適用 Floyd-Warshall? | 說明 |
| --- | --- | --- | --- |
| 743 | Network Delay Time | ✅（n ≤ 100） | 取單源最遠值。可用 FW 解但 Dijkstra 更佳。 |
| 787 | Cheapest Flights Within K Stops | ❌ | 「最多 K 站」限制讓 FW 不再正確。建議 Bellman-Ford 變形或 BFS 分層。 |
| 1334 | Find the City With the Smallest Number of Neighbors at a Threshold Distance | ✅（直球題） | n ≤ 100，FW 求出所有點對距離後計算每節點門檻內鄰居數即可。 |
| 847 | Shortest Path Visiting All Nodes | ❌ | 含「拜訪所有節點」狀態，需 BFS + bitmask。 |

### 1334 解題思路

1. 用 Floyd-Warshall 求出 `dist[i,j]`。
2. 對每個節點 `i`，數 `count_i = #{ j : i != j && dist[i,j] <= threshold }`。
3. 取 `count_i` 最小者；同分時取編號最大者。

### 743 解題思路

跑 FW 後，從來源 k 取 `max(dist[k, j] for all j)`；若該值為 `Infinity` 則回傳 -1。

---

## 14. 參考資料

- Cormen, Leiserson, Rivest, Stein. *Introduction to Algorithms* (CLRS), Chapter 25.2.
- Wikipedia: <https://en.wikipedia.org/wiki/Floyd%E2%80%93Warshall_algorithm>
- LeetCode 題庫：743 / 787 / 1334 / 847
