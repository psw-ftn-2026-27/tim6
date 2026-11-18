using Xunit;

namespace Host.Tests.DependencyTests;

// Another module is reachable only through its Contracts project, including Identity's public contract.
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

    // A module stores an image only as its reference string and receives a position from its own page,
    // so it never touches the types of the Shared image and position capabilities.
    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void Module_does_not_depend_on_the_shared_capabilities(string module)
    {
        foreach (var assembly in Layers.Select(layer => $"{module}.{layer}"))
        {
            AssertNoDependency(assembly, "Shared.Application");
            AssertNoNamespaceDependency(assembly, "Shared.Domain.Images", "Shared.Domain.Positions");
        }
    }

    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void Module_does_not_depend_on_identity_internals(string module)
    {
        foreach (var assembly in Layers.Select(layer => $"{module}.{layer}"))
        {
            AssertNoDependency(assembly, "Identity");
        }
    }
}
