using System.Reflection;
using System.Text.Json;
using IntelOrca.Biohazard;
using IntelOrca.Biohazard.Room;

namespace IntelOrca.Biohazard.BioRand.RECV.Modifiers;

[Order(ModifierOrders.Item)]
public sealed class ItemModifier : ICvModifier
{
    public void Apply(ReCvRandomizerContext context, RandomizerLogger logger)
    {
        var graph = LoadGraph();

        var placements = KeyRouting(graph, context, logger);
        placements = LootFilling(graph, context, placements, logger);
        RdtEditing(graph, context, placements, logger);
    }

    private static Dictionary<int, ItemPlacement> KeyRouting(
        GraphData graph, ReCvRandomizerContext context, RandomizerLogger logger)
    {
        var keyRandoEnabled = context.Config.GetValueOrDefault<bool>("items/randomize-keys", true);
        if (!keyRandoEnabled)
            return [];

        logger.LogLine("Randomizing key item locations...");
        var rng = context.GetRng("key");
        var keyRandomizer = new ReCvKeyRandomizer();
        var result = keyRandomizer.Randomize(graph, rng);
        var placements = new Dictionary<int, ItemPlacement>(result.Placements);
        context.MermaidGraph = result.Route.Graph.ToMermaid(useLabels: true, includeItems: false);

        logger.LogLine($"Key route found: {result.Route.AllNodesVisited}, {placements.Count} placements");

        if (!result.Route.AllNodesVisited)
            logger.LogLine("WARNING: Route did not visit all nodes");

        foreach (var (globalId, placement) in placements)
        {
            var keyName = graph.Keys.FirstOrDefault(k => k.Id == placement.Type)?.Name;
            if (keyName == null)
                continue;

            var slotRoom = graph.Rooms.FirstOrDefault(r =>
                r.Items.Any(s => s.GlobalId == globalId));
            if (slotRoom == null)
                continue;

            var roomLabel = slotRoom.Id;
            if (slotRoom.Rdts.Length > 0)
                roomLabel += $", {string.Join(", ", slotRoom.Rdts)}";

            logger.LogLine($"Placing {keyName} at #{globalId} [{roomLabel}] {slotRoom.Name ?? ""}");
        }

        return placements;
    }

    private static Dictionary<int, ItemPlacement> LootFilling(
        GraphData graph, ReCvRandomizerContext context,
        Dictionary<int, ItemPlacement> placements, RandomizerLogger logger)
    {
        var nonKeyRandoEnabled = context.Config.GetValueOrDefault<bool>("items/randomize-non-key-items", true);
        if (!nonKeyRandoEnabled)
            return placements;

        var rng = context.GetRng("item-apply");
        var pool = BuildNonKeyItemPool(graph, context.Config);

        var totalWeight = pool.Sum(x => x.Weight);
        logger.LogLine($"Distribution weights: {string.Join(", ", pool.Select(x => $"{x.Type.Kind}={x.Weight:F2}"))}");
        logger.LogLine($"Total weight sum: {totalWeight:F2}");

        var placedKinds = new List<string>();

        foreach (var room in graph.Rooms)
        {
            foreach (var item in room.Items)
            {
                if (placements.ContainsKey(item.GlobalId))
                    continue;
                if (item.Requires.Length > 0)
                    continue;
                if (item.Type == 0)
                    continue;

                var result = PickNonKeyItem(pool, rng);
                if (result != null)
                {
                    var (newType, kind) = result.Value;
                    placements[item.GlobalId] = new ItemPlacement(
                        item.GlobalId, newType, (ushort)item.Amount);
                    placedKinds.Add(kind);
                }
            }
        }

        if (placedKinds.Count > 0)
        {
            var counts = placedKinds
                .GroupBy(k => k)
                .OrderBy(g => g.Key)
                .Select(g => $"{g.Key}: {g.Count()}")
                .ToList();
            logger.LogLine($"Non-key item distribution: {string.Join(", ", counts)}");
            logger.LogLine($"Total non-key items placed: {placedKinds.Count}");
        }

        return placements;
    }

    private static void RdtEditing(
        GraphData graph, ReCvRandomizerContext context,
        Dictionary<int, ItemPlacement> placements, RandomizerLogger logger)
    {
        logger.LogLine("Applying item modifications to rooms...");

        foreach (var room in graph.Rooms)
        {
            foreach (var rdtId in room.Rdts)
            {
                if (!context.RoomIndexById.TryGetValue(rdtId, out var roomIndex))
                    continue;

                var builder = context.Rooms[roomIndex].ToBuilder();
                var hasItemTableChanges = false;

                foreach (var item in room.Items)
                {
                    if (!placements.TryGetValue(item.GlobalId, out var placement))
                        continue;

                    if (item.Offsets.Length == 0 && item.Id < builder.Aots.Count)
                    {
                        var stage = builder.Aots[item.Id].Stage;
                        if (stage < builder.Items.Count)
                        {
                            var rdtItem = builder.Items[stage];
                            rdtItem.Type = placement.Type;
                            builder.Items[stage] = rdtItem;
                            hasItemTableChanges = true;
                        }
                    }
                }

                if (!hasItemTableChanges)
                    continue;

                var rdtBytes = builder.ToRdt().Data.ToArray();

                foreach (var item in room.Items)
                {
                    if (!placements.TryGetValue(item.GlobalId, out var placement))
                        continue;
                    if (item.Offsets.Length == 0)
                        continue;

                    foreach (var offsetStr in item.Offsets)
                    {
                        if (!int.TryParse(offsetStr, out var off))
                            continue;
                        if (off < 0 || off >= rdtBytes.Length)
                            continue;

                        var opcode = rdtBytes[off];
                        var typeOff = opcode switch
                        {
                            0x08 => off + 2,
                            0x06 when off + 2 < rdtBytes.Length
                                && rdtBytes[off + 1] == 8
                                && rdtBytes[off + 2] == 0 => off + 3,
                            0x7C => off + 4,
                            0xC7 => off + 2,
                            _ => -1,
                        };
                        if (typeOff >= 0 && typeOff < rdtBytes.Length)
                            rdtBytes[typeOff] = placement.Type;
                    }
                }

                context.Rooms[roomIndex] = new RdtCv(rdtBytes);
            }
        }

        logger.LogLine("Item modifications applied");
    }

    private static GraphData LoadGraph()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(
            "IntelOrca.Biohazard.BioRand.RECV.data.graph.json");
        if (stream == null)
            throw new InvalidOperationException("Embedded resource data/graph.json not found");

        return JsonSerializer.Deserialize<GraphData>(stream, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
        }) ?? throw new InvalidOperationException("Failed to deserialize graph.json");
    }

    private static List<(GraphDataItemType Type, double Weight)> BuildNonKeyItemPool(
        GraphData graph, RandomizerConfiguration config)
    {
        var pool = new List<(GraphDataItemType, double)>();

        foreach (var kvp in graph.ItemTypes)
        {
            var kind = kvp.Value.Kind;
            if (kind.StartsWith("key/") || kind.StartsWith("weapon/"))
                continue;

            var weight = config.GetValueOrDefault<double>($"items/ratio/{kind}", 0.5);
            if (weight <= 0)
                continue;

            pool.Add((kvp.Value, weight));
        }

        return pool;
    }

    private static (byte ItemId, string Kind)? PickNonKeyItem(
        List<(GraphDataItemType Type, double Weight)> pool, Rng rng)
    {
        if (pool.Count == 0)
            return null;

        var totalWeight = pool.Sum(x => x.Weight);
        var roll = rng.NextDouble() * totalWeight;
        var cumulative = 0.0;
        foreach (var (type, weight) in pool)
        {
            cumulative += weight;
            if (roll <= cumulative)
            {
                var itemId = FindItemIdByKind(type.Kind);
                if (itemId != null)
                    return (itemId.Value, type.Kind);
                var matching = pool.Where(x => x.Type.Kind == type.Kind).ToList();
                if (matching.Count == 0)
                    return null;
                var index = rng.Next(0, matching.Count);
                itemId = FindItemIdByKind(matching[index].Type.Kind);
                if (itemId != null)
                    return (itemId.Value, type.Kind);
                return null;
            }
        }

        return null;
    }

    private static byte? FindItemIdByKind(string kind)
    {
        return kind switch
        {
            "heal" => ReCvItemIds.FAidSpray,
            "ink-ribbon" => ReCvItemIds.InkRibbon,
            "ammo/handgun" => ReCvItemIds.HandgunBullets,
            "ammo/shotgun" => ReCvItemIds.ShotgunShells,
            "ammo/magnum" => ReCvItemIds.MagnumBullets,
            "ammo/grenade" => ReCvItemIds.GrenadeRounds,
            "ammo/bow-gun" => ReCvItemIds.BowGunArrows,
            "ammo/sub-machine-gun" => ReCvItemIds.MGunBullets,
            "ammo/sniper-rifle" => ReCvItemIds.RifleBullets,
            "ammo/assault-rifle" => ReCvItemIds.ARifleBullets,
            "gunpowder" => ReCvItemIds.BowGunPowder,
            "special" => ReCvItemIds.SidePack,
            "document" => ReCvItemIds.File,
            "quest" => ReCvItemIds.FamilyPicture,
            _ => null,
        };
    }
}
