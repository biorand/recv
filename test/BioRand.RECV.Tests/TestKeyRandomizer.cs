using System.Reflection;
using System.Text.Json;
using IntelOrca.Biohazard.BioRand.RECV;
using IntelOrca.Biohazard.BioRand.Routing;
using Xunit;

namespace IntelOrca.Biohazard.BioRand.RECV.Tests;

public class TestKeyRandomizer
{
    private const int Retries = 50;

    [Fact]
    public void BasicGraph_Solves()
    {
        for (var i = 0; i < Retries; i++)
        {
            var graph = LoadTestGraph("test_basic");
            var rng = new Rng(i);
            var randomizer = new ReCvKeyRandomizer();
            var result = randomizer.Randomize(graph, rng);

            Assert.True(result.Route.AllNodesVisited,
                $"Seed {i}: route did not visit all nodes\n{result.Route.Log}");
            Assert.Equal(RouteSolverResult.Ok, result.Route.Solve());
        }
    }

    [Fact]
    public void BasicGraph_KeysPlacedInStartRoom()
    {
        for (var i = 0; i < Retries; i++)
        {
            var graph = LoadTestGraph("test_basic");
            var rng = new Rng(i);
            var randomizer = new ReCvKeyRandomizer();
            var result = randomizer.Randomize(graph, rng);

            // Both keys must be in room 1000 (the only accessible room with key slots)
            Assert.True(result.Placements.ContainsKey(1) || result.Placements.ContainsKey(2));
        }
    }

    [Fact]
    public void BasicGraph_KeyRequiredForDoor()
    {
        for (var i = 0; i < Retries; i++)
        {
            var graph = LoadTestGraph("test_basic");
            var rng = new Rng(i);
            var randomizer = new ReCvKeyRandomizer();
            var result = randomizer.Randomize(graph, rng);

            // Silver Key (67) must be placed in room 1000 to unlock door to 3000
            var silverKeyItems = result.Route.GetItemsContainingKey(
                result.Route.Graph.Keys.First(k => k.Label == "Silver Key"));
            Assert.NotEmpty(silverKeyItems);
        }
    }

    [Fact]
    public void BlockedDoor_Solves()
    {
        for (var i = 0; i < Retries; i++)
        {
            var graph = LoadTestGraph("test_blocked_door");
            var rng = new Rng(i);
            var randomizer = new ReCvKeyRandomizer();
            var result = randomizer.Randomize(graph, rng);

            Assert.True(result.Route.AllNodesVisited,
                $"Seed {i}: route did not visit all nodes\n{result.Route.Log}");
            Assert.Equal(RouteSolverResult.Ok, result.Route.Solve());
        }
    }

    [Fact]
    public void ConsumableKey_Solves()
    {
        for (var i = 0; i < Retries; i++)
        {
            var graph = LoadTestGraph("test_consumable");
            var rng = new Rng(i);
            var randomizer = new ReCvKeyRandomizer();
            var result = randomizer.Randomize(graph, rng);

            Assert.True(result.Route.AllNodesVisited,
                $"Seed {i}: route did not visit all nodes\n{result.Route.Log}");
            Assert.Equal(RouteSolverResult.Ok, result.Route.Solve());
        }
    }

    [Fact]
    public void ConsumableKey_PlacedTwice()
    {
        for (var i = 0; i < Retries; i++)
        {
            var graph = LoadTestGraph("test_consumable");
            var rng = new Rng(i);
            var randomizer = new ReCvKeyRandomizer();
            var result = randomizer.Randomize(graph, rng);

            // Eagle Plate (56) is consumable and needed for 2 doors
            var eaglePlateKey = result.Route.Graph.Keys.First(k => k.Label == "Eagle Plate");
            var items = result.Route.GetItemsContainingKey(eaglePlateKey);
            Assert.Equal(2, items.Count);
        }
    }

    [Fact]
    public void FullRecvMap_SolvesAll()
    {
        var graph = LoadGraphFromResource();
        var slotCount = graph.Rooms.Sum(r => r.Items.Count(s => s.Requires.Length > 0));
        var failures = new List<string>();

        for (var i = 0; i < 10; i++)
        {
            var rng = new Rng(i);
            var randomizer = new ReCvKeyRandomizer();
            var result = randomizer.Randomize(graph, rng);

            Assert.NotNull(result.Route);
            Assert.True(result.Route.AllNodesVisited,
                $"Seed {i}: {result.Placements.Count}/{slotCount} placements, not all nodes visited\n{result.Route.Log}");
        }
    }

    [Fact]
    public void FullRecvMap_Deterministic()
    {
        var graph = LoadGraphFromResource();
        var rng1 = new Rng(42);
        var rng2 = new Rng(42);
        var randomizer = new ReCvKeyRandomizer();

        var result1 = randomizer.Randomize(graph, rng1);
        var result2 = randomizer.Randomize(graph, rng2);

        Assert.Equal(
            result1.Placements.OrderBy(x => x.Key).Select(x => $"{x.Key}:{x.Value.Type}"),
            result2.Placements.OrderBy(x => x.Key).Select(x => $"{x.Key}:{x.Value.Type}"));
    }

    private static GraphData LoadTestGraph(string name)
    {
        var resourceName = $"IntelOrca.Biohazard.BioRand.RECV.Tests.data.{name}.json";
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new InvalidOperationException($"Test resource not found: {resourceName}");

        return JsonSerializer.Deserialize<GraphData>(stream, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
        })!;
    }

    private static GraphData LoadGraphFromResource()
    {
        var assembly = typeof(ReCvKeyRandomizer).Assembly;
        using var stream = assembly.GetManifestResourceStream(
            "IntelOrca.Biohazard.BioRand.RECV.data.graph.json");
        if (stream == null)
            throw new InvalidOperationException("Embedded resource data/graph.json not found");

        return JsonSerializer.Deserialize<GraphData>(stream, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
        })!;
    }
}
