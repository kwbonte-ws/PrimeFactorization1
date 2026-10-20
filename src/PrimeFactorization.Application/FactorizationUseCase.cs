using PrimeFactorization.Domain;

namespace PrimeFactorization.Application;

public class FactorizationUseCase
{
    public IReadOnlyList<int> Execute(int number)
    {
        return new PrimeFactorizer().Factorize(number);
    }
}
