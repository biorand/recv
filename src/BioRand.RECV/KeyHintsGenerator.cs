using System.Net;
using System.Text;

namespace IntelOrca.Biohazard.BioRand.RECV;

internal sealed record KeyHintRow(
    int Depth,
    string Item,
    string RoomId,
    string RoomName,
    int GlobalId,
    int LocalId);

internal static class KeyHintsGenerator
{
    /// <summary>
    /// Builds one row per key placement, ordered by the depth of its room
    /// from the start room (BFS over the room graph).
    /// </summary>
    public static List<KeyHintRow> BuildRows(
        GraphData graph,
        IReadOnlyDictionary<int, ItemPlacement> placements)
    {
        var itemsByGlobalId = new Dictionary<int, (GraphDataRoom Room, GraphDataItem Item)>();
        foreach (var room in graph.Rooms)
        {
            foreach (var item in room.Items)
                itemsByGlobalId[item.GlobalId] = (room, item);
        }

        var keyNames = graph.Keys.ToDictionary(k => k.Id, k => k.Name);
        var depths = ComputeRoomDepths(graph);

        var rows = new List<KeyHintRow>();
        foreach (var (globalId, placement) in placements)
        {
            if (!keyNames.TryGetValue(placement.Type, out var itemName))
                continue;
            if (!itemsByGlobalId.TryGetValue(globalId, out var slot))
                continue;

            var depth = depths.TryGetValue(slot.Room.Id, out var d) ? d : int.MaxValue;
            rows.Add(new KeyHintRow(
                depth,
                itemName,
                slot.Room.Id,
                slot.Room.Name ?? "",
                globalId,
                slot.Item.Id));
        }

        return rows
            .OrderBy(r => r.Depth)
            .ThenBy(r => r.RoomId, StringComparer.Ordinal)
            .ThenBy(r => r.LocalId)
            .ToList();
    }

    public static string RenderHtml(IReadOnlyList<KeyHintRow> rows, int seed)
    {
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html>\n<html lang=\"en\">\n<head>\n<meta charset=\"utf-8\">\n");
        sb.Append("<title>BioRand RECV - Key Locations</title>\n<style>\n");
        sb.Append("body { font-family: Arial, Helvetica, sans-serif; margin: 2rem; background: #1a1a2e; color: #eee; }\n");
        sb.Append("h1 { font-size: 1.4rem; }\n");
        sb.Append("table { border-collapse: collapse; width: 100%; }\n");
        sb.Append("th, td { border: 1px solid #555; padding: 0.4rem 0.6rem; text-align: left; }\n");
        sb.Append("th { background: #16213e; }\n");
        sb.Append("tr:nth-child(even) { background: #22223e; }\n");
        sb.Append("</style>\n</head>\n<body>\n");
        sb.Append($"<h1>BioRand RECV &mdash; Key Locations (Seed {seed})</h1>\n");
        sb.Append("<table>\n<thead><tr>");
        sb.Append("<th>Depth</th><th>Item</th><th>Room Id</th><th>Room Name</th>");
        sb.Append("<th>Global Item Id</th><th>Local Item Id</th>");
        sb.Append("</tr></thead>\n<tbody>\n");

        foreach (var row in rows)
        {
            sb.Append("<tr>");
            sb.Append($"<td>{(row.Depth == int.MaxValue ? "&mdash;" : row.Depth.ToString())}</td>");
            sb.Append($"<td>{WebUtility.HtmlEncode(row.Item)}</td>");
            sb.Append($"<td>{WebUtility.HtmlEncode(row.RoomId)}</td>");
            sb.Append($"<td>{WebUtility.HtmlEncode(row.RoomName)}</td>");
            sb.Append($"<td>{row.GlobalId}</td>");
            sb.Append($"<td>{row.LocalId}</td>");
            sb.Append("</tr>\n");
        }

        sb.Append("</tbody>\n</table>\n</body>\n</html>\n");
        return sb.ToString();
    }

    private static Dictionary<string, int> ComputeRoomDepths(GraphData graph)
    {
        // Build undirected adjacency: a room is reachable if it connects to the
        // start room by any path, regardless of edge direction. This handles
        // one-way / no-return edges where the room is entered from elsewhere.
        var roomsById = graph.Rooms.ToDictionary(r => r.Id);
        var adjacency = new Dictionary<string, List<string>>();
        foreach (var room in graph.Rooms)
        {
            if (!adjacency.TryGetValue(room.Id, out var list))
                adjacency[room.Id] = list = [];
            foreach (var edge in room.Edges)
            {
                if (!roomsById.ContainsKey(edge.Target))
                    continue;
                if (!adjacency.TryGetValue(edge.Target, out var targetList))
                    adjacency[edge.Target] = targetList = [];
                list.Add(edge.Target);
                targetList.Add(room.Id);
            }
        }

        var depths = new Dictionary<string, int>();
        var visited = new HashSet<string>();
        var queue = new Queue<(string Id, int Depth)>();

        if (graph.Start != null && roomsById.ContainsKey(graph.Start))
        {
            visited.Add(graph.Start);
            queue.Enqueue((graph.Start, 0));
        }

        while (queue.Count > 0)
        {
            var (id, depth) = queue.Dequeue();
            depths[id] = depth;

            if (!adjacency.TryGetValue(id, out var neighbors))
                continue;

            foreach (var neighbor in neighbors)
            {
                if (visited.Contains(neighbor))
                    continue;
                visited.Add(neighbor);
                queue.Enqueue((neighbor, depth + 1));
            }
        }

        return depths;
    }
}
