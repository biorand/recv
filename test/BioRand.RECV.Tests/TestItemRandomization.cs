using System.Reflection;
using System.Text.Json;
using IntelOrca.Biohazard;
using IntelOrca.Biohazard.Room;
using IntelOrca.Biohazard.BioRand.RECV.Modifiers;
using Xunit;

namespace IntelOrca.Biohazard.BioRand.RECV.Tests;

/// <summary>
/// Verifies that the RDT files produced by the randomizer actually have
/// randomized item types written into them. The game reads the item type from
/// byte 6 of each item-table entry (patched ELF 0x266E30), so a randomized
/// RDT must have byte 6 differ from the vanilla byte 4 type.
/// </summary>
public class TestItemRandomization
{
    private const int RdxFileCount = 205;

    [Fact]
    public void RdtIdMapping_MatchesClassic()
    {
        var context = CreateContext(new RdtCv[RdxFileCount], seed: 0);

        // Classic RdtId strings (graph space) must map to the correct AFS files.
        Assert.Equal("1000", context.GetRdtId(0));    // RM_0000
        Assert.Equal("1010", context.GetRdtId(1));    // RM_0010
        Assert.Equal("1070", context.GetRdtId(9));    // RM_0070
        Assert.Equal("8170", context.GetRdtId(153));  // RM_7230
        Assert.Equal("A200", context.GetRdtId(199));  // RM_9320
        Assert.Equal(0, context.RoomIndexById["1000"]);
        Assert.Equal(153, context.RoomIndexById["8170"]);
        Assert.Equal(199, context.RoomIndexById["A200"]);

        // RdtId room digits are DECIMAL in the file name: "40F0" (stage 4-1=3,
        // room 0x0F=15, variant 0) maps to RM_3150.RDX, NOT RM_30F0.
        Assert.Equal("40F0", context.GetRdtId(65));   // RM_3150
        Assert.Equal(65, context.RoomIndexById["40F0"]);
        Assert.Equal("4080", context.GetRdtId(57));   // RM_3080
        Assert.Equal(57, context.RoomIndexById["4080"]);

        // Every file must round-trip through the id lookup.
        Assert.Equal(RdxFileCount, context.RoomIndexById.Count);
        foreach (var (id, index) in context.RoomIndexById)
        {
            Assert.Equal(index, context.RoomIndexById[context.GetRdtId(index)!]);
            Assert.Equal(id, context.GetRdtId(index));
        }
    }

    [Fact]
    public void GraphRoomRdts_AllResolveToFiles()
    {
        var context = CreateContext(new RdtCv[RdxFileCount], seed: 0);
        var graph = LoadGraph();

        Assert.NotEmpty(graph.Rooms);
        foreach (var room in graph.Rooms)
        {
            foreach (var rdtId in room.Rdts)
            {
                Assert.True(
                    context.RoomIndexById.ContainsKey(rdtId),
                    $"Graph room {room.Id} references RDT '{rdtId}' which does not resolve to any AFS file");
            }
        }
    }

    [Fact]
    public void ItemModifier_RandomizesItemTypes_InRealRdt()
    {
        // RM_0000 (START) matches graph room "1000": graph item ids 4, 7, 8
        // map to AOT stages 4, 0, 6 -> item table entries 0x0C, 0x08, 0x15.
        // ItemModifier must overwrite byte 6 of those entries with a placed
        // type (different from vanilla) and keep every AOT slot pickable.
        var randomized = false;
        for (var seed = 0; seed < 10 && !randomized; seed++)
        {
            var original = BuildStartRoom();
            var vanillaTypes = GraphSlotByte4(original);

            var context = CreateContext(new[] { original }, seed);
            new ItemModifier().Apply(context, context.Logger);

            var modified = context.Rooms[0];

            // The RDT bytes must actually have been rewritten.
            Assert.False(original.Data.Span.SequenceEqual(modified.Data.Span));

            // Byte 6 of every AOT-referenced slot must be non-zero so the
            // pickup hack never reads a zero item type.
            for (var aotIndex = 0; aotIndex < modified.Aots.Length; aotIndex++)
            {
                var stage = modified.Aots[aotIndex].Stage;
                Assert.True(stage < modified.Items.Length, $"AOT[{aotIndex}] stage {stage} out of range");
                var type = modified.Items[stage].Type;
                Assert.NotEqual(0, (type >> 16) & 0xFF);
            }

            // At least one graph-mapped slot must now differ from its vanilla
            // byte 4 type (i.e. the item was actually randomized).
            var newTypes = GraphSlotType(modified);
            if (newTypes.Zip(vanillaTypes).Any(pair => pair.First != pair.Second))
                randomized = true;
        }

        Assert.True(
            randomized,
            "ItemModifier did not change any graph-mapped item type across seeds 0-9");
    }

    private static ReCvRandomizerContext CreateContext(RdtCv[] rooms, int seed)
    {
        var config = new ReCvRandomizer("recvx.iso").DefaultConfiguration;
        return new ReCvRandomizerContext(null!, rooms, seed, config, new RandomizerLogger());
    }

    /// <summary>
    /// Vanilla item type (byte 4) of the slots referenced by the graph-mapped
    /// AOTs in RM_0000 (AOT indices 4, 7, 8).
    /// </summary>
    private static byte[] GraphSlotByte4(RdtCv room)
    {
        int[] graphAotIndices = [4, 7, 8];
        return graphAotIndices
            .Select(aotIndex => room.Aots[aotIndex].Stage)
            .Select(stage => (byte)(room.Items[stage].Type & 0xFF))
            .ToArray();
    }

    /// <summary>
    /// Item type byte actually read by the game after the pickup patch
    /// (byte 6, the high 16 bits of the item table entry) for the slots
    /// referenced by the graph-mapped AOTs in RM_0000 (AOT indices 4, 7, 8).
    /// </summary>
    private static byte[] GraphSlotType(RdtCv room)
    {
        int[] graphAotIndices = [4, 7, 8];
        return graphAotIndices
            .Select(aotIndex => room.Aots[aotIndex].Stage)
            .Select(stage => (byte)(room.Items[stage].Type >> 16))
            .ToArray();
    }

    private static GraphData LoadGraph()
    {
        var assembly = typeof(ReCvKeyRandomizer).Assembly;
        using var stream = assembly.GetManifestResourceStream(
            "IntelOrca.Biohazard.BioRand.RECV.data.graph.json");
        if (stream == null)
            throw new InvalidOperationException("Embedded resource data/graph.json not found");

        return JsonSerializer.Deserialize<GraphData>(stream, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
        })!;
    }

    /// <summary>
    /// Builds a synthetic RM_0000 (START) RDT: real vanilla item table and the
    /// AOT stage mapping for graph item ids 4, 7, 8, plus minimal but valid
    /// model/motion/script/texture chunks so the Builder round-trip works.
    /// </summary>
    private static RdtCv BuildStartRoom()
    {
        var builder = new RdtCv.Builder();

        // Vanilla RM_0000 item table (byte 4 type).
        foreach (var type in new byte[] { 0x08, 0x5F, 0x68, 0x32, 0x0C, 0x5F, 0x15, 0x85 })
        {
            builder.Items.Add(new RdtCv.Item { Type = type });
        }

        // AOTs: graph item id -> Aot[Id].Stage -> Items[Stage]. Mirror the
        // real RM_0000 mapping for ids 4 (-> Items[4] 0x0C), 7 (-> Items[0]
        // 0x08) and 8 (-> Items[6] 0x15).
        for (var i = 0; i < 9; i++)
        {
            var stage = i switch
            {
                4 => 4,
                7 => 0,
                8 => 6,
                _ => 0,
            };
            builder.Aots.Add(new RdtCv.Aot
            {
                Kind = (byte)(i is 4 or 7 or 8 ? 4 : 3),
                Stage = (byte)stage,
            });
        }

        // Minimal but valid chunks so ToBuilder()/ToRdt() round-trips cleanly.
        builder.Models = new CvModelList(0, new byte[64]);   // 16 zero ints
        builder.Motions = new CvMotionList(0, new byte[64]);
        builder.Script = new ScdProcedureList(BioVersion.BiohazardCv, new byte[16]);

        using var texture = new MemoryStream();
        var bw = new BinaryWriter(texture);
        bw.Write(1);          // group count
        bw.Write(8);          // group offset
        bw.Write(0x324D4954); // TIM2 magic
        bw.Write(0);          // data length
        bw.Write(new byte[24]); // padding to 32 bytes
        builder.Textures = new CvTextureList(0, texture.ToArray());

        return builder.ToRdt();
    }
}
