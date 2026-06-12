using System.Reflection;

using FluentAssertions;

using NetArchTest.Rules;

namespace Notism.Application.Tests.Architecture;

public class ApplicationLayerArchitectureTests
{
    private static readonly Assembly ApplicationAssembly =
        typeof(Notism.Application.Common.Persistence.IReadDbContext).Assembly;

    [Fact]
    public void Application_ShouldNotReference_Infrastructure()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn("Notism.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "the Application layer must not depend on Infrastructure: {0}",
            string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }
}