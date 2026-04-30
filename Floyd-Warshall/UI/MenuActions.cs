using System.Diagnostics;
using Floyd_Warshall.Algorithms;
using Floyd_Warshall.IO;
using Floyd_Warshall.Models;
using Floyd_Warshall.Samples;

namespace Floyd_Warshall.UI;

/// <summary>
/// 互動式選單背後的實作，將計算與 IO 操作以方法形式封裝以利測試。
/// </summary>
public sealed class MenuActions
{
    private readonly TextReader input;
    private readonly TextWriter output;

    /// <summary>目前載入的圖；尚未載入則為 null。</summary>
    public Graph? CurrentGraph { get; private set; }

    /// <summary>目前最近一次計算結果；尚未計算則為 null。</summary>
    public FloydWarshallResult? CurrentResult { get; private set; }

    /// <summary>
    /// 建立 <see cref="MenuActions"/>。預設使用 <see cref="Console.In"/>/<see cref="Console.Out"/>。
    /// </summary>
    public MenuActions(TextReader? input = null, TextWriter? output = null)
    {
        this.input = input ?? Console.In;
        this.output = output ?? Console.Out;
    }

    /// <summary>載入內建範例。</summary>
    public void LoadBuiltInSample()
    {
        var samples = BuiltInSamples.All;

        output.WriteLine("可用的內建範例：");

        for (var i = 0; i < samples.Count; i++)
        {
            output.WriteLine($"  {i + 1}) {samples[i].Name} - {samples[i].Description}");
        }

        output.Write("請選擇編號: ");
        var line = input.ReadLine();

        if (!int.TryParse(line, out var idx) || idx < 1 || idx > samples.Count)
        {
            output.WriteLine("✘ 編號無效。");
            return;
        }

        try
        {
            CurrentGraph = samples[idx - 1].Build();
            CurrentResult = null;
            output.WriteLine($"✔ 已載入：{samples[idx - 1].Name}（n={CurrentGraph.NodeCount}）");
        }
        catch (Exception ex)
        {
            output.WriteLine($"✘ 載入失敗：{ex.Message}");
        }
    }

    /// <summary>由指定路徑與載入器載入圖。</summary>
    public void LoadFromFile(IGraphLoader loader, string formatName)
    {
        output.Write($"請輸入 {formatName} 檔案路徑: ");
        var path = input.ReadLine();

        if (string.IsNullOrWhiteSpace(path))
        {
            output.WriteLine("✘ 路徑不得為空。");
            return;
        }

        try
        {
            CurrentGraph = loader.Load(path.Trim());
            CurrentResult = null;
            output.WriteLine($"✔ 載入成功（n={CurrentGraph.NodeCount}, 方向={CurrentGraph.Direction}）");
        }
        catch (GraphLoadException ex)
        {
            output.WriteLine($"✘ 載入失敗：{ex.Message}");
        }
    }

    /// <summary>顯示目前圖的節點與邊。</summary>
    public void ShowGraph()
    {
        if (CurrentGraph is null)
        {
            output.WriteLine("尚未載入任何圖。");
            return;
        }

        output.WriteLine($"節點數：{CurrentGraph.NodeCount}, 方向：{CurrentGraph.Direction}");
        output.WriteLine("標籤：" + string.Join(", ", CurrentGraph.Labels));
        output.WriteLine("邊：");

        foreach (var e in CurrentGraph.Edges)
        {
            output.WriteLine($"  {e.From} -> {e.To} (w={e.Weight})");
        }
    }

    /// <summary>執行 Floyd-Warshall。</summary>
    public void Solve()
    {
        if (CurrentGraph is null)
        {
            output.WriteLine("尚未載入任何圖。");
            return;
        }

        var sw = Stopwatch.StartNew();
        CurrentResult = FloydWarshallSolver.Solve(CurrentGraph);
        sw.Stop();

        output.WriteLine(
            $"✔ 已完成計算（n={CurrentGraph.NodeCount}, 用時 {sw.Elapsed.TotalMilliseconds:F2} ms）");

        if (CurrentResult.HasNegativeCycle)
        {
            output.WriteLine($"⚠ 偵測到負環，受影響節點：{string.Join(", ", CurrentResult.NegativeCycleNodes)}");
        }
    }

    /// <summary>顯示距離矩陣。</summary>
    public void ShowDistance()
    {
        if (CurrentResult is null)
        {
            output.WriteLine("尚未執行計算。");
            return;
        }

        output.WriteLine(GraphPrinter.FormatDistance(CurrentResult.Distance, CurrentGraph?.Labels));
    }

    /// <summary>顯示 next 矩陣。</summary>
    public void ShowNext()
    {
        if (CurrentResult is null)
        {
            output.WriteLine("尚未執行計算。");
            return;
        }

        output.WriteLine(GraphPrinter.FormatNext(CurrentResult.Next, CurrentGraph?.Labels));
    }

    /// <summary>互動式查詢任意兩點最短路徑。</summary>
    public void QueryPath()
    {
        if (CurrentResult is null || CurrentGraph is null)
        {
            output.WriteLine("尚未執行計算。");
            return;
        }

        var n = CurrentGraph.NodeCount;

        if (!TryReadIndex("請輸入起點: ", n, out var from)
            || !TryReadIndex("請輸入終點: ", n, out var to))
        {
            return;
        }

        var distance = CurrentResult.Distance[from, to];
        var path = PathReconstructor.Reconstruct(CurrentResult, from, to);

        if (path.Count == 0)
        {
            if (CurrentResult.HasNegativeCycle
                && (CurrentResult.NegativeCycleNodes.Contains(from)
                    || CurrentResult.NegativeCycleNodes.Contains(to)))
            {
                output.WriteLine("路徑受負環影響，無法定義最短路徑。");
            }
            else
            {
                output.WriteLine("不可達。");
            }

            return;
        }

        output.WriteLine($"最短距離: {distance}");
        var labels = CurrentGraph.Labels;
        output.WriteLine("路徑: " + string.Join(" → ", path.Select(i => labels[i])));
    }

    /// <summary>顯示是否存在負環。</summary>
    public void ShowNegativeCycle()
    {
        if (CurrentResult is null)
        {
            output.WriteLine("尚未執行計算。");
            return;
        }

        if (!CurrentResult.HasNegativeCycle)
        {
            output.WriteLine("未偵測到負環。");
            return;
        }

        output.WriteLine("⚠ 偵測到負環。");
        output.WriteLine("受影響節點：" + string.Join(", ", CurrentResult.NegativeCycleNodes));
    }

    private bool TryReadIndex(string prompt, int nodeCount, out int value)
    {
        output.Write(prompt);
        var line = input.ReadLine();

        if (!int.TryParse(line, out value) || value < 0 || value >= nodeCount)
        {
            output.WriteLine($"✘ 索引需介於 0 ~ {nodeCount - 1}。");
            value = -1;
            return false;
        }

        return true;
    }
}
