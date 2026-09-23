using Xunit;

namespace Architecture.Tests;

public sealed class DependencyRuleTests
{
    [Fact]
    public void Domain_does_not_reference_outer_layers()
    {
        var references = Domain.AssemblyReference.Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.DoesNotContain("Application", references);
        Assert.DoesNotContain("Infrastructure", references);
        Assert.DoesNotContain("Web", references);
    }

    [Fact]
    public void Application_does_not_reference_infrastructure_or_web()
    {
        var references = Application.AssemblyReference.Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.DoesNotContain("Infrastructure", references);
        Assert.DoesNotContain("Web", references);
    }

    [Fact]
    public void Infrastructure_does_not_reference_web()
    {
        var references = Infrastructure.AssemblyReference.Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.DoesNotContain("Web", references);
    }
}
