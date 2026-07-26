using System.Collections.Immutable;
using IntelOrca.Biohazard;
using IntelOrca.Biohazard.Room;
using Ps2IsoTools.UDF;
using Ps2IsoTools.UDF.Files;

namespace IntelOrca.Biohazard.BioRand.RECV;

public sealed class ReCvRandomizerGenerator
{
    private const string ElfFileName = "SLUS_201.84";

    private readonly string _inputPath;
    private readonly string _outputPath;
    private readonly RandomizerInput _input;
    private readonly IRandomizerProgress _progress;

    public Action<string>? OnLog { get; set; }
    public Action<string>? OnMermaid { get; set; }

    private UdfEditor? _udfEditor;
    private ElfRegion? _elf;
    private RdtCv[]? _rooms;
    private byte[][]? _originalRoomData;
    private AfsFile? _rdxAfs;
    private AfsFile? _advAfs;
    private AfsFile? _systemAfs;
    private FileIdentifier? _rdxAfsFileId;
    private FileIdentifier? _advAfsFileId;
    private FileIdentifier? _elfFileId;
    private FileIdentifier? _systemAfsFileId;

    public ReCvRandomizerGenerator(
        string inputPath,
        string outputPath,
        RandomizerInput input,
        IRandomizerProgress progress)
    {
        _inputPath = inputPath;
        _outputPath = outputPath;
        _input = input;
        _progress = progress;
    }

    public async Task<RandomizerOutput> GenerateAsync()
    {
        var logger = new RandomizerLogger();
        logger.LogVersionTimeInfo("BioRand RECV", "Ted John");

        _progress.RunTask("Reading ISO", OpenIso);
        _progress.RunTask("Loading ELF", LoadElf);
        _progress.RunTask("Loading rooms", LoadRooms);

        var context = new ReCvRandomizerContext(
            _elf!, _rooms!, _input.Seed, _input.Configuration, logger);
        context.AdvAfs = _advAfs;

        logger.LogHeader("Applying patches");
        ApplyPatches(context);

        logger.LogHeader("Applying modifiers");
        ApplyModifiers(context);

        _advAfs = context.AdvAfs ?? _advAfs;

        _progress.RunTask("Saving rooms", SaveRooms);
        _progress.RunTask("Building ISO", BuildIso);

        var isoData = await File.ReadAllBytesAsync(_outputPath);
        var asset = new RandomizerOutputAsset(
            "iso",
            "BioRand RECV",
            "Randomized Resident Evil Code: Veronica ISO",
            "recvx_biorand.iso",
            isoData);

        var instructions = "<p>Door skip has been applied.</p>";

        logger.LogHeader("Output");
        logger.LogLine(_outputPath);

        OnLog?.Invoke(logger.Output);

        if (context.MermaidGraph != null)
            OnMermaid?.Invoke(context.MermaidGraph);

        return new RandomizerOutput(ImmutableArray.Create(asset), instructions);
    }

    private void OpenIso()
    {
        _udfEditor = new UdfEditor(_inputPath, _outputPath);

        _rdxAfsFileId = _udfEditor.GetFileByName("RDX_LNK.AFS")
            ?? throw new RandomizerUserException("RDX_LNK.AFS not found in ISO");
        _advAfsFileId = _udfEditor.GetFileByName("ADV.AFS")
            ?? throw new RandomizerUserException("ADV.AFS not found in ISO");
        _systemAfsFileId = _udfEditor.GetFileByName("SYSTEM.AFS")
            ?? throw new RandomizerUserException("SYSTEM.AFS not found in ISO");
        _elfFileId = _udfEditor.GetFileByName(ElfFileName)
            ?? throw new RandomizerUserException($"{ElfFileName} not found in ISO");

        _advAfs = ReadAfs(_advAfsFileId);
        _systemAfs = ReadAfs(_systemAfsFileId);
    }

    private void LoadElf()
    {
        var elfData = ReadIsoFile(_elfFileId!);
        _elf = new ElfRegion(elfData);
    }

    private void LoadRooms()
    {
        var rdxData = ReadIsoFile(_rdxAfsFileId!);
        _rdxAfs = new AfsFile(rdxData);
        var roomCount = _rdxAfs.Count;
        _rooms = new RdtCv[roomCount];
        _originalRoomData = new byte[roomCount][];

        for (var i = 0; i < roomCount; i++)
        {
            var compressed = _rdxAfs.GetFileData(i);
            var prs = new PrsFile(compressed);
            var uncompressed = prs.Uncompressed;
            _originalRoomData[i] = uncompressed.ToArray();
            _rooms[i] = new RdtCv(uncompressed);
        }
    }

    private void ApplyPatches(ReCvRandomizerContext context)
    {
        var patchTypes = GetType().Assembly.GetTypes()
            .Where(t => !t.IsAbstract && typeof(ICvPatch).IsAssignableFrom(t));

        foreach (var type in patchTypes)
        {
            var patch = (ICvPatch)Activator.CreateInstance(type)!;
            context.Logger.Push(type.Name);
            patch.Apply(context);
            context.Logger.Pop();
        }
    }

    private void ApplyModifiers(ReCvRandomizerContext context)
    {
        var modifierTypes = GetType().Assembly.GetTypes()
            .Where(t => !t.IsAbstract && typeof(ICvModifier).IsAssignableFrom(t))
            .OrderBy(t =>
            {
                var attr = t.GetCustomAttributes(typeof(OrderAttribute), false);
                return attr.Length > 0 ? ((OrderAttribute)attr[0]).Order : 0;
            });

        foreach (var type in modifierTypes)
        {
            var modifier = (ICvModifier)Activator.CreateInstance(type)!;
            context.Logger.Push(type.Name);
            modifier.Apply(context, context.Logger);
            context.Logger.Pop();
        }
    }

    private void SaveRooms()
    {
        var builder = _rdxAfs!.ToBuilder();
        var anyModified = false;
        for (var i = 0; i < _rooms!.Length; i++)
        {
            var room = _rooms[i];
            var currentData = room.Data.ToArray();
            var originalData = _originalRoomData![i];

            if (currentData.AsSpan().SequenceEqual(originalData))
                continue;

            var rebuilt = room.ToBuilder().ToRdt();
            var compressed = PrsFile.Compress(rebuilt.Data);
            builder.Replace(i, compressed.Data.ToArray());
            anyModified = true;
        }
        if (anyModified)
            _rdxAfs = builder.ToAfsFile();
    }

    private void BuildIso()
    {
        var elfData = _elf!.ToArray();
        _udfEditor!.ReplaceFileStream(_elfFileId!, new MemoryStream(elfData));
        _udfEditor.ReplaceFileStream(_rdxAfsFileId!, new MemoryStream(_rdxAfs!.Data.ToArray()));
        _udfEditor.ReplaceFileStream(_advAfsFileId!, new MemoryStream(_advAfs!.Data.ToArray()));
        _udfEditor.ReplaceFileStream(_systemAfsFileId!, new MemoryStream(_systemAfs!.Data.ToArray()));
        _udfEditor.Rebuild(_outputPath);
        _udfEditor.Dispose();
        _udfEditor = null;
    }

    private AfsFile ReadAfs(FileIdentifier fileId)
    {
        var data = ReadIsoFile(fileId);
        return new AfsFile(data);
    }

    private byte[] ReadIsoFile(FileIdentifier fileId)
    {
        using var stream = _udfEditor!.GetFileStream(fileId);
        var data = new byte[stream.Length];
        stream.ReadExactly(data);
        return data;
    }
}
