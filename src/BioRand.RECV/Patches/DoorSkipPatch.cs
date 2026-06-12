namespace IntelOrca.Biohazard.BioRand.RECV.Patches;

public sealed class DoorSkipPatch : ICvPatch
{
    public void Apply(ReCvRandomizerContext context)
    {
        if (!context.Config.GetValueOrDefault<bool>("doors/skip", true))
            return;

        context.Logger.LogLine("Patching door skip...");
        context.Elf.Nop(ElfAddresses.DoorSkip1);
        context.Elf.Nop(ElfAddresses.DoorSkip2);
        context.Logger.LogLine($"Door skip patched at 0x{ElfAddresses.DoorSkip1:X} and 0x{ElfAddresses.DoorSkip2:X}");
    }
}
