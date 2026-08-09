using System.Collections.Immutable;

namespace IntelOrca.Biohazard.BioRand.RECV;

/// <summary>
/// Data-driven item pools built from graph.json.
/// Groups items by kind and provides random selection within each pool.
/// </summary>
public sealed class ReCvItemPool
{
    private readonly Dictionary<string, ImmutableArray<byte>> _pools;

    /// <summary>
    /// Build pools from graph data, grouping items by kind.
    /// Key items (key/*), weapons (weapon/*) and documents are excluded.
    /// </summary>
    public ReCvItemPool(GraphData graph)
    {
        var pools = new Dictionary<string, List<byte>>();

        foreach (var kvp in graph.ItemTypes)
        {
            var kind = kvp.Value.Kind;

            if (!IsLootKind(kind))
                continue;

            if (!pools.TryGetValue(kind, out var list))
                pools[kind] = list = [];

            if (byte.TryParse(kvp.Key, out var itemId))
                list.Add(itemId);
        }

        _pools = pools.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToImmutableArray());
    }

    /// <summary>
    /// Returns true if the kind can be placed as random loot.
    /// Key items and weapons are handled by the key randomizer and are excluded,
    /// and documents are never placed as loot.
    /// </summary>
    internal static bool IsLootKind(string kind) =>
        IsNonKeyNonWeaponKind(kind) && kind != "document";

    /// <summary>
    /// Returns true if the kind is neither a key item nor a weapon.
    /// </summary>
    internal static bool IsNonKeyNonWeaponKind(string kind) =>
        !kind.StartsWith("key/") && !kind.StartsWith("weapon/");

    /// <summary>
    /// Pick a random item ID from the pool for the given kind.
    /// Returns null if the kind is unknown or has no items.
    /// </summary>
    public byte? Pick(string kind, Rng rng)
    {
        if (string.IsNullOrEmpty(kind))
            return null;

        if (!_pools.TryGetValue(kind, out var items) || items.Length == 0)
            return null;

        // Single-item pool: deterministic, no RNG needed
        if (items.Length == 1)
            return items[0];

        return items[rng.Next(0, items.Length)];
    }

    /// <summary>
    /// Returns a read-only view of the pools for testing.
    /// </summary>
    internal IReadOnlyDictionary<string, ImmutableArray<byte>> GetPools() => _pools;
}
