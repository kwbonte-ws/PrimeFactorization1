using PrimeFactorization.Application;

namespace PrimeFactorization.Console;

public class ConsoleSession(FactorizationUseCase factorization, TextReader input, TextWriter output)
{
    public void Run()
    {
        output.WriteLine("Welcome to PrimeFactorization!");
        while (true)
        {
            output.Write("Enter a positive integer or type \"exit\": ");
            var line = input.ReadLine();
            if (line is null)
            {
                return;
            }

            if (string.Equals(line.Trim(), "exit", StringComparison.OrdinalIgnoreCase))
            {
                output.WriteLine("Goodbye!");
                return;
            }

            if (!int.TryParse(line, out var number) || number <= 0)
            {
                output.WriteLine($"Please enter a positive integer from 1 to {int.MaxValue}, or type \"exit\".");
                continue;
            }

            var factors = factorization.Execute(number);
            WriteFactors(number, factors);
        }
    }

    private void WriteFactors(int number, IReadOnlyList<int> factors)
    {
        var result = factors.Count == 0
            ? $"{number} has no prime factors."
            : $"{number} = {string.Join(" × ", factors)}";

        output.WriteLine(result);
    }
}
