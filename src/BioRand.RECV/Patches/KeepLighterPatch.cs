using System.Linq;
using IntelOrca.Biohazard.BioRand.RECV;
using IntelOrca.Biohazard.Room;

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
/// Offsets match the classic biorand-classic randomizer.
/// </summary>
public sealed class KeepLighterPatch : ICvPatch
{
    // CV SCD NOP byte (0xF4 = SH-2 NOP), NOT MIPS NOP (0x00000000)
    private const byte ScdNop = 0xF4;

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

        var data = context.Rooms[roomIndex].Data.ToArray();
        var patched = false;

        foreach (var offset in offsets)
        {
            if (offset < 0 || offset + 4 > data.Length)
            {
                context.Logger.LogLine($"WARNING: RDT {rdtId} offset 0x{offset:X} out of bounds, skipping");
                continue;
            }

            // Write 4 bytes of SCD NOP to fully replace the instruction
            for (var i = 0; i < 4; i++)
                data[offset + i] = ScdNop;
            patched = true;
        }

        if (patched)
        {
            context.Rooms[roomIndex] = new RdtCv(data);
            context.Logger.LogLine($"  Patched RDT {rdtId}: {string.Join(", ", offsets.Select(o => $"0x{o:X}"))}");
        }
    }
}
