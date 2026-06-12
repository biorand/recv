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

        var items = general.CreateGroup("Items");
        items.Items.Add(new RandomizerConfigurationDefinition.GroupItem
        {
            Id = "items/randomize-quantity",
            Label = "Randomize Item Quantities",
            Description = "Randomize the quantity of ammo and item pickups",
            Type = "switch",
            Default = true
        });
        items.Items.Add(new RandomizerConfigurationDefinition.GroupItem
        {
            Id = "items/quantity-multiplier",
            Label = "Ammo Quantity",
            Description = "Average quantity of ammunition per pickup",
            Type = "slider",
            Default = 4,
            Min = 0,
            Max = 7,
            Step = 1
        });
        items.Items.Add(new RandomizerConfigurationDefinition.GroupItem
        {
            Id = "items/randomize-keys",
            Label = "Randomize Key Items",
            Description = "Randomize locations of progression key items",
            Type = "switch",
            Default = true
        });
        items.Items.Add(new RandomizerConfigurationDefinition.GroupItem
        {
            Id = "items/randomize-non-key-items",
            Label = "Randomize Other Items",
            Description = "Randomize non-key item pickups (ammo, healing, etc.)",
            Type = "switch",
            Default = true
        });

        var graph = LoadGraph();
        if (graph != null)
        {
            var distribution = general.CreateGroup("Distribution");
            var ratioKinds = graph.ItemTypes
                .Select(x => x.Value.Kind)
                .Distinct()
                .Where(k => !k.StartsWith("key/") && !k.StartsWith("weapon/"))
                .OrderBy(k => k);

            foreach (var kind in ratioKinds)
            {
                var label = FormatKindLabel(kind);
                distribution.Items.Add(new RandomizerConfigurationDefinition.GroupItem
                {
                    Id = $"items/ratio/{kind}",
                    Label = $"{label}",
                    Description = $"Relative frequency of {label.ToLowerInvariant()} pickups",
                    Type = "slider",
                    Default = 5,
                    Min = 0,
                    Max = 10,
                    Step = 1
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

    private static string FormatKindLabel(string kind)
    {
        var parts = kind.Split('/');
        for (var i = 0; i < parts.Length; i++)
        {
            if (parts[i].Length > 0)
                parts[i] = char.ToUpper(parts[i][0]) + parts[i][1..];
        }
        return string.Join(" / ", parts);
    }
}
