using VAA.EAPI.Common.Http;

namespace VAA.EAPI.Microservices.Tests;

public sealed class CommandParserTests
{
    [Fact]
    public void ParseReturnsUnchangedTemplateWhenNoDynamicCommandsExist()
    {
        var template = new string('x', 1024 * 1024);
        using var parser = (CommandParser)template;

        var result = parser.Parse();

        Assert.Same(template, result);
    }

    [Fact]
    public void ParseStillProcessesDynamicCommands()
    {
        using var parser = (CommandParser)"{Guid()}";

        var result = parser.Parse();

        Assert.True(Guid.TryParse(result, out _));
    }
}
