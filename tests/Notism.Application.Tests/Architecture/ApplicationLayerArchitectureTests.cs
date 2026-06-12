using System.Reflection;

using FluentAssertions;

using NetArchTest.Rules;

namespace Notism.Application.Tests.Architecture;

/// <summary>
/// Guards the layering boundary: the Application layer composes its persistence over the
/// <c>IReadDbContext</c> port and the <c>IRepository</c> write boundary, executing reads with
/// Entity Framework Core operators directly. It may reference EF Core, but it must NOT depend
/// on <c>Notism.Infrastructure</c> — query execution composes over ports, never over an
/// Infrastructure type.
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
}
