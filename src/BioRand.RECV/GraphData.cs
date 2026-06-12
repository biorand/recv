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
    public int Group { get; init; }
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
    public GraphDataSlot[] Slots { get; init; } = [];
}

public sealed class GraphDataEdge
{
    public string To { get; init; } = "";
    public string[] Requires { get; init; } = [];
    public string Kind { get; init; } = "";
}

public sealed class GraphDataSlot
{
    public int GlobalId { get; init; }
    public int Group { get; init; }
    public int Type { get; init; }
    public int Amount { get; init; }
    public string[] Requires { get; init; } = [];
    public string Priority { get; init; } = "";
}
