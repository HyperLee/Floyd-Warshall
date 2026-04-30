using Floyd_Warshall.Algorithms;
using Floyd_Warshall.Models;

namespace Floyd_Warshall.Tests;

public class PathReconstructorTests
{
    [Fact]
    public void Reconstruct_DirectlyConnectedNodes_ReturnsTwoNodePath()
    {
        var graph = new Graph(
            2,
            GraphDirection.Directed,
            [new Edge(0, 1, 5)]);

        var result = FloydWarshallSolver.Solve(graph);
        var path = PathReconstructor.Reconstruct(result, 0, 1);

        Assert.Equal([0, 1], path);
    }

    [Fact]
    public void Reconstruct_MultiHopPath_ReturnsFullSequence()
    {
        // 0 -> 1 -> 2 -> 3 為 0->3 唯一可達路徑。
        var graph = new Graph(
            4,
            GraphDirection.Directed,
            [
                new Edge(0, 1, 1),
                new Edge(1, 2, 1),
                new Edge(2, 3, 1),
            ]);

        var result = FloydWarshallSolver.Solve(graph);
        var path = PathReconstructor.Reconstruct(result, 0, 3);

        Assert.Equal([0, 1, 2, 3], path);
    }

    [Fact]
    public void Reconstruct_UnreachablePair_ReturnsEmpty()
    {
        var graph = new Graph(
            3,
            GraphDirection.Directed,
            [new Edge(0, 1, 1)]);

        var result = FloydWarshallSolver.Solve(graph);
        var path = PathReconstructor.Reconstruct(result, 2, 0);

        Assert.Empty(path);
    }

    [Fact]
    public void Reconstruct_SelfToSelf_ReturnsSingleNode()
    {
        var graph = new Graph(3, GraphDirection.Directed, [new Edge(0, 1, 4)]);
        var result = FloydWarshallSolver.Solve(graph);

        var path = PathReconstructor.Reconstruct(result, 1, 1);

        Assert.Equal([1], path);
    }
}
