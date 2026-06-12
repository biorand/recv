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

        var keyRandoEnabled = context.Config.GetValueOrDefault<bool>("items/randomize-keys", true);
        var nonKeyRandoEnabled = context.Config.GetValueOrDefault<bool>("items/randomize-non-key-items", true);

        Dictionary<int, ItemPlacement> keyPlacements;
        if (keyRandoEnabled)
        {
            logger.LogLine("Randomizing key item locations...");
            var rng = context.GetRng("key");
            var keyRandomizer = new ReCvKeyRandomizer();
            var result = keyRandomizer.Randomize(graph, rng);
            keyPlacements = new Dictionary<int, ItemPlacement>(result.Placements);
            logger.LogLine($"Key route found: {result.Route.AllNodesVisited}, {keyPlacements.Count} placements");

            if (!result.Route.AllNodesVisited)
            {
                logger.LogLine("WARNING: Route did not visit all nodes");
            }

            foreach (var (globalId, placement) in keyPlacements)
            {
                var keyName = graph.Keys.FirstOrDefault(k => k.Id == placement.Type)?.Name;
                if (keyName == null)
                    continue;

                var slotRoom = graph.Rooms.FirstOrDefault(r =>
                    r.Slots.Any(s => s.GlobalId == globalId));
                if (slotRoom == null)
                    continue;

                var roomLabel = slotRoom.Id;
                if (slotRoom.Rdts.Length > 0)
                    roomLabel += $", {string.Join(", ", slotRoom.Rdts)}";

                logger.LogLine($"Placing {keyName} at #{globalId} [{roomLabel}]");
            }
        }
        else
        {
            keyPlacements = [];
        }

        if (!keyRandoEnabled && !nonKeyRandoEnabled)
            return;

        logger.LogLine("Applying item modifications to rooms...");

        var itemRng = context.GetRng("item-apply");
        var nonKeyItems = BuildNonKeyItemPool(graph, context.Config);

        foreach (var room in graph.Rooms)
        {
            foreach (var rdtId in room.Rdts)
            {
                if (!context.RoomIndexById.TryGetValue(rdtId, out var roomIndex))
                    continue;

                var rdt = context.Rooms[roomIndex];
                var builder = rdt.ToBuilder();
                var modified = false;

                foreach (var slot in room.Slots)
                {
                    var itemIndex = slot.GlobalId & 0xFF;
                    if (itemIndex >= builder.Items.Count)
                        continue;

                    if (keyPlacements.TryGetValue(slot.GlobalId, out var placement))
                    {
                        var item = builder.Items[itemIndex];
                        item.Type = placement.Type;
                        builder.Items[itemIndex] = item;
                        modified = true;
                        keyPlacements.Remove(slot.GlobalId);
                    }
                    else if (nonKeyRandoEnabled && slot.Group == 0)
                    {
                        if (slot.Type == 0)
                            continue;

                        var newType = PickNonKeyItem(nonKeyItems, itemRng);
                        if (newType != null)
                        {
                            var item = builder.Items[itemIndex];
                            item.Type = newType.Value;
                            builder.Items[itemIndex] = item;
                            modified = true;
                        }
                    }
                }

                if (modified)
                    context.Rooms[roomIndex] = builder.ToRdt();
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

    private static List<GraphDataItemType> BuildNonKeyItemPool(
        GraphData graph, RandomizerConfiguration config)
    {
        var pool = new List<GraphDataItemType>();

        foreach (var kvp in graph.ItemTypes)
        {
            var kind = kvp.Value.Kind;
            if (kind.StartsWith("key/") || kind.StartsWith("weapon/"))
                continue;

            var weight = config.GetValueOrDefault($"items/ratio/{kind}", 5);
            if (weight <= 0)
                continue;

            for (var i = 0; i < weight; i++)
                pool.Add(kvp.Value);
        }

        return pool;
    }

    private static byte? PickNonKeyItem(List<GraphDataItemType> pool, Rng rng)
    {
        if (pool.Count == 0)
            return null;

        var index = rng.Next(0, pool.Count);
        var kind = pool[index].Kind;

        var itemId = FindItemIdByKind(kind);
        if (itemId == null)
        {
            var matching = pool.Where(x => x.Kind == kind).ToList();
            if (matching.Count == 0)
                return null;
            index = rng.Next(0, matching.Count);
            itemId = FindItemIdByKind(matching[index].Kind);
        }

        return itemId;
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
