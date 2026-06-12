namespace IntelOrca.Biohazard.BioRand.RECV.Patches;

public sealed class ItemPickupPatch : ICvPatch
{
    public void Apply(ReCvRandomizerContext context)
    {
        context.Logger.LogLine("Hacking item pickup...");
        context.Elf.WriteBytes(0x266E30, [0x06]);
        context.Logger.LogLine("Item pickup hacked at 0x266E30");
    }
}
