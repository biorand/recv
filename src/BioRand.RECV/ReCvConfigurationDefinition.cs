using System.Reflection;
using System.Text.Json;

namespace IntelOrca.Biohazard.BioRand.RECV;

public static class ReCvConfigurationDefinition
{
    public static RandomizerConfigurationDefinition Create()
    {
        var definition = new RandomizerConfigurationDefinition();
        var general = definition.CreatePage("General");

        var doors = general.CreateGroup("Doors");
        doors.Items.Add(new RandomizerConfigurationDefinition.GroupItem
        {
            Id = "doors/skip",
            Label = "Door Skip",
            Description = "Skip door opening animations",
            Type = "switch",
            Default = true
        });
        doors.Items.Add(new RandomizerConfigurationDefinition.GroupItem
        {
            Id = "doors/random",
            Label = "Randomize Doors",
            Description = "Randomize door destinations",
            Type = "switch",
            Default = false
        });

        var itemsPage = definition.CreatePage("Items");

        var itemSettings = itemsPage.CreateGroup("Items");
        itemSettings.Items.Add(new RandomizerConfigurationDefinition.GroupItem
        {
            Id = "items/randomize-quantity",
            Label = "Randomize Item Quantities",
            Description = "Randomize the quantity of ammo and item pickups",
            Type = "switch",
            Default = true
        });
        itemSettings.Items.Add(new RandomizerConfigurationDefinition.GroupItem
        {
            Id = "items/quantity-multiplier",
            Label = "Ammo Quantity",
            Description = "Average quantity of ammunition per pickup",
            Type = "range",
            Default = 4,
            Min = 0,
            Max = 7,
            Step = 1
        });
        itemSettings.Items.Add(new RandomizerConfigurationDefinition.GroupItem
        {
            Id = "items/randomize-keys",
            Label = "Randomize Key Items",
            Description = "Randomize locations of progression key items",
            Type = "switch",
            Default = true
        });
        itemSettings.Items.Add(new RandomizerConfigurationDefinition.GroupItem
        {
            Id = "items/randomize-non-key-items",
            Label = "Randomize Other Items",
            Description = "Randomize non-key item pickups (ammo, healing, etc.)",
            Type = "switch",
            Default = true
        });
        itemSettings.Items.Add(new RandomizerConfigurationDefinition.GroupItem
        {
            Id = "items/randomize-starting-inventory",
            Label = "Random Starting Inventory",
            Description = "Give Claire a random weapon in the first inventory slot at game start",
            Type = "switch",
            Default = false
        });

        var graph = LoadGraph();
        if (graph != null)
        {
            var distribution = itemsPage.CreateGroup("Distribution");
            var ratioKinds = graph.ItemTypes
                .Select(x => x.Value.Kind)
                .Distinct()
                .Where(ReCvItemPool.IsLootKind)
                .OrderBy(k => k);

            foreach (var kind in ratioKinds)
            {
                var label = FormatKindLabel(graph, kind);
                distribution.Items.Add(new RandomizerConfigurationDefinition.GroupItem
                {
                    Id = $"items/ratio/{kind}",
                    Label = label,
                    Description = $"Relative frequency of {label.ToLowerInvariant()} pickups",
                    Type = "range",
                    Default = 0.5,
                    Min = 0,
                    Max = 1,
                    Step = 0.01
                });
            }
        }

        return definition;
    }

    private static GraphData? LoadGraph()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(
                "IntelOrca.Biohazard.BioRand.RECV.data.graph.json");
            if (stream == null)
                return null;

            return JsonSerializer.Deserialize<GraphData>(stream, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReadCommentHandling = JsonCommentHandling.Skip,
            });
        }
        catch
        {
            return null;
        }
    }

    private static string FormatKindLabel(GraphData graph, string kind)
    {
        var first = graph.ItemTypes.Values.FirstOrDefault(x => x.Kind == kind);
        if (first != null)
            return first.Name;
        return kind;
    }
}
