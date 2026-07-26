using IntelOrca.Biohazard.BioRand.RECV;

namespace IntelOrca.Biohazard.BioRand.RECV.Patches;

/// <summary>
/// Patches the ELF binary to start the player with the Lighter (0x37) in the
/// special inventory slot and first inventory slot. Always applied — the Lighter
/// is essential for progression through the Prison area and the classic randomizer
/// always starts Claire with it.
/// </summary>
public sealed class InitialLighterPatch : ICvPatch
{
    // PS2 virtual addresses in SLUS_201.84
    private const uint SpecialSlotDefinition = 0x3340B0;  // 32-bit: [item_id, 0x00, 0xFF, 0x00]
    private const uint SpecialSlotItem = 0x2A6CE8;        // 8-bit: item type
    private const uint FirstInventoryItem = 0x2A6CF0;     // 8-bit: item type

    public void Apply(ReCvRandomizerContext context)
    {
        context.Logger.LogLine("Setting initial lighter...");

        // Special slot struct at 0x3340B0: [item_id: 0x37, padding: 0x00, flags: 0xFF, padding: 0x00]
        context.Elf.WriteBytes(SpecialSlotDefinition, [ReCvItemIds.Lighter, 0x00, 0xFF, 0x00]);

        // Special slot item type
        context.Elf.WriteBytes(SpecialSlotItem, [ReCvItemIds.Lighter]);

        // First inventory slot item type
        context.Elf.WriteBytes(FirstInventoryItem, [ReCvItemIds.Lighter]);

        context.Logger.LogLine($"Lighter (0x{ReCvItemIds.Lighter:X2}) set at special slot and first inventory");
    }
}
