using System.CommandLine;
using IntelOrca.Biohazard.BioRand;
using IntelOrca.Biohazard.BioRand.RECV;

#if DEBUG
if (args.Length > 0 && args[0] == "generate" && System.IO.File.Exists(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "biorand-recv.slnx")))
{
    System.Console.Error.WriteLine("error: generating a randomizer from the solution directory is not allowed. change to a seed output directory first.");
    return 1;
}
#endif

var inputOption = new Option<FileInfo>("-i", ["--input"])
{
    Description = "Input vanilla ISO file",
    Required = true
};

var outputOption = new Option<FileInfo>("-o", ["--output"])
{
    Description = "Output randomized ISO file",
    Required = true
};

var seedOption = new Option<int>("--seed", [])
{
    Description = "Random seed",
    DefaultValueFactory = _ => 0
};

var configOption = new Option<FileInfo?>("-c", ["--config"])
{
    Description = "Configuration file (JSON)"
};

var generateCommand = new Command("generate", "Generate a randomized ISO")
{
    inputOption,
    outputOption,
    seedOption,
    configOption
};

generateCommand.SetAction(parseResult =>
{
    var input = parseResult.GetValue(inputOption);
    var output = parseResult.GetValue(outputOption);
    var seed = parseResult.GetValue(seedOption);
    var configFile = parseResult.GetValue(configOption);

    var config = new RandomizerConfiguration();
    if (configFile != null && configFile.Exists)
    {
        var json = File.ReadAllText(configFile.FullName);
        config = RandomizerConfiguration.FromJson(json);
    }

    var randoInput = new RandomizerInput
    {
        Seed = seed,
        Configuration = config
    };

    var progress = new ConsoleRandomizerProgress();
    var generator = new ReCvRandomizerGenerator(
        input!.FullName,
        output!.FullName,
        randoInput,
        progress);

    var logOutput = "";
    var mermaidOutput = "";
    generator.OnLog += msg =>
    {
        Console.WriteLine(msg);
        logOutput = msg;
    };
    generator.OnMermaid += msg => mermaidOutput = msg;

    var result = generator.GenerateAsync().GetAwaiter().GetResult();

    Console.WriteLine();

    if (logOutput.Length > 0)
    {
        var logPath = Path.ChangeExtension(output!.FullName, ".log");
        File.WriteAllText(logPath, logOutput);
        Console.WriteLine($"Log: {logPath}");
    }

    if (mermaidOutput.Length > 0)
    {
        var mmdPath = Path.ChangeExtension(output!.FullName, ".mmd");
        File.WriteAllText(mmdPath, mermaidOutput);
        Console.WriteLine($"Graph: {mmdPath}");
    }

    foreach (var asset in result.Assets)
    {
        if (asset.Key == "iso")
            continue;

        var assetPath = Path.Combine(
            Path.GetDirectoryName(output!.FullName)!,
            $"{Path.GetFileNameWithoutExtension(output.FullName)}.{asset.FileName}");
        File.WriteAllBytes(assetPath, asset.Data);
        Console.WriteLine($"{asset.Title}: {assetPath}");
    }

    Console.WriteLine($"Generated: {output.FullName}");
    return 0;
});

var urlArgument = new Argument<string>("url")
{
    Description = "BioRand API base URI"
};

var apiKeyOption = new Option<string>("-k", [])
{
    Description = "API key",
    Required = true
};

var isoFileOption = new Option<FileInfo>("-i", [])
{
    Description = "Input vanilla ISO file",
    Required = true
};

var agentCommand = new Command("agent", "Run as a cloud agent")
{
    urlArgument,
    apiKeyOption,
    isoFileOption
};

agentCommand.SetAction(parseResult =>
{
    var baseUri = parseResult.GetValue(urlArgument);
    var apiKey = parseResult.GetValue(apiKeyOption);
    var isoFile = parseResult.GetValue(isoFileOption);

    var client = new RandomizerClient(baseUri!);
    var games = client.GetGamesAsync().GetAwaiter().GetResult();
    var game = games.FirstOrDefault(g => g.Moniker == "recv");
    if (game == null)
    {
        Console.Error.WriteLine("Game 'recv' not found on server");
        return 1;
    }

    var handler = new ReCvRandomizer(isoFile!.FullName);
    var agent = new RandomizerAgent(baseUri!, apiKey!, game.Id, handler);
    agent.RunAsync().GetAwaiter().GetResult();
    return 0;
});

var rootCommand = new RootCommand("BioRand RECV - Resident Evil Code: Veronica Randomizer");
rootCommand.Subcommands.Add(generateCommand);
rootCommand.Subcommands.Add(agentCommand);

return rootCommand.Parse(args).Invoke();

sealed class ConsoleRandomizerProgress : IRandomizerProgress
{
    public void RunTask(string text, Action cb)
    {
        Console.Write($"{text}... ");
        cb();
        Console.WriteLine("done");
    }
}
