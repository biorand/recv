namespace IntelOrca.Biohazard.BioRand.RECV;

public sealed class ElfRegion
{
    private const int AddressOffset = 0xFFF80;

    private readonly byte[] _originalData;
    private readonly Dictionary<long, byte[]> _patches = [];

    public ElfRegion(byte[] data)
    {
        _originalData = data;
    }

    public void Nop(uint ps2Address)
    {
        var offset = ps2Address - AddressOffset;
        _patches[offset] = new byte[4];
    }

    public void WriteUInt32(uint ps2Address, uint value)
    {
        var offset = ps2Address - AddressOffset;
        _patches[offset] =
        [
            (byte)(value & 0xFF),
            (byte)((value >> 8) & 0xFF),
            (byte)((value >> 16) & 0xFF),
            (byte)((value >> 24) & 0xFF),
        ];
    }

    public void WriteBytes(uint ps2Address, byte[] data)
    {
        var offset = ps2Address - AddressOffset;
        _patches[offset] = data;
    }

    public byte[] ToArray()
    {
        var result = new byte[_originalData.Length];
        Array.Copy(_originalData, result, _originalData.Length);
        foreach (var (offset, data) in _patches)
        {
            Array.Copy(data, 0, result, offset, data.Length);
        }
        return result;
    }
}
