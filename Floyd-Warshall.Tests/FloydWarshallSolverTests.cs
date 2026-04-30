using Floyd_Warshall.Algorithms;
using Floyd_Warshall.Models;

namespace Floyd_Warshall.Tests;

public class FloydWarshallSolverTests
{
    [Fact]
    public void Solve_StandardFourNodeGraph_ReturnsCorrectDistances()
    {
        var graph = new Graph(
            4,
            GraphDirection.Directed,
            [
                new Edge(0, 1, 5),
                new Edge(0, 3, 10),
                new Edge(1, 2, 3),
                new Edge(2, 3, 1),
            ]);

        var result = FloydWarshallSolver.Solve(graph);

        Assert.Equal(0, result.Distance[0, 0]);
        Assert.Equal(5, result.Distance[0, 1]);
        Assert.Equal(8, result.Distance[0, 2]);
        Assert.Equal(9, result.Distance[0, 3]);
        Assert.False(result.HasNegativeCycle);
    }

    [Fact]
    public void Solve_DisconnectedPair_ReturnsInfinity()
    {
        var graph = new Graph(
            3,
            GraphDirection.Directed,
            [
                new Edge(0, 1, 1),
            ]);

        var result = FloydWarshallSolver.Solve(graph);

        Assert.Equal(GraphConstants.Infinity, result.Distance[0, 2]);
        Assert.Equal(GraphConstants.Infinity, result.Distance[2, 0]);
        Assert.Null(result.Next[0, 2]);
    }

    [Fact]
    public void Solve_NegativeEdgeWithoutCycle_ReturnsCorrectDistances()
    {
        // 0 -> 1 (4), 0 -> 2 (5), 1 -> 2 (-3) => 0->2 應為 1
        var graph = new Graph(
            3,
            GraphDirection.Directed,
            [
                new Edge(0, 1, 4),
                new Edge(0, 2, 5),
                new Edge(1, 2, -3),
            ]);

        var result = FloydWarshallSolver.Solve(graph);

        Assert.Equal(1, result.Distance[0, 2]);
        Assert.False(result.HasNegativeCycle);
    }

    [Fact]
    public void Solve_SingleNodeGraph_DiagonalIsZero()
    {
        var graph = new Graph(1, GraphDirection.Directed, []);
        var result = FloydWarshallSolver.Solve(graph);

        Assert.Equal(0, result.Distance[0, 0]);
        Assert.False(result.HasNegativeCycle);
    }

    [Fact]
    public void Solve_EmptyEdgesGraph_DiagonalZeroOthersInfinity()
    {
        var graph = new Graph(3, GraphDirection.Directed, []);
        var result = FloydWarshallSolver.Solve(graph);

        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                var expected = i == j ? 0 : GraphConstants.Infinity;
                Assert.Equal(expected, result.Distance[i, j]);
            }
        }
    }

    [Fact]
    public void Solve_UndirectedGraph_DistanceMatrixIsSymmetric()
    {
        var graph = new Graph(
            4,
            GraphDirection.Undirected,
            [
                new Edge(0, 1, 2),
                new Edge(1, 2, 3),
                new Edge(2, 3, 4),
                new Edge(0, 3, 100),
            ]);

        var result = FloydWarshallSolver.Solve(graph);

        for (var i = 0; i < 4; i++)
        {
            for (var j = 0; j < 4; j++)
            {
                Assert.Equal(result.Distance[i, j], result.Distance[j, i]);
            }
        }

        Assert.Equal(9, result.Distance[0, 3]);
    }
}
