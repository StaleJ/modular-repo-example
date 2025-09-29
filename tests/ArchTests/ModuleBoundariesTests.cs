using NetArchTest.Rules;
using Xunit;

namespace ArchTests;

public class ModuleBoundariesTests
{
    [Fact]
    public void LoanApplication_Should_Not_Depend_On_Customer_Infrastructure()
    {
        // Load LoanApplication assemblies
        var loanAppAssemblies = new[]
        {
            typeof(LoanApplication.Infrastructure.LoanAppDbContext).Assembly,
            typeof(LoanApplication.Domain.Class1).Assembly,
            typeof(LoanApplication.Contracts.Class1).Assembly
        };

        var result = Types.InAssemblies(loanAppAssemblies)
            .Should()
            .NotHaveDependencyOnAll("Customer.Infrastructure")
            .GetResult();

        var failing = result.FailingTypeNames ?? System.Array.Empty<string>();
        Assert.True(result.IsSuccessful, string.Join(System.Environment.NewLine, failing));
    }
}
