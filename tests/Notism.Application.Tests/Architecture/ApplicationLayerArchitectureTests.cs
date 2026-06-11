using System.Reflection;

using FluentAssertions;

using NetArchTest.Rules;

namespace Notism.Application.Tests.Architecture;

/// <summary>
/// Guards the read/write boundary: the Application layer composes its reads through the
/// <c>IReadDbContext</c> port and never reaches into persistence. It must depend on
/// neither <c>Notism.Infrastructure</c> nor Entity Framework Core, so query execution
/// stays entirely in Infrastructure.
/// </summary>
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

    [Fact]
    public void Application_ShouldNotReference_EntityFrameworkCore()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "the Application layer must not depend on Entity Framework Core: {0}",
            string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }
}
