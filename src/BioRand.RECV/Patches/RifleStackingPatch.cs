namespace IntelOrca.Biohazard.BioRand.RECV.Patches;

public sealed class RifleStackingPatch : ICvPatch
{
    public void Apply(ReCvRandomizerContext context)
    {
        context.Logger.LogLine("Fixing rifle stacking...");
        context.Elf.WriteBytes(0x35B1F4, [0x02, 0x0E]);
        context.Elf.WriteBytes(0x35B200, [0x02, 0x16]);
        context.Logger.LogLine("Rifle stacking fixed at 0x35B1F4 and 0x35B200");
    }
}
