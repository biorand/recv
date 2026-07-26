using System.Linq;
using IntelOrca.Biohazard.Room;

namespace IntelOrca.Biohazard.BioRand.RECV.Patches;

/// <summary>
/// Wraps a single RDT room for byte-level script patching.
/// Provides <see cref="Nop(int)"/>, <see cref="Nop(int, int)"/> and
/// <see cref="Patch(int, byte)"/> operations that modify the raw RDT data
/// and replace the room in the context.
/// 
/// The SCD NOP byte is <c>0xF4</c> (SH-2 NOP), <b>not</b> the MIPS NOP
/// (<c>0x00000000</c>) used by <c>ElfRegion.Nop()</c>.
/// </summary>
internal sealed class ReCvRdtPatcherRoom
{
    private const byte ScdNop = 0xF4;

    private readonly ReCvRandomizerContext _context;
    private readonly int _roomIndex;
    private readonly string _rdtId;

    /// <summary>
    /// The raw byte data of the room. Modifications are flushed to the context
    /// in <see cref="Flush"/>.
    /// </summary>
    public byte[] Data { get; private set; }

    public ReCvRdtPatcherRoom(ReCvRandomizerContext context, int roomIndex, string rdtId)
    {
        _context = context;
        _roomIndex = roomIndex;
        _rdtId = rdtId;
        Data = context.Rooms[roomIndex].Data.ToArray();
    }

    /// <summary>
    /// Writes 4 bytes of SCD NOP (0xF4) at the given offset to replace one instruction.
    /// </summary>
    public void Nop(int offset)
    {
        if (offset < 0 || offset + 4 > Data.Length)
        {
            _context.Logger.LogLine($"  WARNING: RDT {_rdtId} offset 0x{offset:X} out of bounds, skipping NOP");
            return;
        }
        for (var i = 0; i < 4; i++)
            Data[offset + i] = ScdNop;
    }

    /// <summary>
    /// Writes SCD NOP (0xF4) over a range of bytes from <paramref name="start"/> to
    /// <paramref name="end"/> inclusive. Used to NOP multiple consecutive instructions.
    /// </summary>
    public void Nop(int start, int end)
    {
        if (start < 0 || end >= Data.Length || start > end)
        {
            _context.Logger.LogLine($"  WARNING: RDT {_rdtId} range 0x{start:X}-0x{end:X} out of bounds, skipping NOP");
            return;
        }
        for (var i = start; i <= end; i++)
            Data[i] = ScdNop;
    }

    /// <summary>
    /// Writes a single byte at the given offset. Used for patches that change
    /// a condition value rather than disabling an instruction entirely.
    /// </summary>
    public void Patch(int offset, byte value)
    {
        if (offset < 0 || offset >= Data.Length)
        {
            _context.Logger.LogLine($"  WARNING: RDT {_rdtId} offset 0x{offset:X} out of bounds, skipping patch");
            return;
        }
        Data[offset] = value;
    }

    /// <summary>
    /// Flushes modified data back to the context by creating a new <see cref="RdtCv"/>.
    /// Call this after all NOPs/patches for this room are applied.
    /// </summary>
    public void Flush()
    {
        _context.Rooms[_roomIndex] = new RdtCv(Data);
        _context.Logger.LogLine($"  Patched RDT {_rdtId}");
    }
}
