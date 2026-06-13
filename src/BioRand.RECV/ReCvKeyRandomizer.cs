using System.Text.RegularExpressions;
using IntelOrca.Biohazard.BioRand.Routing;

namespace IntelOrca.Biohazard.BioRand.RECV;

internal sealed class ReCvKeyRandomizer
{
    public ReCvKeyRandomizerResult Randomize(GraphData graphData, Rng rng)
    {
        var seed = rng.Next(0, int.MaxValue);
        var builder = new GraphBuilder();

        var roomNodes = new Dictionary<string, Node>();
        var flagNodes = new Dictionary<string, Node>();
        var keyToItemId = new Dictionary<Key, int>();
        var itemIdToKey = new Dictionary<int, Key>();
        var itemNodeToGlobalId = new Dictionary<Node, int>();

        foreach (var k in graphData.Keys)
        {
            var keyKind = (KeyKind)Enum.Parse(typeof(KeyKind), k.Kind, true);
            var key = builder.Key(k.Name, 1, keyKind);
            keyToItemId[key] = k.Id;
            itemIdToKey[k.Id] = key;
        }

        var startNode = builder.Room("START");
        foreach (var r in graphData.Rooms)
        {
            roomNodes[r.Id] = builder.Room(r.Name != null ? $"{r.Id}|{r.Name}" : r.Id);
        }
        builder.Door(startNode, roomNodes[graphData.Start]);

        foreach (var r in graphData.Rooms)
        {
            if (!roomNodes.TryGetValue(r.Id, out var source))
                continue;

            var seenTargets = new HashSet<string>();
            foreach (var e in r.Edges)
            {
                if (!roomNodes.TryGetValue(e.Target, out var target))
                    continue;

                if (!seenTargets.Add(e.Target))
                    continue;

                var requirements = ParseRequirements(
                    e.Requires, itemIdToKey, roomNodes, flagNodes, builder);

                var edgeKind = e.Kind switch
                {
                    "unblock" => EdgeKind.UnlockTwoWay,
                    "oneWay" => EdgeKind.OneWay,
                    "noReturn" => EdgeKind.NoReturn,
                    _ => EdgeKind.TwoWay,
                };

                switch (edgeKind)
                {
                    case EdgeKind.UnlockTwoWay:
                        builder.BlockedDoor(source, target, requirements);
                        break;
                    case EdgeKind.OneWay:
                        builder.OneWay(source, target, requirements);
                        break;
                    case EdgeKind.NoReturn:
                        builder.NoReturn(source, target, requirements);
                        break;
                    default:
                        builder.Door(source, target, requirements);
                        break;
                }
            }
        }

        foreach (var r in graphData.Rooms)
        {
            if (!roomNodes.TryGetValue(r.Id, out var roomNode))
                continue;

            foreach (var s in r.Items)
            {
                var requirements = ParseRequirements(
                    s.Requires, itemIdToKey, roomNodes, flagNodes, builder);
                var label = $"{r.Id}|item({s.GlobalId})";
                var itemNode = builder.Item(label, 1, roomNode, requirements);
                itemNodeToGlobalId[itemNode] = s.GlobalId;
            }
        }

        var routingGraph = builder.ToGraph();
        var route = routingGraph.GenerateRoute(seed);

        var placements = new Dictionary<int, ItemPlacement>();
        foreach (var kvp in itemNodeToGlobalId)
        {
            var itemNode = kvp.Key;
            var globalId = kvp.Value;
            var key = route.GetItemContents(itemNode);

            if (key == null)
            {
                var slotData = FindItem(graphData, globalId);
                if (slotData != null)
                {
                    placements[globalId] = new ItemPlacement(
                        globalId, (byte)slotData.Type, (ushort)slotData.Amount);
                }
                continue;
            }

            var gameItemId = keyToItemId[key.Value];

            placements[globalId] = new ItemPlacement(
                globalId, (byte)gameItemId, (ushort)1);
        }

        return new ReCvKeyRandomizerResult
        {
            Placements = placements,
            Route = route,
        };
    }

    private static GraphDataItem? FindItem(GraphData graphData, int globalId)
    {
        foreach (var r in graphData.Rooms)
        {
            foreach (var s in r.Items)
            {
                if (s.GlobalId == globalId)
                    return s;
            }
        }
        return null;
    }

    private static Requirement[] ParseRequirements(
        string[] requires,
        Dictionary<int, Key> itemIdToKey,
        Dictionary<string, Node> roomNodes,
        Dictionary<string, Node> flagNodes,
        GraphBuilder builder)
    {
        if (requires.Length == 0)
            return [];

        var result = new List<Requirement>();
        foreach (var r in requires)
        {
            var match = Regex.Match(r, @"^([a-z]+)\(([A-Za-z0-9_]+)\)$");
            if (!match.Success)
                continue;

            var kind = match.Groups[1].Value;
            var value = match.Groups[2].Value;

            switch (kind)
            {
                case "item":
                    if (int.TryParse(value, out var itemId) && itemIdToKey.TryGetValue(itemId, out var key))
                        result.Add(new Requirement(key));
                    break;
                case "flag":
                    if (!flagNodes.TryGetValue(value, out var flagNode))
                    {
                        flagNode = builder.Room($"FLAG:{value}");
                        flagNodes[value] = flagNode;
                    }
                    result.Add(new Requirement(flagNode));
                    break;
                case "room":
                case "node":
                    if (roomNodes.TryGetValue(value, out var roomNode))
                        result.Add(new Requirement(roomNode));
                    break;
            }
        }
        return result.ToArray();
    }
}

public sealed class ReCvKeyRandomizerResult
{
    public IReadOnlyDictionary<int, ItemPlacement> Placements { get; init; } = new Dictionary<int, ItemPlacement>();
    public Route Route { get; init; } = null!;
}

public sealed class ItemPlacement
{
    public int GlobalId { get; }
    public byte Type { get; }
    public ushort Amount { get; }

    public ItemPlacement(int globalId, byte type, ushort amount)
    {
        GlobalId = globalId;
        Type = type;
        Amount = amount;
    }
}
