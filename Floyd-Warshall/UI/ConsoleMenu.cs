using Floyd_Warshall.IO;

namespace Floyd_Warshall.UI;

/// <summary>
/// 互動式主選單；負責顯示選項、讀入指令並分派至 <see cref="MenuActions"/>。
/// </summary>
public sealed class ConsoleMenu
{
    private readonly MenuActions actions;
    private readonly TextReader input;
    private readonly TextWriter output;

    /// <summary>建立預設使用 <see cref="Console"/> 的選單。</summary>
    public ConsoleMenu() : this(new MenuActions(), Console.In, Console.Out)
    {
    }

    /// <summary>以可注入的 IO 與動作集建立選單，方便測試。</summary>
    public ConsoleMenu(MenuActions actions, TextReader input, TextWriter output)
    {
        this.actions = actions;
        this.input = input;
        this.output = output;
    }

    /// <summary>啟動主迴圈，直到使用者選擇離開。</summary>
    public void Run()
    {
        while (true)
        {
            PrintMenu();
            output.Write("> ");
            var line = input.ReadLine();

            if (line is null)
            {
                return;
            }

            try
            {
                if (!Dispatch(line.Trim()))
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                // 任何未捕捉的例外都轉為訊息，避免 UI 因為輸入錯誤而 crash。
                output.WriteLine($"✘ 發生錯誤：{ex.Message}");
            }

            output.WriteLine();
        }
    }

    private bool Dispatch(string command)
    {
        switch (command)
        {
            case "1":
                actions.LoadBuiltInSample();
                break;
            case "2":
                actions.LoadFromFile(new CsvGraphLoader(), "CSV");
                break;
            case "3":
                actions.LoadFromFile(new JsonGraphLoader(), "JSON");
                break;
            case "4":
                actions.ShowGraph();
                break;
            case "5":
                actions.Solve();
                break;
            case "6":
                actions.ShowDistance();
                break;
            case "7":
                actions.ShowNext();
                break;
            case "8":
                actions.QueryPath();
                break;
            case "9":
                actions.ShowNegativeCycle();
                break;
            case "0":
            case "q":
            case "Q":
                output.WriteLine("再見！");
                return false;
            default:
                output.WriteLine("✘ 未知指令，請重新輸入。");
                break;
        }

        return true;
    }

    private void PrintMenu()
    {
        output.WriteLine("=== Floyd-Warshall Demo ===");
        output.WriteLine("1) 載入內建範例");
        output.WriteLine("2) 從 CSV 載入");
        output.WriteLine("3) 從 JSON 載入");
        output.WriteLine("4) 顯示目前圖");
        output.WriteLine("5) 執行 Floyd-Warshall");
        output.WriteLine("6) 顯示距離矩陣");
        output.WriteLine("7) 顯示 next 矩陣");
        output.WriteLine("8) 查詢最短路徑");
        output.WriteLine("9) 檢查負環");
        output.WriteLine("0) 離開");
    }
}
