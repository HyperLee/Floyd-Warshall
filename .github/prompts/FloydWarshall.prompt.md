---
description: 'Floyd-Warshall 演算法專案開發規格書'
---

# Floyd-Warshall 演算法開發規格書

> 本文件為 Floyd-Warshall 演算法 .NET 10 / C# 14 專案的「開發規格書（Spec）」。
> 涵蓋專案目標、需求範圍、架構設計、模組契約、互動式 Console 介面、檔案格式、
> 測試規劃、文件規劃、驗收標準與里程碑，作為後續實作的單一事實來源。

---

## 1. 專案概述

### 1.1 目的
本專案以教學與實務並重的方式，深入介紹 Floyd-Warshall 演算法：
- 提供可重複使用的核心演算法函式庫（class library 風格的類別）。
- 提供互動式 Console 應用程式，讓使用者能載入圖、執行演算法、檢視結果。
- 以 README 詳細說明演算法原理、步驟、優缺點、應用場景與 LeetCode 練習題。
- 以 xUnit 撰寫單元測試，驗證演算法正確性與邊界條件。

### 1.2 演算法簡述
Floyd-Warshall 是以動態規劃求解 **所有點對最短路徑（All-Pairs Shortest Path, APSP）** 的經典演算法，能處理含負權重邊但不含負環的圖，時間複雜度 O(n³)、空間複雜度 O(n²)。

### 1.3 目標讀者
- 想學習圖論與動態規劃的學生與工程師。
- 想以實際 C# 範例理解 APSP 的開發者。
- 想在專案中重用 Floyd-Warshall 函式庫的 .NET 開發者。

---

## 2. 技術棧與環境

| 項目 | 規格 |
| --- | --- |
| Runtime / SDK | .NET 10 |
| 語言 | C# 14（最新版本特性） |
| Nullable | `enable` |
| Implicit Usings | `enable` |
| 主控台應用 | `Floyd-Warshall`（OutputType: Exe） |
| 測試框架 | xUnit + FluentAssertions（可選） |
| 解決方案 | `Floyd-Warshall.sln` |
| 編碼風格 | 遵循 `.editorconfig` 與 `.github/instructions/csharp.instructions.md` |

---

## 3. 解決方案結構

```
Floyd-Warshall.sln
├─ Floyd-Warshall/                       # 主控台應用程式（Exe）
│  ├─ Floyd-Warshall.csproj
│  ├─ Program.cs                         # 進入點，啟動互動式選單
│  ├─ Algorithms/
│  │  ├─ FloydWarshallSolver.cs          # 演算法核心
│  │  ├─ FloydWarshallResult.cs          # 結果聚合（距離矩陣、後繼矩陣、負環旗標）
│  │  └─ PathReconstructor.cs            # 透過 next/predecessor 矩陣還原路徑
│  ├─ Models/
│  │  ├─ Graph.cs                        # 圖資料結構（鄰接矩陣 + Metadata）
│  │  ├─ Edge.cs                         # 邊（from, to, weight）
│  │  └─ GraphDirection.cs               # enum: Directed / Undirected
│  ├─ IO/
│  │  ├─ IGraphLoader.cs                 # 載入器介面
│  │  ├─ CsvGraphLoader.cs               # CSV 載入
│  │  ├─ JsonGraphLoader.cs              # JSON 載入（System.Text.Json）
│  │  └─ GraphPrinter.cs                 # 表格化列印矩陣
│  ├─ Samples/
│  │  └─ BuiltInSamples.cs               # 內建範例（含 LeetCode 題目示範）
│  ├─ UI/
│  │  ├─ ConsoleMenu.cs                  # 互動式主選單
│  │  └─ MenuActions.cs                  # 各選項動作
│  └─ data/                              # 範例輸入檔（CSV / JSON）
│     ├─ sample-directed.csv
│     ├─ sample-undirected.json
│     └─ leetcode-1334.json
├─ Floyd-Warshall.Tests/                 # xUnit 測試專案（新增）
│  ├─ Floyd-Warshall.Tests.csproj
│  ├─ FloydWarshallSolverTests.cs
│  ├─ PathReconstructorTests.cs
│  ├─ NegativeCycleTests.cs
│  ├─ GraphLoaderTests.cs
│  └─ TestData/
│     ├─ valid-graph.csv
│     ├─ invalid-graph.csv
│     └─ negative-cycle.json
└─ README.md                             # 演算法解說與專案使用文件（繁體中文）
```

---

## 4. 功能需求（Functional Requirements）

### FR-1 基本最短路徑距離矩陣
- 接收 `Graph`，輸出 `n×n` 距離矩陣 `dist[i, j]`。
- 不可達使用 `int.MaxValue`（內部以「無窮大」常數封裝，避免相加溢位）。
- 自身距離 `dist[i, i] = 0`。

### FR-2 路徑重建（Path Reconstruction）
- 同時建立 `next[i, j]` 矩陣（記錄 i 走向 j 的下一個節點）。
- 提供 `IReadOnlyList<int> ReconstructPath(int from, int to)`：
  - 不可達回傳空集合。
  - 可達回傳完整節點序列（含起點與終點）。

### FR-3 負權重邊與負環偵測
- 允許負權邊。
- 演算法執行完畢後，檢查 `dist[i, i] < 0` 是否存在；若存在，標記 `HasNegativeCycle = true`，並提供受影響節點集合 `NegativeCycleNodes`。
- 偵測到負環時，距離矩陣對應行列以 `negative infinity` 標示（封裝常數），並禁止對受影響節點對重建路徑（拋 `InvalidOperationException` 或回傳空集合，擇一並於 API 註解明示）。

### FR-4 有向圖 / 無向圖切換
- `Graph` 攜帶 `GraphDirection` 屬性。
- 建構 `Graph` 時：
  - `Directed`：邊僅單向加入。
  - `Undirected`：自動補對稱邊；若兩方向皆已存在但權重不一致，拋例外。

### FR-5 從檔案載入圖資料
- 支援 CSV 與 JSON 兩種格式（介面 `IGraphLoader`）。
- **CSV 格式**（無標頭）：
  ```csv
  # nodeCount,direction
  5,directed
  # from,to,weight
  0,1,3
  0,2,8
  1,3,1
  ...
  ```
  - 第一行為 metadata：節點數、方向。
  - 之後每行為一條邊。
  - 以 `#` 起始的行視為註解，忽略。
- **JSON 格式**：
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
  - `labels` 為選用，僅作顯示用。
- 載入器需驗證：
  - 節點索引範圍。
  - 重複邊（依 `Directed/Undirected` 規則）。
  - 權重型別（int；後續可擴充 long/double，本期固定 int）。
- 驗證失敗丟出 `GraphLoadException`，內含行號與原因。

### FR-6 多組內建範例（含 LeetCode 示範）
`BuiltInSamples` 至少提供以下情境：
1. 教學用標準範例（4–5 節點、混合正權）。
2. 含負權邊但無負環的範例。
3. 含負環的範例（用於展示偵測）。
4. 無向圖範例。
5. LeetCode 題目對應示範：
   - 743. Network Delay Time
   - 787. Cheapest Flights Within K Stops（說明為何 Floyd-Warshall 不適合 K 限制版本，附對照）
   - 1334. Find the City With the Smallest Number of Neighbors at a Threshold Distance（最佳示範題）
   - 847. Shortest Path Visiting All Nodes（說明此題屬 BFS+bitmask，附對照）

### FR-7 互動式 Console 介面
主選單（`ConsoleMenu`）提供：
1. 載入內建範例（顯示清單供選擇）
2. 從 CSV 檔載入
3. 從 JSON 檔載入
4. 顯示目前圖（節點、邊、方向）
5. 執行 Floyd-Warshall
6. 顯示距離矩陣
7. 顯示 next 矩陣
8. 查詢任意兩點最短路徑（輸出距離與節點序列）
9. 顯示是否存在負環（與受影響節點）
0. 離開

要求：
- 鍵盤輸入錯誤需有友善訊息並回到主選單，不得 crash。
- 矩陣輸出需對齊欄位、`MaxValue` 顯示為 `∞`、負環顯示為 `-∞`。

---

## 5. 非功能需求（Non-Functional Requirements）

| 類別 | 要求 |
| --- | --- |
| 效能 | n ≤ 500 須能在合理時間內（< 5s）完成；n ≤ 1000 為延伸目標 |
| 健壯性 | 任何 IO / 解析錯誤均以例外封裝並在 UI 層轉為訊息 |
| 可測試性 | 核心演算法不依賴 Console；UI 不參與計算 |
| 國際化 | 訊息以繁體中文為主，但常數鍵名（如 enum）使用英文 |
| 文件 | 所有 public API 須有 XML 文件註解（含 `<summary>`、`<param>`、`<returns>`、必要時 `<example>`） |
| 安全性 | 檔案載入不接受相對於任意路徑的危險路徑；僅讀取，無寫入使用者檔案 |

---

## 6. 核心 API 契約

```csharp
namespace Floyd_Warshall.Models;

public enum GraphDirection { Directed, Undirected }

public sealed record Edge(int From, int To, int Weight);

public sealed class Graph
{
    public int NodeCount { get; }
    public GraphDirection Direction { get; }
    public IReadOnlyList<string> Labels { get; }
    public IReadOnlyList<Edge> Edges { get; }
    public int[,] ToAdjacencyMatrix(); // 不存在的邊填 InfinityConstant
}
```

```csharp
namespace Floyd_Warshall.Algorithms;

public sealed class FloydWarshallResult
{
    public int[,] Distance { get; init; }            // 距離矩陣
    public int?[,] Next { get; init; }               // next[i, j] 為 i 走向 j 的下一步；不可達為 null
    public bool HasNegativeCycle { get; init; }
    public IReadOnlySet<int> NegativeCycleNodes { get; init; }
}

public static class FloydWarshallSolver
{
    public static FloydWarshallResult Solve(Graph graph);
}

public static class PathReconstructor
{
    /// <summary>
    /// 依 next 矩陣重建從 from 到 to 的最短路徑。
    /// </summary>
    /// <returns>節點索引序列；若不可達或受負環影響則回傳空集合。</returns>
    public static IReadOnlyList<int> Reconstruct(FloydWarshallResult result, int from, int to);
}
```

```csharp
namespace Floyd_Warshall.IO;

public interface IGraphLoader
{
    Graph Load(string path);
}

public sealed class CsvGraphLoader  : IGraphLoader { /* ... */ }
public sealed class JsonGraphLoader : IGraphLoader { /* ... */ }

public sealed class GraphLoadException : Exception
{
    public int? LineNumber { get; }
    public GraphLoadException(string message, int? lineNumber = null, Exception? inner = null);
}
```

實作備註：
- 使用 `const int Infinity = int.MaxValue / 2;` 避免 `dist[i,k] + dist[k,j]` 溢位。
- Solve 內部先建立 `dist` 與 `next`，然後三層迴圈，更新時同步更新 `next[i, j] = next[i, k]`。
- 全部 public 方法均加入 XML 註解；條件分支以 pattern matching 撰寫。

---

## 7. 演算法虛擬碼

```
function FloydWarshall(G):
    n = G.NodeCount
    dist = adjacency matrix of G (missing edges = +INF)
    next = matrix of int? (initial: next[i,j] = j if dist[i,j] < INF else null; next[i,i] = i)

    for k in 0..n-1:
        for i in 0..n-1:
            for j in 0..n-1:
                if dist[i,k] + dist[k,j] < dist[i,j]:
                    dist[i,j] = dist[i,k] + dist[k,j]
                    next[i,j] = next[i,k]

    negativeCycleNodes = { i | dist[i,i] < 0 }
    return { dist, next, hasNegativeCycle = negativeCycleNodes.Any(), negativeCycleNodes }
```

---

## 8. 測試規劃（xUnit）

新增專案 `Floyd-Warshall.Tests`，目標覆蓋：

### 8.1 `FloydWarshallSolverTests`
- 標準 4 節點圖距離矩陣正確。
- 不連通節點對距離為 Infinity。
- 含負權邊但無負環時，距離矩陣正確。
- 單節點圖、空邊圖。
- 無向圖對稱性驗證。

### 8.2 `PathReconstructorTests`
- 直接相連的兩點路徑長度為 2。
- 多跳路徑序列正確。
- 不可達回傳空集合。
- 自身到自身回傳 `[i]`。

### 8.3 `NegativeCycleTests`
- 單純負環圖：`HasNegativeCycle = true`，`NegativeCycleNodes` 涵蓋環上節點。
- 部分節點受負環影響、部分不受影響時的分割正確性。
- 受影響節點對 `Reconstruct` 不丟例外但回傳空集合（或依 API 抉擇拋例外）。

### 8.4 `GraphLoaderTests`
- CSV 正常載入（含註解行）。
- CSV 缺欄、多欄、節點索引越界 → `GraphLoadException` 並含行號。
- JSON 正常載入（含 labels）。
- JSON schema 不符 → `GraphLoadException`。
- 無向圖讀入兩個方向不同權重 → 例外。

> 測試命名沿用既有 C# 專案常用風格（`MethodName_Scenario_ExpectedResult`），不得加上 `// Arrange / Act / Assert` 註解。

---

## 9. 文件規劃（README.md）

README.md（繁體中文）章節：
1. 專案簡介
2. 演算法原理與動態規劃推導（含遞推式）
3. 演算法步驟（流程圖或條列）
4. C# 實作範例（最簡版 + 本專案完整版差異說明）
5. 路徑重建與 next 矩陣解說
6. 負環偵測原理
7. 時間 / 空間複雜度與適用情境
8. 優點與缺點
9. 與 Dijkstra / Bellman-Ford / Johnson 的比較表
10. 應用場景（路徑規劃、網路延遲、資源分配）
11. 專案使用方式（建置、執行、互動式選單操作示範）
12. 檔案格式說明（CSV / JSON 範例）
13. LeetCode 練習題清單與解題思路
    - 743 / 787 / 1334 / 847（每題附是否適用 Floyd-Warshall 之分析）
14. 參考資料

---

## 10. 互動式 Console 範例輸出

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
> 5
✔ 已完成計算（n=4, 用時 0.8 ms）

> 8
請輸入起點: 0
請輸入終點: 3
最短距離: 7
路徑: 0 → 2 → 1 → 3
```

---

## 11. 驗收標準（Definition of Done）

- [ ] 解決方案可在乾淨環境以 `dotnet build` 與 `dotnet test` 全綠通過。
- [ ] 主控台應用程式可從互動選單完成「載入 → 計算 → 查詢路徑」完整流程。
- [ ] CSV、JSON 範例檔位於 `data/`，且能成功載入。
- [ ] 所有 public API 具備 XML 註解。
- [ ] xUnit 測試覆蓋第 8 章列出的所有案例。
- [ ] README.md 章節齊全並含 LeetCode 題目分析。
- [ ] 程式遵循 `.editorconfig` 與 C# 撰寫規範（file-scoped namespace、`is null`、pattern matching 等）。

---

## 12. 開發里程碑（不含時程）

1. **M1 基礎建設**：建立 `Algorithms`/`Models`/`IO`/`UI`/`Samples` 資料夾骨架；新增 xUnit 測試專案並掛入 sln。
2. **M2 核心演算法**：實作 `Graph`、`FloydWarshallSolver`、`FloydWarshallResult`、`PathReconstructor`；完成對應單元測試。
3. **M3 IO 模組**：實作 `CsvGraphLoader`、`JsonGraphLoader`、`GraphLoadException`、`GraphPrinter`；完成載入器測試與範例檔。
4. **M4 互動 UI**：實作 `ConsoleMenu` 與 `MenuActions`，整合至 `Program.cs`。
5. **M5 內建範例**：完成 `BuiltInSamples`，包含教學範例與 LeetCode 對照題。
6. **M6 文件**：完成 README.md（依第 9 章章節）。
7. **M7 收尾**：跑全測試、整理 XML 註解、最終 code review。

---

## 13. 風險與假設

- **整數溢位**：以 `Infinity = int.MaxValue / 2` 緩解；超大權重不在本期支援範圍。
- **大型圖**：n > 1000 之效能不在本期承諾，但結構不阻擋未來改用 `Span<T>` 或平行化。
- **檔案編碼**：CSV 假設為 UTF-8；其他編碼不支援。
- **單執行緒**：本期不提供平行版本；架構保留可擴充空間。

---

## 14. 後續擴充（Out of Scope，僅備忘）

- 平行化 Floyd-Warshall（k 為外層、ij 平行）。
- 支援 `double` 權重與容差比較。
- 視覺化（Spectre.Console 或產出 GraphViz）。
- 與 Dijkstra / Bellman-Ford 的效能 Benchmark（BenchmarkDotNet）。

---

## 15. LeetCode 練習題（含適用性分析）

| # | 題目 | 適用 Floyd-Warshall? | 備註 |
| --- | --- | --- | --- |
| 743 | Network Delay Time | 適用（n ≤ 100） | 取單一來源最遠值；Dijkstra 更佳，但本題可用 FW 示範 |
| 787 | Cheapest Flights Within K Stops | 不直接適用 | 有「最多 K 站」限制，建議 BFS / Bellman-Ford 變形 |
| 1334 | Find the City With the Smallest Number of Neighbors at a Threshold Distance | 非常適用 | FW 直球示範題 |
| 847 | Shortest Path Visiting All Nodes | 不適用 | 屬 BFS + bitmask |

> README 將以同樣分析逐題說明，避免讀者誤用。
