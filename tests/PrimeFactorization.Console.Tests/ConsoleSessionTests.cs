using AwesomeAssertions;
using PrimeFactorization.Application;

namespace PrimeFactorization.Console.Tests;

public class ConsoleSessionTests
{
    [Fact]
    public void Run_One_ExplainsThatThereAreNoPrimeFactorsAndContinues()
    {
        using var input = new StringReader("1\n2\nexit\n");
        using var output = new StringWriter();
        var session = new ConsoleSession(new FactorizationUseCase(), input, output);

        session.Run();

        output.ToString().Should().Be(
            $"Welcome to PrimeFactorization!{Environment.NewLine}" +
            $"Enter a positive integer or type \"exit\": 1 has no prime factors.{Environment.NewLine}" +
            $"Enter a positive integer or type \"exit\": 2 = 2{Environment.NewLine}" +
            $"Enter a positive integer or type \"exit\": Goodbye!{Environment.NewLine}");
    }

    [Theory]
    [InlineData("not a number")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("-2147483648")]
    [InlineData("2147483648")]
    [InlineData("-2147483649")]
    [InlineData("2.5")]
    [InlineData("exit now")]
    public void Run_InvalidInput_DisplaysGuidanceAndContinuesUntilExit(string invalidInput)
    {
        using var input = new StringReader($"{invalidInput}\n6\nexit\n");
        using var output = new StringWriter();
        var session = new ConsoleSession(new FactorizationUseCase(), input, output);

        session.Run();

        output.ToString().Should().Be(
            $"Welcome to PrimeFactorization!{Environment.NewLine}" +
            "Enter a positive integer or type \"exit\": " +
            $"Please enter a positive integer from 1 to 2147483647, or type \"exit\".{Environment.NewLine}" +
            $"Enter a positive integer or type \"exit\": 6 = 2 × 3{Environment.NewLine}" +
            $"Enter a positive integer or type \"exit\": Goodbye!{Environment.NewLine}");
    }

    [Fact]
    public void Run_MultipleNumbers_DisplaysEachResultAndPromptsUntilExit()
    {
        using var input = new StringReader("12\n7\nexit\n");
        using var output = new StringWriter();
        var session = new ConsoleSession(new FactorizationUseCase(), input, output);

        session.Run();

        output.ToString().Should().Be(
            $"Welcome to PrimeFactorization!{Environment.NewLine}" +
            $"Enter a positive integer or type \"exit\": 12 = 2 × 2 × 3{Environment.NewLine}" +
            $"Enter a positive integer or type \"exit\": 7 = 7{Environment.NewLine}" +
            $"Enter a positive integer or type \"exit\": Goodbye!{Environment.NewLine}");
    }

    [Fact]
    public void Run_ValidNumber_DisplaysPrimeFactorsWithMultiplicationSeparators()
    {
        using var input = new StringReader("12\n");
        using var output = new StringWriter();
        var session = new ConsoleSession(new FactorizationUseCase(), input, output);

        session.Run();

        output.ToString().Should().Contain($"12 = 2 × 2 × 3{Environment.NewLine}");
    }

    [Theory]
    [InlineData("EXIT")]
    [InlineData(" Exit ")]
    [InlineData("\texIt\t")]
    public void Run_ExitWithDifferentCasingAndWhitespace_TerminatesSession(string command)
    {
        using var input = new StringReader($"{command}\n12\n");
        using var output = new StringWriter();
        var session = new ConsoleSession(new FactorizationUseCase(), input, output);

        session.Run();

        output.ToString().Should().EndWith($"Goodbye!{Environment.NewLine}");
        input.ReadToEnd().Should().Be("12\n");
    }

    [Fact]
    public void Run_Exit_DisplaysGoodbyeAndLeavesFurtherInputUnread()
    {
        using var input = new StringReader("exit\n12\n");
        using var output = new StringWriter();
        var session = new ConsoleSession(new FactorizationUseCase(), input, output);

        session.Run();

        output.ToString().Should().EndWith($"Goodbye!{Environment.NewLine}");
        input.ReadToEnd().Should().Be("12\n");
    }

    [Fact]
    public void Run_EndOfInput_ReadsOnceAndReturnsWithoutGoodbye()
    {
        using var input = new EndOfInputReader();
        using var output = new StringWriter();
        var session = new ConsoleSession(new FactorizationUseCase(), input, output);

        session.Run();

        input.ReadCount.Should().Be(1);
        output.ToString().Should().NotContain("Goodbye!");
    }

    [Fact]
    public void Run_WhenStarted_DisplaysWelcomeAndPrompt()
    {
        using var input = new StringReader("");
        using var output = new StringWriter();
        var session = new ConsoleSession(new FactorizationUseCase(), input, output);

        session.Run();

        output.ToString().Should().Be(
            $"Welcome to PrimeFactorization!{Environment.NewLine}" +
            "Enter a positive integer or type \"exit\": ");
    }

    private sealed class EndOfInputReader : TextReader
    {
        public int ReadCount
        {
            get;
            private set;
        }

        public override string? ReadLine()
        {
            ReadCount++;
            if (ReadCount > 1)
            {
                throw new InvalidOperationException("Input must not be read again after EOF.");
            }

            return null;
        }
    }
}
