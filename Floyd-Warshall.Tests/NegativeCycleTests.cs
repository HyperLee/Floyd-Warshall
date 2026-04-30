using Floyd_Warshall.Algorithms;
using Floyd_Warshall.Models;

namespace Floyd_Warshall.Tests;

public class NegativeCycleTests
{
    [Fact]
    public void Solve_SimpleNegativeCycle_DetectsAffectedNodes()
    {
        // 0 -> 1 (1), 1 -> 2 (-2), 2 -> 0 (-1) 形成總和 -2 的環
        var graph = new Graph(
            3,
            GraphDirection.Directed,
            [
                new Edge(0, 1, 1),
                new Edge(1, 2, -2),
                new Edge(2, 0, -1),
            ]);

        var result = FloydWarshallSolver.Solve(graph);

        Assert.True(result.HasNegativeCycle);
        Assert.Equal(new HashSet<int> { 0, 1, 2 }, new HashSet<int>(result.NegativeCycleNodes));
    }

    [Fact]
    public void Solve_NegativeCyclePartiallyAffectsGraph_PartitionsCorrectly()
    {
        // 環在 {0,1,2}；節點 3 為孤立節點，不受影響。
        var graph = new Graph(
            4,
            GraphDirection.Directed,
            [
                new Edge(0, 1, 1),
                new Edge(1, 2, -2),
                new Edge(2, 0, -1),
            ]);

        var result = FloydWarshallSolver.Solve(graph);

        Assert.True(result.HasNegativeCycle);
        Assert.DoesNotContain(3, result.NegativeCycleNodes);
        Assert.Equal(0, result.Distance[3, 3]);
        Assert.Equal(GraphConstants.Infinity, result.Distance[3, 0]);
    }

    [Fact]
    public void Reconstruct_AffectedPair_ReturnsEmpty()
    {
        var graph = new Graph(
            3,
            GraphDirection.Directed,
            [
                new Edge(0, 1, 1),
                new Edge(1, 2, -2),
                new Edge(2, 0, -1),
            ]);

        var result = FloydWarshallSolver.Solve(graph);
        var path = PathReconstructor.Reconstruct(result, 0, 2);

        Assert.Empty(path);
    }
}
