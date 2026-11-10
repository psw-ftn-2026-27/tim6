using Xunit;

namespace Host.Tests.DependencyTests;

// Another module is reachable only through its Contracts project, and Identity is reachable by no module at all.
public class ModuleIsolationTests : BaseArchitectureTests
{
    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void Module_reaches_other_modules_only_through_their_contracts(string module)
    {
        foreach (var other in Modules.Where(other => other != module))
        {
            var forbidden = Layers.Where(layer => layer != "Contracts")
                .Select(layer => $"{other}.{layer}")
                .ToArray();
            foreach (var assembly in Layers.Select(layer => $"{module}.{layer}"))
            {
                AssertNoDependency(assembly, forbidden);
            }
        }
    }

    // A module stores an image only as its reference string, so it never touches the Shared image capability's types.
    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void Module_does_not_depend_on_the_shared_image_capability(string module)
    {
        foreach (var assembly in Layers.Select(layer => $"{module}.{layer}"))
        {
            AssertNoDependency(assembly, "Shared.Application");
            AssertNoNamespaceDependency(assembly, "Shared.Domain.Images");
        }
    }

    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void Module_does_not_depend_on_identity(string module)
    {
        foreach (var assembly in Layers.Select(layer => $"{module}.{layer}"))
        {
            AssertNoDependency(assembly, "Identity");
        }
    }
}
