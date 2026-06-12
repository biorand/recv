namespace IntelOrca.Biohazard.BioRand.RECV.Patches;

public sealed class DoorSkipPatch : ICvPatch
{
    public void Apply(ReCvRandomizerContext context)
    {
        if (!context.Config.GetValueOrDefault<bool>("doors/skip", true))
            return;

        context.Logger.LogLine("Patching door skip...");
        context.Elf.Nop(0x133D4C);
        context.Elf.Nop(0x133D54);
        context.Logger.LogLine("Door skip patched at 0x133D4C and 0x133D54");
    }
}
