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
        return definition;
    }
}
