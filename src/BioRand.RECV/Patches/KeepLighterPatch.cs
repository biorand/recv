using IntelOrca.Biohazard.BioRand.RECV;

namespace IntelOrca.Biohazard.BioRand.RECV.Patches;

/// <summary>
/// Patches RDT scripts to prevent losing the Lighter during the Rodrigo
/// medicine scene. The game normally removes the Lighter from inventory when
/// giving medicine to Rodrigo and puts the gift into the special slot.
/// Since the player always starts with the Lighter (via <see cref="InitialLighterPatch"/>),
/// these NOPs preserve it.
/// 
/// This patch modifies RDT script data (context.Rooms[]) rather than the ELF
/// binary. The NOP byte for CV SCD scripts is 0xF4 (SH-2 NOP), NOT the MIPS
/// NOP (0x00000000) used by ElfRegion.Nop(). Always applied — the Rodrigo
/// scene runs regardless of other settings.
/// 
/// NOPs are applied at opcode granularity via <see cref="ReCvRdtPatcherRoom"/>,
/// filling each target instruction's full byte span with 0xF4. Offsets match
/// the classic biorand-classic randomizer.
/// </summary>
public sealed class KeepLighterPatch : ICvPatch
{
    // RDT 1000 (Prison cell / Rodrigo medicine scene)
    private const string Rdt1000 = "1000";
    private static readonly int[] Rdt1000Offsets =
    [
        0x18CD7A, // Remove lighter from inventory
        0x18DB74, // Remove lighter from inventory (variant)
        0x18CD74, // Put gift in special slot
        0x18DB7C, // Put gift in special slot (variant)
    ];

    // RDT 8170 (Military B3 Worm / later prison return)
    private const string Rdt8170 = "8170";
    private static readonly int[] Rdt8170Offsets =
    [
        0x14D26E, // Put gift in special slot
        0x14EB68, // Put gift in special slot (variant)
    ];

    public void Apply(ReCvRandomizerContext context)
    {
        context.Logger.LogLine("Patching keep-lighter script instructions...");

        PatchRdt(context, Rdt1000, Rdt1000Offsets);
        PatchRdt(context, Rdt8170, Rdt8170Offsets);

        context.Logger.LogLine("Keep-lighter patches applied");
    }

    private static void PatchRdt(ReCvRandomizerContext context, string rdtId, int[] offsets)
    {
        if (!context.RoomIndexById.TryGetValue(rdtId, out var roomIndex))
        {
            context.Logger.LogLine($"WARNING: RDT {rdtId} not found, skipping");
            return;
        }

        var room = new ReCvRdtPatcherRoom(context, roomIndex, rdtId);
        foreach (var offset in offsets)
            room.Nop(offset);
        room.Flush();
    }
}
