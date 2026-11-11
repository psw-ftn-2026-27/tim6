using Xunit;

namespace Host.Tests.DependencyTests;

// The host is a composition root and touches a module (and the Shared image capability) only through its two
// wiring extensions, never its inner layers.
public class HostCompositionTests : BaseArchitectureTests
{
    [Fact]
    public void Host_does_not_depend_on_module_domain_application_or_contracts()
    {
        var forbidden = Modules
            .SelectMany(module => new[] { $"{module}.Domain", $"{module}.Application", $"{module}.Contracts" })
            .Append("Shared.Application")
            .ToArray();
        AssertNoDependency("Host.Api", forbidden);
    }
}
