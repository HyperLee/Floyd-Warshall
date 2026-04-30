using Floyd_Warshall.Models;

namespace Floyd_Warshall.IO;

/// <summary>
/// 圖資料載入器的共通介面。
/// </summary>
public interface IGraphLoader
{
    /// <summary>由指定路徑載入並驗證圖。</summary>
    /// <param name="path">檔案路徑。</param>
    /// <returns>解析完成的 <see cref="Graph"/>。</returns>
    /// <exception cref="GraphLoadException">檔案不存在或內容不合法。</exception>
    Graph Load(string path);
}
