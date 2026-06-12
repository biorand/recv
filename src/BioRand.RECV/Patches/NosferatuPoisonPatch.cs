namespace IntelOrca.Biohazard.BioRand.RECV.Patches;

public sealed class NosferatuPoisonPatch : ICvPatch
{
    public void Apply(ReCvRandomizerContext context)
    {
        if (!context.Config.GetValueOrDefault<bool>("doors/random", false))
            return;

        context.Logger.LogLine("Disabling Nosferatu poison...");
        context.Elf.Nop(0x1E6DC4);
        context.Logger.LogLine("Nosferatu poison disabled at 0x1E6DC4");
    }
}
