using VAA.EAPI.Automation.Core.Http.Mocking;
using Xunit;

namespace VAA.EAPI.Automation.Core.Tests;

public sealed class CommandParserTests
{
    [Fact]
    public void ParseReturnsUnchangedTemplateWhenNoDynamicCommandsExist()
    {
        string template = new string('x', 1024 * 1024);
        using CommandParser parser = (CommandParser)template;

        string result = parser.Parse();

        Assert.Same(template, result);
    }

    [Fact]
    public void ParseStillProcessesDynamicCommands()
    {
        using CommandParser parser = (CommandParser)"{Guid()}";

        string result = parser.Parse();

        Assert.True(Guid.TryParse(result, out _));
    }
}
