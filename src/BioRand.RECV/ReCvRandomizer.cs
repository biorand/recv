using System.Reflection;

namespace IntelOrca.Biohazard.BioRand.RECV;

public sealed class ReCvRandomizer : IRandomizerAgentHandler
{
    private readonly string _isoDirectory;

    public ReCvRandomizer(string isoDirectory)
    {
        _isoDirectory = isoDirectory;
    }

    public string BuildVersion =>
        Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
            ?? "0.0.0";

    public RandomizerConfigurationDefinition ConfigurationDefinition =>
        ReCvConfigurationDefinition.Create();

    public RandomizerConfiguration DefaultConfiguration =>
        ConfigurationDefinition.GetDefault();

    public Task<bool> CanGenerateAsync(RandomizerAgent.QueueResponseItem queueItem)
    {
        return Task.FromResult(true);
    }

    public async Task<RandomizerOutput> GenerateAsync(
        RandomizerAgent.QueueResponseItem queueItem,
        RandomizerInput input)
    {
        var inputPath = Path.Combine(_isoDirectory, "recvx.iso");
        var outputPath = Path.GetTempFileName();

        try
        {
            var generator = new ReCvRandomizerGenerator(
                inputPath,
                outputPath,
                input,
                new DummyRandomizerProgress());
            return await generator.GenerateAsync();
        }
        finally
        {
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
    }

    public void LogInfo(string message)
    {
        Console.WriteLine(message);
    }

    public void LogError(Exception ex, string message)
    {
        Console.Error.WriteLine($"{message}: {ex.Message}");
    }
}
