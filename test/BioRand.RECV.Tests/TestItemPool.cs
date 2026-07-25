using System.Reflection;
using System.Text.Json;
using IntelOrca.Biohazard.BioRand.RECV;
using Xunit;

namespace IntelOrca.Biohazard.BioRand.RECV.Tests;

public class TestItemPool
{
    [Fact]
    public void Constructor_LoadsAllNonKeyNonWeaponKinds()
    {
        var pool = CreatePool("test_itempool");
        var pools = pool.GetPools();

        // Should have pools for heal, gunpowder, special, document, ammo/handgun, ink-ribbon
        Assert.Contains("heal", pools.Keys);
        Assert.Contains("gunpowder", pools.Keys);
        Assert.Contains("special", pools.Keys);
        Assert.Contains("document", pools.Keys);
        Assert.Contains("ammo/handgun", pools.Keys);
        Assert.Contains("ink-ribbon", pools.Keys);
    }

    [Fact]
    public void Constructor_SkipsKeyItems()
    {
        var pool = CreatePool("test_itempool");

        // Gas Mask has kind "key/reusuable" — should not be in pools
        Assert.DoesNotContain(pool.GetPools().Keys, k => k.StartsWith("key/"));
    }

    [Fact]
    public void Constructor_SkipsWeapons()
    {
        var pool = CreatePool("test_itempool");

        // Rocket Launcher has kind "weapon/explosive" — should not be in pools
        Assert.DoesNotContain(pool.GetPools().Keys, k => k.StartsWith("weapon/"));
    }

    [Fact]
    public void Pick_HealKind_ReturnsValidItemId()
    {
        var pool = CreatePool("test_itempool");

        for (var i = 0; i < 50; i++)
        {
            var rng = new Rng(i);
            var itemId = pool.Pick("heal", rng);
            Assert.NotNull(itemId);
            Assert.InRange(itemId.Value, 20, 22); // FAidSpray(20), GreenHerb(21), RedHerb(22)
        }
    }

    [Fact]
    public void Pick_HealKind_ReturnsVariety()
    {
        var pool = CreatePool("test_itempool");

        var seenItems = new HashSet<byte>();
        for (var i = 0; i < 200; i++)
        {
            var rng = new Rng(i);
            var itemId = pool.Pick("heal", rng);
            Assert.NotNull(itemId);
            seenItems.Add(itemId.Value);
        }

        // Should see at least 2 different heal items across 200 seeds
        Assert.True(seenItems.Count >= 2,
            $"Expected at least 2 different heal items, got {seenItems.Count}");
    }

    [Fact]
    public void Pick_SingleItemKind_ReturnsSameItemAlways()
    {
        var pool = CreatePool("test_itempool");

        // ink-ribbon pool has exactly 1 item
        for (var i = 0; i < 50; i++)
        {
            var rng = new Rng(i);
            var itemId = pool.Pick("ink-ribbon", rng);
            Assert.NotNull(itemId);
            Assert.Equal((byte)31, itemId!.Value); // Ink Ribbon
        }
    }

    [Fact]
    public void Pick_UnknownKind_ReturnsNull()
    {
        var pool = CreatePool("test_itempool");
        var rng = new Rng(42);
        Assert.Null(pool.Pick("nonexistent", rng));
    }

    [Fact]
    public void Pick_NullKind_ReturnsNull()
    {
        var pool = CreatePool("test_itempool");
        var rng = new Rng(42);
        Assert.Null(pool.Pick(null!, rng));
    }

    [Fact]
    public void Pick_EmptyKind_ReturnsNull()
    {
        var pool = CreatePool("test_itempool");
        var rng = new Rng(42);
        Assert.Null(pool.Pick("", rng));
    }

    [Fact]
    public void Pick_Deterministic_SameSeedReturnsSameItem()
    {
        var pool = CreatePool("test_itempool");

        var rng1 = new Rng(42);
        var rng2 = new Rng(42);

        var result1 = pool.Pick("heal", rng1);
        var result2 = pool.Pick("heal", rng2);

        Assert.Equal(result1, result2);
    }

    [Fact]
    public void Pick_DifferentSeedMayReturnDifferentItem()
    {
        var pool = CreatePool("test_itempool");

        // Multi-item kind: different seeds should sometimes give different items
        var results = new List<byte?>();
        for (var i = 0; i < 20; i++)
        {
            var rng = new Rng(i * 100);
            results.Add(pool.Pick("heal", rng));
        }

        // At least 2 different items from 20 seeds
        var distinct = results
            .Where(r => r.HasValue)
            .Select(r => r!.Value)
            .Distinct()
            .Count();
        Assert.True(distinct >= 2,
            $"Expected at least 2 different items from 20 seeds, got {distinct}");
    }

    [Fact]
    public void Pick_GunpowderKind_ReturnsValidItems()
    {
        var pool = CreatePool("test_itempool");

        for (var i = 0; i < 50; i++)
        {
            var rng = new Rng(i);
            var itemId = pool.Pick("gunpowder", rng);
            Assert.NotNull(itemId);
            Assert.Contains(itemId.Value, new byte[] { 35, 36, 144 });
        }
    }

    [Fact]
    public void Pick_SpecialKind_ReturnsValidItems()
    {
        var pool = CreatePool("test_itempool");

        for (var i = 0; i < 50; i++)
        {
            var rng = new Rng(i);
            var itemId = pool.Pick("special", rng);
            Assert.NotNull(itemId);
            Assert.Contains(itemId.Value, new byte[] { 57, 58, 74, 125 });
        }
    }

    [Fact]
    public void Pick_EmptyGraph_ReturnsNull()
    {
        var pool = new ReCvItemPool(new GraphData());
        var rng = new Rng(42);
        Assert.Null(pool.Pick("heal", rng));
    }

    private static ReCvItemPool CreatePool(string name)
    {
        var graph = LoadTestGraph(name);
        return new ReCvItemPool(graph);
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
}
