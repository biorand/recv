using System.Reflection;
using System.Text.Json;
using IntelOrca.Biohazard.BioRand.RECV;
using IntelOrca.Biohazard.BioRand.Routing;
using Xunit;

namespace IntelOrca.Biohazard.BioRand.RECV.Tests;

public class TestKeyHints
{
    private const int Retries = 50;

    [Fact]
    public void BasicGraph_DepthMatchesRoomBfs()
    {
        for (var i = 0; i < Retries; i++)
        {
            var graph = LoadTestGraph("test_basic");
            var rng = new Rng(i);
            var randomizer = new ReCvKeyRandomizer();
            var result = randomizer.Randomize(graph, rng);

            var rows = KeyHintsGenerator.BuildRows(graph, result.Placements);

            Assert.Equal(2, rows.Count);
            // BFS depths: 1000=0, 2000=1, 3000=2, 4000=3
            foreach (var row in rows)
            {
                var expected = row.RoomId switch
                {
                    "1000" => 0,
                    "2000" => 1,
                    "3000" => 2,
                    _ => 3,
                };
                Assert.Equal(expected, row.Depth);
            }
        }
    }

    [Fact]
    public void BasicGraph_RowsOrderedByDepth()
    {
        for (var i = 0; i < Retries; i++)
        {
            var graph = LoadTestGraph("test_basic");
            var rng = new Rng(i);
            var randomizer = new ReCvKeyRandomizer();
            var result = randomizer.Randomize(graph, rng);

            var rows = KeyHintsGenerator.BuildRows(graph, result.Placements);

            for (var j = 1; j < rows.Count; j++)
                Assert.True(rows[j - 1].Depth <= rows[j].Depth,
                    $"Seed {i}: rows not ordered by depth: {rows[j - 1]} before {rows[j]}");
        }
    }

    [Fact]
    public void BasicGraph_RowsMapToKeys()
    {
        var graph = LoadTestGraph("test_basic");
        var rng = new Rng(0);
        var randomizer = new ReCvKeyRandomizer();
        var result = randomizer.Randomize(graph, rng);

        var rows = KeyHintsGenerator.BuildRows(graph, result.Placements);

        var items = rows.Select(r => r.Item).OrderBy(x => x).ToList();
        Assert.Equal(["Gold Key", "Silver Key"], items);
        Assert.All(rows, r => Assert.True(r.GlobalId is 1 or 2));
        Assert.All(rows, r => Assert.Equal(0, r.LocalId));
    }

    [Fact]
    public void HintHtml_RendersTable()
    {
        var graph = LoadTestGraph("test_basic");
        var rng = new Rng(0);
        var randomizer = new ReCvKeyRandomizer();
        var result = randomizer.Randomize(graph, rng);
        var rows = KeyHintsGenerator.BuildRows(graph, result.Placements);

        var html = KeyHintsGenerator.RenderHtml(rows, 1234);

        Assert.Contains("<table", html);
        Assert.Contains("Silver Key", html);
        Assert.Contains("Gold Key", html);
        Assert.Contains("Key Locations (Seed 1234)", html);
        Assert.Contains("Global Item Id", html);
    }

    [Fact]
    public void HintHtml_Deterministic()
    {
        var graph = LoadTestGraph("test_basic");
        var rng = new Rng(42);
        var randomizer = new ReCvKeyRandomizer();

        var result1 = randomizer.Randomize(graph, new Rng(42));
        var result2 = randomizer.Randomize(graph, new Rng(42));

        var html1 = KeyHintsGenerator.RenderHtml(
            KeyHintsGenerator.BuildRows(graph, result1.Placements), 42);
        var html2 = KeyHintsGenerator.RenderHtml(
            KeyHintsGenerator.BuildRows(graph, result2.Placements), 42);

        Assert.Equal(html1, html2);
    }

    [Fact]
    public void HintHtml_UnreachableDepthRendersDash()
    {
        var rows = new List<KeyHintRow>
        {
            new(int.MaxValue, "Silver Key", "9999", "NOWHERE", 1, 2),
        };

        var html = KeyHintsGenerator.RenderHtml(rows, 1);

        Assert.Contains("<td>&mdash;</td>", html);
    }

    [Fact]
    public void FullRecvMap_AllKeysListed()
    {
        var graph = LoadGraphFromResource();
        var rng = new Rng(0);
        var randomizer = new ReCvKeyRandomizer();
        var result = randomizer.Randomize(graph, rng);

        var rows = KeyHintsGenerator.BuildRows(graph, result.Placements);

        // Every router placement yields exactly one row. Consumable keys may
        // be placed in multiple slots, so row count can exceed key count.
        Assert.Equal(result.Placements.Count, rows.Count);
        var placedTypes = result.Placements.Values.Select(p => (int)p.Type).Distinct();
        Assert.Equal(placedTypes.Count(), rows.Select(r => r.Item).Distinct().Count());
        Assert.All(rows, r => Assert.True(r.Depth >= 0));
        for (var j = 1; j < rows.Count; j++)
            Assert.True(rows[j - 1].Depth <= rows[j].Depth,
                $"Rows not ordered by depth: {rows[j - 1]} before {rows[j]}");

        // Any key in the start room must be at depth 0.
        Assert.All(rows.Where(r => r.RoomId == graph.Start), r => Assert.Equal(0, r.Depth));
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
