namespace IntelOrca.Biohazard.BioRand.RECV;

public sealed class GraphData
{
    public string Start { get; init; } = "";
    public string End { get; init; } = "";
    public GraphDataKey[] Keys { get; init; } = [];
    public Dictionary<string, GraphDataItemType> ItemTypes { get; init; } = [];
    public GraphDataRoom[] Rooms { get; init; } = [];
}

public sealed class GraphDataKey
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Kind { get; init; } = "";
}

public sealed class GraphDataItemType
{
    public string Name { get; init; } = "";
    public string Kind { get; init; } = "";
}

public sealed class GraphDataRoom
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string[] Rdts { get; init; } = [];
    public GraphDataEdge[] Edges { get; init; } = [];
    public GraphDataItem[] Items { get; init; } = [];
}

public sealed class GraphDataEdge
{
    public string Target { get; init; } = "";
    public int Id { get; init; }
    public int EntranceId { get; init; }
    public string[] Requires { get; init; } = [];
    public string Kind { get; init; } = "";
    public string Condition { get; init; } = "";
    public string[] Tags { get; init; } = [];
    public string[] Offsets { get; init; } = [];
}

public sealed class GraphDataItem
{
    public int GlobalId { get; init; }
    public int Type { get; init; }
    public int Amount { get; init; }
    public string[] Requires { get; init; } = [];
    public string Condition { get; init; } = "";
    public string[] Tags { get; init; } = [];
    public string[] Offsets { get; init; } = [];
}
