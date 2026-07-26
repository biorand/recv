namespace IntelOrca.Biohazard.BioRand.RECV.Patches;

/// <summary>
/// Marks a method in <see cref="ReCvRdtPatcher"/> as an RDT script patch.
/// Each parameter should be named <c>rdtXXXX</c> to indicate which RDT it patches
/// (e.g., <c>rdt1010</c> resolves to RDT "1010"). The optional <see cref="Gate"/>
/// property gates the patch behind a config toggle.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class RdtPatchAttribute : Attribute
{
    /// <summary>
    /// Optional config key (e.g., <c>"doors/random"</c>) that must be <c>true</c>
    /// for this patch to be applied. When set to a negated key with a leading <c>!</c>
    /// (e.g., <c>"!doors/random"</c>), the patch is applied only when the value is <c>false</c>.
    /// When <c>null</c>, the patch is always applied.
    /// </summary>
    public string? Gate { get; set; }
}
