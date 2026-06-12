using IntelOrca.Biohazard.Room;

namespace IntelOrca.Biohazard.BioRand.RECV;

public sealed class ReCvRandomizerContext
{
    private readonly Dictionary<string, Rng> _rngCache = [];

    public ElfRegion Elf { get; }
    public RdtCv[] Rooms { get; }
    public int Seed { get; }
    public RandomizerConfiguration Config { get; }
    public RandomizerLogger Logger { get; }

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
