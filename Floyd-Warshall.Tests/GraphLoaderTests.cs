using Floyd_Warshall.IO;
using Floyd_Warshall.Models;

namespace Floyd_Warshall.Tests;

public class GraphLoaderTests : IDisposable
{
    private readonly string tempDir;

    public GraphLoaderTests()
    {
        tempDir = Path.Combine(Path.GetTempPath(), "fw-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private string WriteTemp(string name, string content)
    {
        var path = Path.Combine(tempDir, name);
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void CsvLoader_ValidFileWithComments_LoadsGraph()
    {
        var csv = """
                  # header
                  3,directed
                  # edges
                  0,1,4
                  1,2,5
                  """;

        var path = WriteTemp("ok.csv", csv);
        var graph = new CsvGraphLoader().Load(path);

        Assert.Equal(3, graph.NodeCount);
        Assert.Equal(GraphDirection.Directed, graph.Direction);
        Assert.Equal(2, graph.Edges.Count);
    }

    [Fact]
    public void CsvLoader_MissingColumn_ThrowsWithLineNumber()
    {
        var csv = """
                  3,directed
                  0,1
                  """;

        var path = WriteTemp("bad.csv", csv);
        var loader = new CsvGraphLoader();

        var ex = Assert.Throws<GraphLoadException>(() => loader.Load(path));
        Assert.Equal(2, ex.LineNumber);
    }

    [Fact]
    public void CsvLoader_NodeIndexOutOfRange_ThrowsWithLineNumber()
    {
        var csv = """
                  2,directed
                  0,5,1
                  """;

        var path = WriteTemp("oob.csv", csv);
        var ex = Assert.Throws<GraphLoadException>(() => new CsvGraphLoader().Load(path));

        Assert.Equal(2, ex.LineNumber);
    }

    [Fact]
    public void CsvLoader_TooManyColumns_Throws()
    {
        var csv = """
                  2,directed
                  0,1,3,5
                  """;

        var path = WriteTemp("too-many.csv", csv);
        Assert.Throws<GraphLoadException>(() => new CsvGraphLoader().Load(path));
    }

    [Fact]
    public void JsonLoader_ValidFileWithLabels_LoadsGraph()
    {
        var json = """
                   {
                     "nodeCount": 3,
                     "direction": "undirected",
                     "labels": ["X","Y","Z"],
                     "edges": [
                       { "from": 0, "to": 1, "weight": 2 },
                       { "from": 1, "to": 2, "weight": 4 }
                     ]
                   }
                   """;

        var path = WriteTemp("ok.json", json);
        var graph = new JsonGraphLoader().Load(path);

        Assert.Equal(3, graph.NodeCount);
        Assert.Equal(GraphDirection.Undirected, graph.Direction);
        Assert.Equal("Y", graph.Labels[1]);
        // Undirected graph 應補上對稱邊：原 2 條 → 4 條。
        Assert.Equal(4, graph.Edges.Count);
    }

    [Fact]
    public void JsonLoader_MalformedSchema_Throws()
    {
        var json = """{ "nodeCount": -1, "direction": "directed", "edges": [] }""";
        var path = WriteTemp("schema.json", json);

        Assert.Throws<GraphLoadException>(() => new JsonGraphLoader().Load(path));
    }

    [Fact]
    public void JsonLoader_InvalidDirection_Throws()
    {
        var json = """{ "nodeCount": 2, "direction": "weird", "edges": [] }""";
        var path = WriteTemp("dir.json", json);

        Assert.Throws<GraphLoadException>(() => new JsonGraphLoader().Load(path));
    }

    [Fact]
    public void UndirectedLoader_ConflictingWeights_Throws()
    {
        // 在無向圖中明示 0->1 與 1->0 兩條邊但權重不同 → 例外。
        var json = """
                   {
                     "nodeCount": 2,
                     "direction": "undirected",
                     "edges": [
                       { "from": 0, "to": 1, "weight": 3 },
                       { "from": 1, "to": 0, "weight": 7 }
                     ]
                   }
                   """;

        var path = WriteTemp("conflict.json", json);
        Assert.Throws<GraphLoadException>(() => new JsonGraphLoader().Load(path));
    }
}
