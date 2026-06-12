namespace IntelOrca.Biohazard.BioRand.RECV;

[AttributeUsage(AttributeTargets.Class)]
public sealed class OrderAttribute(int order) : Attribute
{
    public int Order => order;
}

public interface ICvModifier
{
    void Apply(ReCvRandomizerContext context, RandomizerLogger logger);
}
