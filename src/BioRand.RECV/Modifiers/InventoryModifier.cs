using System.Reflection;
using System.Text.Json;
using IntelOrca.Biohazard;
using IntelOrca.Biohazard.BioRand.RECV.Patches;
using IntelOrca.Biohazard.Room;

namespace IntelOrca.Biohazard.BioRand.RECV.Modifiers;

/// <summary>
/// Randomizes Claire's starting inventory by writing a random weapon to the
/// first inventory slot (ELF offset 0x2A6CF0) and NOP-ing the room 1030
/// Steve cutscene SCD instructions that would otherwise overwrite it with
/// the vanilla Handgun.
/// 
/// The special slot always retains the Lighter (set by InitialLighterPatch).
/// Only weapons with common/early-game ammo are included in the pool.
/// Runs after all patches (patches → modifiers pipeline), so it overwrites
/// whatever InitialLighterPatch wrote to 0x2A6CF0.
/// </summary>
[Order(ModifierOrders.StartingInventory)]
public sealed class InventoryModifier : ICvModifier
{
    // PS2 virtual address in SLUS_201.84 — first inventory slot item type (1 byte)
    private const uint FirstInventoryItem = 0x2A6CF0;

    // Starter-friendly weapon kinds (common/early-game ammo)
    private static readonly HashSet<string> StarterWeaponKinds =
    [
        "weapon/handgun",
        "weapon/shotgun",
        "weapon/bow-gun",
        "weapon/knife",
    ];

    // Room 1030 (Steve cutscene) SCD offsets that give Handgun to the player
    private const string SteveRoomId = "1030";
    private static readonly int[] SteveCutsceneOffsets = [2603334, 2607638];

    public void Apply(ReCvRandomizerContext context, RandomizerLogger logger)
    {
        if (!context.Config.GetValueOrDefault<bool>("items/randomize-starting-inventory", false))
            return;

        logger.LogLine("Randomizing starting inventory...");
        var rng = context.GetRng("starting-inventory");

        var graph = LoadGraph();
        var weapons = GetStarterWeapons(graph);

        if (weapons.Length == 0)
        {
            logger.LogLine("WARNING: No starter weapons found in graph, skipping");
            return;
        }

        var chosenWeapon = weapons[rng.Next(0, weapons.Length)];
        logger.LogLine($"First slot weapon: 0x{chosenWeapon:X2} ({GetWeaponName(graph, chosenWeapon)})");

        // Write to ELF first inventory slot — overwrites the Lighter that
        // InitialLighterPatch wrote here. The special slot retains the Lighter.
        context.Elf.WriteBytes(FirstInventoryItem, [chosenWeapon]);

        // NOP the Steve cutscene in room 1030 so it doesn't overwrite our weapon
        PatchSteveCutscene(context, logger);

        logger.LogLine("Starting inventory randomized");
    }

    private static void PatchSteveCutscene(ReCvRandomizerContext context, RandomizerLogger logger)
    {
        if (!context.RoomIndexById.TryGetValue(SteveRoomId, out var roomIndex))
        {
            logger.LogLine($"WARNING: RDT {SteveRoomId} not found, cannot patch Steve cutscene");
            return;
        }

        var patcher = new ReCvRdtPatcherRoom(context, roomIndex, SteveRoomId);
        foreach (var offset in SteveCutsceneOffsets)
        {
            patcher.Nop(offset);
        }
        patcher.Flush();

        logger.LogLine($"  Patched RDT {SteveRoomId}: NOP'd Steve cutscene item give");
    }

    /// <summary>
    /// Collects weapon item IDs from the graph whose kind is in the starter-friendly set.
    /// Sorted by ID for deterministic selection.
    /// </summary>
    private static byte[] GetStarterWeapons(GraphData graph)
    {
        return graph.ItemTypes
            .Where(kvp => StarterWeaponKinds.Contains(kvp.Value.Kind))
            .Select(kvp => byte.TryParse(kvp.Key, out var id) ? id : (byte?)null)
            .OfType<byte>()
            .OrderBy(id => id)
            .ToArray();
    }

    private static string GetWeaponName(GraphData graph, byte weaponId) =>
        graph.ItemTypes.TryGetValue(weaponId.ToString(), out var itemType)
            ? itemType.Name
            : $"Unknown 0x{weaponId:X2}";

    private static GraphData LoadGraph()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(
            "IntelOrca.Biohazard.BioRand.RECV.data.graph.json");
        if (stream == null)
            throw new InvalidOperationException("Embedded resource data/graph.json not found");

        return JsonSerializer.Deserialize<GraphData>(stream, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
        }) ?? throw new InvalidOperationException("Failed to deserialize graph.json");
    }
}
