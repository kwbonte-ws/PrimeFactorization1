using PrimeFactorization.Application;
using PrimeFactorization.Console;

var session = new ConsoleSession(new FactorizationUseCase(), System.Console.In, System.Console.Out);
session.Run();
