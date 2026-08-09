using IntelOrca.Biohazard;
using IntelOrca.Biohazard.Room;

namespace IntelOrca.Biohazard.BioRand.RECV;

public sealed class ReCvRandomizerContext
{
    private static readonly string[] RdxFileNames =
    [
        "RM_0000.RDX", "RM_0010.RDX", "RM_0020.RDX", "RM_0021.RDX",
        "RM_0030.RDX", "RM_0031.RDX", "RM_0040.RDX", "RM_0050.RDX",
        "RM_0060.RDX", "RM_0070.RDX", "RM_0080.RDX", "RM_0090.RDX",
        "RM_0100.RDX", "RM_0110.RDX", "RM_0120.RDX", "RM_0130.RDX",
        "RM_0140.RDX", "RM_0150.RDX", "RM_0160.RDX",
        "RM_1000.RDX", "RM_1001.RDX", "RM_1002.RDX",
        "RM_1020.RDX", "RM_1021.RDX", "RM_1030.RDX",
        "RM_1040.RDX", "RM_1050.RDX", "RM_1060.RDX",
        "RM_1070.RDX", "RM_1080.RDX", "RM_1090.RDX",
        "RM_1100.RDX", "RM_1110.RDX", "RM_1120.RDX",
        "RM_1121.RDX", "RM_1122.RDX", "RM_1130.RDX", "RM_1140.RDX",
        "RM_2000.RDX", "RM_2010.RDX", "RM_2011.RDX",
        "RM_2020.RDX", "RM_2030.RDX", "RM_2031.RDX",
        "RM_2040.RDX", "RM_2050.RDX", "RM_2060.RDX", "RM_2070.RDX",
        "RM_3000.RDX", "RM_3010.RDX", "RM_3011.RDX",
        "RM_3020.RDX", "RM_3030.RDX", "RM_3040.RDX",
        "RM_3050.RDX", "RM_3060.RDX", "RM_3070.RDX",
        "RM_3080.RDX", "RM_3090.RDX", "RM_3091.RDX",
        "RM_3100.RDX", "RM_3110.RDX", "RM_3120.RDX",
        "RM_3130.RDX", "RM_3140.RDX", "RM_3150.RDX",
        "RM_3160.RDX", "RM_3170.RDX", "RM_3180.RDX",
        "RM_3190.RDX", "RM_3200.RDX", "RM_3210.RDX",
        "RM_3220.RDX", "RM_3230.RDX", "RM_3240.RDX",
        "RM_4000.RDX", "RM_4001.RDX", "RM_4010.RDX",
        "RM_4020.RDX", "RM_4030.RDX", "RM_4040.RDX",
        "RM_4050.RDX", "RM_4060.RDX", "RM_4070.RDX",
        "RM_4080.RDX", "RM_4090.RDX", "RM_4100.RDX",
        "RM_4110.RDX", "RM_4120.RDX", "RM_4130.RDX",
        "RM_4140.RDX", "RM_4150.RDX", "RM_4160.RDX",
        "RM_4170.RDX", "RM_4180.RDX", "RM_4190.RDX",
        "RM_5000.RDX", "RM_5001.RDX", "RM_5010.RDX",
        "RM_5011.RDX", "RM_5020.RDX", "RM_5030.RDX",
        "RM_5040.RDX", "RM_5050.RDX",
        "RM_6000.RDX",
        "RM_7000.RDX", "RM_7010.RDX", "RM_7020.RDX",
        "RM_7030.RDX", "RM_7040.RDX", "RM_7050.RDX",
        "RM_7051.RDX", "RM_7060.RDX", "RM_7070.RDX",
        "RM_70A0.RDX",
        "RM_8000.RDX", "RM_8010.RDX", "RM_8020.RDX",
        "RM_8030.RDX", "RM_8031.RDX", "RM_8040.RDX",
        "RM_8050.RDX", "RM_8060.RDX", "RM_8070.RDX",
        "RM_8080.RDX", "RM_8090.RDX", "RM_80A0.RDX",
        "RM_80B0.RDX", "RM_80C0.RDX", "RM_80D0.RDX",
        "RM_80E0.RDX", "RM_80F0.RDX", "RM_8100.RDX",
        "RM_8110.RDX", "RM_8120.RDX", "RM_8130.RDX",
        "RM_8140.RDX", "RM_8150.RDX", "RM_8160.RDX",
        "RM_8170.RDX", "RM_8180.RDX", "RM_8190.RDX",
        "RM_8200.RDX", "RM_8210.RDX",
        "RM_9000.RDX", "RM_9010.RDX",
        "RM_9100.RDX", "RM_9101.RDX", "RM_9102.RDX",
        "RM_9103.RDX", "RM_9110.RDX",
        "RM_9200.RDX",
        "RM_9300.RDX", "RM_9301.RDX", "RM_9302.RDX",
        "RM_A000.RDX", "RM_A010.RDX", "RM_A020.RDX",
        "RM_A030.RDX", "RM_A040.RDX", "RM_A050.RDX",
        "RM_A060.RDX", "RM_A070.RDX", "RM_A080.RDX",
        "RM_A090.RDX", "RM_A0A0.RDX", "RM_A0B0.RDX",
        "RM_A0C0.RDX", "RM_A0D0.RDX", "RM_A0E0.RDX",
        "RM_A0F0.RDX", "RM_A100.RDX", "RM_A110.RDX",
        "RM_A120.RDX", "RM_A130.RDX", "RM_A140.RDX",
        "RM_A150.RDX", "RM_A160.RDX", "RM_A170.RDX",
        "RM_A180.RDX", "RM_A190.RDX", "RM_A1A0.RDX",
        "RM_A1B0.RDX", "RM_A1C0.RDX", "RM_A1D0.RDX",
        "RM_A1E0.RDX", "RM_A1F0.RDX", "RM_A200.RDX",
    ];

    private readonly Dictionary<string, Rng> _rngCache = [];

    public ElfRegion Elf { get; }
    public RdtCv[] Rooms { get; }
    public IReadOnlyDictionary<string, int> RoomIndexById { get; }
    public int Seed { get; }
    public RandomizerConfiguration Config { get; }
    public RandomizerLogger Logger { get; }
    public string? MermaidGraph { get; set; }
    public AfsFile? AdvAfs { get; set; }

    public ReCvRandomizerContext(
        ElfRegion elf,
        RdtCv[] rooms,
        int seed,
        RandomizerConfiguration config,
        RandomizerLogger logger)
    {
        Elf = elf;
        Rooms = rooms;
        Seed = seed;
        Config = config;
        Logger = logger;

        var lookup = new Dictionary<string, int>();
        for (var i = 0; i < rooms.Length && i < RdxFileNames.Length; i++)
        {
            var id = GetRdtId(i);
            if (id != null)
                lookup[id] = i;
        }
        RoomIndexById = lookup;
    }

    public string? GetRdtId(int fileIndex)
    {
        if (fileIndex < 0 || fileIndex >= RdxFileNames.Length)
            return null;
        var name = RdxFileNames[fileIndex];
        if (name.Length < 10 || !name.StartsWith("RM_"))
            return null;
        return name.Substring(3, 4);
    }

    public Rng GetRng(params string[] keys)
    {
        var key = string.Join("/", keys);
        if (!_rngCache.TryGetValue(key, out var rng))
        {
            var hash = HashCode.Combine(Seed, key.GetHashCode());
            rng = new Rng(hash);
            _rngCache[key] = rng;
        }
        return rng;
    }
}
