using IntelOrca.Biohazard.Extensions;
using IntelOrca.Biohazard.Room;
using IntelOrca.Biohazard.Script;

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
    /// Writes SCD NOP (0xF4) over the entire opcode that starts at the given
    /// offset. The opcode length is determined by parsing the room's CV script,
    /// so adjacent instructions are never corrupted. A fixed-width write would
    /// corrupt 2-byte opcodes (e.g. <c>player_item_lost</c>) and 6-byte opcodes
    /// (e.g. <c>set</c>/<c>ck</c>), leaving dangling bytes that decode as <c>end</c>
    /// and truncate the script mid-cutscene.
    /// </summary>
    public void Nop(int offset)
    {
        if (offset < 0 || offset >= Data.Length)
        {
            _context.Logger.LogLine($"  WARNING: RDT {_rdtId} offset 0x{offset:X} out of bounds, skipping NOP");
            return;
        }

        var length = GetOpcodeLength(offset);
        if (length <= 0)
        {
            _context.Logger.LogLine($"  WARNING: RDT {_rdtId} offset 0x{offset:X} is not the start of an opcode, skipping NOP");
            return;
        }

        if (offset + length > Data.Length)
        {
            _context.Logger.LogLine($"  WARNING: RDT {_rdtId} offset 0x{offset:X} opcode spans past end of file, skipping NOP");
            return;
        }

        for (var i = 0; i < length; i++)
            Data[offset + i] = ScdNop;

        _context.Logger.LogLine($"  NOP RDT {_rdtId} 0x{offset:X} ({length} bytes)");
    }

    /// <summary>
    /// Determines the byte length of the CV script opcode that starts at the
    /// given absolute file offset, or 0 if the offset is not an opcode start.
    /// </summary>
    private int GetOpcodeLength(int offset)
    {
        var collector = new OpcodeSpanCollector();
        try
        {
            new RdtCv(Data).ReadScript(collector);
        }
        catch (Exception ex)
        {
            _context.Logger.LogLine($"  WARNING: RDT {_rdtId} failed to parse script: {ex.Message}");
            return 0;
        }
        return collector.GetLengthAt(offset);
    }

    /// <summary>
    /// Collects the absolute file offset and byte length of every opcode in a
    /// CV room script so a target instruction can be NOPped at opcode granularity.
    /// </summary>
    private sealed class OpcodeSpanCollector : BioScriptVisitor
    {
        private readonly Dictionary<int, int> _opcodeLengths = new();

        public override void VisitOpcode(int offset, Span<byte> opcodeBytes)
        {
            if (!_opcodeLengths.ContainsKey(offset))
                _opcodeLengths[offset] = opcodeBytes.Length;
        }

        public int GetLengthAt(int offset) =>
            _opcodeLengths.TryGetValue(offset, out var length) ? length : 0;
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
