using System.CommandLine;
using IntelOrca.Biohazard.BioRand;
using IntelOrca.Biohazard.BioRand.RECV;

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

    generator.OnLog += msg => Console.WriteLine(msg);

    var result = generator.GenerateAsync().GetAwaiter().GetResult();

    Console.WriteLine();
    Console.WriteLine($"Generated: {output.FullName}");
    return 0;
});

var urlArgument = new Argument<string>("url")
{
    Description = "BioRand API base URI"
};

var apiKeyOption = new Option<string>("-k", ["--api-key"])
{
    Description = "API key",
    Required = true
};

var isoDirOption = new Option<DirectoryInfo>("-d", ["--iso-dir"])
{
    Description = "Directory containing vanilla ISO files",
    Required = true
};

var agentCommand = new Command("agent", "Run as a cloud agent")
{
    urlArgument,
    apiKeyOption,
    isoDirOption
};

agentCommand.SetAction(parseResult =>
{
    var baseUri = parseResult.GetValue(urlArgument);
    var apiKey = parseResult.GetValue(apiKeyOption);
    var isoDir = parseResult.GetValue(isoDirOption);

    var handler = new ReCvRandomizer(isoDir!.FullName);
    var agent = new RandomizerAgent(baseUri!, apiKey!, 0, handler);
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
