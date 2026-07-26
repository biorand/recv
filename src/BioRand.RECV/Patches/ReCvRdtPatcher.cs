using System.Reflection;

namespace IntelOrca.Biohazard.BioRand.RECV.Patches;

/// <summary>
/// Applies RDT script patches via reflection-based method discovery.
/// Each method marked with <see cref="RdtPatchAttribute"/> is automatically
/// discovered and invoked. Method parameters named <c>rdtXXXX</c> are resolved
/// to <see cref="ReCvRdtPatcherRoom"/> wrappers for the corresponding RDT.
/// 
/// This follows the same pattern as the classic BioRand <c>Re1RdtPatcher</c>
/// on the redesign branch, but adapted for CV's byte-level SCD patching
/// (0xF4 SH-2 NOP) instead of opcode-tree replacement.
/// 
/// Offsets match the classic <c>ReCvDoorHelper.Begin()</c> in the biorand-classic
/// randomizer.
/// </summary>
public sealed class ReCvRdtPatcher : ICvPatch
{
    private ReCvRandomizerContext _context = null!;

    public void Apply(ReCvRandomizerContext context)
    {
        _context = context;
        context.Logger.LogLine("Applying RDT script patches...");

        var methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<RdtPatchAttribute>();
            if (attr == null)
                continue;

            // Evaluate gate condition
            if (attr.Gate != null)
            {
                var negated = attr.Gate.StartsWith('!');
                var configKey = negated ? attr.Gate[1..] : attr.Gate;
                var configValue = context.Config.GetValueOrDefault<bool>(configKey, false);
                if (negated ? configValue : !configValue)
                {
                    context.Logger.LogLine($"  Skipping {method.Name} (gate: {attr.Gate})");
                    continue;
                }
            }

            // Resolve RDT parameters
            var parameters = method.GetParameters();
            var args = new object?[parameters.Length];
            var roomsPatched = new List<ReCvRdtPatcherRoom>();
            var resolveError = false;

            for (var i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                if (param.ParameterType == typeof(ReCvRdtPatcherRoom) &&
                    param.Name != null &&
                    param.Name.StartsWith("rdt") &&
                    param.Name.Length > 3)
                {
                    var rdtId = param.Name[3..]; // "rdt1010" -> "1010"
                    if (!context.RoomIndexById.TryGetValue(rdtId, out var roomIndex))
                    {
                        context.Logger.LogLine($"  WARNING: RDT {rdtId} not found, skipping {method.Name}");
                        resolveError = true;
                        break;
                    }
                    var room = new ReCvRdtPatcherRoom(context, roomIndex, rdtId);
                    args[i] = room;
                    roomsPatched.Add(room);
                }
                else
                {
                    context.Logger.LogLine($"  WARNING: Unknown parameter '{param.Name}' in {method.Name}, skipping");
                    resolveError = true;
                    break;
                }
            }

            if (resolveError)
                continue;

            context.Logger.LogLine($"  Applying {method.Name}...");
            method.Invoke(this, args);

            // Flush all modified rooms
            foreach (var room in roomsPatched)
            {
                room.Flush();
            }
        }

        context.Logger.LogLine("RDT script patches applied");
    }

    #region Always-applied patches

    /// <summary>
    /// Force window cutscene to trigger on item interaction in the prison
    /// window room (RDT 1070). Patches are always applied — the cutscene
    /// is required for progression when item locations are randomized.
    /// </summary>
    [RdtPatch]
    private void ForceWindowCutscene(ReCvRdtPatcherRoom rdt1070)
    {
        rdt1070.Nop(0x1819AE);
    }

    /// <summary>
    /// Skip the lengthy Steve/Alfred transformation cutscene (RDT 3050).
    /// Always applied to prevent softlocks and speed up gameplay.
    /// </summary>
    [RdtPatch]
    private void SkipSteveAlfredCutscene(ReCvRdtPatcherRoom rdt3050)
    {
        rdt3050.Nop(0x15F288, 0x15F2DA);
        rdt3050.Nop(0x15EEDC, 0x15EEF6);
    }

    /// <summary>
    /// Force Steve to appear at the airport (RDT 5000). Prevents the game
    /// from skipping Steve's appearance when key items are randomized.
    /// Always applied.
    /// </summary>
    [RdtPatch]
    private void ForceSteveAtAirport(ReCvRdtPatcherRoom rdt5000)
    {
        rdt5000.Nop(0x187778, 0x18777A);
        rdt5000.Nop(0x187784, 0x18779C);
    }

    #endregion

    #region Patches gated behind !doors/random

    /// <summary>
    /// Force RDT 1021 (with briefcase) to load instead of RDT 1020
    /// (without briefcase). The script checks conditions to decide which
    /// variant to load — NOPping those checks forces the version with
    /// the briefcase. Only needed when doors are not randomized.
    /// </summary>
    [RdtPatch(Gate = "!doors/random")]
    private void ForceBriefcaseVersion(ReCvRdtPatcherRoom rdt1010)
    {
        rdt1010.Nop(0x3EF2C);
        rdt1010.Nop(0x3EF38, 0x3EF4C);
        rdt1010.Nop(0x3EF50, 0x3EF5A);
    }

    /// <summary>
    /// Force RDT 1031 (with usable medal) to load instead of RDT 1030
    /// (without medal). Same variant-selection approach as the briefcase.
    /// Only needed when doors are not randomized.
    /// </summary>
    [RdtPatch(Gate = "!doors/random")]
    private void ForceMedalVersion(ReCvRdtPatcherRoom rdt1050)
    {
        rdt1050.Nop(0x1DF2AA, 0x1DF2BE);
        rdt1050.Nop(0x1DF2C2, 0x1DF2CC);
    }

    /// <summary>
    /// Fix a softlock when entering room 305 via the ladder without having
    /// picked up the Silver Key (RDT 3060). Patches a condition byte to 0x00
    /// to allow progression. Only needed when doors are not randomized.
    /// </summary>
    [RdtPatch(Gate = "!doors/random")]
    private void FixLadderSilverKeySoftlock(ReCvRdtPatcherRoom rdt3060)
    {
        rdt3060.Patch(0x70A10 + 6, 0x00);
    }

    /// <summary>
    /// Change the transition condition for entering room 4011 so it happens
    /// immediately after the Alfred cutscene (RDT 4080, RDT 40F0).
    /// Only needed when doors are not randomized.
    /// </summary>
    [RdtPatch(Gate = "!doors/random")]
    private void FixTransitionCondition(
        ReCvRdtPatcherRoom rdt4080,
        ReCvRdtPatcherRoom rdt40F0)
    {
        rdt4080.Patch(0x9F86C + 2, 0xC5);
        rdt40F0.Patch(0x7241C + 2, 0xC5);
    }

    #endregion

    #region TODO: Patches gated behind doors/random

    // These patches are for when door randomization is implemented:
    //
    // [RdtPatch(Gate = "doors/random")]
    // private void PreventForcedSwapToChris(ReCvRdtPatcherRoom rdt70A0)
    // {
    //     rdt70A0.Nop(0x1F3140);
    // }
    //
    // Door randomization also needs SetFlag operations on RDTs:
    // 20E0: SetFlag(1, 19, 0)
    // 5040: SetFlag(1, 19, 1)
    // 4080: SetFlag(1, 211, 0) -- replaces FixTransitionCondition when doors are random
    // 6000: SetFlag(1, 35, 0), SetFlag(1, 261, 0)
    // 8080: SetFlag(1, 309, 1), SetFlag(1, 41, 0)
    // 80A0: SetFlag(1, 354, 0)
    //
    // SetFlag requires appending opcode 0x05 bytes to the RDT's script section
    // using the Builder pattern (ToBuilder() -> modify Script -> ToRdt()).

    #endregion
}
