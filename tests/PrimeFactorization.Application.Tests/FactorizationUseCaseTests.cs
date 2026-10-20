using AwesomeAssertions;
using PrimeFactorization.Application;

namespace PrimeFactorization.Application.Tests;

public class FactorizationUseCaseTests
{
    [Theory]
    [InlineData(1, new int[] { })]
    [InlineData(2, new int[] { 2 })]
    [InlineData(12, new int[] { 2, 2, 3 })]
    [InlineData(13, new int[] { 13 })]
    public void Execute_PositiveInteger_ReturnsDomainPrimeFactors(int number, int[] expected)
    {
        var useCase = new FactorizationUseCase();

        var factors = useCase.Execute(number);

        factors.Should().Equal(expected);
    }
}
