namespace IntelOrca.Biohazard.BioRand.RECV.Modifiers;

[Order(ModifierOrders.KeyHints)]
public sealed class KeyHintsModifier : ICvModifier
{
    public void Apply(ReCvRandomizerContext context, RandomizerLogger logger)
    {
        var placements = context.KeyPlacements;
        if (placements == null || placements.Count == 0)
            return;

        var graph = GraphDataLoader.Load();
        var rows = KeyHintsGenerator.BuildRows(graph, placements);
        context.HintSheetHtml = KeyHintsGenerator.RenderHtml(rows, context.Seed);
    }
}
