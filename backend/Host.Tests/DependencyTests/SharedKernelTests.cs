using Xunit;

namespace Host.Tests.DependencyTests;

// No Shared project may know about any module, Shared.Domain stays free of frameworks, and the
// Shared image capability keeps a module's layering.
public class SharedKernelTests : BaseArchitectureTests
{
    [Theory]
    [MemberData(nameof(SharedProjectNames))]
    public void Shared_project_does_not_depend_on_any_module(string sharedProject)
    {
        var moduleAssemblies = Modules
            .SelectMany(module => Layers.Select(layer => $"{module}.{layer}"))
            .Append("Identity.Contracts")
            .Append("Identity")
            .ToArray();
        AssertNoDependency(sharedProject, moduleAssemblies);
    }

    [Fact]
    public void Shared_domain_is_framework_free()
    {
        AssertNoNamespaceDependency("Shared.Domain", "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore");
    }

    // The Shared image capability is layered like a module: the same rules as ModuleLayerTests.
    [Fact]
    public void Shared_application_depends_only_on_shared_domain()
    {
        AssertNoDependency("Shared.Application", "Shared.Infrastructure", "Shared.Api");
        AssertNoNamespaceDependency("Shared.Application", "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore");
    }

    [Fact]
    public void Shared_api_does_not_depend_on_shared_infrastructure_ef_or_image_domain()
    {
        AssertNoDependency("Shared.Api", "Shared.Infrastructure");
        AssertNoNamespaceDependency("Shared.Api", "Microsoft.EntityFrameworkCore", "Shared.Domain.Images");
    }

    [Fact]
    public void Shared_infrastructure_does_not_depend_on_shared_api()
    {
        AssertNoDependency("Shared.Infrastructure", "Shared.Api");
    }
}
