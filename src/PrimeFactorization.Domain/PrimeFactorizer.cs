namespace PrimeFactorization.Domain;

public class PrimeFactorizer
{
    public IReadOnlyList<int> Factorize(int number)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(number);

        var factors = new List<int>();

        for (var divisor = 2; divisor <= number / divisor; divisor++)
        {
            while (number % divisor == 0)
            {
                factors.Add(divisor);
                number /= divisor;
            }
        }

        if (number > 1)
        {
            factors.Add(number);
        }

        return factors;
    }
}
