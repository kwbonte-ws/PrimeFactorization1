using AwesomeAssertions;
using PrimeFactorization.Domain;

namespace PrimeFactorization.Domain.Tests;

public class PrimeFactorizerTests
{
    [Theory]
    [InlineData(9, new int[] { 3, 3 })]
    [InlineData(25, new int[] { 5, 5 })]
    [InlineData(45, new int[] { 3, 3, 5 })]
    [InlineData(49, new int[] { 7, 7 })]
    [InlineData(360, new int[] { 2, 2, 2, 3, 3, 5 })]
    [InlineData(2147395600, new int[] { 2, 2, 2, 2, 5, 5, 7, 7, 331, 331 })]
    [InlineData(int.MaxValue - 1, new int[] { 2, 3, 3, 7, 11, 31, 151, 331 })]
    public void Factorize_CompositeWithOddFactors_ReturnsAllPrimeFactorsInAscendingOrder(
        int number, int[] expected)
    {
        var factorizer = new PrimeFactorizer();

        var factors = factorizer.Factorize(number);

        factors.Should().Equal(expected);
    }

    [Theory]
    [InlineData(4, new int[] { 2, 2 })]
    [InlineData(8, new int[] { 2, 2, 2 })]
    [InlineData(12, new int[] { 2, 2, 3 })]
    public void Factorize_RepeatedFactorsOfTwo_IncludesEveryOccurrence(int number, int[] expected)
    {
        var factorizer = new PrimeFactorizer();

        var factors = factorizer.Factorize(number);

        factors.Should().Equal(expected);
    }

    [Fact]
    public void Factorize_Six_ReturnsPrimeFactorsInAscendingOrder()
    {
        var factorizer = new PrimeFactorizer();

        var factors = factorizer.Factorize(6);

        factors.Should().Equal(2, 3);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(13)]
    [InlineData(int.MaxValue)]
    public void Factorize_PrimeNumber_ReturnsNumberAsTheOnlyPrimeFactor(int number)
    {
        var factorizer = new PrimeFactorizer();

        var factors = factorizer.Factorize(number);

        factors.Should().Equal(number);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Factorize_NegativeNumber_ThrowsArgumentOutOfRangeException(int number)
    {
        var factorizer = new PrimeFactorizer();

        Action action = () => factorizer.Factorize(number);

        action.Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("number");
    }

    [Fact]
    public void Factorize_Zero_ThrowsArgumentOutOfRangeException()
    {
        var factorizer = new PrimeFactorizer();

        Action action = () => factorizer.Factorize(0);

        action.Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("number");
    }

    [Fact]
    public void Factorize_One_ReturnsEmptyCollection()
    {
        var factorizer = new PrimeFactorizer();

        var factors = factorizer.Factorize(1);

        factors.Should().BeEmpty();
    }

    [Fact]
    public void Factorize_Two_ReturnsTwoAsTheOnlyPrimeFactor()
    {
        var factorizer = new PrimeFactorizer();

        var factors = factorizer.Factorize(2);

        factors.Should().Equal(2);
    }
}
