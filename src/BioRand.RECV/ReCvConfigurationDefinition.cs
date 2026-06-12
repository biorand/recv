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
        return definition;
    }
}
